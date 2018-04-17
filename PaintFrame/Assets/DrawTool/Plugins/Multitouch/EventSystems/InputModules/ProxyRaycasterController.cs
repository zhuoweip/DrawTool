/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Multitouch.EventSystems.InputModules
{
    /// <summary>
    /// Takes control of all raycasters in the tree at or below us.
    /// Uses a RaycasterProxy and its camera to determine if the raycast should be done at all,
    /// and transforms the screen point to map to this camera's dimensions. 
    /// </summary>
    public class ProxyRaycasterController : BaseRaycaster
    {
        [Tooltip("This is the camera which renders the portion of the scene for which this controller proxies the raycasters")]
        public Camera RenderCamera;

        /// <summary>
        /// This is the RaycasterProxy on the control which displays the rendered camera image
        /// Assigning a RaycasterProxy also sets this Controller as the controller on the RaycasterProxy
        /// </summary>
        public RaycasterProxy DisplayProxy
        {
            get { return _displayProxy; }
            set
            {
                if (_displayProxy != null)
                    _displayProxy.ProxyRaycaster = null;
                _displayProxy = value;
                _displayProxy.ProxyRaycaster = this;
            }
        }


        protected override void Awake()
        {
            base.Awake();

            var raycasters = GetComponentsInChildren<BaseRaycaster>();
            // Disable each of the raycasters being proxied.
            foreach (var raycaster in raycasters)
            {
                // leave this one alone, and not in list
                if (raycaster != this)
                {
                    raycaster.enabled = false;
                    _proxiedRaycasters.Add(raycaster);
                }
            }
        }

        protected override void OnEnable()
        {
            // DO NOT CALL BASE
            // BaseRaycaster.OnEnable() adds the raycaster to the current active raycasters.
        }

        protected override void OnDisable()
        {
            // DO NOT CALL BASE
            // BaseRaycaster.OnEnable() removes the raycaster from the current active raycasters.
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            // Proxy is not active, don't check here at all
            if (!DisplayProxy.IsProxyActive)
                return;

            // Save original position
            var origPosition = eventData.position;

            // Convert from source screen position to effective screen position for proxied camera
            RectTransform rcProxy = DisplayProxy.RcTransform;
            Camera camProxy = DisplayProxy.EventCamera;
            eventData.position = TransformScreenPosition(origPosition, rcProxy, camProxy,
                new Vector2(eventCamera.pixelWidth, eventCamera.pixelHeight));

            foreach (var raycaster in _proxiedRaycasters)
            {
                if (raycaster.gameObject.activeInHierarchy)
                {
                    raycaster.Raycast(eventData, resultAppendList);
                }
            }

            // Restore original position
            eventData.position = origPosition;
        }

        public Vector2 TransformScreenPosition(Vector2 position, RectTransform rcTransform, Camera cam, Vector2 targetSize)
        {
            Vector2 ptInRect;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rcTransform, position, cam, out ptInRect);
            Vector2 result;
            result.x = ((ptInRect.x - rcTransform.rect.xMin) / (rcTransform.rect.width)) * targetSize.x;
            result.y = ((ptInRect.y - rcTransform.rect.yMin) / (rcTransform.rect.height)) * targetSize.y;
            return result;
        }
        public override Camera eventCamera { get { return RenderCamera; } }

        private readonly List<BaseRaycaster> _proxiedRaycasters = new List<BaseRaycaster>();
        private RaycasterProxy _displayProxy;
    }
}
