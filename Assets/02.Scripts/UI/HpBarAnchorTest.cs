using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class HpBarAnchorTest : MonoBehaviour
    {
        [SerializeField] private float shrinkSpeed = 1f;

        private RectTransform rt;

        private void Awake()
        {
            rt = GetComponent<RectTransform>();

            Vector2 anchorMin = rt.anchorMin;
            Vector2 anchorMax = rt.anchorMax;
            anchorMin.x = 0f;
            anchorMax.x = 1f;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            Vector2 anchorMax = rt.anchorMax;

            anchorMax.x -= shrinkSpeed * Time.deltaTime;

            if (anchorMax.x < 0f)
            {
                anchorMax.x += 1f;
            }

            rt.anchorMax = anchorMax;

            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}