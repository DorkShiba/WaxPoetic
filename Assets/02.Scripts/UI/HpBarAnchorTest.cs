using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class HpBarAnchorTest : MonoBehaviour
{
    [SerializeField] private float shrinkSpeed = 1f; // 초당 anchorMax.x 감소량 (배율 조절용)

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        // 초기 앵커 설정: x min = 0, x max = 1
        Vector2 anchorMin = rt.anchorMin;
        Vector2 anchorMax = rt.anchorMax;
        anchorMin.x = 0f;
        anchorMax.x = 1f;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;

        // LTRB 오프셋 전부 0으로 고정
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        Vector2 anchorMax = rt.anchorMax;

        anchorMax.x -= shrinkSpeed * Time.deltaTime;

        // 0 밑으로 내려가면 1을 더해서 반복 (1 -> 0 -> 1 -> 0 ...)
        if (anchorMax.x < 0f)
        {
            anchorMax.x += 1f;
        }

        rt.anchorMax = anchorMax;

        // 앵커가 바뀌어도 LTRB 오프셋은 항상 (0,0,0,0) 유지
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}