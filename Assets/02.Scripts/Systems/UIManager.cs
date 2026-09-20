using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

namespace Systems
{
    public class UIManager
    {
        int _order = 0;

        // 사전 매핑용 딕셔너리
        private Dictionary<string, Canvas> dict_uiCanvases = new Dictionary<string, Canvas>();
        private Dictionary<string, GameObject> dict_uiPrefabs = new Dictionary<string, GameObject>();

        public void Init()
        {
            // 프리팹 캐싱
            GameObject[] prefabs = Managers.Resource.LoadAll<GameObject>("Prefabs/UI");
            foreach (GameObject prefab in prefabs)
            {
                dict_uiPrefabs[prefab.name] = prefab;
                Debug.Log($"Register Prefab: {prefab.name}");
            }
        }

        public Transform Root
        {
            get
            {
                GameObject root = GameObject.Find("@UI_Root");
                if (root == null)
                {
                    root = new GameObject { name = "@UI_Root" };
                    SetSceneCanvas(root, 0);
                }

                // EventSystem 자동 생성
                EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
                if (eventSystem == null)
                {
                    GameObject esObj = new GameObject("@EventSystem");
                    esObj.AddComponent<EventSystem>();
                    esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                }

                SetSceneCanvas(root, 0);

                return root.transform;
            }
        }

        public T InstantiateUI<T>(Transform parent = null, string name = null) where T : BaseUI
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            if (parent == null)
            {
                parent = dict_uiCanvases.ContainsKey(name) ? dict_uiCanvases[name].transform : Root;
            }
            else if (parent != Root && parent.IsChildOf(Root) == false)
                return null;

            GameObject go;
            if (dict_uiPrefabs.TryGetValue(name, out GameObject prefab))
            {
                go = Managers.Resource.Instantiate(prefab: prefab, parent: parent);
                go.name = name;
            }
            else
            {
                Debug.LogError($"[UIManager] 프리팹 캐시에 {name}이(가) 없습니다. Resources/Prefabs/UI 폴더를 확인하세요.");
                return null;
            }

            T ui = go.GetComponent<T>();
            if (ui == null)
                ui = go.AddComponent<T>();

            return ui;
        }

        public void DestroyUI(BaseUI ui)
        {
            if (ui != null)
            {
                Managers.Resource.Destroy(ui.gameObject);
            }
        }

        public T GetUIByName<T>(string name) where T : BaseUI
        {
            T ui = Util.FindChild<T>(Root.gameObject, name, true);
            return ui;
        }

        public void SetSceneCanvas(GameObject go, int order)
        {
            Canvas canvas = go.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = go.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = order;

            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1620, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GraphicRaycaster raycaster = go.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
                go.AddComponent<GraphicRaycaster>();
        }
    }

}
