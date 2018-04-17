/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using System.Collections.Generic;
using Multitouch.EventSystems.Utils;
using UnityEngine;

namespace Multitouch.EventSystems.Gestures
{

    /// <summary>
    /// this class identifies a spanning tree, linking touch points to their
    /// closest neighbors.
    /// </summary>
    public class SpanningTree : IDisposable
    {
        public const int InvalidClusterId = -1;
        public class Node
        {
            public int TouchIndex;
            public readonly List<Node> Neighbors = new List<Node>(); 
            public void Clear()
            {
                Neighbors.Clear();
            }
        }

        public static SpanningTree Get()
        {
            return SpanningTreePool.Get();
        }

        public int ActiveTouchIndex { get; private set; }

        public int ClusterId
        {
            get { return _clusterId != InvalidClusterId ?  _clusterId : (_clusterId = GetNextClusterId()); }
            private set { _clusterId = value; }
        }

        public Node Root;

        public bool GetTouchCluster(TouchCluster cluster)
        {
            cluster.Clear();

            if (Root == null)
                return false;

            CollectTouchesInTree(Root, cluster);
            cluster.SetActiveTouch(_touchPoints[ActiveTouchIndex]);
            cluster.ClusterId = ClusterId;
            return true;
        }

        public bool ContainsTouchId(int touchId)
        {
            return FindTouchIndex(_touchPoints, touchId) != -1;
        }

        public int GetActiveTouchId()
        {
            if (ActiveTouchIndex == -1)
                return -1;

            return _touchPoints[ActiveTouchIndex].fingerId;
        }

        public void Init()
        {
            ActiveTouchIndex = -1;
            _clusterId = InvalidClusterId;
        }

        public void Clear()
        {
            _touchPoints.Clear();
            ClearTree(Root);
            Root = null;
            ActiveTouchIndex = -1;
            _clusterId = InvalidClusterId;
        }

        public void AddTouch(TouchPt touch)
        {
            _touchPoints.Add(touch);
        }

        public void SetTouchList(List<TouchPt> touches)
        {
            Clear();
            _touchPoints.AddRange(touches);
            ProcessTree();
        }

        public void ProcessTree()
        {
            if (_touchPoints.Count == 0)
            {
                Debug.LogError("Trying to process SpanningTree with zero touchpoints");
            }
            ComputeEdgeDistances();
            ComputeSpanningTree();
            if (Root == null)
            {
                Debug.LogError("Spanning Tree with NULL Root");
            }
            if (ActiveTouchIndex == -1)
            {
                ActiveTouchIndex = FindEarliestTouchIndex(_touchPoints);
            }
        }

        public bool SetTouchAsActiveIfPresent(int touchId)
        {
            int index = FindTouchIndex(_touchPoints, touchId);
            if (index == -1)
                return false;

            ActiveTouchIndex = index;
            return true;
        }

        /// <summary>
        /// This method identifies clusters of points which are separated from others
        /// by a configurable distance.
        /// 
        /// Once separate clusters have been identified, they maintain their coherence even
        /// if the clusters move to overlap each other.
        /// </summary>
        /// <param name="resultList"></param>
        /// <returns></returns>
        public bool SeparateClusters(List<SpanningTree> resultList)
        {
            float maxClusteringEdgeLength = MultiselectEventSystem.current.MaxTouchClusterDistancePixels;
            float sqrMaxClusteringEdgeLength = maxClusteringEdgeLength * maxClusteringEdgeLength;

            // Need to be able to associate active touch with correct split cluster
            int currentClusterActiveTouchId = GetActiveTouchId();

            _splitList.Clear();
            _splitList.Add(Root);

            SplitClusters(Root, sqrMaxClusteringEdgeLength);

            if (_splitList.Count == 1)
            {
                resultList.Add(this);
                return false;
            }

            // Clusters were split off. Create the result list.
            foreach (var node in _splitList)
            {
                _tempTouchPoints.Clear();
                CollectTouchesInTree(node, _tempTouchPoints);
                var newTree = SpanningTreePool.Get();
                newTree.SetTouchList(_tempTouchPoints);
                // migrate the ClusterID and ActiveTouchPoint if applicable
                newTree.ClusterId = newTree.ContainsTouchId(currentClusterActiveTouchId) ? ClusterId : GetNextClusterId();
                newTree.SetTouchAsActiveIfPresent(currentClusterActiveTouchId);
                resultList.Add(newTree);
            }

            return true;
        }

