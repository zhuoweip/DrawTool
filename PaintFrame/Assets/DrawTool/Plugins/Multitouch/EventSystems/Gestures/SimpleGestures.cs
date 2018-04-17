/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using Multitouch.EventSystems.EventData;
using Multitouch.EventSystems.InputModules;
using Multitouch.EventSystems.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems.Gestures
{
    /// <summary>
    /// Event for multitouch pan gesture
    /// </summary>
    public interface IPanHandler : IMultiTouchEventSystemHandler
    {
        void OnPan(SimpleGestures sender, MultiTouchPointerEventData eventData, Vector2 delta);
    }

    /// <summary>
    /// Event for pinch gesture
    /// </summary>
    public interface IPinchHandler : IMultiTouchEventSystemHandler
    {
        void OnPinch(SimpleGestures sender, MultiTouchPointerEventData eventData, Vector2 pinchDelta);
    }

    /// <summary>
    /// Event for Rotation gesture
    /// </summary>
    public interface IRotateHandler : IMultiTouchEventSystemHandler
    {
        void OnRotate(SimpleGestures sender, MultiTouchPointerEventData eventData, float delta);
    }

    /// <summary>
    /// Gesture detection module with simple implementations of Pan, Pinch, and Rotation
    /// </summary>
    [AddComponentMenu("Event/Simple Gestures")]
    [RequireComponent(typeof(MultiselectEventSystem))]
    public class SimpleGestures : MonoBehaviour, IMultiTouchGestureModule
    {
        public MultiTouchProcessResult ResultIfTouchCountMatches = MultiTouchProcessResult.NonExclusiveBlockNormalEvents;
        public MultiTouchProcessResult ResultIfEventSent = MultiTouchProcessResult.Exclusive;

        [Tooltip("Order in which Gesture Modules will be processed (in ascending order).")]
        public int ModulePriority = 0;

        public bool DetectPan = true;
        public bool DetectPinch = true;
        public bool DetectRotate = true;
        public int MinimumFingers = 2;
        public int MaximumFingers = 5;

        public float PinchTollerance = 0.2f;
        public float PinchLimiter = 0.1f;
        public float DragTollerance = 0.05f;
        public float RotateTollerance = 0.3f;
        public float CloseToCenterRatio = 0.4f;

        [HideInInspector]
        public int NumInward;
        [HideInInspector]
        public int NumOutward;

        // ReSharper disable once InconsistentNaming
        [HideInInspector]
        public int NumOffAxisCW;

        // ReSharper disable once InconsistentNaming
        [HideInInspector]
        public int NumOffAxisCCW;
        [HideInInspector]
        public int NumMatchCentroid;
        [HideInInspector]
        public int NumAntiCentroid;
        [HideInInspector]
        public int NumNearCenter;
        [HideInInspector]
        public Vector2 DeltaPinch;
        [HideInInspector]
        public float DeltaRotate;

        public bool HasPriorData { get { return GestureData != null && GestureData.PriorCluster.Count != 0; } }

        protected class SimpleGestureData : IDisposable
        {
            public static SimpleGestureData Get()
            {
                return Pool.Get();
            }

            public TouchCluster PriorCluster = new TouchCluster();
            public void Dispose()
            {
                Pool.Release(this);
            }

            private static readonly Pool<SimpleGestureData> Pool = new Pool<SimpleGestureData>(null, d => d.PriorCluster.Clear());
        }

        public MultiTouchProcessResult Process(MultiTouchPointerEventData eventData, Win10TouchInputModule module)
        {
            GestureData = GetGestureData(eventData);

            int touchCount = eventData.touchCluster.Count;
            if (touchCount < MinimumFingers || touchCount > MaximumFingers)
            {
                Reset(eventData);
                return MultiTouchProcessResult.NotProcessed;         // Wrong touch count, don't check
            }

            if (touchCount != GestureData.PriorCluster.Count)
                Reset(eventData);

            bool anyEvent = false;
            if (HasPriorData)
            {
                NormalizedPriorCluster.CopyFrom(GestureData.PriorCluster);
                NormalizedPriorCluster.NormalizeForCentroidDelta(eventData.touchCluster);
                EvaluateRelativeMotion(eventData);
                anyEvent = CheckPan(eventData) || CheckRotate(eventData) || CheckPinch(eventData);
            }

            if (!HasPriorData || anyEvent)
            {
                GestureData.PriorCluster.CopyFrom(eventData.touchCluster);
            }
            return anyEvent ? ResultIfEventSent : ResultIfTouchCountMatches;
        }

        protected SimpleGestureData GetGestureData(MultiTouchPointerEventData eventData)
        {
            var simpleGestureData = (SimpleGestureData) eventData.GetGestureData(this);
            if (simpleGestureData == null)
            {
                simpleGestureData = SimpleGestureData.Get();
                eventData.SetGestureData(this, simpleGestureData);
            }
            return simpleGestureData;
        }

        public void Reset(MultiTouchPointerEventData eventData)
        {
            if (IsGestureStarted)
            {
                SendEvent<IGestureEndHandler>(gameObject, eventData, GestureEndEvent, false);
            }
            GestureData.PriorCluster.Clear();
            IsGestureStarted = false;
        }

        private bool SendEvent<TU>(GameObject target, MultiTouchPointerEventData eventData, ExecuteEvents.EventFunction<TU> handlerDelegate, bool sendBeginGestureIfNeeded)
            where TU : IEventSystemHandler
        {
            eventData.EventSender = this;
            if (!IsGestureStarted && sendBeginGestureIfNeeded)
            {
                ExecuteEvents.ExecuteHierarchy(target, eventData, (ExecuteEvents.EventFunction<IGestureStartHandler>)GestureStartEvent);
                IsGestureStarted = true;
            }
            return ExecuteEvents.ExecuteHierarchy(target, eventData, handlerDelegate) != null;
        }

        private void ClearPointerPressOrDrag(MultiTouchPointerEventData eventData)
        {
            if (!eventData.singleTouchProcessingEnabled)
                return;

            Win10TouchInputModule module;
            if (Win10TouchInputModule.TryGetCurrentWin10TouchInputModule(out module))
                module.CancelSingleTouchProcessing(eventData);
        }

        private bool CheckPan(MultiTouchPointerEventData eventData)
        {
            // In a pan, all of the touch points are moving in the same direction as the centroid,
            // and none are moving significantly counter to it.
            if (DetectPan && NumAntiCentroid == 0 && NumMatchCentroid >= eventData.touchCluster.Count)
            {
                if (ShouldStartDrag(eventData.centroidPressPosition, eventData.centroidPosition,
                    EventSystem.current.pixelDragThreshold, eventData.useDragThreshold))
                {
                    ClearPointerPressOrDrag(eventData);
                    return SendEvent<IPanHandler>(eventData.multitouchTarget, eventData, PanEvent, true);
                }
            }
            return false;
        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        /// <summary>
        /// Check for a pinch. This would be all normalized touches moving more-or-less toward
        /// or away from the centroid.
        /// </summary>
        /// <returns>true if event fired</returns>
        private bool CheckPinch(MultiTouchPointerEventData eventData)
        {
            if (!DetectPinch)
                return false;

            int countNeeded = eventData.touchCluster.Count - NumNearCenter;
            if (countNeeded > 1 && (NumInward >= countNeeded || NumOutward >= countNeeded))
            {
                ClearPointerPressOrDrag(eventData);
                return SendEvent<IPinchHandler>(eventData.multitouchTarget, eventData, PinchEvent, true);
            }
            return false;
        }

        /// <summary>
        /// Check for rotate. This would be all normalized touches moving the same clock direction
        /// </summary>
        /// <returns></returns>
        private bool CheckRotate(MultiTouchPointerEventData eventData)
        {
            if (!DetectRotate)
                return false;

            int countNeeded = eventData.touchCluster.Count - NumNearCenter;
            if (countNeeded > 1 && (NumOffAxisCCW >= countNeeded || NumOffAxisCW >= countNeeded))
            {
                ClearPointerPressOrDrag(eventData);
                return SendEvent<IRotateHandler>(eventData.multitouchTarget, eventData, RotateEvent, true);
            }
            return false;
        }

        protected void EvaluateRelativeMotion(MultiTouchPointerEventData eventData)
        {
            NumInward = 0;
            NumOutward = 0;
            NumOffAxisCW = 0;
            NumOffAxisCCW = 0;
            NumMatchCentroid = 0;
            NumAntiCentroid = 0;

            NumNearCenter = 0;
            DeltaPinch = Vector2.zero;
            DeltaRotate = 0;

            float clusterRadius = eventData.touchCluster.Radius;

            Vector2 centroidDelta = eventData.centroidPosition - GestureData.PriorCluster.Centroid;
            foreach (var entry in eventData.touchCluster.Touches)
            {
                TouchPt currentTouch = entry.Value;
                TouchPt normalizedPriorTouch;
                if (NormalizedPriorCluster.Touches.TryGetValue(entry.Key, out normalizedPriorTouch))
                {
                    float dot;
                    Vector2 rawMove = currentTouch.position - GestureData.PriorCluster.Touches[currentTouch.fingerId].position;
                    if (rawMove.sqrMagnitude > 0.5f)
                    {
                        dot = Vector2.Dot(centroidDelta.normalized, rawMove.normalized);
                        if (Mathf.Abs(dot) >= (1.0f - DragTollerance))
                            NumMatchCentroid++;
                        else
                        {
                            NumAntiCentroid++;
                        }
                        
                    }
                    Vector2 fromCentroid = eventData.touchCluster.GetRelativeTouchVector(entry.Key);
                    if (!Mathf.Approximately(clusterRadius, 0f))
                    {
                        if (fromCentroid.magnitude / clusterRadius < CloseToCenterRatio)
                            NumNearCenter++;
                    }

                    Vector2 delta = currentTouch.position - normalizedPriorTouch.position;
                    float deltaMagnitude = delta.magnitude;
                    dot = Vector2.Dot(fromCentroid.normalized, delta.normalized);
                    float absDot = Mathf.Abs(dot);

                    if (1.0f - absDot < PinchTollerance)
                    {
                        if (dot > 0)
                        {
                            NumOutward++;
                            DeltaPinch += delta.Abs();
                        }
                        else
                        {
                            NumInward++;
                            DeltaPinch -= delta.Abs();
                        }
                    }
                    else if (absDot < RotateTollerance)
                    {
                        float dotPerp = Vector2.Dot(fromCentroid.Perp(), delta);
                        if (dotPerp < 0)
                        {
                            NumOffAxisCW++;
                            DeltaRotate += deltaMagnitude / fromCentroid.magnitude;
                        }
                        else
                        {
                            NumOffAxisCCW++;
                            DeltaRotate -= deltaMagnitude / fromCentroid.magnitude;
                        }
                    }
                }
            }
            if (NumOffAxisCCW > 0 || NumOffAxisCW > 0)
            {
                DeltaRotate = Mathf.Atan2(DeltaRotate, 1f) * Mathf.Rad2Deg;
                if (DeltaRotate > 180f)
                    DeltaRotate -= 360f;
            }
        }

        protected virtual void Start()
        {
            RotateEvent = new EventWrapper<IRotateHandler, MultiTouchPointerEventData>((h, e) => h.OnRotate(this, e, DeltaRotate));
            PanEvent = new EventWrapper<IPanHandler, MultiTouchPointerEventData>((h, e) => h.OnPan(this, e, e.centroidDelta));
            PinchEvent = new EventWrapper<IPinchHandler, MultiTouchPointerEventData>((h, e) => h.OnPinch(this, e, DeltaPinch));
            GestureStartEvent = new EventWrapper<IGestureStartHandler, MultiTouchPointerEventData>((h, e) => h.OnGestureStart(e));
            GestureEndEvent = new EventWrapper<IGestureEndHandler, MultiTouchPointerEventData>((h, e) => h.OnGestureEnded(e));
        }

        protected Win10TouchInputModule InputModule;
        protected SimpleGestureData GestureData;

        protected TouchCluster NormalizedPriorCluster = new TouchCluster();

        protected EventWrapper<IPanHandler, MultiTouchPointerEventData> PanEvent;
        protected EventWrapper<IPinchHandler, MultiTouchPointerEventData> PinchEvent;
        protected EventWrapper<IRotateHandler, MultiTouchPointerEventData> RotateEvent;
        protected EventWrapper<IGestureStartHandler, MultiTouchPointerEventData> GestureStartEvent;
        protected EventWrapper<IGestureEndHandler, MultiTouchPointerEventData> GestureEndEvent;


        protected bool IsGestureStarted;


        public int Priority { get { return ModulePriority; } }
    }
}
