using UnityEngine;
using System.Collections;
//using UnityEditor;

namespace GestureRecognizer {

    public enum GestureLimitType {
        /// <summary>
        /// No gesture limiting
        /// </summary>
        None,

        /// <summary>
        /// Rect transform of a UI element. Ignores the points that are outside the rect.
        /// Set the containing canvas' render mode to Screen space - camera and assign 
        /// the main camera as the Render Camera for this to work
        /// </summary>
        RectBoundsIgnore,

        /// <summary>
        /// Rect transform of a UI element. Clamps the points that are outside the rect.
        /// Set the containing canvas' render mode to Screen space - camera and assign 
        /// the main camera as the Render Camera for this to work
        /// </summary>
        RectBoundsClamp,
    }

    public class Utility {
		
		// Self-explanatory.
		public static bool IsTouchDevice() {
			return Application.platform == RuntimePlatform.Android || 
				   Application.platform == RuntimePlatform.IPhonePlayer || 
				   Application.platform == RuntimePlatform.WP8Player;
		}
		
		
		/// <summary>
		/// Convert the screen point to world point so that the new point can be put
		/// in the correct position for line renderer.
		/// </summary>
		/// <param name="gesturePoint"></param>
		/// <returns></returns>
		public static Vector3 WorldCoordinateForGesturePoint(Vector3 gesturePoint) {
			Vector3 worldCoordinate = new Vector3(gesturePoint.x, gesturePoint.y, 10);
			return Camera.main.ScreenToWorldPoint(worldCoordinate);
		}


        /// <summary>
        /// Clamp a point to a rectangle
        /// </summary>
        /// <param name="point">Point to clamp</param>
        /// <param name="rect">Rectangle to clamp the point in</param>
        /// <returns>Point</returns>
        public static Vector2 ClampPointToRect(Vector2 point, Rect rect) {
            return new Vector2(Mathf.Clamp(point.x, rect.min.x, rect.max.x), Mathf.Clamp(point.y, rect.min.y, rect.max.y));
        }
    }
}

