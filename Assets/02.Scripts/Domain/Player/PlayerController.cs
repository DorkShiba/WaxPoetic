using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static Define;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;  // 플레이어 데이터 참조
    [SerializeField] private Vector2 moveDirection;
    private const float interactRadius = 3f;      // 상호작용 감지 범위

    private const float lootRadius = 1.5f;        // 루팅 감지 범위
    [SerializeField] private LayerMask npcLayer;  // inspector에서 NPC로 지정
    [SerializeField] private LayerMask itemLayer; // inspecotr에서 item으로 지정
    [SerializeField] private Animator animator;   // 애니메이터 참조

    private DashController _dashController;
    private Rigidbody2D _rb;
    // Hook this up to your damage system later —
    // when true, incoming attacks should be ignored
    public bool IsInvincible => _dashController != null && _dashController.IsDashing;
    void Awake()
    {
        _dashController = GetComponent<DashController>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Managers.Input.OnInteractPerformed -= OnInteract; // 상호작용 이벤트
        Managers.Input.OnInteractPerformed += OnInteract;
    }

    private void OnDestroy()
    {
        Managers.Input.OnInteractPerformed -= OnInteract; // 상호작용 이벤트 해제
    }

    // Update is called once per frame
    void Update()
    {
        // Block movement input while dashing (DashController handles position during dash)
        if (_dashController != null && _dashController.IsDashing) return;

        moveDirection = Managers.Input.MoveDirection;
        animator.SetBool("isMove", moveDirection != Vector2.zero); // 이동 여부에 따라 애니메이션 전환
        flip();

        playerData.CurrPosition = transform.position;  // 현재 위치 업데이트 (맵 상의 좌표)

        TryLootNearbyItems();
    }

    void FixedUpdate()
    {
        if (_dashController != null && _dashController.IsDashing) return;

        // 물리 연산을 통한 이동 (떨림 현상 방지)
        _rb.MovePosition(_rb.position + moveDirection * Time.fixedDeltaTime * playerData.Spd);
    }

    void flip() {
        if (moveDirection.x > 0) {
            transform.localScale = new Vector3(1, 1, 1);
        } else if (moveDirection.x < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void TryLootNearbyItems()
    {
        // Detect all items within loot radius
        Collider2D[] itemColliders = Physics2D.OverlapCircleAll(transform.position, lootRadius, itemLayer);

        foreach (Collider2D col in itemColliders)
        {
            ItemObject coin = col.GetComponent<ItemObject>();
            if (coin != null)
            {
                Managers.Inventory.AddItem(coin.ItemData, coin.Amount);
                coin.Collect();  // destroys the GameObject
            }
            // Future item types: add more GetComponent checks here
        }
    }

    void OnInteract()
    {
        Debug.Log("interact detected.");

        Collider2D npc = Physics2D.OverlapCircle(transform.position, interactRadius, npcLayer);
        if (npc == null)
        {
            Debug.Log("NPC not detected.");
            return;
        }
        else {
            Debug.Log("NPC detected: " + npc.gameObject.name);
        }
    }
}
