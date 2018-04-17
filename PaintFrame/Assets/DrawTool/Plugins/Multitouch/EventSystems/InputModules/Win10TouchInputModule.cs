/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Multitouch.EventSystems.EventData;
using Multitouch.EventSystems.Gestures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems.InputModules
{
    public enum MultiTouchProcessResult
    {
        NotProcessed,
        Exclusive,
        NonExclusiveBlockNormalEvents,
        NonExclusive
    }
    public interface IMultiTouchGestureModule
    {
        int Priority { get; }
        MultiTouchProcessResult Process(MultiTouchPointerEventData eventData, Win10TouchInputModule module);
    }

    /// <summary>
    /// Derive from this interface for Multitouch Events
    /// </summary>
    public interface IMultiTouchEventSystemHandler : IEventSystemHandler
    {
    }

    public interface IGestureStartHandler : IMultiTouchEventSystemHandler
    {
        void OnGestureStart(MultiTouchPointerEventData eventData);
    }

    public interface IGestureEndHandler : IMultiTouchEventSystemHandler
    {
        void OnGestureEnded(MultiTouchPointerEventData eventData);
    }

    /// <summary>
    /// Multitouch handling for Windows 10 devices with support for many touch points. 
    /// (some Perceptive Pixel (PPI) devices support up to 100 touch points)
    /// 
    /// Handles clusters of touch points as distinct entities for gesture recognition
    /// and incorporates Gesture Recognition modules for extensibility.
    /// </summary>
    [AddComponentMenu("Event/Win 10 Touch Input Module")]
    [RequireComponent(typeof(MultiselectEventSystem))]
    public class Win10TouchInputModule : StandaloneInputModule
    {
        [Tooltip("If True, always process multitouch gestures even when there is no multitouch listener, which makes for more consistent behavior.")]
        public bool AlwaysProcessGestures = true;
        [Tooltip("Record touch debugging information in DebugString")]
        public bool RecordDebugInfo;

        public TouchTracker TouchTracker { get { return _touchTracker; } }
        /// <summary>
        /// Get the EventSystem.current.currentInputModule as a Win10TouchInputModule
        /// </summary>
        /// <param name="module">Output parameter receives the module</param>
        /// <returns>true if current input module is a Win10TouchInputModule</returns>
        public static bool TryGetCurrentWin10TouchInputModule(out Win10TouchInputModule module)
        {
            module = EventSystem.current.currentInputModule as Win10TouchInputModule;
            return module != null;
        }

        /// <summary>
        /// Identifies a touch cluster which contains a given touch ID
        /// </summary>
        /// <param name="touchId"></param>
        /// <param name="cluster"></param>
        /// <returns></returns>
        public bool GetTouchClusterForTouchId(int touchId, TouchCluster cluster)
        {
            for (int i = 0; i < _touchTracker.SpanningTreeCount; i++)
            {
                SpanningTree tree = _touchTracker.GetSpanningTree(i);
                if (tree.ContainsTouchId(touchId))
                {
                    return tree.GetTouchCluster(cluster);
                }
            }
            cluster.Clear();
            return false;
        }

        /// <summary>
        /// Returns a string which is constructed during touch processing
        /// to help understand the dynamics of the touches and clusters
        /// </summary>
        public string DebugString
        {
            get { return _debugBuffer.ToString(); }
        }

        protected override void Awake()
        {
            base.Awake();
            _mouseTouchTracker = GetComponent<MouseTouches>();
        }

        /// <summary>
        /// any number of IMultiTouchGestureModule components may be attached to this GameObject
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _multiselectEventSystem = EventSystem.current as MultiselectEventSystem;
            _multitouchModules = GetComponents<IMultiTouchGestureModule>();
            Array.Sort(_multitouchModules, (l, r) => l.Priority - r.Priority);
        }

        public override bool IsModuleSupported()
        {
            return forceModuleActive;
        }

        public override bool ShouldActivateModule()
        {

            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began
                    || touch.phase == TouchPhase.Moved
                    || touch.phase == TouchPhase.Stationary)
                    return true;
            }

            // Fake touch input check
            return _mouseTouchTracker != null && _mouseTouchTracker.ShouldActivate();
        }
 
        public override void Process()
        {
            _debugBuffer.Length = 0;
            bool fakeTouchesProcessed = false;
            if (_lastUpdateUsedFakeTouches)
            {
                // Treat Mouse Touches as primary if they were active last update
                _lastUpdateUsedFakeTouches = HandleMouseTouches(false);
                fakeTouchesProcessed = true;
                if (_lastUpdateUsedFakeTouches)
                    return;        // Still active, so done.

                // No longer active, clear the tracker
                _touchTracker.Clear();
            }

            // Try the real touch inputs
            if (HandleRealTouches())
                return;            // There were some, ignore mouse

            if (fakeTouchesProcessed)
                return;           // already tried processing mouse, and nothing there

            // Check to see if there's new mouse touches.
            _lastUpdateUsedFakeTouches = HandleMouseTouches(true);
        }

        private bool HandleRealTouches()
        {
            if (Input.touchCount == 0)
                return false;

            _touchTracker.BeginUpdate();

            for (int i = 0; i < Input.touchCount; ++i)
                _touchTracker.AddTouch(Input.GetTouch(i));
            _touchTracker.EndUpdate();
            if (RecordDebugInfo) _debugBuffer.Append("Touch Order: ");
            ProcessClusters();
            return true;
        }

        private bool HandleMouseTouches(bool isNewMouseTracking)
        {
            if (_mouseTouchTracker == null || !_mouseTouchTracker.enabled)
                return false;

            _mouseTouchTracker.Process();
            if (_mouseTouchTracker.Touches.Count == 0)
                return false;

            if (isNewMouseTracking)
                _touchTracker.Clear();

            _touchTracker.BeginUpdate();
            _touchTracker.AddTouches(_mouseTouchTracker.Touches);
            _touchTracker.EndUpdate();

            if (_mouseTouchTracker.CurrentTouchIndex != -1)
            {
                int currentTouchId = _mouseTouchTracker.Touches[_mouseTouchTracker.CurrentTouchIndex].fingerId;
                for (int i = 0; i < _touchTracker.SpanningTreeCount; i++)
                {
                    var tree = _touchTracker.GetSpanningTree(i);
                    if (tree.SetTouchAsActiveIfPresent(currentTouchId))
                        break;
                }
            }
            if (RecordDebugInfo) _debugBuffer.Append("Mouse Order: ");
            ProcessClusters();
            return true;
        }

        private void ProcessClusters()
        {
            if (_touchTracker.SpanningTreeCount > 0)
            {
                UpdateTouchOrder();
                for (int i = 0; i < _touchOrder.Count; i++)
                {
                    SpanningTree tree = _touchTracker.GetSpanningTree(_touchOrder[i].TreeIndex);
                    tree.GetTouchCluster(_cluster);
                    try
                    {
                        var touch = _cluster.Touches[_cluster.ActiveTouchId];
                        bool processed = ProcessTouch(touch, _cluster);
                        if (RecordDebugInfo)
                        {
                            _debugBuffer.AppendFormat("{0}[{1}]{2}", _cluster.ClusterId, _cluster.ActiveTouchId,
                                processed ? "* " : "- ");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat(this, "Exception processing cluster. {0} touches, activeTouchId = {1}\n{2}", 
                            _cluster.Touches.Count, _cluster.ActiveTouchId, e.ToString());
                    }

                }
                _usedTargets.Clear();
            }
        }

        private void UpdateTouchOrder()
        {
            if (_touchTracker.SpanningTreeCount == 0)
            {
                _touchOrder.Clear();
                return;
            }

            // mark all trackers as unused
            for (int i = 0; i < _touchOrder.Count; i++)
                SetTouchOrderTree(i, -1);

            // find the trackers for each cluster and set the correct tree index
            for (int i = 0; i < _touchTracker.SpanningTreeCount; i++)
            {
                SpanningTree tree = _touchTracker.GetSpanningTree(i);
                int index = TouchOrderFindIndex(tree.GetActiveTouchId());
                if (index != -1)
                {
                    // Set current tree index for this touch id
                    SetTouchOrderTree(index, i);
                }
                else
                {
                    // Add new tree to end of order
                    _touchOrder.Add(new TouchOrder
                    {
                        FingerId = tree.GetActiveTouchId(),
                        TreeIndex = i
                    });
                }
            }

            // Remove any trackers that no longer have trees associated.
            for (int i = _touchOrder.Count - 1; i >= 0; i--)
            {
                if (_touchOrder[i].TreeIndex == -1)
                    _touchOrder.RemoveAt(i);
            }
        }

        private void SetTouchOrderTree(int index, int tree)
        {
            var order = _touchOrder[index];
            order.TreeIndex = tree;
            _touchOrder[index] = order;
        }

        private int TouchOrderFindIndex(int fingerId)
        {
            for (int i = 0; i < _touchOrder.Count; i++)
                if (_touchOrder[i].FingerId == fingerId)
                    return i;
            return -1;
        }

        private bool ProcessTouch(TouchPt touch, TouchCluster cluster)
        {
            bool released;
            bool pressed;
            var pointer = GetTouchPointerEventData(touch, cluster, out pressed, out released);

            if (pointer.rawPointerPress != null && _usedTargets.Contains(pointer.rawPointerPress))
                return false;       // Don't send to same object another touch has been sent to

            int oldContext = SetSelectedObjectContext(cluster.ClusterId, false);

            bool processed = false;
            bool allowNormalEvents = true;
            if (ProcessMultiTouchPress(pointer, pressed, released) || AlwaysProcessGestures)
            {
                // Only do multitouch gesture processing if there is a target found for them
                foreach (IMultiTouchGestureModule module in _multitouchModules)
                {
                    MultiTouchProcessResult result = module.Process(pointer, this);
                    processed |= result != MultiTouchProcessResult.NotProcessed;

                    if (result == MultiTouchProcessResult.Exclusive)
                    {
                        allowNormalEvents = false;
                        break;
                    }
                    if (result == MultiTouchProcessResult.NonExclusiveBlockNormalEvents)
                    {
                        allowNormalEvents = false;
                    }
                }
            }

            if (pointer.singleTouchProcessingEnabled)
            {
                if (allowNormalEvents)
                {
                    ProcessTouchPress(pointer, pressed, released);
                    // May have sent the following events on Pressed:
                    // for these comments, "go" is pointer.pointerCurrentRaycast.gameObject
                    // IPointerEnter/Exit (all up and down the hierarchy, all "entered" objects held in pointer.hovered
                    //   pointer.pointerEnter = go
                    // IPointerDownHandler (to first handler)
                    //   pointer.pointerPress = handler game object OR IPointerClickHandler game object
                    // pointer.rawPointerPress = go
                    // IInitializePotentialDragHandler if IDragHandler is found
                    //   pointer.pointerDrag = Drag Handler

                    if (!released)
                    {
                        ProcessMove(pointer);
                        ProcessDrag(pointer);
                        processed = true;
                    }
                }
                else if (!pressed)
                {
                    CancelSingleTouchProcessing(pointer);
                }
            }

            if (processed)
                _usedTargets.Add(pointer.rawPointerPress);

            if (released)
            {
                RemovePointerData(pointer);
                if (_multiselectEventSystem != null)
                    _multiselectEventSystem.RemoveSelectedObjectContext(touch.fingerId);
            }

            SetSelectedObjectContext(oldContext, !processed);
            return processed;
        }

        public void CancelSingleTouchProcessing(MultiTouchPointerEventData pointer)
        {
            if (!pointer.singleTouchProcessingEnabled)
                return;

            pointer.singleTouchProcessingEnabled = false;

            // Exit any hovered states
            HandlePointerExitAndEnter(pointer, null);

            pointer.eligibleForClick = false;   // Not elegible for click

            if (pointer.pointerPress)
            {
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                ExecuteEvents.Execute<IPointerUpHandler>(pointer.pointerPress, pointer, ExecuteEvents.pointerUpHandler);
                pointer.pointerPress = null;
                pointer.rawPointerPress = null;
            }
            if (pointer.pointerDrag != null && pointer.dragging)
            {
                // ReSharper disable once RedundantTypeArgumentsOfMethod
                ExecuteEvents.Execute<IEndDragHandler>(pointer.pointerDrag, pointer, ExecuteEvents.endDragHandler);
                pointer.pointerDrag = null;
                pointer.dragging = false;
            }
        }

        protected override void ProcessDrag(PointerEventData pointerEvent)
        {
            var pointer = (MultiTouchPointerEventData) pointerEvent;
            Vector2 maxMotion;
            bool moving = pointer.IsPointerMoving() || pointer.touchCluster.AnyTouchMoving(out maxMotion);

            if (moving && pointerEvent.pointerDrag != null
                && !pointerEvent.dragging
                && (ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold)
                || ShouldStartDrag(pointer.centroidPressPosition, pointer.centroidPosition, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold)))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null)
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }


        private int SetSelectedObjectContext(int contextId, bool removeContext)
        {
            int oldContext = MultiselectEventSystem.InvalidContext;
            if (_multiselectEventSystem != null)
            {
                oldContext = _multiselectEventSystem.CurrentSelectedObjectContext;
                _multiselectEventSystem.SetSelectedObjectContext(contextId);
                if (removeContext)
                    _multiselectEventSystem.RemoveSelectedObjectContext(oldContext);
            }
            return oldContext;
        }

        protected bool GetMultiTouchPointerData(int id, out MultiTouchPointerEventData data, bool create)
        {
            PointerEventData temp;
            if (m_PointerData.TryGetValue(id, out temp))
            {
                data = temp as MultiTouchPointerEventData;
                if (data != null)
                    return false; // good value, pre-existing
            }

            if (!create)
            {
                data = null;
                return false;
            }

            data = new MultiTouchPointerEventData(eventSystem)
            {
                pointerId = id,
            };
            m_PointerData[id] = data;
            return true;
        }

        // New implementation of PointerInputModule.GetTouchPointerEventData()
        // which takes a TouchPt instead of Touch
        protected MultiTouchPointerEventData GetTouchPointerEventData(TouchPt touch, TouchCluster cluster, out bool pressed, out bool released)
        {
            MultiTouchPointerEventData pointerData;
            var created = GetMultiTouchPointerData(cluster.ClusterId, out pointerData, true);

            pointerData.Reset();

            pressed = created || (touch.phase == TouchPhase.Began);
            released = (touch.phase == TouchPhase.Canceled) || (touch.phase == TouchPhase.Ended);

            if (created)
            {
                // position information needs to be set first when
                // EventData has just been created
                pointerData.position = touch.position;
                pointerData.centroidPosition = cluster.Centroid;
                pointerData.touchCluster.CopyFrom(cluster);
            }
            if (pressed)
            {
                pointerData.delta = Vector2.zero;
                pointerData.centroidPressPosition = cluster.Centroid;
                pointerData.centroidDelta = Vector2.zero;
            }
            else
            {
                pointerData.delta = touch.position - pointerData.position;
                pointerData.centroidDelta = cluster.Centroid - pointerData.centroidPosition;
            }

            if (!created)
            {
                // position information needs to be set here after deltas
                // are computed when not first created. (time optimization since cluster copy can take time)
                pointerData.position = touch.position;
                pointerData.centroidPosition = cluster.Centroid;
                pointerData.touchCluster.CopyFrom(cluster);
            }

            pointerData.button = PointerEventData.InputButton.Left;

            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

            RaycastResult raycast;
            while (true)
            {
                raycast = FindFirstRaycast(m_RaycastResultCache);
                var raycastProxy = raycast.gameObject != null ? raycast.gameObject.GetComponent<RaycasterProxy>() : null;
                if (raycastProxy == null || !raycastProxy.IsProxyActive)
                    break;      // found a result that is not an active proxy
                
                // The proxy is the primary item. Therefore, discard the results to this point and raycast within the
                // proxied camera
                m_RaycastResultCache.Clear();
                //raycastProxy.RcTransform.SetAsLastSibling();
                raycastProxy.ProxyRaycaster.Raycast(pointerData, m_RaycastResultCache);
            }

            pointerData.pointerCurrentRaycast = raycast;

            m_RaycastResultCache.Clear();
            return pointerData;
        }

        private bool ProcessMultiTouchPress(MultiTouchPointerEventData pointerEvent, bool pressed, bool released)
        {
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (pressed)
            {
                pointerEvent.singleTouchProcessingEnabled = true;
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                // search for the control that will receive the multitouch events
                pointerEvent.multitouchTarget = ExecuteEvents.GetEventHandler<IMultiTouchEventSystemHandler>(currentOverGo);

                pointerEvent.rawPointerPress = currentOverGo;
            }

            // PointerUp notification
            if (released)
            {
                ExecuteEvents.Execute(pointerEvent.multitouchTarget, pointerEvent, EndGestureHandlerDelegate);
                pointerEvent.multitouchTarget = null;
            }
            return pointerEvent.multitouchTarget != null;
        }

        protected ExecuteEvents.EventFunction<IGestureEndHandler> EndGestureHandlerDelegate
        {
            get { return _endGesturEventFunction ?? (_endGesturEventFunction = Execute); }
        }
        private ExecuteEvents.EventFunction<IGestureEndHandler> _endGesturEventFunction;
        private void Execute(IGestureEndHandler handler, BaseEventData eventData)
        {
            handler.OnGestureEnded(ExecuteEvents.ValidateEventData<MultiTouchPointerEventData>(eventData));
        }



        private bool _lastUpdateUsedFakeTouches;
        private MouseTouches _mouseTouchTracker;
        private IMultiTouchGestureModule[] _multitouchModules;

        private readonly TouchTracker _touchTracker = new TouchTracker();
        private readonly TouchCluster _cluster = new TouchCluster();
        private MultiselectEventSystem _multiselectEventSystem;
        private readonly List<GameObject> _usedTargets = new List<GameObject>();

        private struct TouchOrder
        {
            public int FingerId;
            public int TreeIndex;
        }

        private readonly List<TouchOrder> _touchOrder = new List<TouchOrder>();

        private readonly StringBuilder _debugBuffer = new StringBuilder();
    }
}
