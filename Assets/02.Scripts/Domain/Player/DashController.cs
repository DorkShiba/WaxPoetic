using System.Collections;
using UnityEngine;
using GameData;
using Systems;

namespace Domain.Player
{
    /// <summary>
    /// Handles dodge/avoid dash with 2 charges and a 2s cooldown after both are spent.
    /// Attach to the Player root alongside PlayerController.
    /// </summary>
    public class DashController : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Animator animator;

        private const float DashDuration = 0.3f;
        private const float DashSpeed = 12f;
        private const int MaxCharges = 2;
        private const float ChargeCooldown = 2f;

        private int _charges = MaxCharges;
        private bool _isDashing = false;
        private bool _isCoolingDown = false;

        public bool IsDashing => _isDashing;

        void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        void Start()
        {
            Managers.Input.OnAvoidPerformed -= OnAvoid;
            Managers.Input.OnAvoidPerformed += OnAvoid;
        }

        void OnDestroy()
        {
            Managers.Input.OnAvoidPerformed -= OnAvoid;
        }

        private void OnAvoid()
        {
            if (_isDashing) return;       // already dashing
            if (_charges <= 0) return;    // no charges left

            Debug.Log("Dash activated. Charges left: " + _charges);

            Vector2 moveInput = Managers.Input.MoveDirection;

            Vector2 dashDir;
            if (moveInput != Vector2.zero)
                dashDir = moveInput.normalized;
            else
                dashDir = transform.localScale.x > 0 ? Vector2.left : Vector2.right;

            StartCoroutine(DashRoutine(dashDir));
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            _isDashing = true;
            _charges--;

            animator.SetTrigger("Avoid");

            if (_charges <= 0 && !_isCoolingDown)
                StartCoroutine(ChargeRefillRoutine());

            float elapsed = 0f;
            while (elapsed < DashDuration)
            {
                transform.Translate(direction * DashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
        }

        private IEnumerator ChargeRefillRoutine()
        {
            _isCoolingDown = true;
            Debug.Log("Dash charges depleted. Starting cooldown.");
            yield return new WaitForSeconds(ChargeCooldown);
            _charges = MaxCharges;
            _isCoolingDown = false;
            Debug.Log("Dash charges refilled.");
        }
    }
}
