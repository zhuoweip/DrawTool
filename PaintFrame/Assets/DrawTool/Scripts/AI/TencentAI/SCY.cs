using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SCY
{

    public class Utility
    {
        public static Texture2D Base64Img(string Base64String)
        {
            Texture2D pic = new Texture2D(500, 500);
            byte[] data = System.Convert.FromBase64String(Base64String);
            pic.LoadImage(data);
            return pic;
        }

        public static string ByteArrToStr(byte[] byteArray) {
            return Encoding.UTF8.GetString(byteArray);
        }

        public static string ByteArrToStr64(byte[] bytes){
            return System.Convert.ToBase64String(bytes);
        }

        //public static Bitmap GetWebImage(string url)
        //{
        //    Bitmap bitmap = null;
        //    HttpWebResponse response = null;
        //    try
        //    {
        //        Uri requestUri = new Uri(url);
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
        //        request.Timeout = 0x2bf20;
        //        request.Method = "GET";
        //        response = (HttpWebResponse)request.GetResponse();
        //        bitmap = new Bitmap(response.GetResponseStream());
        //    }
        //    catch (Exception)
        //    {
        //        throw new Exception("图片读取出错!");
        //    }
        //    finally
        //    {
        //        response.Close();
        //    }
        //    return bitmap;
        //}

        public static byte[] JoinByteArr(byte[] byte1, byte[] byte2)
        {
            byte[] buffer = new byte[byte1.Length + byte2.Length];
            Stream stream = new MemoryStream();
            stream.Write(byte1, 0, byte1.Length);
            stream.Write(byte2, 0, byte2.Length);
            stream.Position = 0L;
            if (stream.Read(buffer, 0, buffer.Length) <= 0)
            {
                throw new Exception("读取错误!");
            }
            return buffer;
        }

        public static byte[] StrToByteArr(string str) {
        return    Encoding.UTF8.GetBytes(str);
        }

        public static string UnixTime(double expired = 0.0)
        {
            long num = (DateTime.Now.AddSeconds(expired).ToUniversalTime().Ticks - 0x89f7ff5f7b58000L) / 0x989680L;
            return num.ToString();
        }
        public static string ImgBase64(string path, bool isWebImg = false)
        {
            string base64Info = "";
            //if (!System.IO.File.Exists(path))
            //{
            //    throw new Exception("文件不存在!");
            //}
            if (Application.platform == RuntimePlatform.Android)
            {
                base64Info = Convert.ToBase64String(GetPictureData(path.Replace("//jar:file://", "jar:file:///").Replace("!","//!")));
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                base64Info= Convert.ToBase64String(GetPictureData(path));
            }
            return base64Info;
        }

        //参数是图片的路径  
        public static byte[] GetPictureData(string imagePath)
        {
            FileStream fs = new FileStream(imagePath, FileMode.Open);
            byte[] byteData = new byte[fs.Length];
            fs.Read(byteData, 0, byteData.Length);
            fs.Close();
            return byteData;
        }

        public static IEnumerator LoadTextureFromPath(string path , RawImage raw)
        {
                WWW www = new WWW(path);
                yield return www;
                raw.texture = www.texture;
        }

        public static IEnumerator LoadTextureFromBase64(string bese64, RawImage raw)
        {
            WWW www = new WWW(bese64);
            yield return www;
            raw.texture = www.texture;
        }
    }
}
