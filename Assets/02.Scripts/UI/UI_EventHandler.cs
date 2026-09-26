using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UI_EventHandler : MonoBehaviour,
        IPointerClickHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        public event Action<PointerEventData> OnClickHandler;
        public event Action<PointerEventData> OnDragHandler;
        public event Action<PointerEventData> OnEnterHandler;
        public event Action<PointerEventData> OnExitHandler;
        public event Action<PointerEventData> OnDownHandler;
        public event Action<PointerEventData> OnUpHandler;

        public void OnPointerClick(PointerEventData e) => OnClickHandler?.Invoke(e);
        public void OnPointerDown(PointerEventData e)  => OnDownHandler?.Invoke(e);
        public void OnPointerUp(PointerEventData e)    => OnUpHandler?.Invoke(e);
        public void OnPointerEnter(PointerEventData e) => OnEnterHandler?.Invoke(e);
        public void OnPointerExit(PointerEventData e)  => OnExitHandler?.Invoke(e);
        public void OnDrag(PointerEventData e)         => OnDragHandler?.Invoke(e);
    }
}