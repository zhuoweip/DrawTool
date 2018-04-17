using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class Extension
{
    /// <summary>
    /// 寻找子对象组件剔除父对象
    /// </summary>
    public static T[] GetComponentsInRealChildern<T>(this GameObject go)
    {
        List<T> TList = new List<T>();
        TList.AddRange(go.GetComponentsInChildren<T>());
        TList.RemoveAt(0);
        return TList.ToArray();
    }
}

public class ToolEditor : EditorWindow
{
    [MenuItem("DrawTool/ChooseTool")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ToolEditor));
        GetParent();
    }

    static void GetParent()
    {
        //GameObject[] objs = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] objs = scene.GetRootGameObjects();

        foreach (var item in objs)
        {
            if (item.name == "Tool")
            {
                parent = item.transform;
                return;
            }
            else
                FindChild(item);
        }
    }

    private static void FindChild(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform child = go.transform.GetChild(i);
            string name = child.name;
            if (name == "Tool")
            {
                parent = child.transform;
                Selection.activeGameObject = parent.gameObject;
                Debug.LogError(parent.name);
                return;
            }
            else
                FindChild(child.gameObject);
        }
    }

    private bool groupEnabled;
    private bool Pencil = false;
    private bool Erase = false;
    private bool Crayon = false;
    private bool Gradient = false;
    private bool Paint = false;
    private bool Spark = false;
    private bool Stamp = false;
    private bool PaintCollider = false;
    private bool Size = false;
    private bool Recovery = false;
    private bool Zoom = false;
    private bool Delete = false;
    private bool TakePhoto = false;
    private bool LeaveMsg = false;
    private bool ReviewMsg = false;
    private bool Color = false;
    private bool ColorAtla = false;
    private bool Alpha = false;

    private bool DeleteAllTool = false;

    public static Transform parent;

    private void OnGUI()
    {
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        Pencil = EditorGUILayout.Toggle("铅笔", Pencil);
        Erase = EditorGUILayout.Toggle("橡皮擦", Erase);
        Crayon = EditorGUILayout.Toggle("蜡笔", Crayon);
        Gradient = EditorGUILayout.Toggle("渐变笔", Gradient);
        Paint = EditorGUILayout.Toggle("油画笔", Paint);
        Spark = EditorGUILayout.Toggle("喷漆", Spark);
        Stamp = EditorGUILayout.Toggle("印记", Stamp);
        PaintCollider = EditorGUILayout.Toggle("填充", PaintCollider);

        Delete = EditorGUILayout.Toggle("删除", Delete);
        Size = EditorGUILayout.Toggle("尺寸", Size);
        Color = EditorGUILayout.Toggle("颜色", Color);
        ColorAtla = EditorGUILayout.Toggle("色卡", ColorAtla);
        Alpha = EditorGUILayout.Toggle("透明度", Alpha);
        Recovery = EditorGUILayout.Toggle("撤销还原", Recovery);
        Zoom = EditorGUILayout.Toggle("缩进", Zoom);
        TakePhoto = EditorGUILayout.Toggle("拍照", TakePhoto);
        LeaveMsg = EditorGUILayout.Toggle("上传留言", LeaveMsg);
        ReviewMsg = EditorGUILayout.Toggle("留言回顾", ReviewMsg);


        GUILayout.Label("", EditorStyles.boldLabel);
        DeleteAllTool = EditorGUILayout.Toggle("删除所有工具", DeleteAllTool);

        EditorGUILayout.EndToggleGroup();

        CreatAllTool();
        DeleteAll();
    }

    private void CreatAllTool()
    {
        CreatTool(ref Pencil, ref isChoosePencil, ref pencilStr, ref tPencil);
        CreatTool(ref Erase, ref isChooseErase, ref eraseStr, ref tErase);
        CreatTool(ref Crayon, ref isChooseCrayon, ref crayonStr, ref tCrayon);
        CreatTool(ref Gradient, ref isChooseGradient, ref gradientStr, ref tGradient);
        CreatTool(ref Paint, ref isChoosePaint, ref paintStr, ref tPaint);
        CreatTool(ref Spark, ref isChooseSpark, ref sparkStr, ref tSpark);
        CreatTool(ref Stamp, ref isChooseStamp, ref stampStr, ref tStamp);
        CreatTool(ref PaintCollider, ref isChoosePaintCollider, ref paintColliderStr, ref tPaintCollider);
        CreatTool(ref Delete, ref isChooseDelete, ref deleteStr, ref tDelete);
        CreatTool(ref Size, ref isChooseSize, ref sizeStr, ref tSize);
        CreatTool(ref Color, ref isChooseColor, ref colorStr, ref tColor);
        CreatTool(ref ColorAtla, ref isChooseColorAtla, ref colorAtlaStr, ref tColorAtla);
        CreatTool(ref Alpha, ref isChooseAlpha, ref alphaStr, ref tAlpha);
        CreatTool(ref Recovery, ref isChooseRecovery, ref recoveryStr, ref tRecovery);
        CreatTool(ref Zoom, ref isChooseZoom, ref zoomStr, ref tZoom);
        CreatTool(ref TakePhoto, ref isChooseTakePhoto, ref takePhotoStr, ref tTakePhoto);
        CreatTool(ref LeaveMsg, ref isChooseLeaveMsg, ref leaveMsgStr, ref tLeaveMsg);
        CreatTool(ref ReviewMsg, ref isChooseReviewMsg, ref reviewMsgStr, ref tReviewMsg);
    }


    #region value

    private string eraseStr = "Erase";
    private string pencilStr = "Pencil";
    private string crayonStr = "Crayon";
    private string gradientStr = "Gradient";
    private string paintStr = "Paint";
    private string sparkStr = "Spark";
    private string stampStr = "Stamp";
    private string paintColliderStr = "PaintCollider";
    private string deleteStr = "Delete";
    private string sizeStr = "Size";
    private string colorStr = "Color";
    private string colorAtlaStr = "ColorAtla";
    private string alphaStr = "Alpha";
    private string recoveryStr = "Recovery";
    private string zoomStr = "Zoom";
    private string takePhotoStr = "TakePhoto";
    private string leaveMsgStr = "LeaveMsg";
    private string reviewMsgStr = "ReviewMsg";


    private bool isChoosePencil;
    private bool isChooseErase;
    private bool isChooseCrayon;
    private bool isChooseGradient;
    private bool isChoosePaint;
    private bool isChooseSpark;
    private bool isChooseStamp;
    private bool isChoosePaintCollider;
    private bool isChooseDelete;
    private bool isChooseSize;
    private bool isChooseColor;
    private bool isChooseColorAtla;
    private bool isChooseAlpha;
    private bool isChooseRecovery;
    private bool isChooseZoom;
    private bool isChooseTakePhoto;
    private bool isChooseLeaveMsg;
    private bool isChooseReviewMsg;


    GameObject tPencil;
    GameObject tErase;
    GameObject tCrayon;
    GameObject tGradient;
    GameObject tPaint;
    GameObject tSpark;
    GameObject tStamp;
    GameObject tPaintCollider;
    GameObject tDelete;
    GameObject tSize;
    GameObject tColor;
    GameObject tColorAtla;
    GameObject tAlpha;
    GameObject tRecovery;
    GameObject tZoom;
    GameObject tTakePhoto;
    GameObject tLeaveMsg;
    GameObject tReviewMsg;



    #endregion

    public void CreatTool(ref bool toggle, ref bool isChooseTool, ref string toolName, ref GameObject tempTool)
    {
        if (toggle && !isChooseTool)
        {
            isChooseTool = !isChooseTool;

            //注意Project里面路径的更改，这边也要相应更改
            GameObject toolPrefab = AssetDatabase.LoadAssetAtPath("Assets/DrawTool/Prefab/Tool/" + toolName + ".prefab", typeof(GameObject)) as GameObject;
            GameObject tool = PrefabUtility.InstantiatePrefab(toolPrefab) as GameObject;

            tempTool = tool;
            SetPrefabData(tool);
        }
        else if (!toggle && isChooseTool)
        {
            isChooseTool = !isChooseTool;
            DestroyImmediate(tempTool);
        }
    }

    /// <summary>
    /// 删除所有工具
    /// </summary>
    private void DeleteAll()
    {
        if (DeleteAllTool)
        {
            Pencil = false;
            Erase = false;
            Crayon = false;
            Gradient = false;
            Paint = false;
            Spark = false;
            Stamp = false;
            PaintCollider = false;
            Size = false;
            Recovery = false;
            Zoom = false;
            Delete = false;
            TakePhoto = false;
            LeaveMsg = false;
            ReviewMsg = false;
            Color = false;
            ColorAtla = false;
            Alpha = false;

            DeleteAllTool = false;
        }
    }


    private void SetPrefabData(GameObject go)
    {
        if (parent == null)
            return;
        go.transform.SetParent(parent);
        go.layer = parent.gameObject.layer;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        Image img = go.GetComponent<Image>(); 
        if (img != null)
            img.SetNativeSize();
    }

    private void OnDestroy()
    {
        //Debug.LogError("close");
    }
}
