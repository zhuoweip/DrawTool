using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Linq.Expressions;

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
    private bool groupEnabled;
    private bool Pencil = false;
    private bool Erase = false;
    private bool Crayon = false;
    private bool Gradient = false;
    private bool Brush = false;
    private bool BrushPen = false;
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

    [MenuItem("DrawTool/ChooseTool")]
    static void Init()
    {
        ToolEditor window = (ToolEditor)EditorWindow.GetWindow(typeof(ToolEditor));
        window.Show();
        GetParent();
    }

    static void GetParent()
    {
        prefabPath = string.Empty;

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
                return;
            }
            else
                FindChild(child.gameObject);
        }
    }

    private void OnGUI()
    {
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        Pencil = EditorGUILayout.Toggle("铅笔", Pencil);
        Erase = EditorGUILayout.Toggle("橡皮擦", Erase);
        Crayon = EditorGUILayout.Toggle("蜡笔", Crayon);
        Gradient = EditorGUILayout.Toggle("渐变笔", Gradient);
        Brush = EditorGUILayout.Toggle("刷子", Brush);
        BrushPen = EditorGUILayout.Toggle("毛笔", BrushPen);
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
        //DeleteAll();
    }

    private void CreatAllTool()
    {
        CreatTool(ref Pencil, ToolType.Pencil.ToString());
        CreatTool(ref Erase, ToolType.Erase.ToString());
        CreatTool(ref Crayon, ToolType.Crayon.ToString());
        CreatTool(ref Gradient, ToolType.Gradient.ToString());
        CreatTool(ref Brush, ToolType.Brush.ToString());
        CreatTool(ref BrushPen, ToolType.BrushPen.ToString());
        CreatTool(ref Paint, ToolType.Paint.ToString());
        CreatTool(ref Spark, ToolType.Spark.ToString());
        CreatTool(ref Stamp, ToolType.Stamp.ToString());
        CreatTool(ref PaintCollider, ToolType.PaintCollider.ToString());
        CreatTool(ref Delete, ToolType.Delete.ToString());
        CreatTool(ref Size, ToolType.Size.ToString());
        CreatTool(ref Color, ToolType.Color.ToString());
        CreatTool(ref ColorAtla, ToolType.ColorAtla.ToString());
        CreatTool(ref Alpha, ToolType.Alpha.ToString());
        CreatTool(ref Recovery, ToolType.Recovery.ToString());
        CreatTool(ref Zoom, ToolType.Zoom.ToString());
        CreatTool(ref TakePhoto, ToolType.TakePhoto.ToString());
        CreatTool(ref LeaveMsg, ToolType.LeaveMsg.ToString());
        CreatTool(ref ReviewMsg, ToolType.ReviewMsg.ToString());
    }

    #region value

    static Tuple<bool, GameObject> PencilTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> EraseTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> CrayonTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> GradientTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> BrushTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> BrushPenTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> PaintTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> SparkTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> StampTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> PaintColliderTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> DeleteTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> SizeTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> ColorTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> ColorAtlaTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> AlphaTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> RecoveryTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> ZoomTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> TakePhotoTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> LeaveMsgTuple = new Tuple<bool, GameObject>(false, null);
    static Tuple<bool, GameObject> ReviewMsgTuple = new Tuple<bool, GameObject>(false, null);


    public static Dictionary<string, Tuple<bool, GameObject>> dic = new Dictionary<string, Tuple<bool, GameObject>>()
    {
        {ToolType.Pencil.ToString(), PencilTuple},
        {ToolType.Erase.ToString(), EraseTuple},
        {ToolType.Crayon.ToString(), CrayonTuple},
        {ToolType.Gradient.ToString(), GradientTuple},
        {ToolType.Brush.ToString(), BrushTuple},
        {ToolType.BrushPen.ToString(),BrushPenTuple},
        {ToolType.Paint.ToString(), PaintTuple},
        {ToolType.Spark.ToString(), SparkTuple},
        {ToolType.Stamp.ToString(), StampTuple},
        {ToolType.PaintCollider.ToString(), PaintColliderTuple},
        {ToolType.Delete.ToString(), DeleteTuple},
        {ToolType.Size.ToString(), SizeTuple},
        {ToolType.Color.ToString(), ColorTuple},
        {ToolType.ColorAtla.ToString(), ColorAtlaTuple},
        {ToolType.Alpha.ToString(), AlphaTuple},
        {ToolType.Recovery.ToString(), RecoveryTuple},
        {ToolType.Zoom.ToString(), ZoomTuple},
        {ToolType.TakePhoto.ToString(), TakePhotoTuple},
        {ToolType.LeaveMsg.ToString(), LeaveMsgTuple},
        {ToolType.ReviewMsg.ToString(), ReviewMsgTuple},
    };

    #endregion

    public enum ToolType
    {
        Pencil,
        Erase,
        Crayon,
        Gradient,
        Brush,
        BrushPen,
        Paint,
        Spark,
        Stamp,
        PaintCollider,
        Delete,
        Size,
        Color,
        ColorAtla,
        Alpha,
        Recovery,
        Zoom,
        TakePhoto,
        LeaveMsg,
        ReviewMsg,
    }

    public void CreatTool(ref bool toggle, string toolName)
    {
        if (toggle && !dic[toolName].item1)
        {
            dic[toolName].item1 = !dic[toolName].item1;
            GetPrefabPath();//Project里面只能有一个Tool文件夹
            if (!string.IsNullOrEmpty(prefabPath))
            {
                GameObject toolPrefab = AssetDatabase.LoadAssetAtPath(prefabPath + "/" + toolName + ".prefab", typeof(GameObject)) as GameObject;
                GameObject tool = PrefabUtility.InstantiatePrefab(toolPrefab) as GameObject;
                dic[toolName].item2 = tool;
                SetPrefabData(tool);
            }
            else
            {
                Debug.LogError("Prefab Path Could Not Be Finded");
                return;
            }
        }
        else if (!toggle && dic[toolName].item1)
        {
            dic[toolName].item1 = !dic[toolName].item1;
            DestroyImmediate(dic[toolName].item2);
        }
    }

    static string prefabPath;

    static void GetPrefabPath()
    {
        DirectoryInfo dir = new DirectoryInfo(Application.dataPath);
        GetAllFiles(dir);
    }

    static void GetAllFiles(DirectoryInfo dir)
    {
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
        foreach (FileSystemInfo i in fileinfo)
        {
            if (i is DirectoryInfo)
            {
                string[] strArrary = i.FullName.Split('\\');
                if (strArrary[strArrary.Length - 1] == "Tool")
                {
                    string fullName = i.FullName;
                    int index = fullName.IndexOf("Assets");
                    string path = fullName.Substring(index, fullName.Length - index).Replace("\\", "/");
                    prefabPath = path;
                    return;
                }
                else
                {
                    GetAllFiles((DirectoryInfo)i);
                }
            }
            else { }
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
        PencilTuple.item1 = false;
        EraseTuple.item1 = false;
        CrayonTuple.item1 = false;
        GradientTuple.item1 = false;
        BrushTuple.item1 = false;
        BrushPenTuple.item1 = false;
        PaintTuple.item1 = false;
        SparkTuple.item1 = false;
        StampTuple.item1 = false;
        PaintColliderTuple.item1 = false;
        DeleteTuple.item1 = false;
        SizeTuple.item1 = false;
        ColorTuple.item1 = false;
        ColorAtlaTuple.item1 = false;
        AlphaTuple.item1 = false;
        RecoveryTuple.item1 = false;
        ZoomTuple.item1 = false;
        TakePhotoTuple.item1 = false;
        LeaveMsgTuple.item1 = false;
        ReviewMsgTuple.item1 = false;
    }
}
