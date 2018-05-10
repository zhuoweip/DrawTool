using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Text;
using DevelopEngine;

/// <summary>
/// 配置文件
/// </summary>
public class Configs : MonoSingleton<Configs> {
	private void Awake()
    {
        init();
    }

    private string[] keys;
    public   JsonData jsonData;

    //初始化json
    void init()
    {
        string filepath;


#if UNITY_EDITOR
        filepath =Application.dataPath + "/StreamingAssets" + "/Config.txt";
#elif UNITY_STANDALONE_WIN
				filepath =Application.dataPath + "/StreamingAssets" + "/Config.txt";
#elif UNITY_IPHONE
				filepath = Application.dataPath + "/Raw" + "/Config.txt";	
#elif UNITY_ANDROID
				filepath = "jar:Application.dataPath + "!/assets" + "/Config.txt";
#endif
        bool isContains = FileHandle.instance.isExistFile(filepath);

      

        //Debug.Log(isContains);
        if (isContains)
        {
            string str = FileHandle.instance.FileToString(filepath, Encoding.Default);
            //Debug.Log(str);

            jsonData = JsonMapper.ToObject(str);
            #region  key 遍历
         
            //IDictionary dict = jsonData as IDictionary;

            //int count = 0;
            //keys = new string[dict.Count];
            //foreach (string key in dict.Keys)
            //{

            //    keys[count] = key;
            //    Debug.Log(keys[count]);
            //    count++;

            //}
             
            #endregion
        }
 
    }

   /// <summary>
   /// 外部调用加载图片
   /// </summary>
   /// <param name="name"></param>
   /// <returns></returns>
   public List<Texture2D> LoadTexture(string name)
    {
       
        ArrayList ar = new ArrayList();
       
      //  Debug.Log("name: " + jsonData[name]);
        string path = (string)jsonData[name]["texpath"];
        ar = getRes.instance.getResTex(path);
        List<Texture2D> list = new List<Texture2D>();
        foreach (Texture2D temp in ar)
        {
            list.Add(temp);
        }
        return list;
    }

 
    /// <summary>
    /// 外部调用获取文字信息
    /// </summary>
    /// <param name="name"></param>
    /// <param name="info"></param>
    /// <returns></returns>
   public string LoadText(string name,string info)
   {
       string str = (string)jsonData[name][info];
       //Debug.Log(str);
       return str;
   }

    /// <summary>
    /// 获取子节点个数
    /// </summary>
    /// <returns></returns>
    public int GetJsonCount()
    {
        return jsonData.Count;
    }
	
    /// <summary>
    /// 获取子节点下子节点个数
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetJsonCount(string name)
    {
        return jsonData[name].Count;
    }

    
 
}
