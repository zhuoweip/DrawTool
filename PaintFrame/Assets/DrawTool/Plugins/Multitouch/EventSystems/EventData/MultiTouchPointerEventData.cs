/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Multitouch.EventSystems.Gestures;
using Multitouch.EventSystems.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable InconsistentNaming

namespace Multitouch.EventSystems.EventData
{
    public class MultiTouchPointerEventData : PointerEventData, IDisposable
    {
        public static MultiTouchPointerEventData Get()
        {
            return _pool.Get();
        }

        public MultiTouchPointerEventData() : this(EventSystem.current)
        {

        }

        public MultiTouchPointerEventData(EventSystem eventSystem) : base(eventSystem)
        {
            
        }

        public Vector2 centroidPosition { get; set; }
        public Vector2 centroidDelta { get; set; }
        public Vector2 centroidPressPosition { get; set; }

        public bool singleTouchProcessingEnabled { get; set; }

        public TouchCluster touchCluster
        {
            get
            {
                return _touchCluster;
            }
        }

        public SimpleGestures EventSender { get; set; }

        public GameObject multitouchTarget { get; set; }

        public void SetGestureData(object key, IDisposable data)
        {
            _gestureData[key] = data;
        }

        public IDisposable GetGestureData(object key)
        {
            IDisposable data;
            _gestureData.TryGetValue(key, out data);
            return data;
        }

        public void Clear()
        {
            eligibleForClick = false;
            singleTouchProcessingEnabled = false;

            pointerId = -1;
            position = Vector2.zero; // Current position of the mouse or touch event
            delta = Vector2.zero; // Delta since last update
            pressPosition = Vector2.zero; // Delta since the event started being tracked
            clickTime = 0.0f; // The last time a click event was sent out (used for double-clicks)
            clickCount = 0; // Number of clicks in a row. 2 for a double-click for example.

            scrollDelta = Vector2.zero;
            useDragThreshold = true;
            dragging = false;
            button = InputButton.Left;

            _touchCluster.Clear();

            // Deep disposal of gesture data
            foreach (var disposable in _gestureData)
            {
                disposable.Value.Dispose();
            }
            _gestureData.Clear();
        }
        public void Dispose()
        {
            _pool.Release(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Position</b>: " + position);
            sb.AppendLine("<b>delta</b>: " + delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
            sb.AppendLine("<b>Centroid Position</b>: " + centroidPosition);
            sb.AppendLine("<b>Centroid Delta</b>: " + centroidDelta);
            sb.AppendLine("<b>Centroid Press Position</b>: " + centroidPressPosition);
            sb.AppendLine("<b>Cluster</b>:\n" + touchCluster);
            return sb.ToString();
        }

        public static bool CopyEventData(PointerEventData src, out PointerEventData dest)
        {
            var multitouchSrc = src as MultiTouchPointerEventData;
            if (multitouchSrc != null)
            {
                var multitouchDest = Get();
                dest = multitouchDest;
                CopyMultiTouchEventData(multitouchSrc, multitouchDest);
                return true;
            }

            dest = new PointerEventData(EventSystem.current);
            CopyPointerEventData(src, dest);
            return false;
        }
        public static void CopyPointerEventData(PointerEventData src, PointerEventData dest)
        {
            if (src.used)
                dest.Use();
            else
                dest.Reset();

            dest.pointerEnter = src.pointerEnter;
            dest.rawPointerPress = src.rawPointerPress;
            dest.pointerDrag = src.pointerDrag;
            dest.pointerCurrentRaycast = src.pointerCurrentRaycast;
            dest.pointerPressRaycast = src.pointerPressRaycast;
            dest.eligibleForClick = src.eligibleForClick;
            dest.pointerId = src.pointerId;
            dest.position = src.position;
            dest.delta = src.delta;
            dest.pressPosition = src.pressPosition;
            dest.clickTime = src.clickTime;
            dest.clickCount = src.clickCount;
            dest.scrollDelta = src.scrollDelta;
            dest.useDragThreshold = src.useDragThreshold;
            dest.dragging = src.dragging;
            dest.button = src.button;
            // Cycle lastPress through pointerPress
            dest.pointerPress = src.lastPress;
            dest.pointerPress = src.pointerPress;
        }

        public static void CopyMultiTouchEventData(MultiTouchPointerEventData src, MultiTouchPointerEventData dest)
        {
            CopyPointerEventData(src, dest);
            dest.centroidPosition = src.centroidPosition;
            dest.centroidDelta = src.centroidDelta;
            dest.centroidPressPosition = src.centroidPressPosition;
            dest.touchCluster.CopyFrom(src.touchCluster);
            dest.EventSender = src.EventSender;
            dest.multitouchTarget = src.multitouchTarget;
        }


        private readonly TouchCluster _touchCluster = new TouchCluster();
        private readonly Dictionary<object, IDisposable> _gestureData = new Dictionary<object, IDisposable>();

        private static readonly Pool<MultiTouchPointerEventData> _pool = new Pool<MultiTouchPointerEventData>(null, d => d.Clear());
    }
}
