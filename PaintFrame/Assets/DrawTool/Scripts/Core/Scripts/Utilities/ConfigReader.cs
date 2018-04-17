using UnityEngine;
using System.Text;
using System.Collections.Generic;

namespace DevelopEngine
{
	public class ConfigReader
	{
		byte[] mBuffer;
		int mOffset = 0;
	
		public ConfigReader (byte[] bytes)
		{
			mBuffer = bytes;
		}

		public ConfigReader (TextAsset asset)
		{
			mBuffer = asset.bytes;
		}
	
		public bool canRead { get { return (mBuffer != null && mOffset < mBuffer.Length); } }
	
		static string ReadLine (byte[] buffer, int start, int count)
		{
			return Encoding.UTF8.GetString (buffer, start, count);
		}
	
		public string ReadLine ()
		{
			int max = mBuffer.Length;
	
			while (mOffset < max && mBuffer[mOffset] < 32)
				++mOffset;
	
			int end = mOffset;
	
			if (end < max) {
				for (; ;) {
					if (end < max) {
						int ch = mBuffer [end++];
						if (ch != '\n' && ch != '\r')
							continue;
					} else
						++end;
	
					string line = ReadLine (mBuffer, mOffset, end - mOffset - 1);
					mOffset = end;
					return line;
				}
			}
			mOffset = max;
			return null;
		}
		
		public Dictionary<string, Dictionary<string, string>> ReadDictionary ()
		{
			Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>> ();
			char[] separator = new char[] {'='};
			
			string mainKey = "Config";
			while (canRead) {
				string line = ReadLine ();
				if (line == null)
					break;
				
				if (line.StartsWith("*"))
					continue;
				
				if (line.StartsWith ("[")) {
					int index = line.IndexOf ("]");
					
					mainKey = line.Substring (1, index - 1);		
						
					if (!dict.ContainsKey (mainKey))
						dict.Add (mainKey, new Dictionary<string, string> ());
				} else {
					//string[] split = line.Split(separator, 2, System.StringSplitOptions.RemoveEmptyEntries);
					string[] split = line.Split (separator, 2, System.StringSplitOptions.None);
					if (split.Length == 2) {
						string key = split [0].Trim ();
						string val = split [1].Trim ();

						dict [mainKey].Add (key, val);
					}
				}
			}
			/*
			Debug.Log("key");
			foreach (var v in dict.Keys)
			{
				Debug.Log(v);
			}
			Debug.Log("subkey");
			foreach (var v in dict.Values)
			{
				Debug.Log("value");
				foreach (var vvv  in v.Keys)
				{
					Debug.Log(vvv);
				}
			}
			*/
			
			return dict;
		}
	}
}