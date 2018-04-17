using UnityEngine;
using System.Collections;

namespace DevelopEngine
{
	public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
	{
		private static T instance = null;
		public static T Instance {
			get {
				if (instance == null)
				{
					instance = FindObjectOfType(typeof(T)) as T;
					if (instance == null)
					{
						instance = new GameObject("_" + typeof(T).Name).AddComponent<T>();
						DontDestroyOnLoad(instance);
					}
					if (instance == null)
						Console.LogError("Failed to create instance of " + typeof(T).FullName + ".");
				}
				return instance;
			}
		}
		
		void OnApplicationQuit () { if (instance != null) instance = null; }
		
		public static T CreateInstance ()
		{
			if (Instance != null) Instance.OnCreate();
			return Instance;
		}
		
		protected virtual void OnCreate ()
		{
			
		}

        private void OnDestroy()
        {
            OnApplicationQuit();
        }
    }
}