using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GestureRecognizer;

public class DrawManager : MonoBehaviour
{
    public GameObject cursor;
    public GameObject stampPrefab;
    public GameObject linePrefab;

    private Tool currentTool;
    private Line currentLine;
    public Gradient color;
    public Material material;
    public Texture2D tex;

    public Transform drawCanvas;
    public History history;
    public Camera drawCamera;
    public int currentSortingOrder;
    public bool isForbid;

    public Image bg;
    private Vector3 virtualKeyPosition;
    public RectTransform clampRect;


    private void Start()
    {
        currentTool = new Tool();
        currentTool.width = 1;
        currentTool.color = color;
        currentTool.textureMode = LineTextureMode.Stretch;
        currentTool.material = material;
    }

    private void Update()
    {
        
        if (Utility.IsTouchDevice())
        {
            if (Input.touchCount > 0)
                virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
        }
        else
        {
            //这里不需要按下就可以检测
            virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        }

        //画笔显示
        //if (RectTransformUtility.RectangleContainsScreenPoint(clampRect, virtualKeyPosition, Camera.main))
        //{
        //    DrawCursor(GetCurrentPlatformClickPosition(Camera.main));
        //}

        //else
        //ReleaseAll();
        /*---关闭区域选择的方式，用模板测试的方法来做，更流畅------------------------------------------------*/
        Draw();

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            CreatPoints();
        }
    }

    private void Draw()
    {
        if (isForbid)
            return;

        if (currentTool.toolFeature == Tool.ToolFeature.Line)
        {
            UseLineFeature();
        }
        else if (currentTool.toolFeature == Tool.ToolFeature.Stamp)
        {
            UseStampFeature();
        }
        else if (currentTool.toolFeature == Tool.ToolFeature.Fill)
        {
            UseFillFeature();
        }

        if (currentLine != null)
        {
            currentLine.AddPoint(GetCurrentPlatformClickPosition(drawCamera));
        }
    }

    public void ReleaseAll()
    {
        OnClickRelease();
        OnStampRelease();
        OnFillRelease();
    }

    #region 线条
    private void UseLineFeature()
    {
        if (Input.touchCount != 0)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    OnClickBegan();
                else if (touch.phase == TouchPhase.Ended)
                    OnClickRelease();
            }
            else
                currentLine = null;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                OnClickBegan();
            else if (Input.GetMouseButtonUp(0))
                OnClickRelease();
        }
    }

    private void OnClickBegan()
    {
        GameObject line = Instantiate(linePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        currentLine = line.GetComponent<Line>();

        line.name = "Line";
        line.transform.SetParent(drawCanvas);
        line.transform.localPosition = Vector3.zero;
        line.layer = drawCanvas.gameObject.layer;

        if (currentTool.repeatTexture)
        {
            currentLine.SetMaterial(new Material(Shader.Find("Sprites/Default")));
            currentLine.material.mainTexture = currentTool.texture;
            currentLine.lineRenderer.numCapVertices = 0;//圆角
        }
        else
        {
            currentLine.SetMaterial(currentTool.material);
        }

        currentLine.material.mainTexture = currentTool.texture;
        currentLine.SetWidth(currentTool.width, currentTool.width);
        currentLine.SetColor(currentTool.color);
        currentLine.SetNumCapVertices(currentTool.endCapVertices);
        currentLine.lineRenderer.textureMode = currentTool.textureMode;
        currentLine.createPaintLines = currentTool.creatPaintLine;

        currentSortingOrder++;
        currentLine.SetSortingOrder(currentSortingOrder + 1);//防止第一笔被bg遮挡

        AddToHistory(line);
    }

    public void OnClickRelease()
    {
        if (currentLine != null)
        {
            if (changWriteStyle)
                SetCurrentLineWidthCurve();
            if (currentLine.GetPointsCount() == 0)
            {
                Destroy(currentLine.gameObject);
            }
            else if (currentLine.GetPointsCount() == 1 || currentLine.GetPointsCount() == 2)
            {
                currentLine.lineRenderer.SetVertexCount(2);
                currentLine.lineRenderer.SetPosition(0, currentLine.points[0]);
                currentLine.lineRenderer.SetPosition(1, currentLine.points[0] - new Vector3(0.015f, 0.015f, 0));
            }
            Destroy(currentLine);
        }
    }


    #region 笔锋算法,根据向量的角度设置宽度

    public bool changWriteStyle;
    private List<int> dirList = new List<int>();
    Vector3 dir;

    public float maxAngle = 30;
    public int headRemoveIndex = 5;
    public int endCreatIndex = 3;
    public float maxWidth = 1;
    public float minWidth = 0.9f;
    public float endWidth = 0.3f;
    public int moveFromIndex = 3;//整体前移多少位来调整波峰波谷

    private void SetCurrentLineWidthCurve()
    {
        dirList.Clear();
        LineRenderer lr = currentLine.lineRenderer;
        GetDirList(lr, 1, 0);
        for (int i = 0; i < dirList.Count; i++)
        {
            if (dirList[i] < headRemoveIndex)
            {
                dirList.Remove(dirList[i]);//移除开始时手抖创建的点
            }
        }

        AnimationCurve curve = new AnimationCurve();
        //首尾key
        curve.AddKey(0, currentLine.endWidth);
        curve.AddKey(1, currentLine.endWidth * endWidth);

        for (int i = 0; i < dirList.Count; i++)
        {
            float time = (float)(dirList[i] - moveFromIndex) / (float)lr.positionCount;

            curve.AddKey(time, lr.endWidth * minWidth);

            //在波谷后面创建波峰
            if (dirList[i] - (moveFromIndex - 1) < lr.positionCount - endCreatIndex)//最后多少位不创建key,不用担心小于0的设置,因为小于0默认没有
            {
                float time1 = (float)(dirList[i] - (moveFromIndex - 1)) / (float)lr.positionCount;
                curve.AddKey(time1, lr.endWidth * maxWidth);
            }
        }

        //重新设置key只能重写不能直接设置
        for (int i = 0; i < curve.keys.Length; i++)
        {
            Keyframe k = new Keyframe();
            k.tangentMode = 1;
            k.time = curve.keys[i].time;
            k.value = curve.keys[i].value;
            curve.MoveKey(i, k);
        }
        lr.widthCurve = curve;
    }

    private void GetDirList(LineRenderer lr, int nextIndex, int lastIndex)
    {
        if (nextIndex >= lr.positionCount)
            return;
        dir = (lr.GetPosition(nextIndex) - lr.GetPosition(lastIndex)).normalized;
        for (int i = nextIndex + 1; i < lr.positionCount; i++)
        {
            Vector3 dir1 = (lr.GetPosition(i) - lr.GetPosition(lastIndex)).normalized;
            float angle = Vector3.Angle(dir, dir1);
            if (angle > maxAngle)
            {
                dirList.Add(i);
                if (i + 1 < lr.positionCount)
                    GetDirList(lr, i + 1, i);
                break;
            }
        }
    }

    private void CreatPoints()
    {
        #region creat visual points
        //LineRenderer lr = currentLine.lineRenderer;
        //for (int i = 0; i < lr.positionCount; i++)
        //{
        //    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    go.name = i.ToString();
        //    go.transform.SetParent(lr.transform);
        //    Vector3 pos = lr.GetPosition(i);
        //    go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        //    go.transform.localScale = Vector3.one * 0.5f;
        //    go.layer = lr.gameObject.layer;
        //}
        #endregion
        CreatTurningPoints();
    }

    private void CreatTurningPoints()
    {
        LineRenderer lr;
        int count = history.GetPoolSize();
        if (count >= 2)
        {
            for (int i = 0; i < history.GetPoolSize(); i++)
            {
                lr = history.GetPool()[i].lineRender;
                if (i >= 1)
                {
                    LineRenderer lastLr = history.GetPool()[i - 1].lineRender;
                    lr.SetPosition(0, lastLr.GetPosition(lastLr.positionCount - 1));
                }
                if (i % 2 == 0)
                    SetAnimationCurve(lr, .5f, 0.2f);
                else
                    SetAnimationCurve(lr, 0.2f, .5f);
            }
        }
        else
        {
            lr = currentLine.lineRenderer;
            SetAnimationCurve(lr, .5f, 0.2f);
        }
    }

    private void SetAnimationCurve(LineRenderer lr, float startKey, float lastKey)
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, startKey);
        curve.AddKey(1, lastKey);
        lr.widthCurve = curve;
    }

    #endregion

    #endregion

    #region 填充
    private void UseFillFeature()
    {
        if (Input.touchCount != 0)//touch
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                OnFillBegan();
            else if (touch.phase == TouchPhase.Ended)
                OnFillRelease();
        }
        else//mouse
        {
            if (Input.GetMouseButtonDown(0))
                OnFillBegan();
            else if (Input.GetMouseButtonUp(0))
                OnFillRelease();
        }
    }

    private void OnFillBegan()
    {
        RaycastHit2D hit2d = Physics2D.Raycast(GetCurrentPlatformClickPosition(drawCamera), Vector2.zero);
        if (hit2d.collider != null)
        {
            SpriteRenderer spr = hit2d.collider.GetComponent<SpriteRenderer>();
            if (spr != null)
            {
                spr.color = Color.red;
                currentSortingOrder++;
                spr.sortingOrder = currentSortingOrder;

                AddToHistory(hit2d.collider.gameObject);
            }
        }
    }

    private void OnFillRelease() { }

    #endregion

    #region 印记
    private void UseStampFeature()
    {
        if (Input.touchCount != 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                OnStampBegan();
            else if (touch.phase == TouchPhase.Ended)
                OnStampRelease();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                OnStampBegan();
            else if (Input.GetMouseButtonUp(0))
                OnStampRelease();
        }
    }

    private void OnStampBegan()
    {
        GameObject stamp = Instantiate(stampPrefab, GetCurrentPlatformClickPosition(drawCamera), Quaternion.identity) as GameObject;
        stamp.name = "Stamp";
        float width = currentTool.width;
        stamp.transform.localScale = new Vector3(width, width, 0);
        stamp.transform.SetParent(drawCanvas);
        stamp.layer = drawCanvas.gameObject.layer;
        stamp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-15, 15)));
        Vector3 tempPos = stamp.GetComponent<RectTransform>().anchoredPosition3D;
        tempPos.z = 0;
        stamp.GetComponent<RectTransform>().anchoredPosition3D = tempPos;

        currentSortingOrder++;
        SpriteRenderer sr = stamp.GetComponent<SpriteRenderer>();
        sr.sortingOrder = currentSortingOrder;

        AddToHistory(stamp);
    }

    private void OnStampRelease() { }

    #endregion

    #region 保存操作用来撤销和重做
    private void AddToHistory(GameObject go)
    {
        History.Element element = new History.Element();
        element.transform = go.transform;
        element.type = History.Element.EType.Object;
        element.sortingOrder = currentSortingOrder;
        element.lineRender = currentLine.lineRenderer;
        history.AddToPool(element);
    }
    #endregion

    public Vector3 cursorOffset;

    #region 获取绘制位置
    private void DrawCursor(Vector3 clickPosition)
    {
        if (cursor == null || !cursor.activeInHierarchy)
            return;
        cursor.transform.position = clickPosition + cursorOffset;
    }

    private Vector3 GetCurrentPlatformClickPosition(Camera camera)
    {
        Vector3 clickPosition = Vector3.zero;

        if (Application.isMobilePlatform)
        {
            if (Input.touchCount != 0)
            {
                Touch touch = Input.GetTouch(0);
                clickPosition = touch.position;
            }
        }
        else
        {
            clickPosition = Input.mousePosition;
        }
        clickPosition = camera.ScreenToWorldPoint(clickPosition);//get click position in the world space
        clickPosition.z = 0;
        return clickPosition;
    }
    #endregion

    #region 工具方法
    public void SelectTool(Tool tool)
    {
        cursor.GetComponent<SpriteRenderer>().sprite = tool.transform.GetComponent<Image>().sprite;
        currentTool = tool;
        ChangeMaterialTexture(currentTool.texture);
    }

    public void Delete()
    {
        history.CleanPool();
        foreach (Transform child in drawCanvas)
        {
            if (child.tag != "bg")
                Destroy(child.gameObject);
        }
        currentSortingOrder = 0;
    }

    public void ChangeSize(Tool tool)
    {
        currentTool.width = tool.width;
    }

    public void ChangeMaterialTexture(Texture matTex)
    {
        if (matTex == null)
            return;
        currentTool.material.mainTexture = matTex;
    }

    public void ChangColor(Sprite spr)
    {
        if (spr == null)
            return;
        Color color = spr.texture.GetPixel((int)spr.pivot.x / 2, (int)spr.pivot.y / 2);//取图片中间的颜色
        Gradient gradient = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0].time = 0.0F;
        gck[0].color = color;
        gck[1].time = 1.0F;
        gck[1].color = color;
        GradientAlphaKey[] gak = new GradientAlphaKey[2];
        gak[0].time = 0.0F;
        gak[0].alpha = 1.0F;
        gak[1].time = 1.0F;
        gak[1].alpha = 1.0F;
        gradient.SetKeys(gck, gak);
        currentTool.color = gradient;
    }

    public void ChangColor(Color color)
    {
        Gradient gradient = currentTool.color;
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0].time = 0.0F;
        gck[0].color = color;
        gck[1].time = 1.0F;
        gck[1].color = color;
        GradientAlphaKey[] gak = gradient.alphaKeys;
        gradient.SetKeys(gck, gak);
        currentTool.color = gradient;
    }

    public void ChangBg(Tool tool)
    {
        if (tool.bgSpr == null)
            return;
        bg.sprite = tool.bgSpr;
        bg.SetNativeSize();
    }

    public void ChangAlpha(float value)
    {
        Gradient gradient = currentTool.color;
        GradientColorKey[] gck = gradient.colorKeys;
        GradientAlphaKey[] gak = new GradientAlphaKey[2];
        gak[0].time = 0.0F;
        gak[0].alpha = value;
        gak[1].time = 1.0F;
        gak[1].alpha = value;
        gradient.SetKeys(gck, gak);
        currentTool.color = gradient;
    }

    public void PrintScreen() { }

    public void ShowCursor(bool active)
    {
        if (cursor == null)
            return;
        cursor.SetActive(active);
    }

    #endregion
}
