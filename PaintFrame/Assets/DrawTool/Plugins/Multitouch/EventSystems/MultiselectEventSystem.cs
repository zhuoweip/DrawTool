
/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems
{
    [AddComponentMenu("Event/Multiselect Event System")]
    public class MultiselectEventSystem : EventSystem
    {
        public float ScreenPPI = 100.0f;
        public float MaxTouchClusterDistanceInches = 6f;
        public float MaxTouchClusterDistancePixels { get { return MaxTouchClusterDistanceInches * ScreenPPI; } }


        public float MaxDoubleClickDistancePixels = 5f;
        public float MaxDoubleClickTime = 0.5f;

        public const int InvalidContext = -100;
        public int CurrentSelectedObjectContext { get; private set; }
        public new static MultiselectEventSystem current { get { return (MultiselectEventSystem) EventSystem.current;  } }

        protected override void Awake()
        {
            base.Awake();
            CurrentSelectedObjectContext = InvalidContext;
        }

        public void SetSelectedObjectContext(int touchId)
        {
            if (CurrentSelectedObjectContext == touchId)
                return;

            if (CurrentSelectedObjectContext != InvalidContext)
                _selectedObjectsPerTouchId[CurrentSelectedObjectContext] = currentSelectedGameObject;

            GameObject savedContext;
            if (_selectedObjectsPerTouchId.TryGetValue(touchId, out savedContext))
            {
                if (!savedContext)
                {
                    _selectedObjectsPerTouchId.Remove(touchId);
                    savedContext = null;
                }
            }
            CurrentSelectedObjectContext = touchId;

            // Set the current selected field directly without invoking the normal side effects
            SetCurrentSelectedNoSideEffects(savedContext);
        }

        private FieldInfo _currentSelected;
        private void SetCurrentSelectedNoSideEffects(GameObject selected)
        {
            if (_currentSelected == null)
            {
                _currentSelected = typeof(EventSystem).GetField("m_CurrentSelected",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (_currentSelected == null)
                    return;
            }
            _currentSelected.SetValue(this, selected);
        }


        public void RemoveSelectedObjectContext(int touchId)
        {
            _selectedObjectsPerTouchId.Remove(touchId);
        }

        private readonly Dictionary<int, GameObject> _selectedObjectsPerTouchId = new Dictionary<int, GameObject>(); 
    }
}
