using UnityEngine;
using TencentYoutuYun.SDK.Csharp;
using UnityEngine.UI;
using System.Collections.Generic;
using DevelopEngine;
using System;

public class FacialRecognition : MonoSingleton<FacialRecognition>
{
    string appid = "10139240";//"10126488";
    string secretId = "AKIDb7SddShau31oaBgquoW3PaaP3WovAjWF";//"AKID1qSt9Cce18zbyAHnFRN4ZppNjrk0hsTu";
    string secretKey = "mT4ZyIWfifXJzhLtLgv597zlPtBJG3f5";//"PceU9anlV1JNErGRN6btwmNktnTxDDzW";
    string userid = "youtu_76318_20180701230354_1645";//"1218207883";

    public Text textLog;
    public RawImage rawImage;
    [EnumLabel("模板类型")]
    public Template template;

    void Awake()
    {
        Conf.Instance().setAppInfo(appid, secretId, secretKey, userid, Conf.Instance().YOUTU_END_POINT);
    }

    //检测本地图片
    public void DetectLocalImg(string imgName)
    {
        FaceRecognition(Application.streamingAssetsPath + "/" + imgName + ".jpg");
    }

    public void FaceRecognition(string path)
    {
        //// 人脸检测
        //result = Youtu.detectface(path);
        //print(result);

        //// 人脸对比
        //result = Youtu.facecompare(path, path2);
        //print(result);

        //// 人脸关键点定位 调用demo
        //result = Youtu.faceshape(path);
        //print(result);

        //result = Youtu.getpersonids("group");
        //print(result);

        //// 名片OCR
        //path = System.IO.Directory.GetCurrentDirectory() + "\\ocr_card_01.jpg";
        //result = Youtu.bcocr(path);
        //print(result);

        //// 通用OCR
        //result = Youtu.generalocrurl("http://open.youtu.qq.com/app/img/experience/char_general/ocr_common01.jpg");
        //print(result);

        //// 行驶证OCR
        //path = System.IO.Directory.GetCurrentDirectory() + "\\ocr_xsz_01.jpg";
        //result = Youtu.driverlicenseocr(path, 0);
        //print(result);

        ////多人脸检索
        //List<string> group_ids = new List<string>();
        //result = Youtu.multifaceidentifyurl("http://open.youtu.qq.com/app/img/experience/face_img/face_05.jpg?v=1.0", "test", group_ids, 5, 40);
        //print(result);

        /////识别一个图像是否为暴恐图像
        //result = Youtu.imageterrorismurl("http://open.youtu.qq.com/app/img/experience/terror/img_terror01.jpg");
        //print(result);

        /////自动地检测图片车身以及识别车辆属性
        //result = Youtu.carcalssifyurl("http://open.youtu.qq.com/app/img/experience/car/car_01.jpg");
        //print(result);

        /////银行卡OCR识别，根据用户上传的银行卡图像，返回识别出的银行卡字段信息。
        //result = Youtu.creditcardocrurl("http://open.youtu.qq.com/app/img/experience/char_general/ocr_card_1.jpg");
        //print(result);

        /////营业执照OCR 识别，根据用户上传的营业执照图像，返回识别出的注册号、公司名称、地址字段信息
        //result = Youtu.bizlicenseocrurl("http://open.youtu.qq.com/app/img/experience/char_general/ocr_yyzz_01.jpg");
        //print(result);

        /// 车牌OCR识别，根据用户上传的图像，返回识别出的图片中的车牌号。
        //result = Youtu.plateocrurl("http://open.youtu.qq.com/app/img/experience/char_general/ocr_license_1.jpg");
        //print(result);

        /// 人脸融合，根据用户上传的图像，返回融合后的图像。
        //string result = string.Empty;
        //try
        //{
        //    Debug.LogError(Youtu.GetTemplate(template));
        //    result = Youtu.faceMerge(path, "base64", "[{\"cmd\":\"doFaceMerge\",\"params\":{\"model_id\":\"" + Youtu.GetTemplate(template) + "\"}}]");

        //    ShowRawimage(result, rawImage);
        //}
        //catch (System.Exception e)
        //{
        //    textLog.text = e.Message;
        //}

        //List<string> group_ids = new List<string>();
        //result = Youtu.multifaceidentify(path, "test", group_ids, 5, 40);

        //JsonParse.Multifaceidentify mu=JsonParse.Multifaceidentify.ParseMultifaceidentify(result);

        //textLog.text += "\n面部坐标:"+ mu.results[0].face_rect.x.ToString()+";"+ mu.results[0].face_rect.y;
        //textLog.text += "\n面部宽高:"+ mu.results[0].face_rect.width.ToString()+";"+ mu.results[0].face_rect.height;
    }

    /// <summary>人脸检测</summary>
    public string FaceDect(byte[] bytes)
    {
        string result = string.Empty;
        try
        {
            result = Youtu.detectface(bytes);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        return result;
    }

    /// <summary>人脸融合</summary>
    public void FaceMerge(byte[] bytes,RawImage rImg,Template template)
    {
        string result = string.Empty;
        try
        {
            result = Youtu.faceMerge(bytes, "base64", "[{\"cmd\":\"doFaceMerge\",\"params\":{\"model_id\":\"" + Youtu.GetTemplate(template) + "\"}}]");
            ShowRawimage(result, rImg);
        }
        catch (System.Exception e)
        {
            textLog.text = e.Message;
        }
    }

    /// <summary>人脸追踪</summary>
    public void FaceTracking(byte[] bytes)
    {
        string result = string.Empty;
        try
        {
            result = Youtu.faceshape(bytes);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void ShowRawimage(string json, RawImage raw)
    {
        JsonParse.FaceMerge faceMerge = JsonParse.FaceMerge.ParseJsonFaceMerge(json);
        if (faceMerge.img_base64 != null)
        {
            raw.texture = SCY.Utility.Base64Img(faceMerge.img_base64);
            //raw.SetNativeSize();
        }
        else if (faceMerge.img_url != null)
        {
            Debug.Log("图片地址");
            Application.OpenURL(faceMerge.img_url);
        }
        //textLog.text += faceMerge.msg;
    }


    void LoadTextFromBase64(string base64, RawImage rawImage)
    {
        Texture2D pic = new Texture2D(500, 500);
        byte[] data = System.Convert.FromBase64String(base64);
        pic.LoadImage(data);

        rawImage.texture = pic;
        rawImage.SetNativeSize();
        rawImage.transform.localScale = Vector3.one * 0.3f;
    }

    public string GetGenderStr(int gender)
    {
        if (gender > 50)
            return "男";
        else return "女";
    }

    public string GetExpressionStr(int expression)
    {
        if (expression < 35)
            return "冷漠";
        else if (expression > 65)
            return "大笑";
        else
            return "微笑";
    }
}
