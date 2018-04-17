using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace DevelopEngine
{
	/// <summary>
	/// Console.
	/// This is a tool for tests or debug when running game in editor.
	/// </summary>
	public class Console : MonoSingleton<Console>
	{
#if UNITY_EDITOR
		/// <summary>
		/// E display page type.
		/// </summary>
		public enum EDisplayPageType
		{
			EDisplayPageType_Log,				//log
			EDisplayPageType_Command			//command
		}
		/// <summary>
		/// E message type.
		/// </summary>
		public enum EMsgType
		{
			EMsgType_InputCommand,
			EMsgType_CommandReturn,
			EMsgType_Log,
			EMsgType_LogWarnning,
			EMsgType_LogError
		}
		/// <summary>
		/// Message.
		/// </summary>
		public class Msg
		{
			public Msg(EMsgType type,string text)
			{
				this.type = type;
				this.text = text;
			}
			public EMsgType type;
			public string text;
			
			public static Color [] MsgColors = new Color[]
			{
				Color.white,
				Color.green,
				Color.white,
				Color.yellow,
				Color.red
			};
		}
		//console switch
		private  bool _switch = false;
		private Rect _windowRect;
		private string _inputString = "";
		public int	DisplayCount = 100; 
		private List<Msg> _messagesCommand = new List<Msg>();
		private List<Msg> _messagesLog = new List<Msg>();
		private Vector3 _logScrollPos = Vector3.zero;
		private EDisplayPageType _displayType = EDisplayPageType.EDisplayPageType_Command;
		
		//用于记忆输入操作
		private List<string> _inputTemps = new List<string>();
		private int _tempIndex = 0;
		private int _tempCount = 10;
		
#endif
		
#if UNITY_EDITOR
		public static string  LogFolderPath = Application.dataPath + "/../Log"; 
#elif UNITY_IPHONE
		public static string  LogFolderPath = Application.persistentDataPath + "/Log"; 
#else
		public static string  LogFolderPath = Application.dataPath + "/../Log"; 
#endif
		
		public static string  LogFileName;
		
		private StreamWriter _streamwriter = null;
		
		protected override void OnCreate ()
		{
#if SAVE_LOG
			DirectoryInfo info = new DirectoryInfo(LogFolderPath);
			if(!info.Exists)
			{
				info.Create();
			}
			
			string LogFileName = "/log " + System.DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".txt";
			FileInfo file = new FileInfo(LogFolderPath + LogFileName);
			if (!file.Exists)
			{
				file.Create();
			}
#endif
			
			
#if UNITY_EDITOR
			_windowRect = new Rect(Screen.width/4,Screen.height/8,Screen.width/2,Screen.height * 3/4.0f);
#endif
		}
		

		
//		void Start ()
//		{
//			/// When we use it, it will created by itself, but we don't save it into the scene.
//			//gameObject.hideFlags = HideFlags.HideAndDontSave;
//			DirectoryInfo info = new DirectoryInfo(LogFolderPath);
//			if(!info.Exists)
//			{
//				info.Create();
//			}
//			
//			FileInfo file = new FileInfo(LogFolderPath + LogFileName);
//			if (!file.Exists)
//			{
//				file.Create();
//			}
//			FileStream fs = new FileStream(LogFolderPath + "/log.txt",FileMode.Append,FileAccess.Write,FileShare.Write);
//			fs.Close();
//		
//			
//	#if UNITY_EDITOR
//			_windowRect = new Rect(Screen.width/4,Screen.height/8,Screen.width/2,Screen.height * 3/4.0f);
//	#endif
//		}
		
	#if UNITY_EDITOR
		//set console visible
		public static void SetConsoleVisible(bool isVisible)
		{
			Instance._switch = isVisible;
		}
		void OnGUI ()
		{
			if (Instance != this) return;
			/// Draw the console UI.
			if(_switch)
			{
				_windowRect = GUI.Window(111, _windowRect, MainWindow, "Console");
				GUI.BringWindowToFront(111);	
			}
			
		}
		
		void Update()
		{
			if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp("`"))
			{
				SetConsoleVisible(!Instance._switch);
			}
			
			if(_displayType == EDisplayPageType.EDisplayPageType_Command)
			{
				if(Input.inputString.Length > 0 && (Input.inputString[Input.inputString.Length-1] == '\r' || Input.inputString[Input.inputString.Length-1]== '\n'))
				{
					EvalInputString(_inputString);
					_inputString = "";
					
				}
				
				if(Input.GetKeyUp(KeyCode.UpArrow))
				{
					if(_inputTemps.Count > 0)
					{
						_inputString = _inputTemps[_tempIndex];
						_tempIndex--;
						if(_tempIndex < 0)
						{
							_tempIndex = 0;
						}
					}
					
				}
				if(Input.GetKeyUp(KeyCode.DownArrow))
				{
					if(_inputTemps.Count > 0)
					{
						_inputString = _inputTemps[_tempIndex];
						_tempIndex++;
						if(_tempIndex > _inputTemps.Count -1)
						{
							_tempIndex = _inputTemps.Count -1;
						}
					}
				}
			}
			
		}
		private void MainWindow(int windowID)
    	{
			if(_displayType == EDisplayPageType.EDisplayPageType_Command)
			{
				GUILayout.BeginHorizontal();
	            GUILayout.Space(5.0F);
	            GUILayout.BeginVertical();
				GUILayout.Space(5.0F);
            	GUILayout.Label("Command:");
	            GUILayout.BeginHorizontal();
	            _inputString = GUILayout.TextField(_inputString);
				 if (GUILayout.Button("Enter", GUILayout.Width(60.0F), GUILayout.Height(20.0F)))
	            {
					EvalInputString(_inputString);
	                _inputString = "";
	            }
				if(GUILayout.Button("Log", GUILayout.Width(60.0f),GUILayout.Height(20.0f)))
				{
					_displayType  = EDisplayPageType.EDisplayPageType_Log;
				}
				if(GUILayout.Button("Close", GUILayout.Width(60.0f),GUILayout.Height(20.0f)))
				{
					SetConsoleVisible(false);
				}
	            GUILayout.EndHorizontal();
	            _logScrollPos = GUILayout.BeginScrollView(_logScrollPos);
	            DisplayCommand();
	            GUILayout.EndScrollView();
	            GUILayout.Space(4.0F);
	            GUILayout.Space(4.0F);
	            GUILayout.EndVertical();
	            GUILayout.EndHorizontal();
			}
			else if(_displayType == EDisplayPageType.EDisplayPageType_Log)
			{
				GUILayout.BeginHorizontal();
	            GUILayout.Space(5.0F);
	            GUILayout.BeginVertical();
				GUILayout.Space(5.0F);
            	GUILayout.Label("Log:");
				GUILayout.BeginHorizontal();
				GUILayout.Label("");
	           	if(GUILayout.Button("Command", GUILayout.Width(80.0f),GUILayout.Height(20.0f)))
				{
					_displayType  = EDisplayPageType.EDisplayPageType_Command;
				}
				if(GUILayout.Button("Close", GUILayout.Width(80.0f),GUILayout.Height(20.0f)))
				{
					SetConsoleVisible(false);
				}
	            GUILayout.EndHorizontal();
	            _logScrollPos = GUILayout.BeginScrollView(_logScrollPos);
	            DisplayLog();
	            GUILayout.EndScrollView();
	            GUILayout.Space(4.0F);
	            GUILayout.Space(4.0F);
	            GUILayout.EndVertical();
	            GUILayout.EndHorizontal();
			}
        
		}
		private void DisplayCommand()
		{
			GUILayout.Label(">> ");
	        if (_messagesCommand != null && _messagesCommand.Count > 0)
	        {
	            foreach (Msg m in _messagesCommand)
	            {
					Color oldColor = GUI.color;
					GUI.color = Msg.MsgColors[(int)m.type];
	                GUILayout.Label(">> " + m.text);
					GUI.color = oldColor;
	            }
			}
        }
		
		private void DisplayLog()
		{
			 if (_messagesLog != null && _messagesLog.Count > 0)
	        {
	            foreach (Msg m in _messagesLog)
	            {
					Color oldColor = GUI.color;
					GUI.color = Msg.MsgColors[(int)m.type];
	                GUILayout.Label(">> " + m.text);
					GUI.color = oldColor;
	            }
			}
		}
		
		private void EvalInputString(string inputString)
		{
			if(string.IsNullOrEmpty(inputString))
			{
				return;
			}
			
			_inputTemps.Add(inputString);
			if(_inputTemps.Count  >  _tempCount)
			{
				_inputTemps.RemoveAt(0);
			}
			_tempIndex = _inputTemps.Count -1;
			
			string [] command = inputString.Split(' ');			
			_messagesCommand.Insert(0,new Msg(EMsgType.EMsgType_InputCommand,inputString));
			DeleteDisplayMsgCommand();
			List<string> param = new List<string>();
			for(int i = 1; i < command.Length ; ++i)
			{
				if(!string.IsNullOrEmpty(command[i]))
				{
					param.Add(command[i]);
				}
			}
			string returnStr = Cheats.ExecuteCheat(command[0],param.ToArray());
			if(string.IsNullOrEmpty(returnStr))
			{
				return;
			}
			_messagesCommand.Insert(0,new Msg(EMsgType.EMsgType_CommandReturn,returnStr));
			DeleteDisplayMsgCommand();
			
		}
		
		private void DeleteDisplayMsgCommand()
		{
			if(_messagesCommand.Count > DisplayCount)
			{
				_messagesCommand.RemoveAt(_messagesCommand.Count - 1);
			}
		}
		
		private void DeleteDisplayMsgLog()
		{
			if(_messagesLog.Count > DisplayCount)
			{
				_messagesLog.RemoveAt(_messagesLog.Count -1);
			}
		}
	#endif
		
		public static void Log (string message)
		{
#if DEBUG
			Debug.Log (message);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_Log,"Log:" + message));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Log:" + message);
