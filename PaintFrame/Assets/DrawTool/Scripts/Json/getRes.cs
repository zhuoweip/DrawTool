using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using System.Drawing;

/// <summary>
/// 获取图片
/// </summary>
public class getRes : MonoBehaviour {

    private getRes() { }
    public static readonly getRes instance = new getRes();

    public ArrayList getResTex(string filepath)
    {
        string parentpath = Application.dataPath  +filepath;
        Debug.Log(parentpath);

        ArrayList ar = new ArrayList();
        DirectoryInfo parentdir = new DirectoryInfo(parentpath);
        if (!parentdir.Exists)
        {
            Debug.Log("没有找到 " + parentpath + "路径");
        }
        else
        {
            int num = GetFileNum(parentpath);
             Debug.Log("图片总数： " + num);
            for (int t = 1; t <=num; t++)
            {
                string bgPngPath = parentpath  + t + ".png";
                
                if (File.Exists(bgPngPath))
                {
                   // FileStream fs = new FileStream(bgPngPath, FileMode.Open, FileAccess.Read);
                   // Image img = System.Drawing.Image.FromStream(fs);
                   // MemoryStream ms = new MemoryStream();
                   // img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                  
                   // Texture2D  _tex2 = new Texture2D(img.Width, img.Height);
                   
                   //_tex2.LoadImage(ms.ToArray());
                   //fs.Flush();
                   //fs.Close();
                   //fs.Dispose();
                   // ar.Add(_tex2);

                    FileStream filestream = new FileStream(bgPngPath, FileMode.Open, FileAccess.Read);
                    filestream.Seek(0, SeekOrigin.Begin);
                    byte[] bytes = new byte[filestream.Length];
                 
                    filestream.Read(bytes, 0, (int)filestream.Length);
                       
                    filestream.Close();
                    filestream.Dispose();
                    filestream = null;
                    Image img = System.Drawing.Image.FromFile(bgPngPath);
                    int width = img.Width;
                    int height = img.Height;
                    Texture2D tex = new Texture2D(width, height);
                    tex.LoadImage(bytes);
                    ar.Add(tex);
                }
            }
        }
        Debug.Log("ar: " + ar.Count);
        return ar;
    }

    public int GetFileNum(string srcPath)
    {
        int fileNum = 0;
        try
        {

            // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组  
            string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath,"*.png" );
            // 遍历所有的文件和目录  
            foreach (string file in fileList)
            {
                // 先当作目录处理如果存在这个目录就重新调用GetFileNum(string srcPath)  
                if (System.IO.Directory.Exists(file))
                    GetFileNum(file);
                else
                    fileNum++;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return fileNum;
    }  
}
