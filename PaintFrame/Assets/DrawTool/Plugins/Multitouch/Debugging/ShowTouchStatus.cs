/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System.Collections.Generic;
using System.Text;
using Multitouch.EventSystems.Gestures;
using Multitouch.EventSystems.InputModules;
using UnityEngine;
using UnityEngine.UI;

namespace Multitouch.Debugging
{
    public class ShowTouchStatus : MonoBehaviour
    {
        //public Text DisplayControl;
        public MouseTouches MouseTouchController;

        public static ShowTouchStatus Instance
        {
            get
            {
                return _instance ?? (_instance = FindObjectOfType<ShowTouchStatus>());
            }
        }

        private static ShowTouchStatus _instance;

        public void RegisterCollector(TouchStatusCollector touchStatusCollector)
        {
            if (!_collectors.Contains(touchStatusCollector))
                _collectors.Add(touchStatusCollector);
        }

        public void UnRegisterCollector(TouchStatusCollector touchStatusCollector)
        {
            _collectors.Remove(touchStatusCollector);
        }

        // Update is called once per frame
        protected void Update ()
        {
            _sb.AppendFormat("Alt-L({0}) -R({1}) -GR({2})\n", Input.GetKey(KeyCode.LeftAlt), Input.GetKey(KeyCode.RightAlt), Input.GetKey(KeyCode.AltGr));
            _sb.AppendFormat("Ctl-L({0}) -R({1})\n", Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.RightControl));
            _sb.AppendFormat("Shift-L({0}) -R({1})\n", Input.GetKey(KeyCode.LeftShift), Input.GetKey(KeyCode.RightShift));

            Win10TouchInputModule inputModule;
            if (Win10TouchInputModule.TryGetCurrentWin10TouchInputModule(out inputModule))
            {
                var tracker = inputModule.TouchTracker;
                for (int i = 0; i < tracker.SpanningTreeCount; i++)
                {
                    if (tracker.GetSpanningTree(i).GetTouchCluster(_cluster))
                    {
                        int activeId = _cluster.ActiveTouchId;
                        _sb.AppendFormat("Cluster ID {0} [\n", _cluster.ClusterId);
                        foreach (var touch in _cluster)
                        {
                            _sb.Append(touch.fingerId == activeId ? "**  " : "    ");
                            _sb.AppendLine(touch.ToString());
                        }
                        _sb.AppendLine("]");
                    }
                }

                _sb.Append(inputModule.DebugString).AppendLine();
            }

            foreach (var statusCollector in _collectors)
            {
                statusCollector.CollectStatus(_sb);
            }

            //DisplayControl.text = _sb.ToString();
            _sb.Length = 0;
        }

        private StringBuilder _sb = new StringBuilder();
        private TouchCluster _cluster = new TouchCluster();

        private readonly List<TouchStatusCollector> _collectors = new List<TouchStatusCollector>();
    }
}
