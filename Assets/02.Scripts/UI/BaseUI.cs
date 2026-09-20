using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class BaseUI : MonoBehaviour {
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
    public abstract void Init();

    public virtual void Open() { }
    public virtual void Close() { }

    protected void Bind<T>(Type type) where T : UnityEngine.Object {
        string[] names = Enum.GetNames(type);
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++) {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected T Get<T>(int idx) where T : UnityEngine.Object {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    public void setAnchoredPosition(GameObject go, Vector2 anchoredPosition) {
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        if (rectTransform != null) {
            rectTransform.anchoredPosition = anchoredPosition;
        }
    }

    public static void BindEvent(GameObject go, Action<PointerEventData> clickAction = null, Action<PointerEventData> dragAction = null)
    {
        UI_EventHandler evt = go.GetComponent<UI_EventHandler>();
        if (evt == null)
            evt = go.AddComponent<UI_EventHandler>();

        if (clickAction != null)
        {
            evt.OnClickHandler -= clickAction;
            evt.OnClickHandler += clickAction;
        }

        if (dragAction != null)
        {
            evt.OnDragHandler -= dragAction;
            evt.OnDragHandler += dragAction;
        }
    }
}
