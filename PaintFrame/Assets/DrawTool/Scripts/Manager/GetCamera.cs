using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public enum LoadType
{
    WWW,
    IO
}

public class GetCamera : MonoBehaviour
{
    public LoadType loadType = LoadType.IO;

    public Camera mCamera;
    public GameObject reviewMsgUI;                    //留言回顾UI
    public Button returnBtn;                          //留言回顾关闭按钮
    public Transform startPoint;
    public Transform endPoint;
    public RawImage cameraImage;                      //获取摄像机投影
    public GameObject countDownImg;                   //倒计时图片
    public Sprite[] timingSprite;                     //倒计时序列帧
    public RawImage photo;                          //截取拍摄照片

    public Text genderText;                           //性别
    public Text ageText;                              //年龄
    public Text scoreText;                            //评分
    public Text expressionText;                            //表情
    public Text glassText;                            //是否带眼睛
    public Text raceText;                             //人种

    private WebCamTexture camTexture;
    private JObject detectInfo;
    private string result;

    private int width = 640;
    private int height = 480;

    public QR_Code qr;
    private DrawManager drawManager;
    private int maxDay;
    int w, h;
    Vector3 v1, v2;
    bool Isget = false;
    public int suncount = 0;

    private bool isOpenCameraDevice;
    private bool isOpenAI;
    private void Awake()
    {
        drawManager = GameObject.FindObjectOfType<DrawManager>();
        isOpenCameraDevice = bool.Parse(Configs.Instance.LoadText("开启摄像头", "false/true"));
        maxDay = int.Parse(Configs.Instance.LoadText("本地相片保存期限", "day"));
        isOpenAI = bool.Parse(Configs.Instance.LoadText("开启人脸识别", "openAI"));
    }

    // Use this for initialization
    void Start()
    {
        InitData();
        InitWebCam();
    }

    public void Update()
    {
        if (!drawManager.isForbid && reviewMsgUI.activeInHierarchy)
        {
            drawManager.isForbid = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isShowGUI = !isShowGUI;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            TakePhoto();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RetakePhoto();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            //FaceDetector.Instance.FaceCreatGroup();
            //byte[] bytes = GetPhotoPixel(camTexture); 
            //Debug.Log(FaceDetector.Instance.FaceMatch(bytes));
        }

        if (isOpenAI && camTexture != null)
        {
            //GetPhotoPixel(camTexture);
            //FacialRecognition.Instance.FaceTracking(GetPhotoPixel(camTexture));
            //FaceDetector.Instance.FaceDetect(GetPhotoPixel(camTexture));
        }
    }

    [HideInInspector]
    public static int Index;
    TObjectPool<GameObjData> Tpool = new TObjectPool<GameObjData>(0, GameObjData.CreateObj, GameObjData.ReleaseObj, 0, null);
    public Transform picPoolTrans;

    void InitData()
    {
        if (loadType == LoadType.IO)
            LoadAllPicture();
        else
            StartCoroutine(LoadWWWAllPicture());

        int count = allTex2d.Count < minCount ? minCount : allTex2d.Count;
        Tpool = new TObjectPool<GameObjData>(count, GameObjData.CreateObj, GameObjData.ReleaseObj, string.Empty, picPoolTrans, 0);
        for (int i = 0; i < count; i++)
            Tpool.OnActiveGameObject(pQueue);

        RegistBtn();
    }

    public void InitWebCam()
    {
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

    private void RegistBtn()
    {
        returnBtn.onClick.AddListener(() =>
        {
            lastCount = 0;
            reviewMsgUI.SetActive(false);
            drawManager.isForbid = false;
            //drawManager.ShowCursor(true);
        });
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

            camTexture = new WebCamTexture(deviceName, Screen.width, Screen.height, 60);
            cameraImage.texture = camTexture;

            camTexture.Play();
        }
    }

    //获取摄像头像素
    private byte[] GetPhotoPixel(WebCamTexture ca)
    {
        if (ca == null)
            return null;
        Texture2D texture = new Texture2D(ca.width, ca.height, TextureFormat.RGB24, true);
        texture.SetPixels(ca.GetPixels());
        return texture.EncodeToJPG();
    }

