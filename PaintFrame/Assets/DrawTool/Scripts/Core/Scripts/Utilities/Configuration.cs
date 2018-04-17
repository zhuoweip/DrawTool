using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DevelopEngine
{
	public class Configuration : MonoSingleton<Configuration>
	{		
		private static WWW www;
		
		private static bool isDone = false;
		public static bool IsDone {
			get {
				if (www != null && www.isDone)
				{
					Load(www.bytes);
					isDone = true;
					return isDone;
				}
				return isDone;
			}
		}
	
		protected static Dictionary<string, Dictionary<string, string>> mDictionary = new Dictionary<string, Dictionary<string, string>>();
		
		public static void LoadConfig (string configPath)
		{			
			if (isDone)
				return;
			
			if (www == null)
			{
#if UNITY_EDITOR
				www = new WWW("file://" + Application.dataPath + "/StreamingAssets" + configPath);
#elif UNITY_STANDALONE_WIN
				www = new WWW("file://" + Application.dataPath + "/StreamingAssets" + configPath);
#elif UNITY_IPHONE
				www = new WWW("file://" + Application.dataPath + "/Raw" + configPath);	
#elif UNITY_ANDROID
				www = new WWW("jar:file://" + Application.dataPath + "!/assets" + configPath);
#endif
			}
		}
		
		static void Load (byte[] bytes)
		{
			if (isDone)
				return;
			
			ConfigReader reader = new ConfigReader(bytes);
			
			mDictionary = reader.ReadDictionary();
			
			www = null;
		}
	
//		void Load (TextAsset asset)
//		{
//			ConfigReader reader = new ConfigReader(asset);
//			mDictionary = reader.ReadDictionary();
//			//NGUITools.Broadcast("OnConfig", this);
//		}
	
		//public Dictionary<string, Dictionary<string, string>> ConfigDictionary {get {return mDictionary;}}
		
		public static string Get (string mainKey, string subKey)
		{
			if (mDictionary.ContainsKey(mainKey) && mDictionary[mainKey].ContainsKey(subKey))
				return mDictionary[mainKey][subKey];
			
			return mainKey + "." + subKey;
		}
		
		public static Dictionary<string, string> Get (string mainKey)
		{
			if (mDictionary.ContainsKey(mainKey))
				return mDictionary[mainKey];
			
			return null;
		}
		
		public static int GetInt (string mainKey, string subKey)
		{
			int ret;
			int.TryParse(Get(mainKey, subKey), out ret);
			return ret;
		}
		
		public static float GetFloat (string mainKey, string subKey)
		{
			float ret;
			float.TryParse(Get(mainKey, subKey), out ret);
			return ret;
		}
		
		public static int GetChildCount(string mainKey)
		{
			if(mDictionary.ContainsKey(mainKey))
			{
				int ChildCount = mDictionary[mainKey].Count;
				return ChildCount;
			}
			return 0;
		}
		
		public static string GetContent (string mainKey, string subKey)
		{
			string ret = Get(mainKey, subKey);
			if(ret.StartsWith("\"")) ret = ret.Substring(1, ret.Length-1);
			if(ret.EndsWith(";")) ret = ret.Substring(0, ret.Length-2);
			if(ret.EndsWith("\"")) ret = ret.Substring(0, ret.Length-2);
			return ret;
		}
	}
}