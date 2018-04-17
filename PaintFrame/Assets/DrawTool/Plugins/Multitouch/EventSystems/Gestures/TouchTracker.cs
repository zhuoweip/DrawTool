/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multitouch.EventSystems.Gestures
{
    public class TouchTracker
    {
        public int SpanningTreeCount { get { return _currentTrees.Count; } }

        public SpanningTree GetSpanningTree(int i)
        {
            return _currentTrees[i];
        }

        public void BeginUpdate()
        {
            var temp = _tempTouchList;
            _tempTouchList = _touchList;
            _touchList = temp;

            // clear any non-revivifiable touches.
            for (int i = _lingeringTouchPts.Count - 1; i >= 0; i--)
            {
                if ((Time.time - _lingeringTouchPts[i].timeEnded) > MultiselectEventSystem.current.MaxDoubleClickTime)
                    _lingeringTouchPts.RemoveAt(i);
            }
        }

        public void AddTouch(TouchPt touch)
        {
            TouchPt existingTouch;
            if (GetExistingTouch(touch, out existingTouch))
            {
                existingTouch.Update(touch);
                touch = existingTouch;
            }

            _touchList.Add(touch);
        }

        private bool GetExistingTouch(TouchPt touch, out TouchPt existingTouch)
        {
            int index = FindIndex(_tempTouchList, touch.fingerId);
            if (index != -1)
            {
                existingTouch = _tempTouchList[index];
                _tempTouchList.RemoveAt(index);
                return true;
            }

            index = FindIndex(_lingeringTouchPts, touch.fingerId);
            if (index != -1)
            {
                existingTouch = _lingeringTouchPts[index];
                _lingeringTouchPts.RemoveAt(index);
                return true;
            }

            // Didn't have a match on fingerId. Try to match position up with any
            // lingering touches
            // Start with bestDistance set to the maximum allowable
            float maxDistance = MultiselectEventSystem.current.MaxDoubleClickDistancePixels;
            float bestDistance = maxDistance * maxDistance;
            int bestIndex = -1;

            for (int i = 0; i < _lingeringTouchPts.Count; i++)
            {
                float sqrDistance = (_lingeringTouchPts[i].position - touch.position).sqrMagnitude;
                if (sqrDistance < bestDistance)
                {
                    bestDistance = sqrDistance;
                    bestIndex = i;
                }
            }
            if (bestIndex != -1)
            {
                existingTouch = _lingeringTouchPts[bestIndex];
                _lingeringTouchPts.RemoveAt(bestIndex);
                return true;
            }
            existingTouch = default(TouchPt);
            return false;
        }




        public void AddTouches(IEnumerable<TouchPt> touches)
        {
            foreach (var touch in touches)
                AddTouch(touch);
        }

        public void EndUpdate()
        {
            // Move any leftover touches to the lingering list
            _lingeringTouchPts.AddRange(_tempTouchList);
            _tempTouchList.Clear();

            if (_touchList.Count > 0)
            {
                if (_currentTrees.Count == 0)
                {
                    SpanningTree newTree = SpanningTree.Get();
                    newTree.SetTouchList(_touchList);
                    if (newTree.SeparateClusters(_currentTrees))
                    {
                        newTree.Dispose();
                    }
                }
                else
                {
                    // Step one iterates all current trees, updating all touchpoints being used
                    // in the trees. The absense of a touch that has been tracked indicates it has ended.
                    // The list _tempTrees contains the updated trees.
                    foreach (var tree in _currentTrees)
                    {
                        // UpdateTouchPoints returns zero if the tree has been emptied of all touch points
                        // SeparateClusters adds either tree, or the results of the split trees into _tempTrees
                        // and returns true if the tree was split
                        if (tree.UpdateTouchPoints(_touchList) == 0 || tree.SeparateClusters(_tempTrees))
                            tree.Dispose();
                    }

                    // Swap tree lists, making the result of the update the _currentTrees
                    var temp = _currentTrees;
                    _currentTrees = _tempTrees;
                    _tempTrees = temp;
                    // clear the _tempTrees list
                    // Any trees that are not current have already been disposed
                    _tempTrees.Clear();


                    // Any touches left in _touchList are ones that are new
                    foreach (var touch in _touchList)
                    {
                        // add any new touches into the closest tree
                        // but only if the distance is less than the maximum edge length
                        float bestDistance = MultiselectEventSystem.current.MaxTouchClusterDistancePixels;
                        SpanningTree tree = null;
                        foreach (var spanningTree in _currentTrees)
                        {
                            float treeDistance = spanningTree.ComputeMinimumDistanceToPoint(touch.position);
                            if (treeDistance < bestDistance)
                            {
                                bestDistance = treeDistance;
                                tree = spanningTree;
                            }
                        }
                        if (tree != null)
                        {
                            tree.AddTouch(touch);
                            tree.ProcessTree();
                        }
                        else
                        {
                            // Too far from any tree, create a new one
                            tree = SpanningTree.Get();
                            tree.AddTouch(touch);
                            tree.ProcessTree();
                            _currentTrees.Add(tree);
                        }
                    }
                }
            }
            else
            {
                ReleaseTrees(_currentTrees);
            }

        }

        public void Clear()
        {
            ReleaseTrees(_currentTrees);
            ReleaseTrees(_tempTrees);
            _touchList.Clear();
            _tempTouchList.Clear();
        }

        private void ReleaseTrees(List<SpanningTree> trees)
        {
            foreach (var tree in trees)
            {
                tree.Dispose();
            }
            trees.Clear();
        }

        public int FindIndex(List<TouchPt> list, int fingerId)
        {
            _idToFind = fingerId;
            return list.FindIndex(TouchFindPredicate);
        }
        private Predicate<TouchPt> TouchFindPredicate
        {
            get { return _touchFindPredicate ?? (_touchFindPredicate = IsTouchToFind); }
        }
        private Predicate<TouchPt> _touchFindPredicate;
        private int _idToFind;

        private bool IsTouchToFind(TouchPt t)
        {
            return t.fingerId == _idToFind;
        }


        private List<SpanningTree> _currentTrees = new List<SpanningTree>();
        private List<SpanningTree> _tempTrees = new List<SpanningTree>();
        private List<TouchPt> _touchList = new List<TouchPt>();
        private List<TouchPt> _tempTouchList = new List<TouchPt>();
        private readonly List<TouchPt> _lingeringTouchPts = new List<TouchPt>();
    }
}
