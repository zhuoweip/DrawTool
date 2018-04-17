using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PPTLoad : MonoBehaviour            //PPT图片得加载和轮播，优先执行RunEx脚本
{
    public Image info;
    public GameObject ppt;
    Texture2D mTexture;
    bool mTex = true;

    float stopTime = 2f;

    public void Awake()
    {
        stopTime = float.Parse(LoadXml.ReadXml("Within", "stopTime"));
        StartCoroutine(LoadWWWAllPicture());
    }

    public void PlayLuBo()
    {
        mTex = true;
        id = 1;
        info.transform.localPosition = Vector3.zero;
        info.GetComponent<Image>().color = new Color(1,1,1,1);
        //StopCoroutine("Lun");
        StartCoroutine("Lun");
    }

    public void StopLuBo()
    {
        mTex = false;
        info.transform.DOPause();
        StopCoroutine("Lun");
        ppt.SetActive(false);
    }

    int id = 1;
    IEnumerator Lun()
    {
        while (mTex)
        {
            LunPlayer(id);

            yield return new WaitForSeconds(stopTime);           
            info.transform.localPosition = Vector3.zero;
            info.transform.DOMoveX(-1f, 0.5f);
            info.transform.GetComponent<Image>().DOColor(new Color(1, 1, 1, 0), 0.5f);         
            yield return new WaitForSeconds(0.5f);
            id++;
            id = id > allTex2d.Count ? 1 : id;

            LunPlayer(id);
            info.transform.localPosition = new Vector3(135f, info.transform.localPosition.y, 0);
            info.transform.DOMoveX(0f, 0.5f);
            info.transform.GetComponent<Image>().DOColor(new Color(1, 1, 1, 1), 0.5f);           
        }
    }

    //轮播得ID的方法
    public void LunPlayer(int id)
    {
        //id = 2;
        //print(allTex2d.Count);
        for (int i = 0; i < allTex2d.Count; i++)
        {
            if (id == int.Parse(allTex2d[i].name))
            {
                //print(id);
                //print(int.Parse(allTex2d[i].name));
                Texture2D tmp = allTex2d[i];
                tmp.filterMode = FilterMode.Point;
                Sprite sprite = Sprite.Create(tmp, new Rect(0, 0, tmp.width, tmp.height), new Vector2(1f, 1f));
                info.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
                info.sprite = sprite;
            }
        }
    }

    List<Texture2D> allTex2d = new List<Texture2D>();
    Hashtable ht = new Hashtable();
    public IEnumerator LoadWWWAllPicture()
    {
        allTex2d.Clear();
        string streamingPath = Application.streamingAssetsPath;
        DirectoryInfo dir = new DirectoryInfo(streamingPath + "/Release/textures/");//初始化一个DirectoryInfo类的对象
        //print(dir);
        GetAllFiles(dir);
        double startTime = (double)Time.time;
        //int value = 0;
        foreach (DictionaryEntry de in ht)
        {
            WWW www = new WWW("file://" + streamingPath + "/" + de.Key);
            yield return www;
            if (www != null)
            {
                Texture2D tmp = www.texture;
                tmp.name = de.Key.ToString().Substring(de.Key.ToString().LastIndexOf('\\') + 1).Replace(".png", "");                       //根据数字下标命名获取图片重新排序
                allTex2d.Add(tmp);
                //print(tmp.name);
            }
            if (www.isDone)
            {
                www.Dispose();
            }
        }
    }

    //读取StreamingAssets文件夹下的图片
    public void GetAllFiles(DirectoryInfo dir)
    {
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();   //初始化一个FileSystemInfo类型的实例
        foreach (FileSystemInfo i in fileinfo)              //循环遍历fileinfo下的所有内容
        {
            if (i is DirectoryInfo)             //当在DirectoryInfo中存在i时
            {
                GetAllFiles((DirectoryInfo)i);  //获取i下的所有文件
            }
            else
            {
                string str = i.FullName;        //记录i的绝对路径
                string path = Application.streamingAssetsPath;
                string strType = str.Substring(path.Length);
                //print(str);
                if (strType.Substring(strType.Length - 3).ToLower() == "png")
                {
                    if (ht.Contains(strType))
                    {
                        ht[strType] = strType;
                    }
                    else
                    {
                        ht.Add(strType, strType);
                    }

                }
            }
        }
    }
}
