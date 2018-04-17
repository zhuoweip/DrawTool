using UnityEngine;
using System.Collections;
using ZXing;
using ZXing.QrCode;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class QR_Code : MonoBehaviour
{
    public GameObject hidecan;
    public Texture2D encoded;

    public string Lastresult;
    public GameObject savaInto;
    public Image img;
    void Start()
    {
        encoded = new Texture2D(256, 256);
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter {
            Format = BarcodeFormat.QR_CODE,
            Options = new
            QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return
        writer.Write(textForEncoding);
    }

    void Update()
    {
        if (isshowqr)
        { 
            var textForEncoding = Lastresult;

            if(textForEncoding != null)
            {
                var color32 = Encode(textForEncoding, encoded.width, encoded.height);

                encoded.SetPixels32(color32);

                encoded.Apply();
            }

            SetImgActive(true);
            Sprite sprite = Sprite.Create(encoded, new Rect(0, 0, encoded.width, encoded.height), new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
        }
    }

    private void SetImgActive(bool active)
    {
        Image[] img = savaInto.GetComponentsInChildren<Image>();
        for (int i = 0; i < img.Length; i++)
        {
            img[i].enabled = active;
        }
    }

    public void Close()
    {
        SetImgActive(false);
        isshowqr = false;

        if (hidecan == null)
            return;
        hidecan.SetActive(true);
    }

    string url = "http://sq.gzcloudbeingbu.com/Webpage/MyScreenshots.aspx";
    private bool isshowqr=false;

    public void UpLoad(byte[] bytes, string timestamp)
    {
        StartCoroutine(UploadPNG(bytes, timestamp));
    }

    //上传图片扫描二维码识别
    IEnumerator UploadPNG(byte[] bytes,string timestamp)
    {

        Dictionary<string, string> heads = new Dictionary<string, string>();
        heads.Add("PicNum", timestamp);
        heads.Add("Continen", "");

        WWW www = new WWW(url, bytes, heads);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print(www.error);
        }
        else
        {
            print("Finished Uploading Screenshot");
            isshowqr = true;
            Lastresult = "http://sq.gzcloudbeingbu.com/Picture/" + DateTime.Now.ToString("yyyy-MM-dd")+"/" + timestamp+ ".png";
        }
    }

    public static long GetTimeStamp(bool bflag = true)
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long ret;
        if (bflag)
            ret = Convert.ToInt64(ts.TotalSeconds);
        else
            ret = Convert.ToInt64(ts.TotalMilliseconds);
        return ret;
    }
}