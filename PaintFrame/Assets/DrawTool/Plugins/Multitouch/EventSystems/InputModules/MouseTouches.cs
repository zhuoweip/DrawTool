/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System.Collections.Generic;
using Multitouch.EventSystems.Gestures;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems.InputModules
{
    /// <summary>
    /// Class which simulates multitouch using mouse clicks. Holding the ALT key preserves
    /// a touch point when the button is released.
    /// </summary>
    [AddComponentMenu("Event/Mouse Touches")]
    public class MouseTouches : MonoBehaviour
    {
        public List<TouchPt> Touches = new List<TouchPt>();
        public int CurrentTouchIndex;

        public enum EmulateRetainedTouchSet
        {
            AltKeys,
            ControlKeys,
            ShiftKeys
        }

        public EmulateRetainedTouchSet RetainedTouchKeys = EmulateRetainedTouchSet.AltKeys;

        [Tooltip("True if used as a standalone Component. False if used with Win10TouchInputModule which will call Process() during UI input module processing.")]
        public bool ProcessOnOwnUpdate;

        public void Start()
        {
            CurrentTouchIndex = -1;
            _nextTouchId = 1;
        }

        public void Update()
        {
            if (ProcessOnOwnUpdate)
                Process();
        }
        public void Process()
        {
            if (RetainedTouchKeyPressed())
            {
                // Alt key held, accumulate mouse down actions
                HandleRetainedInput(true);
            }
            else
            {
                HandleSingleTouchInput();
            }
        }

        private bool RetainedTouchKeyPressed()
        {
            switch (RetainedTouchKeys)
            {
                case EmulateRetainedTouchSet.AltKeys:
                    return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                case EmulateRetainedTouchSet.ControlKeys:
                    return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                case EmulateRetainedTouchSet.ShiftKeys:
                    return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
            return false;
        }

        public bool ShouldActivate()
        {
            // We should be considered active if the left mouse button is currently pressed
            // or if there are touches active and the alt key is pressed.
            bool anyTouchesActive = false;
            foreach (var touch in Touches)
            {
                if (touch.phase == TouchPhase.Began
                    || touch.phase == TouchPhase.Moved
                    || touch.phase == TouchPhase.Stationary)
                {
                    anyTouchesActive = true;
                    break;
                }
            }
            return Input.GetMouseButton((int) PointerEventData.InputButton.Left) ||
                   (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) && anyTouchesActive);
        }

        public bool HasActiveTouches()
        {
            foreach (var touch in Touches)
            {
                if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
                    return true;
            }
            return false;
        }
        private void HandleRetainedInput(bool preserveEndedTouches)
        {
            bool isNewPoint = true;
            if (CurrentTouchIndex >= 0)
            {
                var eventSystem = MultiselectEventSystem.current;
                TouchPt currentTouch = Touches[CurrentTouchIndex];
                isNewPoint = currentTouch.UpdatePointFromMouse(eventSystem.MaxDoubleClickDistancePixels, eventSystem.MaxDoubleClickTime);
                if (preserveEndedTouches && 
                    (currentTouch.phase == TouchPhase.Ended || currentTouch.phase == TouchPhase.Canceled))
                {
                    // in retained mode, keep the completed point around as though it is still pressed
                    currentTouch.phase = TouchPhase.Stationary;
                    currentTouch.deltaPosition = Vector2.zero;
                }
                Touches[CurrentTouchIndex] = currentTouch;
            }
            if (isNewPoint)
            {
                // Add new point
                if (Input.GetMouseButton((int) PointerEventData.InputButton.Left))
                {
                    Touches.Add(TouchPt.GetMouseAsTouchPt(_nextTouchId++));
                    CurrentTouchIndex = Touches.Count - 1;
                }
                else
                {
                    CurrentTouchIndex = -1;
                }
            }
        }

        private void HandleSingleTouchInput()
        {
            bool isCurrent = CurrentTouchIndex >= 0;
            for (int i = Touches.Count - 1; i >= 0; i--)
            {
                TouchPt touch = Touches[i];
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // touch ended last update, remove it from the list now
                    Touches.RemoveAt(i);
                }
                else if (!isCurrent)
                {
                    // Set as canceled so it will be removed next time
                    touch.phase = TouchPhase.Canceled;
                    Touches[i] = touch;
                }
                isCurrent = false;
            }
            CurrentTouchIndex = Touches.Count - 1;
            HandleRetainedInput(false);
        }

        private int _nextTouchId;
    }
}
