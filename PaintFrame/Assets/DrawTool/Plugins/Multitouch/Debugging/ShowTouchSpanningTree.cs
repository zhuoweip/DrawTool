/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
using System.Collections.Generic;
using JetBrains.Annotations;
using Multitouch.EventSystems;
using Multitouch.EventSystems.Gestures;
using Multitouch.EventSystems.InputModules;
using Multitouch.EventSystems.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Multitouch.Debugging
{
    public class ShowTouchSpanningTree : MonoBehaviour
    {
        public LineRenderer RendererPrefab;
        public Image PointMarkerPrefab;
        public Color TouchActive;
        public Color TouchCancelled;
        public Color TouchEnded;
        public Canvas TouchCanvas;
        public MouseTouches MouseTouchTracker;

        [UsedImplicitly]
        void Start()
        {
        }

        [UsedImplicitly]
        void Update()
        {

            GatherTouches();
            ProcessInput();
        }

        private void GatherTouches()
        {
            _touchList.Clear();
            _pointList.Clear();

            // Get input from local sources
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                    AddTouchToListIfActive(Input.GetTouch(i));
            }
            else if (MouseTouchTracker != null)
            {
                for (int i = 0; i < MouseTouchTracker.Touches.Count; i++)
                    AddTouchToListIfActive(MouseTouchTracker.Touches[i]);
            }
        }

        private void AddTouchToListIfActive(TouchPt touch)
        {
            if (touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
                _touchList.Add(touch);
            _pointList.Add(touch);
        }
        private void ProcessInput()
        {
            if (RendererPrefab != null)
            {
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
                            if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                                continue; // don't add old touches

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
                                _currentTrees.Add(tree);
                            }
                        }


                    }

                    EnsureRenderers(_currentTrees.Count);
                    for (int i = 0; i < _currentTrees.Count; i++)
                    {
                        DisplayTreeInRenderer(_renderers[i], _currentTrees[i]);
                        _renderers[i].gameObject.SetActive(true);
                    }
                }
                else
                {
                    ReleaseTrees(_currentTrees);
                }

                for (int i = _currentTrees.Count; i < _renderers.Count; i++)
                {
                    _renderers[i].gameObject.SetActive(false);
                }
            }

            EnsurePointMarkers(_pointList.Count);
            RectTransform rcTransform = (RectTransform) transform;
            Vector2 offset = rcTransform.localPosition.XY();
            for (int i = 0; i < _pointList.Count; i++)
            {
                TouchPt touch = _pointList[i];
                Vector2 pos;

                if (TouchCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    pos = touch.position - offset;
                }
                else
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, touch.position,
                        TouchCanvas.worldCamera, out pos);
                }

                _pointMarkers[i].transform.localPosition = pos.AsVector3();
                _pointMarkers[i].color = touch.phase == TouchPhase.Ended
                    ? TouchEnded
                    : touch.phase == TouchPhase.Canceled ? TouchCancelled : TouchActive;
            }
        }

        private void EnsureRenderers(int numberNeeded)
        {
            for (int i = _renderers.Count; i < numberNeeded; i++)
            {
                LineRenderer newRenderer = Instantiate(RendererPrefab);
                newRenderer.transform.SetParent(transform, false);
                _renderers.Add(newRenderer);
            }
        }

        private void EnsurePointMarkers(int numberNeeded)
        {
            // Create more if needed
            for (int i = _pointMarkers.Count; i < numberNeeded; i++)
            {
                var marker = Instantiate(PointMarkerPrefab);
                marker.transform.SetParent(transform, false);
                _pointMarkers.Add(marker);
            }

            // Enable the correct number
            for (int i = 0; i < _pointMarkers.Count; i++)
            {
                _pointMarkers[i].gameObject.SetActive(i < numberNeeded);
            }
        }
        
        private void DisplayTreeInRenderer(LineRenderer lineRenderer, SpanningTree tree)
        {
            _lines.Clear();
            ComputeDisplayEdges(tree.Root, tree);
            lineRenderer.positionCount = _lines.Count;
            for (int i = 0; i < _lines.Count; i++)
                lineRenderer.SetPosition(i, _lines[i]);
            lineRenderer.enabled = true;
        }

        private void ReleaseTrees(List<SpanningTree> trees)
        {
            foreach (var tree in trees)
            {
                tree.Dispose();
            }
            trees.Clear();
        }

        private void ComputeDisplayEdges(SpanningTree.Node current, SpanningTree tree)
        {
            if (current == null)
            {
                return;
            }
            TouchPt touch = tree.GetTouchPoint(current);
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) transform, touch.position,
                Camera.main, out pos);

            _lines.Add(pos);

            foreach (var neighbor in current.Neighbors)
            {
                ComputeDisplayEdges(neighbor, tree);
                _lines.Add(pos);
            }

        }

        private readonly List<Vector3> _lines = new List<Vector3>(); 
        private readonly List<LineRenderer> _renderers = new List<LineRenderer>(); 
        private readonly List<Image> _pointMarkers = new List<Image>(); 
        private List<SpanningTree> _currentTrees = new List<SpanningTree>();
        private List<SpanningTree> _tempTrees = new List<SpanningTree>();
        private readonly List<TouchPt> _touchList = new List<TouchPt>();
        private readonly List<TouchPt> _pointList = new List<TouchPt>();

    }
}