        public int FindEarliestTouchIndex(List<TouchPt> touches)
        {
            int index = -1;
            float earliestTouchTime = float.MaxValue;
            for (int i = 0; i < _touchPoints.Count; i++)
            {
                if (_touchPoints[i].timeBegan < earliestTouchTime)
                {
                    earliestTouchTime = _touchPoints[i].timeBegan;
                    index = i;
                }
            }
            return index;
        }
        public void Dispose()
        {
            SpanningTreePool.Release(this);
        }

        private void SplitClusters(Node current, float sqrMaxLength)
        {
            for (int i = current.Neighbors.Count - 1; i >= 0; i--)
            {
                Node node = current.Neighbors[i];
                SplitClusters(node, sqrMaxLength);
                if (_edges[current.TouchIndex, node.TouchIndex] > sqrMaxLength)
                {
                    // add the next node as a new root
                    _splitList.Add(node);
                    // Remove as a neighbor to break the link
                    current.Neighbors.RemoveAt(i);
                }
            }
        }

        private void CollectTouchesInTree(Node node, ICollection<TouchPt> list)
        {
            list.Add(_touchPoints[node.TouchIndex]);
            foreach (var n in node.Neighbors)
            {
                CollectTouchesInTree(n, list);
            }
        }
        public TouchPt GetTouchPoint(Node node)
        {
            return _touchPoints[node.TouchIndex];
        }

        /// <summary>
        /// Identify those touch points that are currently in use in SpanningTree.
        /// Remove any touches currently in tree that are not in the list.
        /// Remove touches from the list that are currenlty in the SpanningTree.
        /// 
        /// After processing the list on all current trees, the trees will only contain current
        /// touchpoints, updated with the current values and the list will only contain new touches 
        /// not in any tree.
        /// </summary>
        /// <param name="currentTouches"></param>
        /// <returns>number of points in resultant tree (and thus removed from currentTouches</returns>
        public int UpdateTouchPoints(List<TouchPt> currentTouches)
        {
            for (int i = _touchPoints.Count - 1; i >= 0 ; i--)
            {
                int index = FindTouchIndex(currentTouches, _touchPoints[i].fingerId);
                if (index != -1)
                {
                    // Copy the new data into the tree
                    _touchPoints[i] = currentTouches[index];
                    currentTouches.RemoveAt(index); // consume it from current touches
                }
                else
                {
                    _touchPoints.RemoveAt(i); // Touch is dead, remove it
                    if (ActiveTouchIndex == i)
                        ActiveTouchIndex = -1;      // And touch was the active touch
                }
            }
            if (_touchPoints.Count > 0)
                ProcessTree();
            return _touchPoints.Count;
        }

        public int FindTouchIndex(List<TouchPt> list, int fingerId)
        {
            _touchToFind = fingerId;
            return list.FindIndex(TouchFindPredicate);

        }
        // Only create the predicate delegate once to eliminate heap churn
        public Predicate<TouchPt> TouchFindPredicate
        {
            get { return _touchFindPredicate ?? (_touchFindPredicate = IsTouchToFind); }
        }

        private Predicate<TouchPt> _touchFindPredicate; 
        private int _touchToFind;

        private bool IsTouchToFind(TouchPt t)
        {
            return t.fingerId == _touchToFind;
        }

        public float ComputeMinimumDistanceToPoint(Vector2 point)
        {
            if (Root == null)
                return 0;

            float distance;
            GetClosestNodeToPoint(Root, point, out distance);
            return Mathf.Sqrt(distance);
        }

        private void ComputeEdgeDistances()
        {
            _edges.Init(_touchPoints.Count);
            for (int i = 0; i < _touchPoints.Count; i++)
            {
                Vector2 point1 = _touchPoints[i].position;
                for (int j = i + 1; j < _touchPoints.Count; j++)
                {
                    Vector2 point2 = _touchPoints[j].position;
                    float dist2 = (point2 - point1).sqrMagnitude;
                    _edges[i, j] = dist2;
                }
            }
        }