#endif
		}
		
		public static void Log (string message, Object context)
		{
#if DEBUG
			Debug.Log (message, context);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_Log,"Log:" + message + ";" + context.ToString()));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Log:" + message,context.ToString());
#endif
			
		}
		
		public static void LogWarning (string message)
		{
#if DEBUG
			Debug.LogWarning(message);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_LogWarnning,"Warning:" + message));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Warning:" + message);
#endif
			
		}
		
		public static void LogWarning (string message, Object context)
		{
#if DEBUG
			Debug.LogWarning(message, context);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_LogWarnning,"Warning:" + message + ";" + context.ToString()));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Warning:" + message,context.ToString());
#endif
		}
		
		public static void LogError (string message)
		{
#if DEBUG
			Debug.LogError(message);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_LogError,"Error:" + message));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Error:" + message);
#endif
			
		}
		
		public static void LogError (string message, Object context)
		{
#if DEBUG
			Debug.LogError(message, context);
#endif
			
#if UNITY_EDITOR
//			Instance._messagesLog.Insert(0,new Msg(EMsgType.EMsgType_LogError,"Error:" +message + ";" + context.ToString()));
//			Instance.DeleteDisplayMsgLog();
#endif
			
#if SAVE_LOG
			/// Write in file.
			WriteLogInFile("Error:" +message,context.ToString());
#endif
		}
		
		//Write in file
		private static void WriteLogInFile(string message,string context)
		{
#if SAVE_LOG
			if(Instance._streamwriter == null)
			{
				Instance._streamwriter = new StreamWriter(LogFolderPath + LogFileName,true); 
			}
			Instance._streamwriter.Write(message + ";"+context + "\r\n");
			Instance._streamwriter.Flush();
			//_streamwriter.Close();
		//	File.AppendAllText(LogFolderPath + "/log.txt",message + ";"+context + "\r\n");
#endif

		}
		
		private static void WriteLogInFile(string message)
		{
#if SAVE_LOG
			if(Instance._streamwriter == null)
			{
				Instance._streamwriter = new StreamWriter(LogFolderPath + LogFileName,true); 
			}
			Instance._streamwriter.Write(message  + "\r\n");
			Instance._streamwriter.Flush();
			//_streamwriter.Close();	
			//File.AppendAllText(LogFolderPath + "/log.txt",message  + "\r\	
#endif
		}
		
		void OnDestroy()
		{
			if(_streamwriter != null)
			{
				_streamwriter.Close();
				_streamwriter = null;
			}
		}
		
		void OnApplicationQuit()
		{
			if(_streamwriter != null)
			{
				_streamwriter.Close();
				_streamwriter = null;
			}
		}
		
		
	}
	
}