    //拍照
    public void TakePhoto()
    {
        if (!Isget)
        {
            Isget = true;
            StartCoroutine(GetTexture2d());
        }
    }

    //重拍
    public void RetakePhoto()
    {
        ReGet();
    }

    //重新截图
    public void ReGet()
    {
        if (Isget)
        {
            Isget = false;
            photo.gameObject.SetActive(false);
        }
    }

    //上传留言
    public void Save()
    {
        v1 = mCamera.WorldToScreenPoint(startPoint.position);
        v2 = mCamera.WorldToScreenPoint(endPoint.position);
        w = int.Parse(Mathf.Abs(v1.x - v2.x).ToString("F0"));
        h = int.Parse(Mathf.Abs(v1.y - v2.y).ToString("F0"));
        //计算鼠标划定范围的长和宽~~
        //Debug.Log(w);
        //Debug.Log(h);
        StartCoroutine(GetCapture());
    }

    IEnumerator GetCapture()
    {
        //等待所有的摄像机跟GUI渲染完成
        yield return new WaitForEndOfFrame();

        photo.gameObject.SetActive(false);

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
        //Debug.Log(string.Format("截屏了一张照片: {0}", filename));
        File.WriteAllBytes(filename, imagebytes);

        string picName = filename.Replace(Application.streamingAssetsPath, string.Empty).Replace('/', '\\');
        //Debug.LogError("filename = " + picName);
        LoadSinglePic(filename, picName);
        Tpool.OnActiveGameObject(pQueue);

        qr.UpLoad(imagebytes, timestamp, QR_Code.ServerType.CAN);
    }

