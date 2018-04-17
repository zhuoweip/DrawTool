using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GetCamera : MonoBehaviour
{
    public Camera mCamera;                            
    public GameObject reviewMsgUI;                      //留言回顾UI
    public Transform startPoint;
    public Transform endPoint;
    public RawImage cameraImage;                      //获取摄像机投影
    public GameObject countDownImg;                   //倒计时图片
    public Sprite[] timingSprite;                     //倒计时序列帧
    public GameObject photo;                          //截取拍摄照片
    WebCamTexture camTexture;
    public QR_Code qr;

    int w, h;
    Vector3 v1, v2;
    bool Isget = false;
    public int suncount = 0;

    //控制翻页笔
    bool patBool = false;
    bool up;
    bool down;

    private void OnEnable()
    {
        up = true;
        down = false;
    }

    private bool isOpenCameraDevice;
    private void Awake()
    {
        //patBool = bool.Parse(LoadXml.ReadXml("Within", "patBool"));
        isOpenCameraDevice = bool.Parse(Configs.Instance.LoadText("开启摄像头", "false/true"));
    }

    // Use this for initialization
    void Start()
    {
        InitData();
    }

    public void Update()
    {
        if (patBool)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.PageUp) && up)
        {
            up = false;
            down = true;
            TakePhoto();
            Debug.Log("Up");
        }

        if (Input.GetKeyDown(KeyCode.PageDown) && down)
        {

            up = true;
            down = false;
            RetakePhoto();
            Debug.Log("Down");
        }
    }

    void InitData()
    {
        StartCoroutine(LoadWWWAllPicture());
        // 设备不同的坐标转换
#if UNITY_IOS || UNITY_IPHONE
        img.transform.Rotate (new Vector3 (0, 180, 90));
#elif UNITY_ANDROID
        cameraImage.transform.Rotate(new Vector3(0, 0, 90));
#endif
        if (WebCamTexture.devices.Length != 0 && isOpenCameraDevice)
        {
            cameraImage.gameObject.SetActive(true);
            StartCoroutine(CallCamera());
        }
        else
        {
            //无摄像头或者手动关闭摄像头
        }
    }

    IEnumerator CallCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (camTexture != null)
                camTexture.Stop();

            WebCamDevice[] cameraDevices = WebCamTexture.devices;
            string deviceName = cameraDevices[0].name;

            camTexture = new WebCamTexture(deviceName, Screen.height, Screen.width, 60);
            cameraImage.texture = camTexture;

            camTexture.Play();
        }
    }

    //拍照
    public void TakePhoto()
    {
        Timing.Instance.Ztime();

        if (!Isget)
        {
            Isget = true;
            StartCoroutine(GetTexture2d());
        }
        up = false;
        down = true;
    }

    //重拍
    public void RetakePhoto()
    {
        ReGet();
    }

    //重新截图
    public void ReGet()
    {
        Timing.Instance.Ztime();

        if (Isget)
        {
            Isget = false;
            photo.SetActive(false);
        }
        up = true;
        down = false;
    }


    //保存图片
    public void Save()
    {
        Timing.Instance.Ztime();
        v1 = mCamera.WorldToScreenPoint(startPoint.position);
        v2 = mCamera.WorldToScreenPoint(endPoint.position);
        //计算鼠标划定范围的长和宽~~
        //Debug.Log(Mathf.Abs(v1.x - v2.x).ToString("F0"));
        //Debug.Log(Mathf.Abs(v1.y - v2.y).ToString("F0"));
        w = int.Parse(Mathf.Abs(v1.x - v2.x).ToString("F0"));
        h = int.Parse(Mathf.Abs(v1.y - v2.y).ToString("F0"));
        StartCoroutine(GetCapture());
    }

    IEnumerator GetCapture()
    {
        //等待所有的摄像机跟GUI渲染完成
        yield return new WaitForEndOfFrame();

        photo.SetActive(false);

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        //----------------------------------------------------------------------------计算区域----------------------------------------------------
        float vx = (v1.x > v2.x) ? v2.x : v1.x;                                 //取较小的x,y作为起始点
        float vy = (v1.y > v2.y) ? v2.y : v1.y;
        tex.ReadPixels(new Rect(vx, vy, w, h), 0, 0, true);
        //-----------------------------------------------------------------------------------------------------------------------------------------
        //byte[] imagebytes = tex.EncodeToPNG();//转化为png图
        byte[] imagebytes = tex.EncodeToJPG(50);//转化为jpg图,可以压缩20倍左右
        tex.Compress(true);//对屏幕缓存进行压缩    

        string timestamp = QR_Code.GetTimeStamp().ToString();

        string filename = Application.streamingAssetsPath + "/Imags/" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + timestamp + ".png";
        Debug.Log(string.Format("截屏了一张照片: {0}", filename));
        File.WriteAllBytes(filename, imagebytes);



        StartCoroutine(LoadWWWAllPicture());
        qr.UpLoad(imagebytes, timestamp);

        //GameObject vc = GameObject.Find("VectorCanvas");

        //if (vc != null)
        //{
        //    qr.hidecan = vc;
        //    qr.hidecan.SetActive(false);
        //}
    }

    List<Texture2D> allTex2d = new List<Texture2D>();
    Hashtable ht = new Hashtable();
    public IEnumerator LoadWWWAllPicture()
    {
        allTex2d.Clear();
        string streamingPath = Application.streamingAssetsPath;
        DirectoryInfo dir = new DirectoryInfo(streamingPath + "/Imags/");//初始化一个DirectoryInfo类的对象
        GetAllFiles(dir);
        double startTime = (double)Time.time;
        foreach (DictionaryEntry de in ht)
        {
            WWW www = new WWW("file://" + streamingPath + "/" + de.Key);
            yield return www;
            if (www != null)
            {
                Texture2D tmp = www.texture;
                tmp.name = de.Key.ToString();
                allTex2d.Add(tmp);
            }
            if (www.isDone)
            {
                www.Dispose();
            }
        }
    }

    /// <summary>  
    /// 获取贴图
    /// </summary>  
    IEnumerator GetTexture2d()
    {
        if (timingSprite.Length != 0)
        {
            for (int i = 0; i < timingSprite.Length; i++)
            {
                countDownImg.GetComponent<Image>().sprite = timingSprite[i];
                yield return new WaitForSeconds(1);
            }
        }

        yield return new WaitForEndOfFrame();
        //把图片数据转换为byte数组  
        Texture2D texture = new Texture2D(camTexture.width, camTexture.height);
        int y = 0;
        while (y < texture.height)
        {
            int x = 0;
            while (x < texture.width)
            {
                Color color = camTexture.GetPixel(x, y);
                texture.SetPixel(x, y, color);
                ++x;
            }
            ++y;
        }
        texture.Apply();

        //创建Sprite
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //处理方法 
        photo.GetComponent<Image>().sprite = sprite;
        photo.SetActive(true);
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

                string creatTime = i.CreationTime.ToShortDateString();
                DestoryHistoryImgs(creatTime,strType, i);
            }
        }
    }

    /// <summary>
    /// 删除历史图片
    /// </summary>
    private void DestoryHistoryImgs(string _creatTime,string _strType, FileSystemInfo file)
    {
        int lerpTime = 0;
        int maxDay = 60;//删除60天之前的图片

        //用图片名获取日期
        //string[] str = _strType.Split('-');
        //int strYearNum = int.Parse(str[0].Substring(7,str[0].Length - 7));//去除前面Images文件夹路径
        //int strMonthNum = int.Parse(str[1]);
        //int strDayNum = int.Parse(str[2]);

        //获取图片创建日期
        string[] str = _creatTime.Split('/');
        int strYearNum = int.Parse(str[2]);
        int strMonthNum = int.Parse(str[1]);
        int strDayNum = int.Parse(str[0]);


        string nowDate = DateTime.Now.ToString("yyyy-MM-dd");
        string[] nowDateStr = nowDate.Split('-');
        int nowYearNum = int.Parse(nowDateStr[0]);
        int nowMonthNum = int.Parse(nowDateStr[1]);
        int nowDayNuM = int.Parse(nowDateStr[2]);

        if (nowYearNum == strYearNum)//年份相同
        {
            if (nowMonthNum == strMonthNum)//月份相同
            {
                lerpTime = nowDayNuM - strDayNum;
            }
            else if (nowMonthNum > strMonthNum)
            {
                AddMonthLerpTime(ref lerpTime, nowYearNum, strYearNum, nowMonthNum, strMonthNum);
                lerpTime = lerpTime - strDayNum + nowDayNuM;
            }
        }
        else if (nowYearNum > strYearNum)//跨年
        {
            AddYearLerpTime(ref lerpTime, nowYearNum, strYearNum, nowMonthNum, strMonthNum);
            lerpTime = lerpTime - strDayNum + nowDayNuM;
        }

        if (lerpTime >= maxDay)
        {
            file.Delete();
            file = null;
        }
        else
        {
            AddStrToHash(_strType);
        }
    }

    private void AddStrToHash(string _strType)
    {
        if (_strType.Substring(_strType.Length - 3).ToLower() == "png")
        {
            if (ht.Contains(_strType))
            {
                ht[_strType] = _strType;
            }
            else
            {
                ht.Add(_strType, _strType);
            }
        }
    }


    //每年平年闰年加的总天数不一样
    private void AddYearLerpTime(ref int lerpTime, int _nowYearNum, int _stryearNum, int _nowMonthNum, int _strMonthNum)
    {
        if (_stryearNum < _nowYearNum)
        {
            if (_strMonthNum <= 12)
            {
                lerpTime += GetMonthLerp(_stryearNum, _strMonthNum);
                _strMonthNum++;
                AddYearLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
            else
            {
                _stryearNum++;
                _strMonthNum = 1;//重置月份
                AddYearLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
        }
        else
        {
            if (_strMonthNum < _nowMonthNum)
            {
                AddMonthLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
        }
    }

    private void AddMonthLerpTime(ref int lerpTime, int _nowYearNum, int _stryearNum, int _nowMonthNum , int _strMonthNum)
    {
        if (_strMonthNum < _nowMonthNum)
        {
            lerpTime += GetMonthLerp(_stryearNum, _strMonthNum);
            _strMonthNum++;
            AddMonthLerpTime(ref lerpTime, _nowYearNum,_stryearNum, _nowMonthNum, _strMonthNum);
        }
    }


    private int GetMonthLerp(int yearNum,int monthNum)
    {
        int monthLerp = 0;
        if (monthNum == 2)
        {
            if (yearNum % 4 == 0)//闰年
                monthLerp = 29;
            else
                monthLerp = 28;
        }
        else if (monthNum == 1 || monthNum == 3 || monthNum == 5 
            || monthNum == 7 || monthNum == 8 || monthNum == 10 || monthNum == 12) 
        {
            monthLerp = 31;
        }
        else
        {
            monthLerp = 30;
        }

        return monthLerp;
    }


    //留言回顾
    public void ImgGet()
    {
        Timing.Instance.Stop();
        //cameraImage.gameObject.SetActive(false);
        reviewMsgUI.SetActive(true);

        ImageMove[] objs = reviewMsgUI.transform.Find("Top").GetComponentsInChildren<ImageMove>();
        foreach (var item in objs)
        {
            Destroy(item.gameObject);
        }
        int index = 0;

        if (allTex2d.Count == 0)
        {
            StartCoroutine(LoadWWWAllPicture());
            if (allTex2d.Count == 0)
                return;
        }

        Vector3 startpos = reviewMsgUI.transform.Find("Top").transform.localPosition + new Vector3(-610, 270);
        suncount = allTex2d.Count / 3;
        int count = allTex2d.Count;
        if (count < 21)
        {
            count = 21;
            suncount = count / 3;
        }
        //print(count);
        for (int z = 0; z < count; z++)
        {
            GameObject obj = new GameObject();
            obj.gameObject.name = "item";
            obj.AddComponent<Image>();
            obj.transform.SetParent(reviewMsgUI.transform.Find("Top").transform, false);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.AddComponent<ImageMove>();
            Texture2D tmp = allTex2d[z > allTex2d.Count - 1 ? UnityEngine.Random.Range(0, allTex2d.Count) : z];
            tmp.filterMode = FilterMode.Point;
            Sprite sprite = Sprite.Create(tmp, new Rect(0, 0, tmp.width, tmp.height), new Vector2(1f, 1f));

            obj.GetComponent<Image>().sprite = sprite;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector3(1296, 864);
            obj.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f);
            if (z / 3 != index)
            {
                //Debug.Log(z / 4);
                index = z / 3;
                startpos = reviewMsgUI.transform.Find("Top").transform.localPosition + new Vector3(-610 + index * 416, 270);
            }
            obj.transform.localPosition = startpos;
            startpos = startpos + new Vector3(0, -274);
        }
    }
}
