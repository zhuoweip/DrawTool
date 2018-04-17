/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using Multitouch.EventSystems.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable InconsistentNaming because this mirrors naming of UnityEngine.Touch

namespace Multitouch.EventSystems.Gestures
{
    /// <summary>
    /// Unity's Touch structure is read-only and cannot be constructed
    /// Since we need to be able to fake Touches with the mouse, we need to be able
    /// to create them. Thus this analog to Touch.
    /// 
    /// Should Unity create a version of Touch that can be created from user code,
    /// This struct can go away.
    /// </summary>
    public struct TouchPt
    {
        public const int InvalidFingerId = -100;
        // Public interface same as UnityEngine.Touch
        // Summary:
        //     The position delta since last change.
        public Vector2 deltaPosition { get; set; }
        //
        // Summary:
        //     Amount of time that has passed since the last recorded change in Touch values.
        public float deltaTime { get; set; }
        //
        // Summary:
        //     The unique index for the touch.
        public int fingerId { get; set; }
        //
        // Summary:
        //     Describes the phase of the touch.
        public TouchPhase phase
        {
            get { return _phase; }
            set
            {
                if (_phase != TouchPhase.Canceled && value == TouchPhase.Ended)
                {
                    timeEnded = Time.time;
                }
                else if (value == TouchPhase.Began)
                {
                    timeEnded = float.NaN;
                }
                _phase = value;

            }
        }

        //
        // Summary:
        //     The position of the touch in pixel coordinates.
        public Vector2 position { get; set; }
        public Vector2 rawPosition { get; set; }
        //
        // Summary:
        //     Number of taps.
        public int tapCount { get; set; }

        /// <summary>
        /// Time at which the touch ended.
        /// A double tap could revive this touch if it occurs within the doubletap time
        /// </summary>
        public float timeEnded { get; set; }

        public float timeBegan { get; set; }

        /// <summary>
        /// Update the current TouchPt based on the current mouse information
        /// </summary>
        /// <param name="maxDoubleClickDistance"></param>
        /// <param name="maxDoubleClickTime"></param>
        /// <returns>true if the current TouchPt cannot be changed to reflect the current position
        /// and therefore the client should ask for a new TouchPt</returns>
        public bool UpdatePointFromMouse(float maxDoubleClickDistance, float maxDoubleClickTime)
        {
            bool updatePosition = true;
            bool mouseButtonDown = Input.GetMouseButton((int) PointerEventData.InputButton.Left);
            Vector2 delta = Input.mousePosition.XY() - position;

            deltaTime = Time.deltaTime;
            if (Input.GetMouseButtonDown((int) PointerEventData.InputButton.Left))
            {
                bool isNewPoint = deltaPosition.magnitude > maxDoubleClickDistance ||
                                  float.IsNaN(timeEnded) ||
                                  Time.time - timeEnded > maxDoubleClickTime;
                // new point button down in wrong phase means this one cancelled
                if (isNewPoint)
                {
                    if (phase != TouchPhase.Ended)
                    {
                        phase = TouchPhase.Canceled;
                    }
                    updatePosition = false;
                }
                else
                {
                    phase = TouchPhase.Began;
                    tapCount++;
                }
            }
            else if (Input.GetMouseButtonUp((int) PointerEventData.InputButton.Left))
            {
                phase = TouchPhase.Ended;
            }
            else if (mouseButtonDown)
            {
                if (phase == TouchPhase.Canceled || phase == TouchPhase.Ended)
                {
                    // Button is down, but did not go down this frame
                    // Dont reuse this TouchPt
                    phase = TouchPhase.Canceled;
                    updatePosition = false;
                }
                else
                {
                    phase = delta != Vector2.zero ? TouchPhase.Moved : TouchPhase.Stationary;
                }
            }
            else if (phase != TouchPhase.Ended)
            {
                // Button is up, but we never got the up status.
                phase = TouchPhase.Canceled;
                updatePosition = false;
            }
            else
            {
                updatePosition = false;
            }

            if (updatePosition)
            {
                rawPosition = position = Input.mousePosition;
                deltaPosition = delta;
            }

            bool queryNewPoint = !updatePosition && mouseButtonDown;
            return queryNewPoint;
        }

        public override string ToString()
        {
            return string.Format("Touch[{0}] pos {1} delta {2} dTime {3}\n  phase {4} tap {5} raw {6} sTime {7} eTime {8}", 
                fingerId, position, deltaPosition, deltaTime, phase, tapCount, rawPosition, timeBegan, timeEnded);
        }

        public static implicit operator TouchPt(Touch touch)
        {
            return new TouchPt
            {
                deltaPosition = touch.deltaPosition,
                deltaTime = touch.deltaTime,
                fingerId = touch.fingerId,
                phase = touch.phase,
                position = touch.position,
                rawPosition = touch.rawPosition,
                tapCount = touch.tapCount,
                timeBegan = Time.time,
                timeEnded = float.NaN
            };
        }

        public void Update(TouchPt touch)
        {
            if (fingerId != touch.fingerId)
                throw new InvalidOperationException(string.Format("Cannot update touch ID {0} from ID {1}", fingerId, touch.fingerId));

            // Update all information except timeBegan and timeEnded
            deltaPosition = touch.deltaPosition;
            deltaTime = touch.deltaTime;
            phase = touch.phase;
            position = touch.position;
            rawPosition = touch.rawPosition;
            tapCount = touch.tapCount;
        }

        public static TouchPt GetMouseAsTouchPt(int fingerId)
        {
            return new TouchPt
            {
                position = Input.mousePosition,
                deltaPosition = Vector2.zero,
                deltaTime = 0,
                fingerId = fingerId,
                phase = TouchPhase.Began,
                rawPosition = Input.mousePosition,
                timeBegan = Time.time,
                timeEnded = float.NaN
            };
        }

        private TouchPhase _phase;
    }
}
