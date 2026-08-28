using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;
using Domain.Combat;
using GameData;
using Systems;

namespace Domain.Player
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private PlayerData playerData;  // 플레이어 데이터 참조
        [SerializeField] private Vector2 moveDirection;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;

        private const float interactRadius = 3f;      // 상호작용 감지 범위
        private const float lootRadius = 1.5f;        // 루팅 감지 범위

        [SerializeField] private LayerMask npcLayer;  // inspector에서 NPC로 지정
        [SerializeField] private LayerMask itemLayer; // inspector에서 item으로 지정
        [SerializeField] private Animator animator;   // 애니메이터 참조

        private DashController _dashController;
        private Rigidbody2D _rb;
        private IInteractable _currentInteractableTarget;

        #region Events
        /// <summary>
        /// Fired when an interactable object enters or exits the player's interaction range.
        /// Passes the current closest IInteractable target, or null if none in range.
        /// UI systems can subscribe to display/hide interact prompts (e.g. "Press [E] to Talk").
        /// </summary>
        public static event Action<IInteractable> OnInteractableDetected;

        /// <summary>
        /// Fired when the player's health changes. Passes (currentHealth, maxHealth).
        /// </summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>
        /// Fired when the player's stamina changes. Passes (currentStamina, maxStamina).
        /// </summary>
        public event Action<float, float> OnStaminaChanged;

        /// <summary>
        /// Fired when the player's health reaches 0.
        /// </summary>
        public event Action OnPlayerDied;
        #endregion

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        // Hook this up to your damage system later —
        // when true, incoming attacks should be ignored
        public bool IsInvincible => _dashController != null && _dashController.IsDashing;

        void Awake()
        {
            _dashController = GetComponent<DashController>();
            _rb = GetComponent<Rigidbody2D>();

            if (playerData != null)
            {
                maxHealth = playerData.MaxHP > 0 ? playerData.MaxHP : maxHealth;
                maxStamina = playerData.MaxStamina > 0 ? playerData.MaxStamina : maxStamina;
            }
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        void Start()
        {
            if (Managers.Input != null)
            {
                Managers.Input.OnInteractPerformed -= OnInteract; // 상호작용 이벤트
                Managers.Input.OnInteractPerformed += OnInteract;
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        private void OnDestroy()
        {
            if (Managers.Input != null)
            {
                Managers.Input.OnInteractPerformed -= OnInteract; // 상호작용 이벤트 해제
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Block movement input while dashing (DashController handles position during dash)
            if (_dashController != null && _dashController.IsDashing) return;

            moveDirection = Managers.Input.MoveDirection;
            animator.SetBool("isMove", moveDirection != Vector2.zero); // 이동 여부에 따라 애니메이션 전환
            flip();

            if (playerData != null)
            {
                playerData.CurrPosition = transform.position; // 현재 위치 업데이트 (맵 상의 좌표)
            }

            TryLootNearbyItems();
            CheckNearbyInteractable();
        }

        void FixedUpdate()
        {
            if (_dashController != null && _dashController.IsDashing) return;

            // 물리 연산을 통한 이동 (떨림 현상 방지)
            float speed = playerData != null ? playerData.Spd : 5f;
            _rb.MovePosition(_rb.position + moveDirection * Time.fixedDeltaTime * speed);
        }

        void flip()
        {
            if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        public void Heal(float amount)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"[PlayerController] Healed {amount} HP. Current HP: {currentHealth}/{maxHealth}");
        }

        public bool TryUseStamina(float amount)
        {
            if (currentHealth <= 0) return false;

            if (currentStamina < amount)
            {
                Debug.Log($"[PlayerController] Not enough stamina! Needed {amount}, current: {currentStamina}/{maxStamina}");
                return false;
            }

            currentStamina -= amount;
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            Debug.Log($"[PlayerController] Consumed {amount} Stamina. Current Stamina: {currentStamina}/{maxStamina}");
            return true;
        }

        public void RestoreStamina(float amount)
        {
            if (currentHealth <= 0) return;

            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            Debug.Log($"[PlayerController] Restored {amount} Stamina. Current Stamina: {currentStamina}/{maxStamina}");
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (IsInvincible || currentHealth <= 0) return;

            currentHealth = Mathf.Max(currentHealth - damageInfo.damage, 0f);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            Debug.Log($"[PlayerController] Took {damageInfo.damage} damage from {damageInfo.attacker?.name}. HP: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("[PlayerController] Player died!");
            OnPlayerDied?.Invoke();
        }

        void TryLootNearbyItems()
        {
            // Fallback to all layers if itemLayer is unassigned in Inspector (itemLayer == 0)
            LayerMask mask = itemLayer.value != 0 ? itemLayer : (LayerMask)~0;
            Collider2D[] itemColliders = Physics2D.OverlapCircleAll(transform.position, lootRadius, mask);

            foreach (Collider2D col in itemColliders)
            {
                ICollectible collectible = col.GetComponent<ICollectible>();
                if (collectible != null)
                {
                    collectible.Collect(gameObject);
                }
            }
        }

        void CheckNearbyInteractable()
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, interactRadius);
            IInteractable closestInteractable = null;
            float minDistance = float.MaxValue;

            foreach (Collider2D col in nearbyColliders)
            {
                IInteractable interactable = col.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    float dist = Vector2.Distance(transform.position, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestInteractable = interactable;
                    }
                }
            }

            if (closestInteractable != _currentInteractableTarget)
            {
                _currentInteractableTarget = closestInteractable;
                OnInteractableDetected?.Invoke(_currentInteractableTarget);
            }
        }

        void OnInteract()
        {
            Debug.Log("Interact key pressed.");

            if (_currentInteractableTarget != null)
            {
                _currentInteractableTarget.Interact(gameObject);
            }
            else
            {
                Debug.Log("No interactable object found nearby.");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ICollectible collectible = collision.GetComponent<ICollectible>();
            if (collectible != null)
            {
                collectible.Collect(gameObject);
            }
        }
    }
}
