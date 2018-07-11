using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class tt : MonoBehaviour {

    public RawImage cameraImage;
    private WebCamTexture camTexture;
    public RawImage photo;

    // Use this for initialization
    void Start () {
        if (WebCamTexture.devices.Length != 0)
        {
            cameraImage.gameObject.SetActive(true);
            StartCoroutine(CallCamera());
        }
        //mCamera.orthographicSize = width * ((float)Screen.height / (float)Screen.width) / 2;
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

            camTexture = new WebCamTexture(deviceName, width, height, 60);
            cameraImage.texture = camTexture;

            camTexture.Play();
        }
    }

    private void OnDestroy()
    {
        if (camTexture != null)
        {
            camTexture.Stop();
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StopAllCoroutines();
            StartCoroutine(GetT());
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            photo.gameObject.SetActive(false);
        }
	}

    string result;
    IEnumerator GetT()
    {
        yield return new WaitForEndOfFrame();
        //byte[] bytes = File.ReadAllBytes(Application.streamingAssetsPath + "/" + "test1" + ".jpg");

        Texture2D texture = new Texture2D(camTexture.width, camTexture.height);
        Debug.Log(camTexture.width);
        Debug.Log(camTexture.height);
        byte[] bytes = GetPhotoPixel(camTexture);
        texture.LoadImage(bytes);
        photo.texture = texture;
        result = AIManager.Instance.AIFaceDetect(bytes);
        JsonParse.BaiduFaceDectect de = JsonParse.BaiduFaceDectect.ParseJsonFaceDetect(result);
        if (de.result != null)
            ShowDetectInfo(de);
        photo.gameObject.SetActive(true);
    }

    private float leftEyeCenter_x, leftEyeCenter_y;
    private float rightEyeCenter_x, rightEyeCenter_y;
    private float noseCenter_x, noseCenter_y;
    private float mouseCenter_x, mouseCenter_y;
    private float faceWidth, faceHeight;
    public GameObject cube1, cube2,cube3,cube4;


    private void ShowDetectInfo(JsonParse.BaiduFaceDectect de)
    {
        JsonParse.BaiduFaceDectect.Result.Face_list face = de.result.face_list[0];

        leftEyeCenter_x = (float)face.landmark[0].x;
        leftEyeCenter_y = (float)face.landmark[0].y;
        rightEyeCenter_x = (float)face.landmark[1].x;
        rightEyeCenter_y = (float)face.landmark[1].y;
        noseCenter_x = (float)face.landmark[2].x;
        noseCenter_y = (float)face.landmark[2].y;
        mouseCenter_x = (float)face.landmark[3].x;
        mouseCenter_y = (float)face.landmark[3].y;

        faceWidth = (float)face.location.width;
        faceHeight = (float)face.location.height;

        var x = leftEyeCenter_x + (faceWidth / 2)        ;
        var y = leftEyeCenter_y + (faceHeight / 2)       ;
                                                         
        var x1 = rightEyeCenter_x + (faceWidth / 2)      ;
        var y1 = rightEyeCenter_y + (faceHeight / 2)     ;
                                                         
        var x2 = noseCenter_x + (faceWidth / 2)          ;
        var y2 = noseCenter_y + (faceHeight / 2)         ;
                                                         
        var x3 = mouseCenter_x + (faceWidth / 2)         ;
        var y3 = mouseCenter_y + (faceHeight / 2)        ;
                                                                    
        //Debug.Log("x = " + x);
        //Debug.Log("y = " + y);
        //Debug.Log(leftEyeCenter_x);
        //Debug.Log(leftEyeCenter_y);
        cube1.transform.position = GetWorldPos(ref x, ref y);
        cube2.transform.position = GetWorldPos(ref x1, ref y1);
        cube3.transform.position = GetWorldPos(ref x2, ref y2);
        cube4.transform.position = GetWorldPos(ref x3, ref y3);

    }

    public Camera mCamera;
    public int width = 1280;
    public int height = 960;

    private Vector3 GetWorldPos(ref float x,ref float y)
    {
        Vector2 v2 = new Vector2(x, y);
        Debug.Log("Screen.width " + Screen.width);
        v2.x = v2.x * Screen.width / width;
        v2.y = v2.y * Screen.height / height;

        Vector3 v3 = mCamera.ScreenToWorldPoint(v2);
        v3.y *= -1;
        v3.z = 0;
        Debug.Log(v3.x);
        Debug.Log(v3.y);
        return v3;
    }

    private byte[] GetPhotoPixel(WebCamTexture ca)
    {
        if (ca == null)
            return null;
        Debug.Log("ca.width = " + ca.width);
        Texture2D texture = new Texture2D(ca.width, ca.height, TextureFormat.RGB24, true);
        texture.SetPixels(ca.GetPixels());
        return texture.EncodeToJPG();
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(leftEyeCenter_x, leftEyeCenter_y, 100, 100),"左眼");
        GUI.Box(new Rect(rightEyeCenter_x, rightEyeCenter_y, 100, 100), "右眼");
        GUI.Box(new Rect(noseCenter_x, noseCenter_y, 100, 100), "鼻子");
        GUI.Box(new Rect(mouseCenter_x, mouseCenter_y, 100, 100), "嘴");
    }
}
