/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multitouch.EventSystems.Gestures
{
    /// <summary>
    /// This class represents a cluster of touches.
    /// Clusters are the base input element used in the MultitouchEventSystem
    /// such that the related touches are processed as a single gesture.
    /// 
    /// TouchClusters are identified by use of the SpanningTree class.
    /// </summary>
    public class TouchCluster : ICollection<TouchPt>
    {
        public Dictionary<int, TouchPt> Touches = new Dictionary<int, TouchPt>();

        public int ActiveTouchId { get; private set; }
        public int ClusterId { get; set; }

        public Vector2 Centroid { get; private set; }

        public float Radius
        {
            get
            {
                // return the maximum distance from centroid to point
                float radius2 = 0.0f;
                foreach (var touch in Touches.Values)
                {
                    float r2 = (touch.position - Centroid).sqrMagnitude;
                    if (r2 > radius2)
                        radius2 = r2;
                }
                return Mathf.Sqrt(radius2);
            }
        }

        public void CopyFrom(TouchCluster other)
        {
            Clear();
            foreach (var entry in other.Touches)
            {
                Touches.Add(entry.Key, entry.Value);
            }
            ActiveTouchId = other.ActiveTouchId;
            Centroid = other.Centroid;
            ClusterId = other.ClusterId;
        }

        public Vector2 GetRelativeTouchVector(int touchId)
        {
            TouchPt touch;
            if (Touches.TryGetValue(touchId, out touch))
            {
                return touch.position - Centroid;
            }
            return Vector2.zero;
        }

        /// <summary>
        /// This method updates the positions of the TouchPts in this TouchCluster
        /// to account for translation of the entire cluster by comparing the centroid positions.
        /// This makes analysis of the relative touch positions (rotation, pinch) easier by removing
        /// the common translation. It also helps identify a pinch or rotation where a single touch
        /// is moving, or where the touches are moving by different amounts.
        /// </summary>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        public bool NormalizeForCentroidDelta(TouchCluster newPosition)
        {
            // Update positions of touches by the amount necessary to align the Centroid to the new Centroid position
            Vector2 delta = newPosition.Centroid - Centroid;
            if (delta == Vector2.zero)
                return false;

            foreach (int key in Touches.Keys.ToList())
            {
                var touch = Touches[key];
                touch.position += delta;
                Touches[key] = touch;
            }
            return true;
        }

        public bool AnyTouchMoving(out Vector2 maxMovement)
        {
            bool anyMoving = false;
            maxMovement = Vector2.zero;
            foreach (var entry in Touches)
            {
                anyMoving |= entry.Value.deltaPosition != Vector2.zero;
                if (entry.Value.deltaPosition.sqrMagnitude > maxMovement.sqrMagnitude)
                    maxMovement = entry.Value.deltaPosition;
            }
            return anyMoving;
        }
        public bool SetActiveTouch(TouchPt touch)
        {
            ActiveTouchId = Touches.ContainsKey(touch.fingerId) ? touch.fingerId : TouchPt.InvalidFingerId;
            return ActiveTouchId != TouchPt.InvalidFingerId;
        }

        public IEnumerator<TouchPt> GetEnumerator()
        {
            return Touches.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TouchPt touch)
        {
            // First remove the old touch if it's present. Remove is a no-op if not present
            // and calling it is more efficient than checking for existence first.
            Remove(touch);
            AddToCentroid(touch.position, Touches.Count);
            Touches.Add(touch.fingerId, touch);
        }

        public bool Remove(TouchPt touch)
        {
            if (!Touches.Remove(touch.fingerId))
                return false;

            RemoveFromCentroid(touch.position, Touches.Count);
            return true;
        }

        private void AddToCentroid(Vector2 newPos, int countBeforeAdd)
        {
            Vector2 scaledCentroid = Centroid * countBeforeAdd;
            scaledCentroid += newPos;
            Centroid = scaledCentroid / (countBeforeAdd + 1);
        }

        private void RemoveFromCentroid(Vector2 oldPos, int countAfterRemoval)
        {
            if (countAfterRemoval == 0)
            {
                Centroid = Vector2.zero;
                return;
            }
            Vector2 scaledCentroid = Centroid * (countAfterRemoval + 1);
            scaledCentroid -= oldPos;
            Centroid = scaledCentroid / countAfterRemoval;
        }

        public void Clear()
        {
            Centroid = Vector2.zero;
            Touches.Clear();
            ActiveTouchId = TouchPt.InvalidFingerId;
        }

        public bool Contains(TouchPt item)
        {
            return Touches.ContainsKey(item.fingerId);
        }

        public void CopyTo(TouchPt[] array, int arrayIndex)
        {
            Touches.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Touches.Count; }
        }

        public bool IsReadOnly { get { return false; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in Touches)
            {
                if (entry.Value.fingerId == ActiveTouchId)
                    sb.Append('*');
                sb.AppendLine(entry.Value.ToString());
            }
            return sb.ToString();
        }
    }
}
