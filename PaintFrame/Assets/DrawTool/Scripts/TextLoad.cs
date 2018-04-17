using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class TextLoad : MonoBehaviour {

    public GameObject gridLayoutGroup; 

    public void Awake()
    {
        LoadTex();
        LoadString(0);
    }

    public static Dictionary<string, List<Sprite>> spritelist = new Dictionary<string, List<Sprite>>();
    public static Dictionary<string, Dictionary<string, List<string>>> list;
    public static List<string> mList = new List<string>();
    //static GameObject debug;
    void Start()
    {
        
    }

    public static string LoadString(int count)
    {
        if (mList.Count != 0)
        {
            mList.Clear();
        }

        string path = Application.streamingAssetsPath+"/Video/";
        string videoString = "";
        if (Directory.Exists(path))
        {
            //获取文件信息
            DirectoryInfo direction = new DirectoryInfo(path);

            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            //print(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                //过滤掉临时文件
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }

                if (files[i].Extension == ".mp4")
                {
                    //print(files[i].Extension); //这个是扩展名
                    //获取不带扩展名的文件名
                    string name = Path.GetFileName(files[i].FullName);
                    videoString = name;
                    //print(videoString);
                    mList.Add(videoString);                   
                }      
            }
            //print(mList[count]);
        }

        return videoString;
    }

    private void LoadTex()
    {
        try
        {
            if (!Directory.Exists(Application.streamingAssetsPath + "/QiHuan"))
            {
                //Debug.Log("没有找到存放图片的建筑艺术文件夹");
                return;
            }
            string[] parent = Directory.GetDirectories(Application.streamingAssetsPath + "/QiHuan/");

            foreach (var item in parent)
            {
                //Debug.Log("item" + item.Length);

                string[] Items = Directory.GetDirectories(item);
                for (int i = 0; i < Items.Length; i++)
                {
                    //图片
                    DirectoryInfo direction = new DirectoryInfo(Items[i]);

                    //Debug.Log("dir" + direction);
                    string[] Names = Items[i].Split('/');
                    string Name = Names[Names.Length - 1];
                    //Debug.Log("Name/" + Name);
                    Name = Name.Replace("\\", "");
                    //Debug.Log("Name\\" + Name);
                    FileInfo[] Iconfiles = direction.GetFiles("*", SearchOption.AllDirectories);

                    //Debug.Log("Icon" + Iconfiles);

                    List<Sprite> list = new List<Sprite>();
                    for (int z = 0; z < Iconfiles.Length; z++)
                    {
                        if (Iconfiles[z].Extension != ".png")
                            continue;

                        string[] SceneNames = Iconfiles[z].FullName.Split('\\');
                        //Debug.Log("mSceneName" + SceneNames);
                        string sceneName = SceneNames[SceneNames.Length - 1];
                        //Debug.Log("SceneName"+sceneName);
                        //Debug.Log("Iconfiles[z]" + Iconfiles[z].FullName);
                        Sprite sprite = GetSprite(Iconfiles[z].FullName);
                        sceneName = sceneName.Replace(".png", "");
                        sprite.name = sceneName;
                        //Debug.Log(sprite.name);
                        //if (Name.IndexOf("3级") != -1)
                        //{
                        //    string typeName = SceneNames[SceneNames.Length - 2];
                        //    sprite.name = typeName + "-" + sceneName;
                        //}
                        list.Add(sprite);
                    }

                    spritelist.Add(Name, list);
                }
            }
            //Resources.UnloadUnusedAssets();
        }
        catch (System.Exception ex)
        {

            //Debug.Log(ex.Message);
        }
    }


    public static Sprite GetSprite(string address)
    {
        using (FileStream stream = File.Open(address, FileMode.Open))
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            Texture2D texture = new Texture2D(1920, 1080);
            texture.LoadImage(bytes);
            //创建Sprite
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //处理方法 
            return sprite;
        }
    }

}