        private void ComputeSpanningTree()
        {
            // Add point 0 as root
            Root = CreateNode(0);

            // Add all other points to the points to process list
            _pointsToProcess.Clear();
            for (int i = 1; i < _touchPoints.Count; i++)
                _pointsToProcess.Add(i);


            while (_pointsToProcess.Count > 0)
            {
                Node fromNode;
                int nextPoint = GetNextClosestPointToTree(out fromNode);
                _pointsToProcess.Remove(nextPoint);
                fromNode.Neighbors.Add(CreateNode(nextPoint));
            }
        }

        private int GetNextClosestPointToTree(out Node fromNode)
        {
            float minDistance = float.MaxValue;
            int minPoint = -1;
            fromNode = null;
            foreach (int pointIndex in _pointsToProcess)
            {
                float distance;
                Node node = GetClosestNodeToPoint(Root, pointIndex, out distance);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPoint = pointIndex;
                    fromNode = node;
                }
            }
            return minPoint;
        }

        /// <summary>
        /// Recursively find the node in the tree which is closest to pointIndex.
        /// 
        /// </summary>
        /// <param name="currentNode">Current node in the tree</param>
        /// <param name="pointIndex">point to find distances to</param>
        /// <param name="sqrDistance">distance squared to returned node</param>
        /// <returns>Closest node</returns>
        private Node GetClosestNodeToPoint(Node currentNode, int pointIndex, out float sqrDistance)
        {
            Node best = currentNode;
            sqrDistance = _edges[currentNode.TouchIndex, pointIndex];
            foreach (Node neighbor in currentNode.Neighbors)
            {
                float childDistance;
                Node bestChild = GetClosestNodeToPoint(neighbor, pointIndex, out childDistance);
                if (childDistance < sqrDistance)
                {
                    best = bestChild;
                    sqrDistance = childDistance;
                }
            }
            return best;
        }

        /// <summary>
        /// Recursively find the node in the tree which is closest to an arbitrary point.
        /// Does not use the cached edge distances.
        /// </summary>
        /// <param name="currentNode">Current node in the tree</param>
        /// <param name="point">point to find distances to</param>
        /// <param name="sqrDistance">distance squared to returned node</param>
        /// <returns>Closest node</returns>
        private Node GetClosestNodeToPoint(Node currentNode, Vector2 point, out float sqrDistance)
        {
            Node best = currentNode;
            sqrDistance = (_touchPoints[currentNode.TouchIndex].position - point).sqrMagnitude;
            foreach (Node neighbor in currentNode.Neighbors)
            {
                float childDistance;
                Node bestChild = GetClosestNodeToPoint(neighbor, point, out childDistance);
                if (childDistance < sqrDistance)
                {
                    best = bestChild;
                    sqrDistance = childDistance;
                }
            }
            return best;
        }

        private Node CreateNode(int touchIndex)
        {
            Node n = NodePool.Get();
            n.TouchIndex = touchIndex;
            return n;
        }

        private void ClearTree(Node node)
        {
            if (node == null)
                return;

            foreach (var n in node.Neighbors)
                ClearTree(n);

            NodePool.Release(node);
        }

        private class Edges
        {
            private int _capacity;
            private float[,] _edgeDistances;

            public void Init(int numberNodes)
            {
                if (numberNodes > _capacity)
                {
                    // Only reallocate if we need a larger number of nodes than
                    // existing capacity.
                    _capacity = numberNodes;
                    _edgeDistances = new float[numberNodes, numberNodes];
                }
                for (int i = 0; i < numberNodes; i++)
                    for (int j = 0; j < numberNodes; j++)
                        _edgeDistances[i, j] = float.NaN;
            }

            public float this[int n1, int n2]
            {
                get { return _edgeDistances[n1, n2]; }
                set
                {
                    _edgeDistances[n1, n2] = value;
                    _edgeDistances[n2, n1] = value;
                }
            }
        }

        private int GetNextClusterId()
        {
            return _nextClusterId++;
        }

        private readonly Edges _edges = new Edges();

        private readonly List<TouchPt> _touchPoints = new List<TouchPt>(); 
        private readonly List<TouchPt> _tempTouchPoints = new List<TouchPt>(); 
        private readonly List<int> _pointsToProcess = new List<int>(); 
        private readonly List<Node> _splitList = new List<Node>();
        private int _clusterId;

        private static readonly Pool<Node> NodePool = new Pool<Node>(null, n => n.Clear());
        private static readonly Pool<SpanningTree> SpanningTreePool = new Pool<SpanningTree>(t => t.Init(), t => t.Clear());
        private static int _nextClusterId = 1;

    }
}
