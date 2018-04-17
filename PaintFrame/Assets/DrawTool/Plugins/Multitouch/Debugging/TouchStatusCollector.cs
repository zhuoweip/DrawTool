/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
using System.Text;
using Multitouch.EventSystems.EventData;
using Multitouch.EventSystems.Gestures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.Debugging
{
    public class TouchStatusCollector : MonoBehaviour
        , IPointerDownHandler
        , IPointerEnterHandler
        , IPointerExitHandler
        , IPointerUpHandler
        , IPointerClickHandler
        , IBeginDragHandler
        , IDragHandler
        , IEndDragHandler
        , IRotateHandler
        , IPanHandler
        , IPinchHandler
        , IInitializePotentialDragHandler
    {
        protected void OnEnable()
        {
            if (_pointerStatus == null)
            {
                _pointerStatus = new StringBuilder();
                _eventDesc = new StringBuilder();
            }
            _pointerStatus.Length = 0;
            _eventDesc.Length = 0;
            //ShowTouchStatus.Instance.RegisterCollector(this);
        }

        protected void OnDisable()
        {
            //ShowTouchStatus.Instance.UnRegisterCollector(this);
        }

        public StringBuilder CollectStatus(StringBuilder sb)
        {
            var elapsed = Time.frameCount - _lastFrameNumber;
            if (elapsed > 100 || _pointerStatus.Length == 0)
                return sb;

            sb.AppendLine().AppendFormat("{0}: {1}", name, _pointerStatus);
            for (var i = elapsed; i > 0; i -= 10)
                sb.Append('.');
            sb.AppendLine().Append(_eventDesc).AppendLine();
            return sb;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            LogPointerEvent("OnPointerDown", eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            LogPointerEvent("OnPointerEnter", eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LogPointerEvent("OnPointerExit", eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            LogPointerEvent("OnPointerUp", eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            LogPointerEvent("OnPointerClick", eventData);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            LogPointerEvent("OnBeginDrag", eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            LogPointerEvent("OnDrag", eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            LogPointerEvent("OnEndDrag", eventData);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            LogPointerEvent("OnInitializePotentialDrag", eventData);
        }
        public void OnRotate(SimpleGestures sender, MultiTouchPointerEventData eventData, float delta)
        {
            LogMultiTouchEvent("OnRotate", eventData, delta);
        }

        public void OnPan(SimpleGestures sender, MultiTouchPointerEventData eventData, Vector2 delta)
        {
            LogMultiTouchEvent("OnPan", eventData, delta);
        }

        public void OnPinch(SimpleGestures sender, MultiTouchPointerEventData eventData, Vector2 pinchDelta)
        {
            LogMultiTouchEvent("OnPinch", eventData, pinchDelta);
        }

        private void LogPointerEvent(string eventName, PointerEventData eventData)
        {
            if (_lastFrameNumber != Time.frameCount)
            {
                _pointerStatus.Length = 0;
                _lastFrameNumber = Time.frameCount;
                _eventDesc.Length = 0;
            }
            _eventDesc.AppendFormat("Pointer ID: {4}, clickable: {5}\nPos: {0} delta {1}\nscrollDelta: {2} dragging: {3}\n",
                eventData.position, eventData.delta, eventData.scrollDelta, eventData.dragging ? "YES" : "no",
                eventData.pointerId, eventData.eligibleForClick ? "YES" : "no");
            _pointerStatus.Append(eventName).Append(", ");
        }

        private void LogMultiTouchEvent<T>(string eventName, MultiTouchPointerEventData eventData, T delta)
        {
            LogPointerEvent(eventName, eventData);
            _eventDesc.AppendFormat("Centroid[pos {0}, delta {1}, pressed pos {2}] event delta {3}\n",
                eventData.centroidPosition, eventData.centroidDelta, eventData.centroidPressPosition, delta);
        }

        private int _lastFrameNumber;
        private StringBuilder _pointerStatus;
        private StringBuilder _eventDesc;
    }
}
