using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

/// <summary>
/// 读取文件
/// </summary>
public class FileHandle   {

	private FileHandle(){}
	public static readonly FileHandle instance = new FileHandle();

	//the filepath if there is a file
	public bool isExistFile(string filepath){
		return File.Exists(filepath);
	}

	public bool IsExistDirectory(string directorypath){
		return Directory.Exists(directorypath);
	}

	public bool Contains(string Path,string seachpattern){
		try{
			string[] fileNames = GetFilenNames(Path,seachpattern,false);
			return fileNames.Length!=0;
		}
		catch{
			return false;
		}
	}

	//return a file all rows
	public static int GetLineCount(string filepath){
		string[] rows = File.ReadAllLines(filepath);
		return rows.Length;
	}


    /// <summary>
    /// 创建文件
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
	public bool CreateFile(string filepath){
		try{
			if(!isExistFile(filepath)){
				StreamWriter sw;
				FileInfo file = new FileInfo(filepath);
				//FileStream fs = file.Create();
				//fs.Close();
				sw = file.CreateText();
				sw.Close();
			}
		}
		catch{
			return false;
		}
		return true;
	}

	public string[] GetFilenNames(string directorypath,string searchpattern,bool isSearchChild){
		if(!IsExistDirectory(directorypath)){
			throw new FileNotFoundException();
		}
		try{
			
			return Directory.GetFiles(directorypath,searchpattern,isSearchChild?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly);
		}
		catch{
			return null;
		}

	}


    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="filepath">地址</param>
    /// <param name="content">内容</param>
	public void WriteText(string filepath,string content)
	{
		//File.WriteAllText(filepath,content);
		FileStream fs = new FileStream(filepath,FileMode.Append);
		StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
		sw.WriteLine(content);
		sw.Close();
		fs.Close();
		Debug.Log("write: filepath: "+filepath);
		Debug.Log("write: content: "+content);
		Debug.Log("写入完毕");
	}

	public void AppendText(string filepath,string content){
		File.AppendAllText(filepath,content);
	}


    /// <summary>
    ///  读取问字内容
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
	public string FileToString(string filepath,Encoding encoding){
		FileStream fs = new FileStream(filepath,FileMode.Open,FileAccess.Read);
		StreamReader reader = new StreamReader(fs,encoding);
		try{
			return reader.ReadToEnd();
		}
		catch{
			return string.Empty;
		}
		finally{
			fs.Close();
			reader.Close();
			//Debug.Log("读取完毕");
		}
	}


    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filepath"></param>
	public void ClearFile(string filepath){
		 

		File.Delete(filepath);
		CreateFile(filepath);
	}

}
