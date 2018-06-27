using UnityEngine;
using System.Collections;
using ZXing;
using ZXing.QrCode;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class QR_Code : MonoBehaviour
{
    private Texture2D encoded;
    public string Lastresult;
    public GameObject savaInto;
    public RawImage img;
    public Button closeBtn;
    private DrawManager drawManager;

    private void Awake()
    {
        drawManager = GameObject.FindObjectOfType<DrawManager>();
    }

    void Start()
    {
        encoded = new Texture2D(256, 256);
        RegistBtn();
    }

    private void RegistBtn()
    {
        closeBtn.onClick.AddListener(() =>
        {
            savaInto.SetActive(false);
            drawManager.isForbid = false;
        });
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        BarcodeWriter writer = new BarcodeWriter {
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
        if (!drawManager.isForbid && savaInto.activeInHierarchy)
        {
            drawManager.isForbid = true;
        }
    }

    private void ShowSaveInfo()
    {
        string textForEncoding = Lastresult;

        if (textForEncoding != null)
        {
            Color32[] color32 = Encode(textForEncoding, encoded.width, encoded.height);

            encoded.SetPixels32(color32);

            encoded.Apply();
        }

        img.texture = encoded;
        SetImgActive(true);
    }

    private void SetImgActive(bool active)
    {
        savaInto.SetActive(active);
        drawManager.isForbid = true;
    }

    string url = "http://sq.gzcloudbeingbu.com/Webpage/MyScreenshots.aspx";

    public void UpLoad(byte[] bytes, string timestamp)
    {
        StartCoroutine(UploadPNG(bytes, timestamp));
    }

    //上传图片扫描二维码识别
    private IEnumerator UploadPNG(byte[] bytes,string timestamp)
    {
        Dictionary<string, string> heads = new Dictionary<string, string>();
        heads.Add("PicNum", timestamp);
        heads.Add("Continen", "");

        WWW www = new WWW(url, bytes, heads);//上传到服务器
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print(www.error);
        }
        else
        {
            print("Finished Uploading Screenshot");
            Lastresult = "http://sq.gzcloudbeingbu.com/Picture/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + timestamp + ".png";
            ShowSaveInfo();
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