    //加载单张本地图片
    private void LoadSinglePic(string path, string texName)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream);
        //int width = image.Width;
        //int height = image.Height;
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        int width = w;
        int height = h;
        Texture2D tmp = new Texture2D(width, height);
        tmp.LoadImage(bytes);
        tmp.name = texName;
        allTex2d.Add(tmp);
    }

    List<Texture2D> allTex2d = new List<Texture2D>();
    Hashtable ht = new Hashtable();

    /// <summary>
    /// WWW加载
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadWWWAllPicture()
    {
        allTex2d.Clear();
        string streamingPath = Application.streamingAssetsPath;
        string path = streamingPath + "/Imags";
        if (!File.Exists(path))
            Directory.CreateDirectory(path);
        DirectoryInfo dir = new DirectoryInfo(path + "/");//初始化一个DirectoryInfo类的对象
        FileManager.GetAllFiles(dir, maxDay, ht);
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
                Resources.UnloadUnusedAssets();
            }
        }
    }

    /// <summary>
    /// IO加载
    /// </summary>
    public void LoadAllPicture()
    {
        allTex2d.Clear();
        string streamingPath = Application.streamingAssetsPath;
        string path = streamingPath + "/Imags";
        if (!File.Exists(path))
            Directory.CreateDirectory(path);
        DirectoryInfo dir = new DirectoryInfo(path + "/");//初始化一个DirectoryInfo类的对象
        FileManager.GetAllFiles(dir, maxDay, ht);
        foreach (DictionaryEntry de in ht)
        {
            //Debug.LogError("de.Key = " + de.Key); 
            LoadSinglePic(streamingPath + "/" + de.Key, de.Key.ToString());
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
                countDownImg.GetComponent<UnityEngine.UI.Image>().sprite = timingSprite[i];
                yield return new WaitForSeconds(1);//倒计时
            }
        }

        yield return new WaitForEndOfFrame();
        //把图片数据转换为byte数组  
        Texture2D texture = new Texture2D(camTexture.width, camTexture.height);
        byte[] bytes = GetPhotoPixel(camTexture);
        texture.LoadImage(bytes);
        photo.texture = texture;
        if (isOpenAI && TcpManager.IsOnLine())//在线检测
        {
            result = AIManager.Instance.AIFaceDetect(bytes);
            //Debug.Log(result);
            if (AIManager.Instance.type == AIType.Tencent)
            {
                JsonParse.TencentFaceDetect de = JsonParse.TencentFaceDetect.ParseJsonFaceDetect(result);
                if (de.face != null && de.face.Length > 0)
                    ShowDetectInfo(de);
                AIManager.Instance.AIFaceMerge(bytes, photo);
            }
            else
            {
                JsonParse.BaiduFaceDectect de = JsonParse.BaiduFaceDectect.ParseJsonFaceDetect(result);
                if (de.result != null)
                    ShowDetectInfo(de);
                //FaceDetector.Instance.SignUpNewPerson(bytes);
            }
        }
        photo.gameObject.SetActive(true);
    }

    //public float leftEyeCenter_x, leftEyeCenter_y;
    //private float faceWidth, faceHeight;

    private void ShowDetectInfo(JsonParse.TencentFaceDetect de)
    {
        JsonParse.TencentFaceDetect.Faces face = de.face[0];
        string genderMsg = FacialRecognition.Instance.GetGenderStr(face.gender);
        string ageMsg = (face.age).ToString();
        string scoreMsg = (face.beauty).ToString();
        string expressionMsg = FacialRecognition.Instance.GetExpressionStr(face.expression);
        string glassesMsg = (face.glasses).ToString();
        string raceMsg = "null";
        SetInfoText(genderMsg, ageMsg, scoreMsg, expressionMsg, glassesMsg, raceMsg);
    }

    public RawImage tImg;
    public GameObject cube;

    private void ShowDetectInfo(JsonParse.BaiduFaceDectect de)
    {
        JsonParse.BaiduFaceDectect.Result.Face_list face = de.result.face_list[0];
        float leftEyeCenter_x = (float)face.landmark[0].x;
        //Debug.Log(leftEyeCenter_x);
        float leftEyeCenter_y = (float)face.landmark[0].y;
        float faceWidth = (float)face.location.width;
        float faceHeight = (float)face.location.height;
        float faceTop = (float)face.location.top;
        float faceLeft = (float)face.location.left;

        var x = leftEyeCenter_x + (faceWidth / 2);
        var y = leftEyeCenter_y + (faceHeight / 2);

        //Debug.Log(leftEyeCenter_y);
        cube.transform.localPosition = GetWorldPos(new Vector2(x, y), faceWidth, faceHeight, faceTop, faceLeft);

        Debug.Log(face.face_token);
        string genderMsg = face.gender.type == "male" ? "男" : "女";
        string ageMsg = face.age.ToString();
        string scoreMsg = face.beauty.ToString();
        string expressionMsg = FaceDetector.Instance.GetExpressionStr(face.expression.type);
        string glassesMsg = FaceDetector.Instance.GetGlassStr(face.glasses.type);
        string raceMsg = FaceDetector.Instance.GetRaceStr(face.race.type);
        SetInfoText(genderMsg, ageMsg, scoreMsg, expressionMsg, glassesMsg, raceMsg);
    }

    private void SetInfoText(string gender, string age, string score, string expression, string glasses, string race)
    {
        genderText.text = StringUtil.SetStringColor("性别", STRING_COLOR.Red) + StringUtil.SetStringColor(gender, STRING_COLOR.Blue);
        ageText.text = StringUtil.SetStringColor("年龄", STRING_COLOR.Red) + StringUtil.SetStringColor(age, STRING_COLOR.Yellow);
        scoreText.text = StringUtil.SetStringColor("颜值", STRING_COLOR.Red) + StringUtil.SetStringColor(score, STRING_COLOR.Yellow);
        expressionText.text = StringUtil.SetStringColor("表情", STRING_COLOR.Red) + StringUtil.SetStringColor(expression, STRING_COLOR.Yellow);
        glassText.text = StringUtil.SetStringColor("配戴眼镜", STRING_COLOR.Red) + StringUtil.SetStringColor(glasses, STRING_COLOR.Yellow);
        raceText.text = StringUtil.SetStringColor("人种", STRING_COLOR.Red) + StringUtil.SetStringColor(race, STRING_COLOR.Yellow);
    }

    private const int minCount = 21;
    private int lastCount;
    Queue<GameObjData> pQueue = new Queue<GameObjData>();

    //对象池对象隐藏
    private void InActiveGameObject()
    {
        if (pQueue.Count == 0)
            return;
        GameObjData obj = pQueue.Dequeue();
        obj.gameObject.SetActive(false);
        Tpool.InActiveGameObject(obj);
    }

    //留言回顾
    public void ImgGet()
    {
        float t = Time.realtimeSinceStartup;
        //cameraImage.gameObject.SetActive(false);
        reviewMsgUI.SetActive(true);
        drawManager.isForbid = true;

        int index = 0;

        if (allTex2d.Count == 0)
        {
            if (loadType == LoadType.IO)
                LoadAllPicture();
            else
                StartCoroutine(LoadWWWAllPicture());
        }

        allTex2d.Sort(IndexSort);

        float t1 = Time.realtimeSinceStartup - t;
        //Debug.LogError(t1);
        t = Time.realtimeSinceStartup;

        if (allTex2d.Count > 0)
        {
            Transform topTrans = reviewMsgUI.transform.Find("Top").transform;

            Vector3 startpos = topTrans.localPosition + new Vector3(-610, 270);
            suncount = allTex2d.Count / 3;
            int count = allTex2d.Count;
            if (count < minCount)
            {
                count = minCount;
                suncount = count / 3;
            }

            //Debug.Log(pPool.GetActiveObjectCount());
            for (int i = 0; i < Tpool.GetActiveObjectCount(); i++)
            {
                GameObjData objData = Tpool.GetActiveObject(i);
                GameObject obj = objData.gameObject;
                //obj.name = "item" + i;
                if (!obj.GetComponent<RawImage>())
                    obj.AddComponent<RawImage>();
                obj.transform.SetParent(topTrans, false);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;
                if (!obj.GetComponent<ImageMove>())
                    obj.AddComponent<ImageMove>();

                if (i - lastCount > allTex2d.Count - 1)
                {
                    lastCount = i;
                }
                //最近的图片在最前
                Texture2D tmp = allTex2d[i - lastCount];//allTex2d[i > allTex2d.Count - 1 ? UnityEngine.Random.Range(0, allTex2d.Count) : i]; //allTex2d[i];
                tmp.filterMode = FilterMode.Point;

                obj.GetComponent<RawImage>().texture = tmp;
                obj.GetComponent<RectTransform>().sizeDelta = new Vector3(1296, 864);
                obj.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f);
                if (i / 3 != index)
                {
                    index = i / 3;
                    startpos = topTrans.localPosition + new Vector3(-610 + index * 416, 270);
                }
                obj.transform.localPosition = startpos;
                startpos = startpos + new Vector3(0, -274);
            }

            float t2 = Time.realtimeSinceStartup - t;
            //Debug.LogError(t2);
        }
    }

    public static int IndexSort(Texture2D a, Texture2D b)
    {
        return string.Compare(b.name, a.name);
    }

    private void OnDestroy()
    {
        camTexture = null;
    }

    private Vector3 GetWorldPos(Vector2 v2,float texWidth,float texHeight,float texTop,float texLeft)
    {
        Vector2 tmp = v2;
        tmp = new Vector2(tmp.x + texLeft,tmp.y + texTop);
        tmp = mCamera.ScreenToWorldPoint(tmp);
        ////归一化
        //tmp = new Vector2(tmp.x / texWidth, tmp.y / texHeight);
        return tmp;
    }

    private Vector3 GetWorldPos(Vector2 v2)
    {
        v2.x = v2.x * Screen.width / width;
        v2.y = v2.y * Screen.height / height;

        Vector3 v3 = mCamera.ScreenToWorldPoint(v2);
        v3.y *= -1;
        v3.z = 0;
        return v3;
    }

    private bool isShowGUI;
    Vector3 mousePositionOnScreen;
    Vector3 mousePositionInWorld;
    private void OnGUI()
    {
        if (!isShowGUI)
            return;
        //获取鼠标在场景中坐标
        mousePositionOnScreen = Input.mousePosition;
        //让场景中的Z=鼠标坐标的Z
        mousePositionOnScreen.z = 0;
        //将相机中的坐标转化为世界坐标
        mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);

        GUIStyle s = new GUIStyle();
        s.fontSize = 50;
        s.normal.textColor = UnityEngine.Color.white;
        GUI.Label(new Rect(50, 50, 100, 100), mousePositionOnScreen.ToString(), s);
        GUI.Label(new Rect(50, 150, 100, 100), mousePositionInWorld.ToString(), s);
    }
}
