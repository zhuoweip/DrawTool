/* 
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */

using UnityEngine;

namespace Multitouch.EventSystems.Utils
{
    /// <summary>
    /// A few Extension methods for converting between Vector3 and Vector2
    /// and to perform certain other operations.
    /// </summary>
    public static class VectorOps
    {
        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 YZ(this Vector3 v)
        {
            return new Vector2(v.y, v.z);
        }

        public static Vector2 Floor(this Vector2 v)
        {
            return new Vector2((int) v.x, (int) v.y);
        }

        public static Vector2 Round(this Vector2 v)
        {
            return new Vector2((int) (v.x + 0.5f), (int) (v.y + 0.5f));
        }

        public static Vector2 Perp(this Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }

        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        public static Vector3 AsVector3(this Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }
    }
}