/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems
{

    public abstract class EventWrapper
    {
        public abstract string Name { get; }
        public abstract GameObject ExecuteHierarchy(GameObject target, BaseEventData eventData);
        public abstract bool Execute(GameObject target, BaseEventData eventData);
    }

    /// <summary>
    /// Generic class which creates a wrapper for an event
    /// 
    /// Usage Example from SimpleGesture.cs:
    /// void Start() {
    ///     // Initialize and provide the delegate needed for calling the appropriate method on the event handler handler
    ///     RotateEvent = new EventWrapper<IRotateHandler, MultiTouchPointerEventData>((h, e) => h.OnRotate(this, e, DeltaRotate));
    /// }
    /// 
    /// // Declaration
    /// protected EventWrapper<IRotateHandler, MultiTouchPointerEventData> RotateEvent;
    /// 
    /// // Invocation:
    /// ExecuteEvents.ExecuteHierarchy(target, eventData, RotateEvent);
    /// --or--
    /// RotateEvent.ExecuteHierarchy(target, eventData);
    /// 
    /// </summary>
    /// <typeparam name="TEvent">The Event class type</typeparam>
    /// <typeparam name="TData">The type of data for the event</typeparam>
    public class EventWrapper<TEvent, TData> : EventWrapper
        where TEvent : IEventSystemHandler 
        where TData : BaseEventData
    {
        public delegate void EventDelegate(TEvent handler, TData eventData);

        /// <summary>
        /// Implicit cast to the appropriate EventFunction type. This allows the wrapper
        /// to be used as the argument to any ExecuteEvents method
        /// </summary>
        /// <param name="wrapper"></param>
        public static implicit operator ExecuteEvents.EventFunction<TEvent>(EventWrapper<TEvent, TData> wrapper)
        {
            return wrapper._handlerDelegate;
        }

        /// <summary>
        /// Initializes the wrapper with an instance of the delegate (either method or lambda expression)
        /// to be used when invoking the event method on the handler
        /// </summary>
        /// <param name="invoker">invoker delegate</param>
        public EventWrapper(EventDelegate invoker)
        {
            _invoker = invoker;
            _handlerDelegate = Execute;
        }

        public override string Name { get { return typeof (TEvent).Name; } }

        /// <summary>
        /// Helper which invokes the event via ExecuteEvents.Execute
        /// </summary>
        /// <param name="target"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public override bool Execute(GameObject target, BaseEventData eventData)
        {
            return ExecuteEvents.Execute(target, eventData, _handlerDelegate);
        }
        /// <summary>
        /// Helper which invokes the event via ExecuteEvents.ExecuteHierarchy
        /// </summary>
        /// <param name="target"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public override GameObject ExecuteHierarchy(GameObject target, BaseEventData eventData)
        {
            return ExecuteEvents.ExecuteHierarchy(target, eventData, _handlerDelegate);
        }

        private readonly ExecuteEvents.EventFunction<TEvent> _handlerDelegate;
        private readonly EventDelegate _invoker;

        private void Execute(TEvent handler, BaseEventData eventData)
        {
            _invoker(handler, ExecuteEvents.ValidateEventData<TData>(eventData));
        }
    }
}
