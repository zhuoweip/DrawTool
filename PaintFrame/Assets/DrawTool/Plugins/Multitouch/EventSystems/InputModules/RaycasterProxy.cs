/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using System;
using UnityEngine;

namespace Multitouch.EventSystems.InputModules
{
    /// <summary>
    /// Attach this Raycaster proxy to a UI object which is to display
    /// the RenderTexture from another camera.
    /// 
    /// Link this object to the ProxyRaycasterController in the hierarchy
    /// which contains the Raycasters which will be used to handle touches on this
    /// UI object as though they came from the remote camera's "screen"
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RaycasterProxy : MonoBehaviour
    {
        public ProxyRaycasterController ProxyRaycaster { get; set; }
        public enum ProxyMode
        {
            Remote,
            Local
        }

        public Func<ProxyMode> ModeFunction;
        public ProxyMode Mode
        {
            get
            {
                if (ModeFunction == null)
                    return ProxyMode.Local;
                return ModeFunction();
            }
        }

        public bool IsProxyActive { get { return Mode == ProxyMode.Remote; } }

        public Camera EventCamera
        {
            get
            {
                if (_eventCamera == null)
                {
                    var canvas = GetComponentInParent<Canvas>();
                    _eventCamera = canvas.worldCamera;
                }
                return _eventCamera;
            }
        }

        public Vector2 GetRemoteCoordinateSpace()
        {
            return new Vector2(EventCamera.pixelWidth, EventCamera.pixelHeight);
        }

        public RectTransform RcTransform
        {
            get { return _rcTransform ?? (_rcTransform = (RectTransform) transform); }
        }

        private RectTransform _rcTransform;
        private Camera _eventCamera;

    }
}
