using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using Random = UnityEngine.Random;
using System.Threading;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#region GameTool
public static class GameTool
{
    /// <summary>查找子物体的Transform</summary>
    public static Transform FindTheChild(GameObject goParent, string childName)
    {
        Transform searchTrans = goParent.transform.Find(childName);
        if (searchTrans == null)
        {
            foreach (Transform trans in goParent.transform)
            {
                searchTrans = FindTheChild(trans.gameObject, childName);
                if (searchTrans != null)
                {
                    return searchTrans;
                }
            }
        }
        return searchTrans;
    }

    /// <summary>查找子物体的GameObject</summary>
    public static GameObject FindTheChildGameObject(GameObject goParent, string childName)
    {
        return FindTheChild(goParent, childName).gameObject;
    }

    public static bool IsParentOf(Transform child, Transform parent)
    {
        bool ret = false;

        Transform p = child;
        while (p != null)
        {
            p = p.parent;
            if (p == parent)
            {
                ret = true;
                break;
            }
        }

        return ret;
    }

    /// <summary>
    /// 递归设置子物体层级
    /// </summary>
    /// <param name="t"></param>
    /// <param name="layer"></param>
    public static void SetPhysicsLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;

        foreach (Transform child in t)
        {
            SetPhysicsLayerRecursive(child, layer);
        }
    }

    /// <summary>
    /// 获取父级层级总数
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static int GetNodeLayer(Transform t)
    {
        int ret = 0;

        Transform p = t.parent;
        while (p != null)
        {
            ret++;
            p = p.parent;
        }
        return ret;
    }

    public static Camera FindCameraForLayer(int layer)
    {
        int layerMask = 1 << layer;

        Camera cam = Camera.main;
        if (cam && (cam.cullingMask & layerMask) != 0) return cam;

        Camera[] cameras = new Camera[Camera.allCamerasCount];
        int camerasFound = Camera.GetAllCameras(cameras);
        for (int i = 0; i < camerasFound; ++i)
        {
            cam = cameras[i];
            if (cam && cam.enabled && (cam.cullingMask & layerMask) != 0)
                return cam;
        }
        return null;
    }

    /// <summary>获取子物体上面的组件</summary>
    public static T GetTheChildComponent<T>(GameObject goParent, string childName) where T : Component
    {
        Transform searchTrans = FindTheChild(goParent, childName);
        if (searchTrans != null)
        {
            return searchTrans.GetComponent<T>();
        }
        else
        {
            Debug.LogError("not exist this component");
            return null;
        }
    }

    /// <summary>设置子物体层级</summary>
    public static void SetChildrenLayer(Transform transform, LayerMask layer)
    {
        foreach (Transform childTransform in transform)
        {
            childTransform.gameObject.layer = layer;
            SetChildrenLayer(childTransform, layer);
        }
    }

    public static T InvokeConstructor<T>(this Type type, Type[] paramTypes = null, object[] paramValues = null)
    {
        return (T)type.InvokeConstructor(paramTypes, paramValues);
    }

    public static object InvokeConstructor(this Type type, Type[] paramTypes = null, object[] paramValues = null)
    {
        if (paramTypes == null || paramValues == null)
        {
            paramTypes = new Type[] { };
            paramValues = new object[] { };
        }

        var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);

        return constructor.Invoke(paramValues);
    }

    public static T Invoke<T>(this object o, string methodName, params object[] args)
    {
        var value = o.Invoke(methodName, args);
        if (value != null)
        {
            return (T)value;
        }

        return default(T);
    }

    public static T Invoke<T>(this object o, string methodName, Type[] types, params object[] args)
    {
        var value = o.Invoke(methodName, types, args);
        if (value != null)
        {
            return (T)value;
        }

        return default(T);
    }

    public static object Invoke(this object o, string methodName, params object[] args)
    {
        Type[] types = new Type[args.Length];
        for (int i = 0; i < args.Length; i++)
            types[i] = args[i] == null ? null : args[i].GetType();

        return o.Invoke(methodName, types, args);
    }

    public static object Invoke(this object o, string methodName, Type[] types, params object[] args)
    {
        var type = o is Type ? (Type)o : o.GetType();
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, types, null);
        if (method == null)
            method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        return method.Invoke(o, args);
    }

    #region Invoke
    private static string GetMethodName(Expression<Action> expr)
    {
        return ((MethodCallExpression)expr.Body).Method.Name;
    }

    public static void Invoke(this MonoBehaviour monoBehaviour, Expression<Action> expr, float time)
    {
        monoBehaviour.Invoke(GetMethodName(expr), time);
    }

    public static bool IsInvoking(this MonoBehaviour monoBehaviour, Expression<Action> expr)
    {
        return monoBehaviour.IsInvoking(GetMethodName(expr));
    }

    public static void CancelInvoke(this MonoBehaviour monoBehaviour, Expression<Action> expr)
    {
        monoBehaviour.CancelInvoke(GetMethodName(expr));
    }

    //bool isInvoking = this.IsInvoking(() => InvokedFunc());
    public static void InvokeRepeating(this MonoBehaviour monoBehaviour, Expression<Action> expr, float time, float repeatRate)
    {
        monoBehaviour.InvokeRepeating(GetMethodName(expr), time, repeatRate);
    }

    #endregion

    public static T GetFieldValue<T>(this object o, string name)
    {
        var value = o.GetFieldValue(name);
        if (value != null)
        {
            return (T)value;
        }

        return default(T);
    }

    public static object GetFieldValue(this object o, string name)
    {
        var field = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            return field.GetValue(o);
        }

        return null;
    }

    public static void SetFieldValue(this object o, string name, object value)
    {
        var field = o.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(o, value);
        }
    }

    public static T GetPropertyValue<T>(this object o, string name)
    {
        var value = o.GetPropertyValue(name);
        if (value != null)
        {
            return (T)value;
        }

        return default(T);
    }

    public static object GetPropertyValue(this object o, string name)
    {
        var property = o.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            return property.GetValue(o, null);
        }

        return null;
    }

    public static void SetPropertyValue(this object o, string name, object value)
    {
        var property = o.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            property.SetValue(o, value, null);
        }
    }

    /// <summary>
    /// 运行cmd命令
    /// 会显示命令窗口
    /// </summary>
    /// <param name="cmdExe">指定应用程序的完整地址</param>
    /// <param name="cmdStr">执行命令行参数</param>
    public static bool RunCmd(string cmdExe, string cmdStr)
    {
        bool result = false;
        try
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(cmdExe, cmdStr);
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                result = true;
            }
        }
        catch
        {
        }
        return result;
    }

    /// <summary>
    /// 运行cmd命令
    /// 不现实窗口
    /// </summary>
    /// <param name="cmdExe">指定应用程序的完整地址</param>
    /// <param name="cmdStr">执行命令行参数</param>
    public static bool RunCmd2(string cmdExe, string cmdStr)
    {
        bool result = false;
        try
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string value = string.Format("\"{0}\" {1} {2}", cmdExe, cmdStr, "&exit");
                process.StandardInput.WriteLine(value);
                process.StandardInput.AutoFlush = true;
                process.WaitForExit();
                result = true;
            }
        }
        catch
        {
        }
        return result;
    }

    /// <summary>交换参数的值</summary>
    public static void Swap<T>(ref T first, ref T second)
    {
        T t = first;
        first = second;
        second = t;
    }

    /// <summary>
    /// 按名称排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int IndexSort<T>(T t1,T t2) where T : UnityEngine.Object
    {
        return string.Compare(t2.name, t1.name);
    }

    /// <summary>
    /// 按名称倒序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int ReverseIndexSort<T>(T t1,T t2) where T : UnityEngine.Object
    {
        return string.Compare(t1.name, t2.name);
    }

    /// <summary>
    /// 世界坐标转UGUI坐标，用这个，rImg.rectTransform.anchoredPostion = World2UGUIPos(#,#,#);
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="scaler"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static Vector2 World2UGUIPos(Vector3 worldPos,CanvasScaler scaler,Camera camera)
    {
        float x = scaler.referenceResolution.x;
        float y = scaler.referenceResolution.y;
        Vector2 viewPos = camera.WorldToViewportPoint(worldPos);
        Vector2 uiPos = new Vector2(viewPos.x - 0.5f*x, viewPos.y - 0.5f*y);
        return uiPos;
    }

    /// <summary>
    /// 世界坐标转UGUI坐标 坐标转换 RenderMode.Overlay
    /// </summary>
    /// <param name="worldObj"></param>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static Vector3 World2UGUIPos(Vector3 worldPos, Canvas canvas)
    {
        Vector2 uisize = canvas.GetComponent<RectTransform>().sizeDelta;//得到画布的尺寸
        Vector2 screenpos = Camera.main.WorldToScreenPoint(worldPos);//将世界坐标转换为屏幕坐标
        Vector2 screenpos2;
        screenpos2.x = screenpos.x - (Screen.width / 2);//转换为以屏幕中心为原点的屏幕坐标
        screenpos2.y = screenpos.y - (Screen.height / 2);
        Vector2 uipos;
        uipos.x = (screenpos2.x / Screen.width) * uisize.x;
        uipos.y = (screenpos2.y / Screen.height) * uisize.y;//得到UGUI的anchoredPosition
        return uipos;
    }

    /// <summary>
    /// 屏幕坐标转UGUI坐标
    /// </summary>
    /// <param name="screenpos"></param>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static Vector2 Screen2UGUIPos(Vector2 screenpos, Canvas canvas)
    {
        Vector2 uisize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 screenpos2;
        screenpos2.x = screenpos.x - (Screen.width / 2);
        screenpos2.y = screenpos.y - (Screen.height / 2);
        Vector2 uipos;
        uipos.x = (screenpos2.x / Screen.width) * uisize.x;
        uipos.y = (screenpos2.y / Screen.height) * uisize.y;
        return uipos;
    }

    /// <summary>
    /// upRect是否显示在bottomRect上面
    /// </summary>
    /// <param name="bottomRect"></param>
    /// <param name="upRect"></param>
    /// <returns></returns>
    public static bool IsSheltered(Graphic bottomRect,Graphic upRect)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(bottomRect.rectTransform, new Vector2(upRect.rectTransform.position.x, upRect.rectTransform.position.y));
    }
}
#endregion

#region 自定义委托
public delegate void CallbackFunc();
public delegate void CallbackFuncWithInt(int param);
public delegate void CallbackFuncWithBool(bool param);
public delegate void CallbackFuncWithGameObj(GameObject param);
public delegate void CallbackFuncWithVector3(Vector3 param);
public delegate void CallbackFuncWithCollider(Collider param);

public delegate bool BoolCallbackFunc();
public delegate bool BoolCallbackFuncWithBool(bool param);
public delegate bool BoolCallbackFuncWithInt(int param);
public delegate bool BoolCallbackFuncWithStr(string param);
public delegate bool BoolCallbackFuncWithGameObj(GameObject param);

public delegate int IntCallbackFunc();
public delegate int IntCallbackFuncWithInt(int param);
public delegate int IntCallbackFuncWithFloat(float param);
#endregion

#region Func拓展
public sealed class FuncUtil
{
    /// <summary>
    /// 发起一个持续一段时间的Update函数，每一帧更新
    /// </summary>
    /// <param name="continueTime">更新持续的时间</param>
    /// <param name="updateFunc">更新的动作</param>
    public static IEnumerator IUpdateDo(float continueTime, CallbackFunc updateFunc, CallbackFunc doneFunc = null)
    {
        float timePassed = 0F;
        while (timePassed <= continueTime)
        {
            updateFunc();

            timePassed += Time.deltaTime;
            yield return 1;
        }
        if (doneFunc != null)
            doneFunc();
    }

    /// <summary>
    /// 启动一个update协程，并且可以指定间隔时间，停止条件, 最开始的cbContinueCondition要为真才可以运行，然后为假就中断
    /// </summary>
    /// <param name="deltaTime">update 间隔时间</param>
	/// <param name="cbContinueCondition">继续更新的条件</param>
	public static IEnumerator IUpdateDo(CallbackFunc updateFunc, float deltaTime = 0, BoolCallbackFunc cbContinueCondition = null)
    {
        //如果没有设置退出条件的回调函数，则设置回调函数为true造成死循环
        if (cbContinueCondition == null)
        {
            cbContinueCondition = () => { return true; };
        }
        if (deltaTime > 0)
        {
            //有间隔更新
            WaitForSeconds waitSeconds = new WaitForSeconds(deltaTime);
            while (cbContinueCondition() == true)
            {
                updateFunc();
                yield return waitSeconds;
            }
        }
        else
        {
            //每一帧更新
            while (cbContinueCondition() == true)
            {
                updateFunc();
                yield return 1;
            }
        }
    }

    /// <summary>
    /// 延迟一段时间才执行动作
    /// </summary>
    public static IEnumerator IDelayDoSth(CallbackFunc delayAction, float delayTime)
    {
        while (delayTime > 0)
        {
            yield return 1;
            delayTime -= Time.deltaTime;
        }
        delayAction();
    }

    /// <summary>
    /// 延迟一帧才执行动作
    /// </summary>
    public static IEnumerator IDelayDoSth(CallbackFunc delayAction)
    {
        yield return 0;
        delayAction();
    }

    //StartCoroutine(FuncUtil.IDelayDoSth(delegate () { return Input.GetKeyDown(KeyCode.F); }, () => { Debug.Log(111); }, Time.deltaTime));
    /// <summary>
    /// 延迟直到条件满足才执行动作
    /// </summary>
    /// <param name="cb_delayAction">延迟执行的动作</param>
    /// <param name="cb_isSatisfied">当某个条件满足时才能执行</param>
    /// <param name="checkTime">每次检查条件的时间</param>
    public static IEnumerator IDelayDoSth(BoolCallbackFunc cb_isSatisfied, CallbackFunc cb_delayAction, float checkTime)
    {
        //如果没有设置退出条件的回调函数，或者时间为负，直接完成
        if (cb_isSatisfied == null || checkTime < 0)
        {
            Debug.LogError("Condition:" + cb_isSatisfied + " ,checkTime:" + checkTime);
            cb_delayAction();
        }

        //有间隔更新
        WaitForSeconds waitSeconds = new WaitForSeconds(checkTime);
        //--条件不满足时，不断等待：
        while (cb_isSatisfied() == false)
            yield return waitSeconds;

        cb_delayAction();
    }

    /// <summary>
    /// 缩放物体 注：这个是以中心点做缩放，所以物体距离参考平面的高度是不固定的
    /// </summary>
    /// <param name="endValue">缩放倍数</param>
    public static IEnumerator IScaleDownTrans(Transform m_transform, float endValue = 0.4f, float time = 1f)
    {
        bool isShrink = endValue < 1F;                    //是否是缩小
        Vector3 currScale = m_transform.localScale;
        float currScaleY = currScale.y;
        float tarScaleY = currScaleY * endValue;
        float rateToScale = (currScaleY - tarScaleY);// *( isShrink ? 1F : -1F ); //是缩小的话，则缩小率为（当前-目标）
        //是放大的话，则为（目标-当前）

        while (((currScaleY > tarScaleY) && isShrink)              //缩小的情况下，则是在当前还比目标状态大的情况下进行缩小
            ||
            ((!isShrink) && currScaleY < tarScaleY)                //放大时，则是在当前还比target小的情况下进行缩大
            )
        {
            //缩小时减少，放大时增大
            currScaleY -= rateToScale * Time.deltaTime / time;// *( isShrink ? -1 : 1 );
            currScale.y = currScaleY;
            m_transform.localScale = currScale;
            yield return 1;
        }

        currScale.y = tarScaleY;
        m_transform.localScale = currScale;
    }

    /// <summary>
    /// 简单加密
    /// </summary>
    /// <param name="s">被加密的字符串</param>
    /// <param name="num">加密偏移值</param>
    public static StringBuilder encryption(string s, int num = 5)
    {
        StringBuilder stringBuilder = new StringBuilder();
        char[] array = s.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append((char)((int)array[i] + i + num));
        }
        return stringBuilder;
    }

    /// <summary>
    /// 解密（只能解密本类进行加密过的字符串）
    /// </summary>
    /// <param name="s">进行解密的字符串</param>
    /// <param name="num">偏移值与加密时必须一致</param>
    public static StringBuilder Decrypt(string s, int num = 5)
    {
        char[] array = s.ToCharArray();
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append((char)((int)array[i] - i - num));
        }
        return stringBuilder;
    }
}
#endregion

#region Component拓展
public static class ComponentUtil
{
    #region Methods
    public static T Create<T>() where T : Component
    {
        return Create<T>(typeof(T).Name);
    }

    public static T Create<T>(string name) where T : Component
    {
        return (T)Create(typeof(T), name);
    }

    public static Component Create(Type t)
    {
        return Create(t, t.Name);
    }

    public static Component Create(Type t, string name)
    {
        return new GameObject(name).AddComponent(t);
    }

    public static void SetEnabledAll(
        bool enabled,
        params Behaviour[] behaviors)
    {
        foreach (var b in behaviors)
        {
            if (b != null)
            {
                b.enabled = enabled;
            }
        }
    }
    #endregion
}
#endregion

#region 协程拓展
public sealed class EmptyMonoBehaviour : MonoBehaviour { }

public static class CoroutineUtil
{
    #region Fields
    private static readonly Dictionary<string, Coroutine> _keyedRoutines =
        new Dictionary<string, Coroutine>();

    private static EmptyMonoBehaviour _surrogate;

    #endregion

    #region Properties

    private static EmptyMonoBehaviour Surrogate
    {
        get
        {
            EnsureSurrogate();

            return _surrogate;
        }
    }
    #endregion

    #region Methods
    public static Coroutine StartCoroutine(IEnumerator routine)
    {
        return Surrogate.StartCoroutine(routine);
    }

    public static Coroutine StartCoroutine(string key, IEnumerator routine)
    {
        var coroutine = Surrogate.StartCoroutine(routine);
        _keyedRoutines.Add(key, coroutine);

        return coroutine;
    }

    public static void StopAllCoroutines()
    {
        Surrogate.StopAllCoroutines();
        _keyedRoutines.Clear();
    }

    public static void StopKeyedCoroutines()
    {
        foreach (var coroutine in _keyedRoutines.Values)
        {
            StopCoroutine(coroutine);
        }

        _keyedRoutines.Clear();
    }

    public static void StopCoroutine(IEnumerator routine)
    {
        Surrogate.StopCoroutine(routine);
    }

    public static void StopCoroutine(Coroutine routine)
    {
        Surrogate.StopCoroutine(routine);
    }

    public static void StopCoroutine(string key)
    {
        Coroutine routine;

        if (_keyedRoutines.TryGetValue(key, out routine))
        {
            Surrogate.StopCoroutine(routine);
        }
    }

    private static void EnsureSurrogate()
    {
        if (!_surrogate)
        {
            _surrogate =
                ComponentUtil.Create<EmptyMonoBehaviour>("CoroutineUtilitySurrogate");

            _surrogate.hideFlags = HideFlags.NotEditable;
            UnityEngine.Object.DontDestroyOnLoad(_surrogate);
        }
    }
    
    /// <summary>
    /// 协程等待 Allow for a routine to be yielded within another Coroutine without calling StartCoroutine
    /// e.g. yield return RoutineUtil.Sub(DoSomething());
    /// </summary>
    public static IEnumerator Sub(IEnumerator routine)
    {
        while (true)
        {
            if (!routine.MoveNext())
                yield break;

            yield return routine.Current;
        }
    }

    /// <summary>
    /// Allow for sequential routines in one fell swoop.
    /// e.g. yield return RoutineUtil.Sequence(DoSomething(), DoAnotherThing());
    /// </summary>
    public static IEnumerator Sequence(params IEnumerator[] routines)
    {
        for (int i = 0; i < routines.Length; i++)
            yield return Sub(routines[i]);
    }
    #endregion
}
#endregion

#region 数学
public static class MathHelpr
{
    /// <summary>求一条线上某一x值对应的y值</summary>
    public static float GetYByStartEndPointAndX(Vector2 startPoint, Vector2 endPoint, float x)
    {
        //斜率无穷的情况，需要做下调整
        if (startPoint.x == endPoint.x)
            startPoint.x -= 0.00001f;
        float y;
        y = startPoint.y + (endPoint.y - startPoint.y) / (endPoint.x - startPoint.x) * (x - startPoint.x);
        return y;
    }

    /// <summary>求两个角度之间的夹角</summary>
    public static int AngleDistance(int d1, int d2)
    {
        d1 = d1 % 360;
        d2 = d2 % 360;
        int val = d1 - d2;
        val = val < 0 ? -val : val;
        if (val > 180) val = 360 - val;
        return val;
    }

    /// <summary>获取欧拉角</summary>
    public static ushort GetEulerY(ushort direct)
    {
        int EulerY = (90 - direct);
        if (EulerY < 0) EulerY += 360;
        return (ushort)EulerY;
    }

    /// <summary>欧拉角获取角度</summary>
    public static ushort GetDirectByEulerY(float eulerY)
    {
        eulerY = 90 - eulerY;
        if (eulerY < 0) eulerY += 360;
        return (ushort)eulerY;
    }

    /// <summary>旋转一个2D的点</summary>
    public static Vector3 RotationPosition2D(Vector3 pos, float rad)
    {
        float x = pos.x;
        float z = pos.z;
        pos.x = x * Mathf.Cos(rad) - z * Mathf.Sin(rad);
        pos.z = x * Mathf.Sin(rad) + z * Mathf.Cos(rad);
        return pos;
    }

    /// <summary>求两个角度之间的夹角</summary>
    public static float AngleDistance(float d1, float d2)
    {
        d1 = Mathf.Repeat(d1, 360);
        d2 = Mathf.Repeat(d2, 360);
        float val = Mathf.Abs(d1 - d2);
        if (val > 180) val = 360 - val;
        return val;
    }

    /// <summary>线段跟矩形的交点 </summary>
    public static bool CaluIntrRectPoint(Vector2 a, Vector2 b, Vector4 rect, ref Vector2 pos)
    {
        // 2 3
        // 1 4  
        //坐标系
        //↑y
        //|
        //|
        //|-------->x
        //
        Vector2 s1 = new Vector2(rect.x, rect.y);
        Vector2 s2 = new Vector2(rect.x, rect.w);
        Vector2 s3 = new Vector2(rect.z, rect.w);
        Vector2 s4 = new Vector2(rect.z, rect.x);

        if (MathHelpr.CaluIntrPoint(a, b, s1, s2, ref pos))
            return true;
        if (MathHelpr.CaluIntrPoint(a, b, s3, s4, ref pos))
            return true;
        if (MathHelpr.CaluIntrPoint(a, b, s2, s3, ref pos))
            return true;
        if (MathHelpr.CaluIntrPoint(a, b, s1, s4, ref pos))
            return true;
        return false;
    }

    /// <summary>线段是否相交 相交的话求两线段的交点</summary>
    public static bool CaluIntrPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p)
    {
        // 三角形abc 面积的2倍
        float area_abc = (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);
        // 三角形abd 面积的2倍
        float area_abd = (a.x - d.x) * (b.y - d.y) - (a.y - d.y) * (b.x - d.x);
        // 面积符号相同则两点在线段同侧,不相交 (对点在线段上的情况,本例当作不相交处理);  
        if (area_abc * area_abd >= 0)
            return false;

        // 三角形cda 面积的2倍
        var area_cda = (c.x - a.x) * (d.y - a.y) - (c.y - a.y) * (d.x - a.x);
        // 三角形cdb 面积的2倍
        // 注意: 这里有一个小优化.不需要再用公式计算面积,而是通过已知的三个面积加减得出.  
        var area_cdb = area_cda + area_abc - area_abd;
        if (area_cda * area_cdb >= 0)
            return false;

        //计算交点坐标  
        float t = area_cda / (area_abd - area_abc);
        float dx = t * (b.x - a.x),
            dy = t * (b.y - a.y);

        p.x = a.x + dx;
        p.y = a.y + dy;
        return true;
    }

    /// <summary>检测两个球体碰撞</summary>
    public static bool CheckCircleCollider(float x1, float y1, float z1, float r1,
        float x2, float y2, float z2, float r2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        float dz = z2 - z1;
        float disSqua = (dx * dx) + (dy * dy) + (dz * dz);
        float rSqua = (r1 + r2) * (r1 + r2);
        return disSqua < rSqua;
    }

    /// <summary>十进制数字转换为任意进制格式 </summary>
    public static string NumberToBaseString(int number, byte b = 2)
    {
        if (number < b)
        {
            return Number2Sign(number);
        }
        else
        {
            int remainder = number % b;
            int reducedNumber = (number - remainder) / b;
            string restOfString = NumberToBaseString(reducedNumber, b);
            return restOfString + Number2Sign(remainder);
        }
    }
    /// <summary> 任意进制转换为十进制数字</summary>
    public static int BaseStringToNumber(string digit, byte b = 2)
    {
        if (string.IsNullOrEmpty(digit))
            return 0;

        int length = digit.Length;
        int output = Sign2Number(digit[length - 1]);
        string remainingString = digit.Substring(0, length - 1);
        int valueOfRemaining = BaseStringToNumber(remainingString, b);
        output += b * valueOfRemaining;
        return output;
    }

    /// <summary>数字转换为进制符</summary>
    public static string Number2Sign(int number)
    {
        switch (number)
        {
            case 10:
                return "A";
            case 11:
                return "B";
            case 12:
                return "C";
            case 13:
                return "D";
            case 14:
                return "E";
            case 15:
                return "F";
            default:
                return number.ToString();
        }
    }
    /// <summary>进制符转换为数字</summary>
    public static int Sign2Number(char hex)
    {
        switch (hex)
        {
            case 'A':
                return 10;
            case 'B':
                return 11;
            case 'C':
                return 12;
            case 'D':
                return 13;
            case 'E':
                return 14;
            case 'F':
                return 15;
            default:
                return hex - '0';
        }
    }
    public static int Sign2Number(string hex)
    {
        return Sign2Number(hex[0]);
    }

    /// <summary>获取数组内最大元素的十进制位数(仅适用整型)</summary>
    public static int GetMaxLength(int[] arr)
    {
        int num = int.MinValue;
        for (int i = 0; i < arr.Length; i++)
        {
            int num2 = arr[i];
            bool flag = num2 > num;
            if (flag)
            {
                num = num2;
            }
        }
        return num.ToString().Length;
    }

    /// <summary>
    /// 最大Int
    /// </summary>
    /// <returns></returns>
    public static int IntegerMAX()
    {
        return int.MaxValue;
    }

    /// <summary>
    /// 最小Int
    /// </summary>
    /// <returns></returns>
    public static int IntegerMin()
    {
        return int.MinValue;
    }

    /// <summary>
    /// 正无穷大
    /// </summary>
    /// <returns></returns>
    public static float MaxInfinity()
    {
        return float.PositiveInfinity;
    }

    /// <summary>
    /// 负无穷大
    /// </summary>
    /// <returns></returns>
    public static float MinInfinity()
    {
        return float.NegativeInfinity;
    }

    //Color color = new Color(Mathf.PerlinNoise(Time.time, 0), Mathf.PerlinNoise(0, Time.time), Mathf.PerlinNoise(Time.time, Time.time));
    /// <summary>
    /// 柏林噪声
    /// </summary>
    public static float PerlinNoise(PerlinNoise per)
    {
        if (per == global::PerlinNoise.Right)
            return Mathf.PerlinNoise(Time.time, 0);
        else if (per == global::PerlinNoise.Down)
            return Mathf.PerlinNoise(0, Time.time);
        else
            return Mathf.PerlinNoise(Time.time, Time.time);
    }

    public static Vector2 MakePixelPerfect(this Vector2 position)
    {
        return new Vector2((int)position.x, (int)position.y);
    }

    public static Vector2 RotateAngleAxis(this Vector2 vector, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.forward) * vector;
    }

    public static float Angle(this Vector2 direction)
    {
        return direction.y > 0
                   ? Vector2.Angle(new Vector2(1, 0), direction)
                   : -Vector2.Angle(new Vector2(1, 0), direction);
    }

    /// <summary>
    /// 快速比较距离，两点相减然后取平方，然后用该值与某个距离值的平方进行比较，不建议使用Vector3.Distance，因为Vector3.Distance(a,b) 相当于 (a-b).magnitude，即求平方后开根，而sqrMagnitude方法省去了求平方根的操作，所以比magnitude执行快。
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static float Distance(Vector3 v1,Vector3 v2,float distance)
    {
        return (v1 - v2).sqrMagnitude - distance * distance;
    }

    public enum Side
    {
        None,
        Left,
        Right
    }

    public const float TwoPI = 2.0f * Mathf.PI;
    public const float HalfPI = 0.5f * Mathf.PI;

    //------------- Interpolates ---------------

    /// <summary>
    /// Cosine interpolation
    /// </summary>
    public static float Clerp(float start, float end, float t)
    {
        float ft = t * Mathf.PI;
        float f = (1.0f - Mathf.Cos(ft)) * 0.5f;
        return start * (1.0f - f) + end * f;
    }

    //-------------- 2D --------------

    public static void Limit(ref Vector2 v, float limit)
    {
        float dSqr = v.sqrMagnitude;
        if (dSqr > limit * limit)
        {
            v /= Mathf.Sqrt(dSqr);
            v *= limit;
        }
    }

    public static Vector2 Limit(Vector2 v, float limit)
    {
        Limit(ref v, limit);
        return v;
    }

    public static Vector2 Reflect(Vector2 v, Vector2 n)
    {
        return v - (2.0f * Vector2.Dot(v, n)) * n;
    }

    public static Vector2 Slide(Vector2 v, Vector2 n)
    {
        return v - Vector2.Dot(v, n) * n;
    }

    public static float CheckSideSign(Vector2 up1, Vector2 up2)
    {
        return Cross(up1, up2) < 0 ? -1 : 1;
    }

    /// <summary>
    /// Checks which side up1 is in relation to up2
    /// </summary>
    public static Side CheckSide(Vector2 up1, Vector2 up2)
    {
        float s = Cross(up1, up2);
        return s == 0 ? Side.None : s < 0 ? Side.Right : Side.Left;
    }

    public static Vector2 Rotate(Vector2 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);

        return new Vector2(v.x * c + v.y * s, -v.x * s + v.y * c);
    }

    public static Vector2 RotateAngle(Vector2 v, float angle)
    {
        return Rotate(v, angle * Mathf.Deg2Rad);
    }

    public static float Cross(Vector2 v1, Vector2 v2)
    {
        return (v1.x * v2.y) - (v1.y * v2.x);
    }

    public static Vector2 Perpendicular(Vector2 v)
    {
        return new Vector2(-v.y, v.x);
    }

    /// <summary>
    /// Caps given destDir with angleLimit (degree) on either side of srcDir, returns which side the destDir is capped relative to srcDir.
    /// </summary>
    /// <returns>
    /// The side destDir is relative to srcDir. (-1 or 1)
    /// </returns>
    public static float DirCap(Vector2 srcDir, ref Vector2 destDir, float angleLimit)
    {

        float side = CheckSideSign(srcDir, destDir);

        float angle = Mathf.Acos(Vector2.Dot(srcDir, destDir));

        float limitAngle = angleLimit * Mathf.Deg2Rad;

        if (angle > limitAngle)
        {
            destDir = Rotate(srcDir, -side * limitAngle);
        }

        return side;
    }

    public static Vector2 Steer(Vector2 velocity, Vector2 desired, float cap, float factor)
    {
        return Limit(desired - velocity, cap) * factor;
    }

    public static Vector2 Hermite(Vector2 v1, Vector2 t1, Vector2 v2, Vector2 t2, float s)
    {
        float s2 = s * s, s3 = s2 * s;

        float h1 = 2 * s3 - 3 * s2 + 1;          // calculate basis function 1
        float h2 = -2 * s3 + 3 * s2;              // calculate basis function 2
        float h3 = s3 - 2 * s2 + s;         // calculate basis function 3
        float h4 = s3 - s2;              // calculate basis function 4

        return new Vector2(
            h1 * v1.x + h2 * v2.x + h3 * t1.x + h4 * t2.x,
            h1 * v1.y + h2 * v2.y + h3 * t1.y + h4 * t2.y);
    }

    public static Vector2 CatMullRom(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float s)
    {
        Vector2 t1 = new Vector2((v3.x - v1.x) * 0.5f, (v3.y - v1.y) * 0.5f);
        Vector2 t2 = new Vector2((v4.x - v2.x) * 0.5f, (v4.y - v2.y) * 0.5f);

        return Hermite(v2, t1, v3, t2, s);
    }

    /// <summary>
    /// Compute the Barycentric (a,b,c) based on given p in relation to triangle (p0, p1, p2)
    /// </summary>
    public static Vector3 Barycentric(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 v0 = p1 - p0, v1 = p2 - p0, v2 = p - p0;
        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        Vector3 ret;
        ret.y = (d11 * d20 - d01 * d21) / denom;
        ret.z = (d00 * d21 - d01 * d20) / denom;
        ret.x = 1.0f - ret.y - ret.z;
        return ret;
    }

    public static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        if (t == 0f) return p0;
        if (t == 1f) return p2;

        float tInv = (1 - t);
        float a = tInv * tInv;
        float b = 2 * tInv * t;
        float c = t * t;

        return new Vector2(a * p0.x + b * p1.x + c * p2.x, a * p0.y + b * p1.y + c * p2.y);
    }

    //-------------- 3D --------------

    public static bool CompareApprox(Vector3 v1, Vector3 v2, float approx)
    {
        if (Mathf.Abs(v2.x - v1.x) > approx) return false;

        if (Mathf.Abs(v2.y - v1.y) > approx) return false;

        if (Mathf.Abs(v2.z - v1.z) > approx) return false;

        return true;
    }

    /// <summary>
    /// Caps given destDir with angleLimit (degree) of srcDir, returns dir.
    /// </summary>
    /// <returns>
    /// The capped dir
    /// </returns>
    public static Vector3 DirCap(Vector3 srcDir, Vector3 destDir, float angleLimit)
    {
        float angle = Mathf.Acos(Vector3.Dot(srcDir, destDir));

        if (Mathf.Abs(angle) > angleLimit * Mathf.Deg2Rad)
        {
            Vector3 cross = Vector3.Cross(srcDir, destDir);
            Quaternion r = Quaternion.AngleAxis(angleLimit, cross);
            return r * srcDir;
        }

        return destDir;
    }

    public static Vector3 Steer(Vector3 velocity, Vector3 desired, float cap, float factor)
    {
        return Limit(desired - velocity, cap) * factor;
    }

    public static void Limit(ref Vector3 v, float limit)
    {
        float d = v.magnitude;
        if (d > limit)
        {
            v /= d;
            v *= limit;
        }
    }

    public static Vector3 Limit(Vector3 v, float limit)
    {
        Limit(ref v, limit);
        return v;
    }

    public static Vector3 Reflect(Vector3 v, Vector3 n)
    {
        return v - (2.0f * Vector3.Dot(v, n)) * n;
    }

    public static Vector3 Slide(Vector3 v, Vector3 n)
    {
        return v - Vector3.Dot(v, n) * n;
    }

    public static float DistanceSqr(Vector3 pt1, Vector3 pt2)
    {
        return Vector3.SqrMagnitude(pt1 - pt2);
    }

    public static Vector3 MidPoint(Vector3 pt1, Vector3 pt2)
    {
        return new Vector3((pt1.x + pt2.x) * 0.5f, (pt1.y + pt2.y) * 0.5f, (pt1.z + pt2.z) * 0.5f);
    }

    /// <summary>
    /// get the square distance from a point to a line segment.
    /// </summary>
    /// <param name="lineP1">line segment start point</param>
    /// <param name="lineP2">line segment end point</param>
    /// <param name="point">point to get distance to</param>
    /// <param name="closestPoint">set to either 1, 2, or 4, determining which end the point is closest to (p1, p2, or the middle)</param>
    /// <returns></returns>
    public static float DistanceToLineSqr(Vector3 lineP1, Vector3 lineP2, Vector3 point, out int closestPoint)
    {
        Vector3 v = lineP2 - lineP1;
        Vector3 w = point - lineP1;

        float c1 = Vector3.Dot(w, v);

        if (c1 <= 0)
        {//closest point is p1
            closestPoint = 1;
            return DistanceSqr(point, lineP1);
        }

        float c2 = Vector3.Dot(v, v);

        if (c2 <= c1)
        {//closest point is p2
            closestPoint = 2;
            return DistanceSqr(point, lineP2);
        }

        float b = c1 / c2;
        Vector3 pb = lineP1 + b * v;

        closestPoint = 4;
        return DistanceSqr(point, pb);
    }

    /// <summary>
    /// Get the angle in degrees between given forward axis to target position. WorldToLocal determines the
    /// axis' space relative to given point.
    /// If you want to create a rotation, use Quaternion.AngleAxis(angle, Vector3.up);
    /// </summary>
    public static float AngleForwardAxisDir(Matrix4x4 worldToLocal, Vector3 axis, Vector3 dir)
    {
        dir = worldToLocal.MultiplyVector(dir);
        dir.y = 0.0f;

        float s = CheckSideSign(new Vector2(dir.x, dir.z), new Vector2(axis.x, axis.z));

        float angle = Vector3.Angle(axis, dir);

        return s * angle;
    }

    /// <summary>
    /// Get the angle in degrees between given forward axis to target position. WorldToLocal determines the
    /// axis' space relative to given point.
    /// If you want to create a rotation, use Quaternion.AngleAxis(angle, Vector3.up);
    /// </summary>
    public static float AngleForwardAxis(Matrix4x4 worldToLocal, Vector3 point, Vector3 axis, Vector3 target)
    {
        return AngleForwardAxisDir(worldToLocal, axis, target - point);
    }

    public static bool RotateToUp(Vector3 up, Vector3 right, Vector3 forward, ref Quaternion rotate)
    {
        Vector3 f = Vector3.Cross(up, right);
        if (f == Vector3.zero)
        {
            Vector3 l = Vector3.Cross(up, forward);
            f = Vector3.Cross(l, up);
        }

        if (f != Vector3.zero)
        {
            rotate = Quaternion.LookRotation(f, up);
            return true;
        }

        return false;
    }

    public static bool VectorEqualApproximately(Vector3 lhs, Vector3 rhs)
    {
        return Mathf.Approximately(lhs.x, rhs.x) && Mathf.Approximately(lhs.y, rhs.y) && Mathf.Approximately(lhs.z, rhs.z);
    }

    /// <summary>
    /// Compute the Barycentric (a,b,c) based on given p in relation to triangle (p0, p1, p2)
    /// </summary>
    public static Vector3 Barycentric(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 v0 = p1 - p0, v1 = p2 - p0, v2 = p - p0;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        Vector3 ret;
        ret.y = (d11 * d20 - d01 * d21) / denom;
        ret.z = (d00 * d21 - d01 * d20) / denom;
        ret.x = 1.0f - ret.y - ret.z;
        return ret;
    }

    //-------------- Misc. --------------

    /// <summary>
    /// Greatest common factor.
    /// </summary>
    public static int Gcf(int a, int b)
    {
        while (b != 0)
        {
            int c = b;
            b = a % b;
            a = c;
        }

        return a;
    }

    /// <summary>
    /// Least common multiple.
    /// </summary>
    public static int Lcm(int a, int b)
    {
        return (a / Gcf(a, b)) * b;
    }
}

//-------------- Bounds --------------
public struct BoundsUtil
{
    public static bool Intersect(Bounds b1, Bounds b2, out Bounds bOut)
    {
        bool ret = b1.Intersects(b2);
        if (ret)
        {
            bOut = new Bounds();
            bOut.SetMinMax(
                Vector3.Max(b1.min, b2.min),
                Vector3.Min(b1.max, b2.max));
        }
        else
            bOut = b1;
        return ret;
    }
}

//-------------- Easing --------------

public struct Easing
{
    public static float In(float t, float tMax, float start, float delta)
    {
        return start + delta * _in(t / tMax);
    }

    private static float _in(float r)
    {
        return r * r * r;
    }

    public static float Out(float t, float tMax, float start, float delta)
    {
        return start + delta * _out(t / tMax);
    }

    private static float _out(float r)
    {
        float ir = r - 1.0f;
        return ir * ir * ir + 1.0f;
    }

    public static float OutElastic(float t, float tMax, float start, float delta)
    {
        return start + (delta * _outElastic(t / tMax));
    }

    private static float _outElastic(float ratio)
    {
        if (ratio == 0.0f || ratio == 1.0f) return ratio;

        float p = 0.3f;
        float s = p / 4.0f;
        return -1.0f * Mathf.Pow(2.0f, -10.0f * ratio) * Mathf.Sin((ratio - s) * 2.0f * Mathf.PI / p) + 1.0f;
    }

    public static float InBounce(float t, float tMax, float start, float delta)
    {
        return start + (delta * _inBounce(t / tMax));
    }

    private static float _inBounce(float ratio)
    {
        return 1.0f - _outBounce(1.0f - ratio);
    }

    private static float _outBounce(float ratio)
    {
        float s = 7.5625f;
        float p = 2.75f;
        float l;
        if (ratio < (1.0f / p))
            l = s * Mathf.Pow(ratio, 2.0f);
        else
        {
            if (ratio < (2.0f / p))
            {
                ratio = ratio - (1.5f / p);
                l = s * Mathf.Pow(ratio, 2.0f) + 0.75f;
            }
            else
            {
                if (ratio < (2.5f / p))
                {
                    ratio = ratio - (2.25f / p);
                    l = s * Mathf.Pow(ratio, 2.0f) + 0.9375f;
                }
                else
                {
                    ratio = ratio - (2.65f / p);
                    l = s * Mathf.Pow(ratio, 2.0f) + 0.984375f;
                }
            }
        }
        return l;
    }

    public static float InElastic(float t, float tMax, float start, float delta)
    {
        return start + (delta * _inElastic(t / tMax));
    }

    private static float _inElastic(float ratio)
    {
        if (ratio == 0.0f || ratio == 1.0f) return ratio;

        float p = 0.3f;
        float s = p / 4.0f;
        float invRatio = ratio - 1.0f;
        return -1 * Mathf.Pow(2.0f, 10.0f * invRatio) * Mathf.Sin((invRatio - s) * 2.0f * Mathf.PI / p);
    }
}

public enum PerlinNoise
{
    Right,
    Down,
    RightDown,
}

#endregion

#region 加载器
public sealed class LoadUtil
{
    /// <summary>
    /// 加载文件夹下所有图片
    /// </summary>
    /// <param name="texList"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static IEnumerator LoadWWWAllPicture(List<Texture2D> texList,string path)
    {
        texList.Clear();
        Hashtable ht = new Hashtable();
        DirectoryInfo dir = new DirectoryInfo(path);
        GetAllFiles(dir, ht);
        foreach (DictionaryEntry de in ht)
        {
            WWW www = new WWW("file://" + path + "/" + de.Key);
            yield return www;
            if (www != null)
            {
                Texture2D tmp = www.texture;
                tmp.name = de.Key.ToString();
                texList.Add(tmp);
            }
            if (www.isDone)
            {
                www.Dispose();
                Resources.UnloadUnusedAssets();
            }
        }
    }

    public static void GetAllFiles(DirectoryInfo dir, Hashtable ht)
    {
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos(); 
        foreach (FileSystemInfo i in fileinfo)  
        {
            if (i is DirectoryInfo)             
            {
                GetAllFiles((DirectoryInfo)i, ht);  
            }
            else
            {
                string str = i.FullName;      
                string path = Application.streamingAssetsPath;
                string strType = str.Substring(path.Length);
                string suffix = strType.Substring(strType.Length - 3).ToLower();
                if (suffix == "png"| suffix == "jpg")
                {
                    if (ht.Contains(strType))
                        ht[strType] = strType;
                    else
                        ht.Add(strType, strType);
                }
            }
        }
    }
}
#endregion

#region Linq拓展
public static class LinqUtil
{
    #region SelectAction_Test
    //private static bool SelectRayCast(RawImage rImg)
    //{
    //    return rImg.raycastTarget = true;
    //}

    //public static void SelectRimg()
    //{
    //    List<RawImage> list = new List<RawImage>();
    //    var select =  Selectarray<RawImage>(list, SelectRayCast);
    //}
    #endregion

    #region CustomIntersect_Test
    //static string[] AA = new string[5] { "1", "2", "3", "4", "5" };
    //static string[] BB = new string[5] { "1", "2", "3", "4", "6" };
    //static string[] CC = (string[])LinqUtil.CustomIntersect<string>(AA, BB);
    //static string[] DD = (string[])LinqUtil.CustomExcept(AA, BB);
    //static string[] EE = (string[])LinqUtil.CustomExcept(BB, AA);
    //static string[] GG = (string[])LinqUtil.CustomWhere(AA, x => x == "1").ToArray();
    //static bool FF = LinqUtil.IsArrayEqual(AA, BB);
    #endregion

    /// <summary>
    /// 将输入字典中的每个值传给回调函数,如果回调函数返回 true，则把输入字典中的当前键值对加入结果字典中
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam><typeparam name="TValue">字典值类型</typeparam><param name="source">规定字典</param><param name="predicate">回调函数</param>
    public static IDictionary<TKey, TValue> Filter<TKey, TValue>(IDictionary<TKey, TValue> source, Func<TKey, TValue, bool> predicate)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(predicate != null);
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
        {
            if (predicate(keyValuePair.Key, keyValuePair.Value))
                dictionary[keyValuePair.Key] = keyValuePair.Value;
        }
        return (IDictionary<TKey, TValue>)dictionary;
    }

    /// <summary>
    /// 将输入字典中的每个值传给回调函数，如果回调函数返回 true，则移除字典中对应的元素，并返回被移除的元素
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam><typeparam name="TValue">字典值类型</typeparam><param name="source">规定字典</param><param name="predicate">回调函数</param><return>被移除的元素</return>
    public static KeyValuePair<TKey, TValue>[] Remove<TKey, TValue>(IDictionary<TKey, TValue> source, Func<TKey, TValue, bool> predicate)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(predicate != null);
        List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
        {
            if (predicate(keyValuePair.Key, keyValuePair.Value))
                list.Add(keyValuePair);
        }
        foreach (KeyValuePair<TKey, TValue> keyValuePair in list)
            source.Remove(keyValuePair.Key);
        return list.ToArray();
    }

    /// <summary>
    /// 将输入字典中的每个值传给回调函数，回调函数的返回值用于修改元素的值
    /// </summary>
    public static void Modify<TKey, TValue>(IDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> callback)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(callback != null);
        Dictionary<TKey, TValue> dictionary = (Dictionary<TKey, TValue>)null;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
        {
            TValue obj = callback(keyValuePair.Key, keyValuePair.Value);
            if (!keyValuePair.Equals((object)obj))
            {
                dictionary = dictionary ?? new Dictionary<TKey, TValue>();
                dictionary[keyValuePair.Key] = obj;
            }
        }
        foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
            source[keyValuePair.Key] = keyValuePair.Value;
    }

    /// <summary>
    /// 将元素批量添加到字典
    /// </summary>
    /// <typeparam name="TKey">字典键</typeparam><typeparam name="TValue">字典值</typeparam><param name="source">目标字典</param><param name="added">增加的内容</param><param name="replaced">遇到重复是否替换，如果不进行替换遇到重复将会抛出一个异常</param>
    public static void AddRange<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> added, bool replaced = true)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        if (added == null)
            return;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)added)
        {
            if (replaced)
                source[keyValuePair.Key] = keyValuePair.Value;
            else
                source.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// 将字典值传入用户自定义函数，自定义函数返回的值作为新的字典值
    /// </summary>
    public static IDictionary<TKey, TValue> Map<TKey, TValue>(IDictionary<TKey, TValue> source, Func<TKey, TValue, TValue> callback)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(callback != null);
        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
            dictionary[keyValuePair.Key] = callback(keyValuePair.Key, keyValuePair.Value);
        return (IDictionary<TKey, TValue>)dictionary;
    }

    /// <summary>
    /// 获取字典的键数组
    /// </summary>
    public static TKey[] Keys<TKey, TValue>(IDictionary<TKey, TValue> source)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        TKey[] keyArray = new TKey[source.Count];
        int num = 0;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
            keyArray[num++] = keyValuePair.Key;
        return keyArray;
    }

    /// <summary>
    /// 获取字典的值数组
    /// </summary>
    public static TValue[] Values<TKey, TValue>(IDictionary<TKey, TValue> source)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        TValue[] objArray = new TValue[source.Count];
        int num = 0;
        foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
            objArray[num++] = keyValuePair.Value;
        return objArray;
    }

    /// <summary>
    /// 使用点（.）来访问深度字典
    /// </summary>
    public static object Get(IDictionary<string, object> dict, string key, object def = null)
    {
        if (dict == null)
            return def;
        if (string.IsNullOrEmpty(key))
            return (object)dict;
        string str = key;
        char[] chArray = new char[1];
        int index = 0;
        int num = 46;
        chArray[index] = (char)num;
        string[] keys = Reverse<string>(str.Split(chArray), 0, new int?());
        return GetValueByDepthArray(dict, ref keys) ?? def;
    }

    /// <summary>
    /// 使用点（.）来访问深度字典，并为其指定位置设定一个值
    /// </summary>
    /// <param name="dict">规定字典</param><param name="key">键，支持使用点（.）来进行深度访问</param><param name="val">设定的值</param>
    public static void Set(IDictionary<string, object> dict, string key, object val)
    {
        Guard.Requires<ArgumentNullException>(dict != null);
        Guard.Requires<ArgumentNullException>(key != null);
        string str = key;
        char[] chArray = new char[1];
        int index = 0;
        int num = 46;
        chArray[index] = (char)num;
        string[] keys = Reverse<string>(str.Split(chArray), 0, new int?());
        SetValueByDepthArray(dict, ref keys, val);
    }

    /// <summary>
    /// 使用点（.）来访问深度字典，并移除其中指定的值
    /// </summary>
    /// <param name="dict">规定字典</param><param name="key">键，支持使用点（.）来进行深度访问</param>
    public static bool Remove(IDictionary<string, object> dict, string key)
    {
        Guard.Requires<ArgumentNullException>(dict != null);
        Guard.Requires<ArgumentNullException>(key != null);
        string str = key;
        char[] chArray = new char[1];
        int index = 0;
        int num = 46;
        chArray[index] = (char)num;
        string[] keys = Reverse<string>(str.Split(chArray), 0, new int?());
        return RemoveValueByDepthArray(dict, ref keys);
    }

    /// <summary>
    /// 通过深度数组来访问字典
    /// </summary>
    /// <param name="dict">规定字典</param><param name="keys">深度数组（深度数组以倒序传入）</param>
    /// <returns>
    /// 字典值
    /// </returns>
    private static object GetValueByDepthArray(IDictionary<string, object> dict, ref string[] keys)
    {
        object obj;
        while (dict.TryGetValue(Pop<string>(ref keys), out obj) && keys.Length != 0)
        {
            dict = obj as IDictionary<string, object>;
            if (dict == null)
                return (object)null;
        }
        return obj;
    }

    /// <summary>
    /// 通过深度数组来访问字典，并为其指定位置设定一个值
    /// </summary>
    /// <param name="dict">规定字典</param><param name="keys">深度数组（深度数组以倒序传入）</param><param name="value">设定值</param>
    private static void SetValueByDepthArray(IDictionary<string, object> dict, ref string[] keys, object value)
    {
        while (keys.Length > 1)
        {
            string key = Pop<string>(ref keys);
            object obj;
            if (!dict.TryGetValue(key, out obj) || !(obj is IDictionary<string, object>))
                dict[key] = (object)(Dictionary<string, object>)(obj = (object)new Dictionary<string, object>());
            dict = (IDictionary<string, object>)obj;
        }
        dict[Pop<string>(ref keys)] = value;
    }


    /// <summary>
    /// 字典排序
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dic"></param>
    /// <param name="_ascending"></param>
    /// 对Vector3排序
    /// dicSort = from objDic in dic orderby objDic.Key.z ascending select objDic;
    public static void DictionarySort<T1, T2>(IDictionary<T1, T2> dic,bool _ascending)
    {
        IOrderedEnumerable<KeyValuePair<T1, T2>>dicSort;
        if (_ascending)
            dicSort = from objDic in dic orderby objDic.Key ascending select objDic;
        else
            dicSort = from objDic in dic orderby objDic.Key descending select objDic;
        foreach (KeyValuePair<T1, T2> item in dicSort)
            Debug.Log(string.Format("item.Key = {0},item.Value = {1}", item.Key, item.Value));
    }

    /// <summary>
    /// 通过深度数组来移除数组中的一个值
    /// 
    /// </summary>
    /// <param name="dict">规定字典</param><param name="keys">深度数组（深度数组以倒序传入）</param>
    private static bool RemoveValueByDepthArray(IDictionary<string, object> dict, ref string[] keys)
    {
        Stack<KeyValuePair<string, IDictionary<string, object>>> stack = new Stack<KeyValuePair<string, IDictionary<string, object>>>(keys.Length);
        while (keys.Length > 1)
        {
            string key = Pop<string>(ref keys);
            object obj;
            if (!dict.TryGetValue(key, out obj) || !(obj is IDictionary<string, object>))
                return false;
            stack.Push(new KeyValuePair<string, IDictionary<string, object>>(key, dict));
            dict = (IDictionary<string, object>)obj;
        }
        dict.Remove(Pop<string>(ref keys));
        while (stack.Count > 0)
        {
            KeyValuePair<string, IDictionary<string, object>> keyValuePair = stack.Pop();
            if (((ICollection<KeyValuePair<string, object>>)keyValuePair.Value[keyValuePair.Key]).Count <= 0)
                keyValuePair.Value.Remove(keyValuePair.Key);
            else
                break;
        }
        return true;
    }

    /// <summary>
    /// 删除字典键值
    /// </summary>
    public static void RemoveDicKeyValue<T1, T2>(IDictionary<T1, T2> dic, BoolCallbackFunc callBackFunc = null) 
        where T2 : UnityEngine.Object
    {
        foreach (var item in dic.ToList())
        {
            bool condition = callBackFunc == null ? true : callBackFunc();
            if (condition)
            {
                UnityEngine.Object.Destroy(item.Value);
                dic.Remove(item.Key);
            }
        }
    }

    /// <summary>
    /// 字典通过值获得键
    /// </summary>
    private static T1 GetKey<T1,T2>(this Dictionary<T1, T2> dic, T2 name)
    {
        var keyvaluepair = dic.SingleOrDefault(k => k.Value.Equals(name));
        var key = keyvaluepair.Key;
        return key;
    }

    /// <summary>将多个规定数组合并成一个数组</summary>
    public static T[] Merge<T>(params T[][] sources)
    {
        Guard.Requires<ArgumentNullException>(sources != null);
        int length = 0;
        foreach (T[] objArray in sources)
            length += objArray.Length;
        T[] objArray1 = new T[length];
        int destinationIndex = 0;
        foreach (T[] objArray2 in sources)
        {
            Array.Copy((Array)objArray2, 0, (Array)objArray1, destinationIndex, objArray2.Length);
            destinationIndex += objArray2.Length;
        }
        return objArray1;
    }

    /// <summary>
    /// 从规定数组中获取一个或者指定数量的随机值
    /// </summary>
    public static T[] Rand<T>(T[] source, int number = 1)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        number = Math.Max(number, 1);
        source = Shuffle<T>(source, new int?());
        T[] objArray = new T[number];
        int num = 0;
        foreach (T obj in source)
        {
            if (num < number)
                objArray[num++] = obj;
            else
                break;
        }
        return objArray;
    }

    /// <summary>
    /// 将规定数组中的元素打乱
    /// </summary>
    public static T[] Shuffle<T>(T[] source, int? seed = null)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        T[] objArray = new T[source.Length];
        Array.Copy((Array)source, (Array)objArray, source.Length);
        System.Random random = Util.MakeRandom(seed);
        for (int index1 = 0; index1 < objArray.Length; ++index1)
        {
            int index2 = random.Next(0, objArray.Length - 1);
            if (index2 != index1)
            {
                T obj = objArray[index1];
                objArray[index1] = objArray[index2];
                objArray[index2] = obj;
            }
        }
        return objArray;
    }

    /// <summary>
    /// 从数组中移除指定长度的元素，如果给定了<paramref name="replSource"/>参数，那么新元素从<paramref name="start"/>位置开始插入
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="start">删除元素的开始位置。
    /// <para>
    /// 如果该值设置为正数，则从前往后开始删除
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则从后向前取 <paramref name="start"/> 绝对值。-2 意味着从数组的倒数第二个元素开始
    /// </para>
    /// </param><param name="length">删除元素的个数，也是被返回数组的长度
    /// <para>
    /// 如果该值设置为整数，则返回该数量的元素。
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则则从后向前取 <paramref name="length"/> 绝对值位置终止删除。-1 意味着从数组的倒数第一个元素前删除
    /// </para>
    /// <para>
    /// 如果该值未设置，则返回从 <paramref name="start"/> 参数设置的位置开始直到数组末端的所有元素。
    /// </para>
    /// </param><param name="replSource">在start位置插入的数组</param>
    /// <returns>
    /// 被删除的数组
    /// </returns>
    public static T[] Splice<T>(ref T[] source, int start, int? length = null, T[] replSource = null)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Util.NormalizationPosition(source.Length, ref start, ref length);
        T[] objArray1 = new T[length.Value];
        if (length.Value == source.Length)
        {
            Array.Copy((Array)source, (Array)objArray1, source.Length);
            source = replSource ?? new T[0];
            return objArray1;
        }
        Array.Copy((Array)source, start, (Array)objArray1, 0, length.Value);
        if (replSource == null || replSource.Length == 0)
        {
            T[] objArray2 = new T[source.Length - length.Value];
            if (start > 0)
                Array.Copy((Array)source, 0, (Array)objArray2, 0, start);
            Array.Copy((Array)source, start + length.Value, (Array)objArray2, start, source.Length - (start + length.Value));
            source = objArray2;
        }
        else
        {
            T[] objArray2 = new T[source.Length - length.Value + replSource.Length];
            if (start > 0)
                Array.Copy((Array)source, 0, (Array)objArray2, 0, start);
            Array.Copy((Array)replSource, 0, (Array)objArray2, start, replSource.Length);
            Array.Copy((Array)source, start + length.Value, (Array)objArray2, start + replSource.Length, source.Length - (start + length.Value));
            source = objArray2;
        }
        return objArray1;
    }

    /// <summary>
    /// 将数组分为新的数组块
    /// <para>
    /// 其中每个数组的单元数目由 <paramref name="size"/> 参数决定。最后一个数组的单元数目可能会少几个。
    /// </para>
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="size">每个分块的大小</param>
    /// <returns/>
    public static T[][] Chunk<T>(T[] source, int size)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        size = Math.Max(1, size);
        T[][] objArray1 = new T[source.Length / size + (source.Length % size == 0 ? 0 : 1)][];
        T[] objArray2 = (T[])null;
        for (int index = 0; index < source.Length; ++index)
        {
            int num = index / size;
            if (index % size == 0)
            {
                if (objArray2 != null)
                    objArray1[num - 1] = objArray2;
                objArray2 = new T[index + size <= source.Length ? size : source.Length - index];
            }
            objArray2[index - num * size] = source[index];
        }
        objArray1[objArray1.Length - 1] = objArray2;
        return objArray1;
    }

    /// <summary>
    /// 对数组进行填充，如果传入了规定数组，那么会在规定数组的基础上进行填充
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="start">起始下标</param><param name="length">填充长度</param><param name="value">填充的值</param><param name="source">规定数组</param>
    /// <returns>
    /// 填充后的数组
    /// </returns>
    public static T[] Fill<T>(int start, int length, T value, T[] source = null)
    {
        Guard.Requires<ArgumentOutOfRangeException>(start >= 0);
        Guard.Requires<ArgumentOutOfRangeException>(length > 0);
        int destinationIndex = start + length;
        T[] objArray = new T[source == null ? destinationIndex : source.Length + destinationIndex];
        if (start > 0 && source != null)
            Array.Copy((Array)source, (Array)objArray, Math.Min(source.Length, start));
        for (int index = start; index < destinationIndex; ++index)
            objArray[index] = value;
        if (source != null && start < source.Length)
            Array.Copy((Array)source, start, (Array)objArray, destinationIndex, source.Length - start);
        return objArray;
    }

    /// <summary>
    /// 将数组每个值传给回调函数，如果回调函数返回 true，则移除数组中对应的元素，并返回被移除的元素
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="predicate">回调函数</param>
    /// <returns>
    /// 被移除的数组
    /// </returns>
    public static T[] Remove<T>(ref T[] source, Predicate<T> predicate)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(predicate != null);
        if (source.Length == 0)
            return new T[0];
        List<T> list = new List<T>();
        for (int index = source.Length - 1; index >= 0; --index)
        {
            if (predicate(source[index]))
            {
                list.Add(source[index]);
                RemoveAt<T>(ref source, index);
            }
        }
        return Reverse<T>(list.ToArray(), 0, new int?());
    }

    /// <summary>
    /// 输入数组中的每个值传给回调函数,如果回调函数返回 true，则把输入数组中的当前值加入结果数组中
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="predicate">回调函数</param>
    /// <returns>
    /// 需求数组
    /// </returns>
    public static T[] Filter<T>(T[] source, Predicate<T> predicate)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(predicate != null);
        T[] array = new T[source.Length];
        int newSize = 0;
        foreach (T obj in source)
        {
            if (predicate(obj))
                array[newSize++] = obj;
        }
        Array.Resize<T>(ref array, newSize);
        return array;
    }

    /// <summary>
    /// 将数组值传入用户自定义函数，自定义函数返回的值作为新的数组值
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="callback">自定义函数</param>
    /// <returns>
    /// 处理后的数组
    /// </returns>
    public static T[] Map<T>(T[] source, Func<T, T> callback)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(callback != null);
        T[] objArray = new T[source.Length];
        Array.Copy((Array)source, (Array)objArray, source.Length);
        for (int index = 0; index < source.Length; ++index)
            objArray[index] = callback(source[index]);
        return objArray;
    }

    /// <summary>
    /// 删除数组中的最后一个元素，并将删除的元素作为返回值返回
    /// </summary>
    /// <typeparam name="T">删除数组中的最后一个元素</typeparam><param name="source">规定数组</param>
    /// <returns>
    /// 被删除的元素
    /// </returns>
    public static T Pop<T>(ref T[] source)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<InvalidOperationException>((uint)source.Length > 0U);
        T obj = source[source.Length - 1];
        Array.Resize<T>(ref source, source.Length - 1);
        return obj;
    }

    /// <summary>
    /// 将一个或多个元素加入数组尾端
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="elements">要加入的元素</param>
    /// <returns>
    /// 数组的元素个数
    /// </returns>
    public static int Push<T>(ref T[] source, params T[] elements)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<InvalidOperationException>(elements != null);
        Array.Resize<T>(ref source, source.Length + elements.Length);
        Array.Copy((Array)elements, 0, (Array)source, source.Length - elements.Length, elements.Length);
        return source.Length;
    }

    /// <summary>
    /// 向用户自定义函数发送数组中的值，并返回一个字符串
    /// <para>
    /// 如果数组是空的且未传递<paramref name="initial"/>参数，该函数返回 null
    /// </para>
    /// <para>
    /// 如果指定了<paramref name="initial"/>参数，则该参数将被当成是数组中的第一个值来处理，如果数组为空的话就作为最终返回值(string)
    /// </para>
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="callback">自定义函数</param><param name="initial">初始值</param>
    /// <returns/>
    public static string Reduce<T>(T[] source, Func<object, T, string> callback, object initial = null)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(callback != null);
        object obj1 = initial;
        foreach (T obj2 in source)
            obj1 = (object)callback(obj1, obj2);
        if (obj1 != null)
            return obj1.ToString();
        return (string)null;
    }

    /// <summary>
    /// 在数组中根据条件取出一段值，并返回。
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="start">取出元素的开始位置。
    /// <para>
    /// 如果该值设置为正数，则从前往后开始取
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则从后向前取 <paramref name="start"/> 绝对值。-2 意味着从数组的倒数第二个元素开始
    /// </para>
    /// </param><param name="length">被返回数组的长度
    /// <para>
    /// 如果该值设置为整数，则返回该数量的元素。
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则则从后向前取 <paramref name="length"/> 绝对值位置终止取出。-1 意味着从数组的倒数第一个元素前终止
    /// </para>
    /// <para>
    /// 如果该值未设置，则返回从 <paramref name="start"/> 参数设置的位置开始直到数组末端的所有元素。
    /// </para>
    /// </param>
    /// <returns>
    /// 取出的数组
    /// </returns>
    public static T[] Slice<T>(T[] source, int start, int? length = null)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Util.NormalizationPosition(source.Length, ref start, ref length);
        T[] objArray = new T[length.Value];
        Array.Copy((Array)source, start, (Array)objArray, 0, length.Value);
        return objArray;
    }

    /// <summary>
    /// 删除数组中第一个元素，并返回被删除元素的值
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param>
    /// <returns>
    /// 被删除元素的值
    /// </returns>
    public static T Shift<T>(ref T[] source)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<InvalidOperationException>((uint)source.Length > 0U);
        T obj = source[0];
        T[] objArray = new T[source.Length - 1];
        Array.Copy((Array)source, 1, (Array)objArray, 0, source.Length - 1);
        source = objArray;
        return obj;
    }

    /// <summary>
    /// 向数组插入新元素。新数组的值将被插入到数组的开头。
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="elements">插入的元素</param>
    /// <returns>
    /// 数组元素个数
    /// </returns>
    public static int Unshift<T>(ref T[] source, params T[] elements)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(elements != null);
        T[] objArray = new T[source.Length + elements.Length];
        Array.Copy((Array)elements, (Array)objArray, elements.Length);
        Array.Copy((Array)source, 0, (Array)objArray, elements.Length, source.Length);
        source = objArray;
        return source.Length;
    }

    /// <summary>
    /// 以相反的顺序返回数组
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="start">起始元素的开始位置。
    /// <para>
    /// 如果该值设置为正数，则从前往后开始取
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则从后向前取 <paramref name="start"/> 绝对值。-2 意味着从数组的倒数第二个元素开始
    /// </para>
    /// </param><param name="length">被返回数组的长度
    /// <para>
    /// 如果该值设置为整数，则返回该数量的元素。
    /// </para>
    /// <para>
    /// 如果该值设置为负数，则则从后向前取 <paramref name="length"/> 绝对值位置终止取出。-1 意味着从数组的倒数第一个元素前终止
    /// </para>
    /// <para>
    /// 如果该值未设置，则返回从 <paramref name="start"/> 参数设置的位置开始直到数组末端的所有元素。
    /// </para>
    /// </param>
    /// <returns>
    /// 反转的数组
    /// </returns>
    public static T[] Reverse<T>(T[] source, int start = 0, int? length = null)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        if (source.Length == 1)
            return source;
        Util.NormalizationPosition(source.Length, ref start, ref length);
        T[] objArray1 = new T[source.Length];
        Array.Copy((Array)source, (Array)objArray1, source.Length);
        Array.Reverse((Array)objArray1, start, length.Value);
        T[] objArray2 = new T[length.Value];
        Array.Copy((Array)objArray1, start, (Array)objArray2, 0, length.Value);
        return objArray2;
    }

    /// <summary>
    /// 从数组中检索指定的值并返回所在的下标，如果返回-1则代表没有出现
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="match">要匹配的值，如果有多个，只有全部的匹配才算匹配</param>
    /// <returns>
    /// 如果要检索的值没有出现，则该方法返回 -1
    /// </returns>
    public static int IndexOf<T>(T[] source, params T[] match)
    {
        if (match == null || match.Length == 0 || (source == null || source.Length == 0))
            return -1;
        for (int index1 = 0; index1 < source.Length; ++index1)
        {
            if (source[index1].Equals((object)match[0]))
            {
                bool flag = true;
                for (int index2 = 0; index2 < match.Length; ++index2)
                {
                    if (index1 + index2 >= source.Length || !source[index1 + index2].Equals((object)match[index2]))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    return index1;
            }
        }
        return -1;
    }

    /// <summary>
    /// 排除掉数组中的指定值
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">需要过滤的数组</param><param name="match">数组需要排除掉的值</param>
    /// <returns>
    /// 过滤后的数组
    /// </returns>
    public static T[] Difference<T>(T[] source, params T[] match)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        if (match == null)
            return source;
        return Filter<T>(source, (Predicate<T>)(val =>
        {
            foreach (T obj in match)
            {
                if (val.Equals((object)obj))
                    return false;
            }
            return true;
        }));
    }

    /// <summary>
    /// 移除并返回指定下标的数组元素
    /// <para>
    /// 如果下标传入的是负数那么将会从末尾移除
    /// </para>
    /// </summary>
    public static T RemoveAt<T>(ref T[] source, int index)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        Guard.Requires<ArgumentNullException>(index < source.Length);
        T[] objArray = Splice<T>(ref source, index, new int?(1), (T[])null);
        if (objArray.Length == 0)
            return default(T);
        return objArray[0];
    }

    /// <summary>
    /// 临时性的回调元素，如果遇到异常或者完成回调后会进行回滚元素回调
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam><param name="source">规定数组</param><param name="process">顺序回调</param><param name="completed">所有回调完成后</param><param name="rollback">回滚回调</param>
    public static void Flash<T>(T[] source, Action<T> process, Action<T> rollback, Action completed)
    {
        Guard.Requires<ArgumentNullException>(source != null);
        if (source.Length == 0)
        {
            completed();
        }
        else
        {
            int index = 0;
            try
            {
                foreach (T obj in source)
                {
                    ++index;
                    process(obj);
                }
                completed();
            }
            finally
            {
                while (--index >= 0)
                    rollback(source[index]);
            }
        }
    }

    /// <summary>向前移动N位</summary>
    public static List<T> Forward<T>(this List<T> list, int step = 1)
    {
        if (step >= list.Count || step == 0)
        {
            Debug.LogError("移动无效");
            return list;
        }

        List<T> templist = new List<T>(step);
        for (int i = 0; i < step; i++)
            templist.Add(list[i]);
        for (int i = 0; i < list.Count; i++)
        {
            if (i + step < list.Count)
                list[i] = list[i + step];
            else
            {
                list[i] = templist[Mathf.Abs(list.Count - step - i)];
            }
        }
        return list;
    }

    /// <summary>向后滚动N位</summary>
    public static List<T> Back<T>(this List<T> list, int step = 1)
    {
        if (step >= list.Count || step == 0)
        {
            Debug.LogError("移动无效");
            return list;
        }

        List<T> templist = new List<T>(step);
        for (int i = list.Count - step; i < list.Count; i++)
            templist.Add(list[i]);

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (i >= step)
                list[i] = list[i - step];
            else
                list[i] = templist[i];
        }
        return list;
    }

    /// <summary>向前移动N位</summary>
    public static T[] Forward<T>(T[] Arrary, int step = 1)
    {
        if (step >= Arrary.Length || step == 0)
        {
            Debug.LogError("移动无效");
            return Arrary;
        }

        List<T> templist = new List<T>(step);
        for (int i = 0; i < step; i++)
            templist.Add(Arrary[i]);

        for (int i = 0; i < Arrary.Length; i++)
        {
            if (i + step < Arrary.Length)
                Arrary[i] = Arrary[i + step];
            else
                Arrary[i] = templist[Mathf.Abs(Arrary.Length - step - i)];
        }
        return Arrary;
    }

    /// <summary>向后滚动N位</summary>
    public static T[] Back<T>(T[] Arrary, int step = 1)
    {
        if (step >= Arrary.Length || step == 0)
        {
            Debug.LogError("移动无效");
            return Arrary;
        }

        List<T> templist = new List<T>(step);
        for (int i = Arrary.Length - step; i < Arrary.Length; i++)
            templist.Add(Arrary[i]);

        for (int i = Arrary.Length - 1; i >= 0; i--)
        {
            if (i >= step)
                Arrary[i] = Arrary[i - step];
            else
                Arrary[i] = templist[i];
        }
        return Arrary;
    }

    public static List<T> ToList<T>(T[] array)
    {
        return array.ToList();
    }

    public static IEnumerable<T> CustomWhere<T>(this List<T> list, Func<T, bool> predicate)
    {
        return list.Where(predicate).ToArray();
    }

    /// <summary>List_Skip往后选择指定位数的元素   Take从头开始选取固定数量的元素</summary>
    public static List<T> SubArray<T>(this List<T> list, int startIndex, int length = -1)
    {
        if (length == -1)
            return list.Skip(startIndex).ToList();

        return list.Skip(startIndex).Take(length).ToList();
    }

    /// <summary>选取符合条件的元素</summary>
    public delegate bool SelectAction<T>(T o);

    public static IEnumerable<T> ToArray<T>(this List<T> list)
    {
        return list.ToArray();
    }

    public static IEnumerable<T> Selectarray<T>(this List<T> list, SelectAction<T> selectAction)
    {
        return (from n in list where selectAction(n) select n);
    }

    /// <summary>取满足指定条件元素</summary>
    public static IEnumerable<T> TakeWhilearray<T>(this List<T> list, Func<T, bool> func)
    {
        return list.TakeWhile(func);
    }

    /// <summary>将满足指定条件元素过滤后选择剩余的所有元素</summary>
    public static IEnumerable<T> SkipWhilearray<T>(this List<T> list, Func<T, bool> func)
    {
        return list.SkipWhile(func);
    }

    /// <summary>自定义选择元素</summary>
    public static IEnumerable<T> CustomWhere<T>(T[] arrary, Func<T, bool> predicate)
    {
        return arrary.Where(predicate).ToArray();
    }

    /*------------比较差异  与两个数组或者List的前后顺序有关
     *            比较的是前一个中不同于后一个的元素，
     *            而不是把所有元素都获取出来-----------------------------------*/

    /// <summary>比较数组并返回相同元素List</summary>
    public static List<T> CustomIntersect<T>(List<T> list1, List<T> list2)
    {
        return list1.Intersect(list2).ToList();
    }

    /// <summary>比较数组并返回相同元素Arrary</summary>
    public static IEnumerable<T> CustomIntersect<T>(T[] array1, T[] array2)
    {
        return array1.Intersect(array2).ToArray();
    }

    /// <summary>比较数组并返回差异元素List</summary>
    public static List<T> CustomExcept<T>(List<T> list1, List<T> list2)
    {
        return list1.Except(list2).ToList();
    }

    /// <summary>比较数组并返回差异元素Arrary</summary>
    public static IEnumerable<T> CustomExcept<T>(T[] array1, T[] array2)
    {
        return array1.Except(array2).ToArray();
    }

    /// <summary>返回数组差异元素 注!!: 这个只能在确定array2比array1少并且其他元素一样的情况下才有比较好的结果</summary>
    public static IEnumerable<T> GetArrayDifference<T>(T[] array1, T[] array2)
    {
        return array1.Where(x => !array2.Contains(x)).ToArray();
    }

    /// <summary>array_Skip往后选择指定位数的元素   Take从头开始选取固定数量的元素</summary>
    public static T[] SubArray<T>(this T[] array, int startIndex, int length = -1)
    {
        if (length == -1)
            return array.Skip(startIndex).ToArray();

        return array.Skip(startIndex).Take(length).ToArray();
    }

    /// <summary>比较数组每一个值是否相等</summary>
    public static bool IsArrayEqual<T>(T[] array1, T[] array2)
    {
        return Enumerable.SequenceEqual(array1, array2);
    }

    /// <summary>删除所传入的数组的最后一个元素</summary>
    public static T ArrayPop<T>(ref T[] oldArray)
    {
        T result = oldArray[oldArray.Length - 1];
        T[] array = new T[oldArray.Length - 1];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = oldArray[i];
        }
        oldArray = array;
        return result;
    }

    /// <summary>删除所传入的数组的第一个元素</summary>
    public static T ArrayShift<T>(ref T[] oldArray)
    {
        T result = oldArray[0];
        T[] array = new T[oldArray.Length - 1];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = oldArray[i + 1];
        }
        oldArray = array;
        return result;
    }

    /// <summary>
    /// 添加新元素到数组最后
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="oldArray">被添加元素的数组</param>
    /// <param name="newElement">被添加的新元素</param>
    /// <returns>新元素将被添加到数组末尾，并返回一个新数组对象，该数组长度比以前大</returns>
    public static T[] ArrayPush<T>(T[] oldArray, T[] newElement)
    {
        int num = oldArray.Length + newElement.Length;
        T[] array = new T[num];
        for (int i = 0; i < array.Length; i++)
        {
            bool flag = i < array.Length - newElement.Length;
            if (flag)
            {
                array[i] = oldArray[i];
            }
            else
            {
                array[i] = newElement[i - oldArray.Length];
            }
        }
        return array;
    }

    /// <summary>
    /// 添加新元素到数组最前
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="oldArray">被添加元素的数组</param>
    /// <param name="newElement">被添加的新元素</param>
    /// <returns>新元素将被添加到数组最前，并返回一个新数组对象，该数组长度比以前大</returns>
    public static T[] ArrayUnshift<T>(T[] oldArray, T[] newElement)
    {
        int num = oldArray.Length + newElement.Length;
        T[] array = new T[num];
        for (int i = 0; i < array.Length; i++)
        {
            bool flag = i < newElement.Length;
            if (flag)
            {
                array[i] = newElement[i];
            }
            else
            {
                array[i] = oldArray[i - newElement.Length];
            }
        }
        return array;
    }

    /// <summary>
    /// 队列Queue 平滑 过滤波器
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="data"></param>
    /// <param name="maxCount = 100"></param>
    /// <returns></returns>
    public static Vector3 QueueAverage(ref Queue<Vector3> queue, Vector3 data,int maxCount)
    {
        queue.Enqueue(data);
        if (queue.Count > maxCount)
            queue.Dequeue();
        Vector3 pos = Vector3.zero;
        foreach (var item in queue)
            pos += item;
        Vector3 averagePos = pos / queue.Count;
        pos = Vector3.zero;
        return averagePos;
    }

    /// <summary>
    /// 过滤波器
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="data"></param>
    /// <param name="filterLength"></param>
    /// <returns></returns>
    public static Vector3 LowPassAccelerometer(ref Queue<Vector3> queue, Vector3 data,int filterLength)
    {
        if (filterLength <= 0)
            return data;
        queue.Enqueue(data);
        queue.Dequeue();

        Vector3 vFiltered = Vector3.zero;
        foreach (Vector3 v in queue)
            vFiltered += v;
        vFiltered /= filterLength;
        return vFiltered;
    }

    /// <summary>
    /// list，Arrary 复合排序
    /// </summary>
    /// <returns></returns>
    //var xxxx = LinqUtil.OrderBy<GameObject,string,Vector3,string>(list.ToArray(), item => item.name,item=>item.transform.localScale);
    public static IEnumerable<T> OrderBy<T,T1,T2,T3>(T[] Arrary,Func<T,T1>keySelector1, Func<T, T2> keySelector2 = null, Func<T, T3> keySelector3 = null) where T : UnityEngine.Object
    {
        return Arrary.OrderBy(keySelector1).ThenBy(keySelector2).ThenBy(keySelector3);
    }

    /// <summary>
    /// 复制 克隆 数组 浅拷贝
    /// </summary>
    public static T[] Clone<T>(T[] Arrary)
    {
        T[] clone = (T[])Arrary.Clone();
        return clone;
    }

    public static void Fill<T>(T[] src, T data)
    {
        for (int i = 0, max = src.Length; i < max; i++)
        {
            src[i] = data;
        }
    }

    public static T[][] NewDoubleArray<T>(int numRow, int numCol)
    {
        T[][] ret = new T[numRow][];
        for (int i = 0; i < numRow; i++)
        {
            ret[i] = new T[numCol];
        }

        return ret;
    }

    public static void Shuffle(Array array)
    {
        for (int i = 0, max = array.Length; i < max; i++)
        {
            int r = UnityEngine.Random.Range(i, max);
            object obj = array.GetValue(i);
            object robj = array.GetValue(r);
            array.SetValue(robj, i);
            array.SetValue(obj, r);
        }
    }

    public static void Shuffle(Array array, int start, int length)
    {
        for (int i = start; i < length; i++)
        {
            int r = UnityEngine.Random.Range(i, length);
            object obj = array.GetValue(i);
            object robj = array.GetValue(r);
            array.SetValue(robj, i);
            array.SetValue(obj, r);
        }
    }

    public static T[] RemoveAt<T>(T[] src, int index)
    {
        T[] dest = new T[src.Length - 1];
        if (index > 0)
        {
            Array.Copy(src, 0, dest, 0, index);
        }

        if (index < src.Length - 1)
        {
            Array.Copy(src, index + 1, dest, index, src.Length - index - 1);
        }

        return dest;
    }

    public static void RemoveAtIndex<T>(ref T[] src, int index)
    {
        T[] dest = new T[src.Length - 1];
        if (index > 0)
        {
            Array.Copy(src, 0, dest, 0, index);
        }

        if (index < src.Length - 1)
        {
            Array.Copy(src, index + 1, dest, index, src.Length - index - 1);
        }

        src = dest;
    }

    public static void InsertAfter<T>(ref T[] src, int index, T val)
    {
        if (index == src.Length - 1)
        {
            System.Array.Resize(ref src, src.Length + 1);
            src[src.Length - 1] = val;
        }
        else
        {
            T[] dest = new T[src.Length + 1];

            Array.Copy(src, dest, index + 1);

            dest[index + 1] = val;

            Array.Copy(src, index + 1, dest, index + 2, src.Length - (index + 1));

            src = dest;
        }
    }

    public static bool Contains(string[] src, string itm)
    {
        if (src != null)
        {
            for (int i = 0; i < src.Length; i++)
                if (src[i] == itm)
                    return true;
        }
        return false;
    }

    public static bool Contains<T>(T[] src, T itm) where T : UnityEngine.Object
    {
        if (src != null)
        {
            for (int i = 0; i < src.Length; i++)
                if (src[i] == itm)
                    return true;
        }
        return false;
    }

    public static bool Contains(string[] src, int startIndex, int count, string itm)
    {
        if (src != null)
        {
            for (int i = 0; i < count; i++)
                if (src[i + startIndex] == itm)
                    return true;
        }
        return false;
    }

    public static bool Contains<T>(T[] src, int startIndex, int count, T itm) where T : UnityEngine.Object
    {
        if (src != null)
        {
            for (int i = 0; i < count; i++)
                if (src[i + startIndex] == itm)
                    return true;
        }
        return false;
    }

    public static int[] GenerateIndexArray(Array array)
    {
        int[] indices = new int[array.Length];
        for (int i = 0; i < indices.Length; i++)
            indices[i] = i;
        return indices;
    }

    #region 排序算法
    #region 归并排序
    /// <summary>
    /// 归并排序
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam>
    /// <param name="array">被操作数组</param>
    /// <param name="first">首位置点</param>
    /// <param name="last">尾位置点</param>
    public static void MergeSortFunction<T>(ref T[] array, int first, int last) where T : IComparable<T>
    {
        try
        {
            bool flag = first < last;
            if (flag)
            {
                int num = (first + last) / 2;
                MergeSortFunction<T>(ref array, first, num);
                MergeSortFunction<T>(ref array, num + 1, last);
                MergeSortCore<T>(ref array, first, num, last);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// 将两个有序的左右子表（以mid区分），合并成一个有序的表
    /// </summary>
    /// <typeparam name="T">数组类型</typeparam>
    /// <param name="array">被操作的数组</param>
    /// <param name="first">左子组首位置</param>
    /// <param name="mid">中间</param>
    /// <param name="last">右子组首位置</param>
    private static void MergeSortCore<T>(ref T[] array, int first, int mid, int last) where T : IComparable<T>
    {
        try
        {
            int i = first;
            int j = mid + 1;
            T[] array2 = new T[last + 1];
            int num = 0;
            while (i <= mid && j <= last)
            {
                bool flag = array[i].CompareTo(array[j]) <= 0;
                if (flag)
                {
                    array2[num++] = array[i++];
                }
                else
                {
                    array2[num++] = array[j++];
                }
            }
            while (i <= mid)
            {
                array2[num++] = array[i++];
            }
            while (j <= last)
            {
                array2[num++] = array[j++];
            }
            num = 0;
            for (int k = first; k <= last; k++)
            {
                array[k] = array2[num++];
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    #endregion

    #region 插入排序
    /// <summary>
    /// 插入排序（必须为实现了IComparable的对象类型）
    /// </summary>
    /// <param name="Array">被排序的数组</param>
    /// <param name="isAscending">是否为升序</param>
    /// <param name="val">要添加进的元素</param>
    public static void InsertSort<T>(List<T> Array, bool isAscending = true, params T[] val) where T : IComparable<T>
    {
        T value = default(T);
        bool flag = val.Length != 0;
        if (flag)
        {
            val.ForEach(delegate (T v)
            {
                Array.Add(v);
            });
        }
        int count = Array.Count;
        for (int i = 1; i < count; i++)
        {
            for (int j = i; j > 0; j--)
            {
                if (isAscending)
                {
                    T t = Array[j];
                    int num = t.CompareTo(Array[j - 1]);
                    bool flag2 = num < 0;
                    if (flag2)
                    {
                        value = Array[j];
                        Array[j] = Array[j - 1];
                        Array[j - 1] = value;
                    }
                }
                else
                {
                    T t = Array[j];
                    int num2 = t.CompareTo(Array[j - 1]);
                    bool flag3 = num2 > 0;
                    if (flag3)
                    {
                        value = Array[j];
                        Array[j] = Array[j - 1];
                        Array[j - 1] = value;
                    }
                }
            }
        }
    }
    #endregion

    #region 基数排序
    /// <summary>
    /// 基数排序(仅自然数)可以在外部处理为同符号后处理再乘回去
    /// </summary>
    /// <param name="arr">被排序的整型数组</param>
    public static void RadixSort(ref int[] arr)
    {
        int maxLength = MathHelpr.GetMaxLength(arr);
        RadixSort(ref arr, maxLength);
    }

    private static void RadixSort(ref int[] arr, int iMaxLength)
    {
        List<int> list = new List<int>();
        List<int>[] array = new List<int>[10];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new List<int>();
        }
        for (int j = 0; j < iMaxLength; j++)
        {
            int[] array2 = arr;
            int k = 0;
            while (k < array2.Length)
            {
                int item = array2[k];
                string text = item.ToString();
                char c;
                try
                {
                    c = text[text.Length - 1 - j];
                }
                catch
                {
                    array[0].Add(item);
                    goto IL_14C;
                }
                goto IL_80;
                IL_14C:
                k++;
                continue;
                IL_80:
                switch (c)
                {
                    case '0':
                        array[0].Add(item);
                        break;
                    case '1':
                        array[1].Add(item);
                        break;
                    case '2':
                        array[2].Add(item);
                        break;
                    case '3':
                        array[3].Add(item);
                        break;
                    case '4':
                        array[4].Add(item);
                        break;
                    case '5':
                        array[5].Add(item);
                        break;
                    case '6':
                        array[6].Add(item);
                        break;
                    case '7':
                        array[7].Add(item);
                        break;
                    case '8':
                        array[8].Add(item);
                        break;
                    case '9':
                        array[9].Add(item);
                        break;
                    default:
                        throw new Exception("未知错误");
                }
                goto IL_14C;
            }
            for (int l = 0; l < array.Length; l++)
            {
                int[] array3 = (int[])array[l].ToArray<int>();
                for (int m = 0; m < array3.Length; m++)
                {
                    int item2 = array3[m];
                    list.Add(item2);
                    array[l].Clear();
                }
            }
            arr = (int[])list.ToArray<int>();
            list.Clear();
        }
    }
    #endregion

    #endregion

    /// <summary>
    /// 泛型为所有对象组提供一个扩展方法，遍历该对象组并执行一个委托方法
    /// </summary>
    /// <typeparam name="T">对象类别,必须是实现了IComparable接口的类别</typeparam>
    /// <param name="n">被遍历的对象组</param>
    /// <param name="action">执行委托方法</param>
    public static void ForEach<T>(this T[] n, Action<T> action) where T : IComparable<T>
    {
        bool flag = n.Length != 0;
        if (flag)
        {
            for (int i = 0; i < n.Length; i++)
            {
                action(n[i]);
            }
        }
    }

    /// <summary>
    /// 获取调和级数到达目标值的元素数量
    /// </summary>
    /// <param name="Denominator">调和级数基础分母值（第一项的分母值）</param>
    /// <param name="Arithmetic">分母等差值</param>
    /// <param name="targetValue">目标值</param>
    /// <returns>返回该级数的元素数量</returns>
    public static ulong HarmonicSeriesCumulation(float Denominator, float Arithmetic, float targetValue)
    {
        ulong num = 0uL;
        try
        {
            double num2 = 0.0;
            while (num2 < (double)targetValue)
            {
                num2 += (double)(1f / (Denominator + Arithmetic * num));
                num += 1uL;
            }
        }
        catch
        {
            throw new Exception("目标值超出可计算范围！");
        }
        return num;
    }


    /// <summary>
    /// 获取调和级数的和1/2 + 1/4 + 1/6 + 1/8
    /// </summary>
    /// <param name="Denominator">基础分母</param>
    /// <param name="Arithmetic">等差值</param>
    /// <param name="ElementNum">元素数量</param>
    /// <returns>返回该级数的和</returns>
    public static float HarmonicSeriesForSum(float Denominator, float Arithmetic, int ElementNum)
    {
        ElementNum--;
        float num = 0f;
        while (ElementNum >= 0)
        {
            float num2 = Denominator + Arithmetic * (float)ElementNum;
            bool flag = num2 != 0f;
            if (flag)
            {
                num += 1f / num2;
            }
            ElementNum--;
        }
        return num;
    }

    /// <summary>
    /// 霍纳法则运算
    /// 求多项式值的合快速算法
    /// </summary>
    /// <param name="Arr">数组</param>
    /// <param name="x">幂参数值</param>
    /// <returns>返回该数组的x值次方的多项式集合的和</returns>
    public static int Horner(int[] Arr, int x)
    {
        int num = 0;
        int num2 = Arr.Length - 1;
        bool flag = num2 >= 1;
        if (flag)
        {
            for (int i = 0; i < num2; i++)
            {
                num = x * (Arr[num2 - i] + num);
            }
        }
        return num + Arr[0];
    }

    /// <summary>
    /// 双精度浮点数组扩展方法，获取这个数组的总和
    /// </summary>
    /// <param name="d">数组</param>
    /// <returns>返回数组总和</returns>
    public static double GetSum(this double[] d)
    {
        double num = 0.0;
        for (int i = 0; i < d.Length; i++)
        {
            double num2 = d[i];
            num += num2;
        }
        return num;
    }
}

//alist = list.Distinct(new ListComparer<int>((p1, p2) => p1 == p2)).ToList();
/// <summary>
/// 去除重复元素
/// </summary>
public class ListComparer<T> : IEqualityComparer<T>
{
    public delegate bool EqualsComparer<F>(F x, F y);

    public EqualsComparer<T> equalsComparer;

    public ListComparer(EqualsComparer<T> _euqlsComparer)
    {
        this.equalsComparer = _euqlsComparer;
    }
    public bool Equals(T x, T y)
    {
        if (null != equalsComparer)
            return equalsComparer(x, y);
        else
            return false;
    }
    public int GetHashCode(T obj)
    {
        return obj.ToString().GetHashCode();
    }
}

#endregion

#region 随机数
public static class RandomUtil
{
    /// <summary>
    /// Return random bool value.
    /// </summary>
    public static bool NextBool()
    {
        return Random.Range(0, 2) == 0;
    }

    /// <summary>
    /// Return random item from item1 and item2 set.
    /// </summary>
    public static T Next<T>(T item1, T item2)
    {
        return NextBool() ? item1 : item2;
    }

    /// <summary>
    /// Return random item from item1, item2 and item3 set.
    /// </summary>
    public static T Next<T>(T item1, T item2, T item3)
    {
        int n = Random.Range(0, 3);
        return n == 0 ? item1 : (n == 1 ? item2 : item3);
    }

    /// <summary>
    /// Return random item from array.
    /// </summary>
    public static T NextItem<T>(T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Return random item from list.
    /// </summary>
    public static T NextItem<T>(List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Return random enum item.
    /// </summary>
    public static T NextEnum<T>()
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(Random.Range(0, values.Length));
    }

    /// <summary>
    /// Return random index of passed array. Index random selection is based on array weights.
    /// </summary>
    public static int NextWeightedInd(int[] weights)
    {
        int randomPoint = Random.Range(0, weights.Sum()) + 1;
        int sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (randomPoint <= sum)
                return i;
        }
        throw new Exception("Logic error!");
    }

    /// <summary>
    /// Return random index of passed array. Index random selection is based on array weights.
    /// </summary>
    public static int NextWeightedInd(float[] weights)
    {
        float randomPoint = Random.Range(0f, weights.Sum());
        float sum = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
            if (randomPoint <= sum)
                return i;
        }
        throw new Exception("Logic error!");
    }

    /// <summary>
    /// Return sub-list of random items from origin list without repeating.
    /// </summary>
    public static List<T> Take<T>(List<T> list, int count)
    {
        List<T> items = new List<T>();
        List<int> remainedIndexes = Enumerable.Range(0, list.Count).ToList();
        for (int i = 0; i < count; i++)
        {
            int selectedIndex = NextItem(remainedIndexes);
            remainedIndexes.Remove(selectedIndex);
            items.Add(list[selectedIndex]);
        }
        return items;
    }

    /// <summary>
    /// Shuffle list of items.
    /// </summary>
    public static void Shuffle<T>(this List<T> list)
    {
        for (int i = 1; i < list.Count; i++)
        {
            int indRnd = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[indRnd];
            list[indRnd] = temp;
        }
    }

    public static void ShuffleList<T>(List<T> list)
    {
        for (int i = 0, max = list.Count; i < max; i++)
        {
            int r = UnityEngine.Random.Range(i, max);
            T obj = list[i];
            T robj = list[r];
            list[i] = robj;
            list[r] = obj;
        }
    }

    /// <summary>
    /// Shuffle array of items.
    /// </summary>
    public static void Shuffle<T>(T[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            int indRnd = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[indRnd];
            array[indRnd] = temp;
        }
    }

    /// <summary>
    /// Return random point on line.
    /// </summary>
    public static Vector2 NextPointOnLine(Vector2 point1, Vector2 point2)
    {
        float t = Random.Range(0f, 1f);
        return new Vector2(Mathf.Lerp(point1.x, point2.x, t), Mathf.Lerp(point1.y, point2.y, t));
    }

    /// <summary>
    /// Return random point on line.
    /// </summary>
    public static Vector3 NextPointOnLine(Vector3 point1, Vector3 point2)
    {
        float t = Random.Range(0f, 1f);
        return new Vector3(Mathf.Lerp(point1.x, point2.x, t), Mathf.Lerp(point1.y, point2.y, t), Mathf.Lerp(point1.z, point2.z, t));
    }

    /// <summary>
    /// Get a chance with given percentage. If percentage is 25 it will return true each 4th time on an average.
    /// </summary>
    public static bool GetChance(int percentage)
    {
        return Random.Range(0, 100) + 1 <= percentage;
    }

    /// <summary>
    /// Gets a chance with give probability. If probability is 0.25 it will return true each 4th time on an average.
    /// </summary>
    public static bool GetChance(float probability)
    {
        return Random.Range(0f, 1f) < probability;
    }

    /// <summary>
    /// Get random normalized 2D direction as Vector2.
    /// </summary>
    public static Vector2 NextDirection()
    {
        return Random.insideUnitCircle.normalized;
    }

    /// <summary>
    /// Return Random.Range between two values which are stored in pairMinMax Vector2.
    /// Where pairMinMax.x is min value and pairMinMax.y is max value.
    /// </summary>
    public static float Range(Vector2 pairMinMax)
    {
        return Random.Range(pairMinMax.x, pairMinMax.y);
    }

    /// <summary>
    /// Return random point from rect bound (inside rect).
    /// </summary>
    public static Vector2 NextPointOnRect(Rect rect)
    {
        return new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
    }

    /// <summary>
    /// Return random point on rect border (perimeter of rect).
    /// </summary>
    public static Vector2 NextPointOnRectBorder(Rect rect)
    {
        float perimeterLength = (rect.width + rect.height) * 2f;
        float pointOnPerimeter = Random.Range(0f, perimeterLength);

        if (pointOnPerimeter < rect.width)//top border
            return new Vector2(rect.xMin + pointOnPerimeter, rect.yMax);

        pointOnPerimeter -= rect.width;

        if (pointOnPerimeter < rect.height)//right border
            return new Vector2(rect.xMax, rect.yMin + pointOnPerimeter);

        pointOnPerimeter -= rect.height;

        if (pointOnPerimeter < rect.width)//bottom border
            return new Vector2(rect.xMin + pointOnPerimeter, rect.yMin);

        pointOnPerimeter -= rect.width;

        //left border
        return new Vector2(rect.xMin, rect.yMin + pointOnPerimeter);
    }

    /// <summary>获取array随机值</summary>
    public static T GetRandom<T>(this T[] arrary)
    {
        if (arrary == null || arrary.Length == 0)
            return default(T);

        return arrary[Random.Range(0, arrary.Length)];
    }

    /// <summary>获取List随机值</summary>
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);

        return list[Random.Range(0, list.Count)];
    }

    /// <summary>获取正负随机值</summary>
    public static int GetRandom(int min, int max, ref int lastRandom)
    {
        if (min * max > 0)
        {
            Debug.LogError("Min*Max非正负数");
            return 0;
        }
        if (lastRandom == 0)
        {
            Debug.LogError("lastRandom必须为负数");
            return 0;
        }

        int random;
        do
        {
            random = Random.Range(min, max);
        }
        while (lastRandom * random >= 0);
        lastRandom = random;
        return random;
    }

    /// <summary>获取一个每次跟上次相差step长度的随机值</summary>
    public static int GetRandom(int min, int max, int step, ref int lastRandom)
    {
        if (step >= Math.Abs(max - min))
        {
            Debug.LogError("步长超限");
            return max;
        }

        int random;
        do
        {
            random = Random.Range(min, max);
        }
        while (Math.Abs(lastRandom - random) < step);
        lastRandom = random;
        return random;
    }

    /// <summary>curve.Evaluate(Random.value)参数是随机出的一个值，可以认为是图中的横坐标，函数会返回它所对应的竖坐标的值</summary>
    public static float GetRandom(AnimationCurve curve)
    {
        return curve.Evaluate(Random.value);
    }

    /// <summary>curve.Evaluate(Random.value)参数是随机出的一个值，可以认为是图中的横坐标，函数会返回它所对应的竖坐标的值</summary>
    public static int GetRandomInt(AnimationCurve curve)
    {
        return Mathf.RoundToInt(GetRandom(curve));
    }

    /// <summary>从一组数组中随机选择一个数，随机概率</summary>
    public static float GetRandom(float[] probs)
    {
        float total = 0;
        foreach (float elem in probs)
            total += elem;

        float randomPoint = Random.value * total;
        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
                return i;
            else
                randomPoint -= probs[i];
        }
        return probs.Length - 1;
    }

    /// <summary>将一组数组整体随机打乱，洗牌</summary>
    public static int[] GetRandomArray(int[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int temp = cards[i];
            int randomIndex = Random.Range(0, cards.Length);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
        return cards;
    }

    // <summary>
    /// 随机生成N个和为目标数的正整数
    /// </summary>
    /// <param name="TotalNum">目标总数</param>
    /// <param name="n">生成的数字量,必然大于1，默认为2</param>
    /// <param name="AbsolutelyAlone">结果值是否绝对唯一，如果为true，则不保证数量符合需求,且计算复杂度增加</param>
    /// <returns>返回一个无符号整型数组</returns>
    public static uint[] GetRandomNumforAll(uint TotalNum, int n = 2, bool AbsolutelyAlone = false)
    {
        System.Random random = new System.Random();
        bool flag = n <= 1 || (long)n > (long)((ulong)TotalNum);
        if (flag)
        {
            while (n <= 1 || (long)n > (long)((ulong)TotalNum))
            {
                n = random.Next(2, (int)(TotalNum + 1u));
            }
        }
        n--;
        List<uint> list = new List<uint>();
        List<uint> list2 = new List<uint>();
        bool flag2 = AbsolutelyAlone;
        do
        {
            list.Add(0u);
            for (int i = 0; i < n; i++)
            {
                uint item = (uint)random.Next(1, (int)TotalNum);
                list.Add(item);
            }
            list.Add(TotalNum);
            LinqUtil.InsertSort<uint>(list, true, new uint[0]);
            for (int j = 0; j < list.Count - 1; j++)
            {
                list2.Add((uint)Math.Abs((long)((ulong)(list[j + 1] - list[j]))));
            }
            bool flag3 = flag2;
            if (flag3)
            {
                flag2 = false;
                HashSet<uint> source = new HashSet<uint>(list2);
                source.ToList<uint>();
                bool flag4 = source.ToList<uint>().Count != list2.Count;
                if (flag4)
                {
                    flag2 = true;
                }
                bool flag5 = flag2;
                if (flag5)
                {
                    list2.Clear();
                    list.Clear();
                    n = random.Next(2, (int)(TotalNum + 1u));
                }
            }
        }
        while (flag2);
        uint[] array = new uint[list2.Count];
        for (int k = 0; k < array.Length; k++)
        {
            array[k] = list2[k];
        }
        return array;
    }

    /// <summary>从一组数组中随机选择N个数作为新的数组</summary>

    public static T[] GetRandomArray<T>(int numRequired, T[] spawnPoints)
    {
        T[] result = new T[numRequired];
        int numToChoose = numRequired;
        for (int numLeft = spawnPoints.Length; numLeft > 0; numLeft--)
        {
            float prob = ((float)numToChoose) / ((float)numLeft);
            if (Random.value < prob)
            {
                numToChoose--;
                result[numToChoose] = spawnPoints[numLeft - 1];
                if (numToChoose == 0)
                    break;
            }
        }
        return result;
    }

    /// <summary>获取不相等随机数,HashSet</summary>
    public static List<int> GetRandomList(int startIndex, int Length)
    {
        HashSet<int> hs = new HashSet<int>();
        System.Random r = new System.Random();
        while (hs.Count < Length)
            hs.Add(r.Next(startIndex, startIndex + Length));
        return hs.ToList();
    }

    /// <summary>获取不相等随机数,种子</summary>
    public static void CreatRangdomList(List<int> setList, int Length)
    {
        setList.Clear();
        List<int> startList = new List<int>();
        for (int i = 0; i < Length; i++)
        {
            startList.Add(i);
        }
        int N = startList.Count;
        int[] resultArray = new int[startList.Count];
        for (int i = 0; i < N; i++)
        {
            int seed = Random.Range(0, startList.Count - i);
            resultArray[i] = startList[seed];
            startList[seed] = startList[startList.Count - i - 1];
            setList.Add(resultArray[i]);
        }
    }
}
#endregion

#region Texture拓展
public enum EncodeType
{
    JPG,
    PNG
}

public enum PsType
{
    /// <summary>
    /// 发黄老旧照片
    /// </summary>
    Sepia,
    /// <summary>
    /// 置灰
    /// </summary>
    Grayscale,
    /// <summary>
    /// 像素化
    /// </summary>
    Pixelate,
    /// <summary>
    /// 反色
    /// </summary>
    Negative,
    /// <summary>
    /// 毛玻璃
    /// </summary>
    Foggy,
    /// <summary>
    /// 柔化
    /// </summary>
    Soft,
    /// <summary>
    /// 锐化
    /// </summary>
    Sharp,
    /// <summary>
    /// 浮雕
    /// </summary>
    Relief,
}

public static class TextureUtil
{
    /// <summary>Texture2D转Sprite</summary>
    public static Sprite Texture2DToSprite(Texture2D tex)
    {
        if (tex == null) { Debug.LogError("tex为空"); return null; }
        Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        return spr;
    }

    /// <summary>Texture转Texture2D</summary>
    public static Texture2D TextureToTexture2D(Texture tex)
    {
        if (tex == null) { Debug.LogError("tex为空"); return null; }
        return tex as Texture2D;
    }

    /// <summary>Texture转byte[]</summary>
    public static byte[] GetPhotoPixel(Texture tex, EncodeType encodeType)
    {
        if (tex == null) { Debug.LogError("tex为空"); return null; }
        Texture2D texture = tex as Texture2D;
        byte[] bytes = null;
        switch (encodeType)
        {
            case EncodeType.JPG:
                bytes = texture.EncodeToJPG();
                break;
            case EncodeType.PNG:
                bytes = texture.EncodeToPNG();
                break;
            default:
                break;
        }
        return bytes;
    }

    /// <summary>
    /// 根据图片路径返回Texture2d格式图片
    /// </summary>
    /// <param name="imgPath"></param>
    /// <returns></returns>
    public static Texture2D GetTexrture2DFromPath(string imgPath)
    {
        //读取文件
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();
        //转化为Texture2D
        System.Drawing.Image img = System.Drawing.Image.FromStream(new MemoryStream(imgBytes));
        Texture2D t2d = new Texture2D(img.Width, img.Height);
        img.Dispose();
        t2d.LoadImage(imgBytes);
        t2d.Apply();
        return t2d;
    }

    /// <summary>
    /// RenderTexture转Texture2D
    /// </summary>
    /// <param name="rt"></param>
    /// <returns>这张渲染的一般是camera的renderTexture不然没有像素</returns>
    public static Texture2D GetTex2DByRenderTexture(RenderTexture rt)
    {
        if (rt == null) return null;
        Texture2D t2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        t2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        t2D.Apply();
        RenderTexture.active = null;
        return t2D;
    }

    /// <summary>
    /// 白平衡 亮度
    /// </summary>
    /// <param name="sourceTex"></param>
    /// <param name="strength"></param>
    /// <returns></returns>
    public static Texture2D SetTextureStrength(Texture2D sourceTex,float strength)
    {
        Color[] cs = sourceTex.GetPixels();
        Color[] cs2 = new Color[cs.Length];
        for (int i = 0; i < cs.Length; i++)
            cs2[i] = cs[i] * Mathf.Pow(2, strength / 2);

        Texture2D t = new Texture2D(sourceTex.width, sourceTex.height);
        t.SetPixels(cs2);
        t.Apply();
        return t;
    }

    /// <summary>
    /// 截图
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="tex"></param>
    /// <returns>
    /// 截取全屏   
    /// StartCoroutine(TextureUtil.GetScreenTexture(0, 0, Screen.width, Screen.height, null));
    /// </returns>
    public static IEnumerator GetScreenTexture(float x,float y,int width,int height,Texture tex)
    {
        yield return new WaitForEndOfFrame();
        Texture2D t = new Texture2D(width, height, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(x, y, width, height), 0, 0, true);
        t.Apply();
        tex = t;
    }

    /// <summary>
    /// 截图
    /// </summary>
    /// <param name="sourceTex"></param>
    /// <returns></returns>
    public static Texture CaptureTexture(Texture sourceTex)
    {
        RenderTexture renderTexture = new RenderTexture(sourceTex.width, sourceTex.height, 24);
        renderTexture.anisoLevel = 0;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.wrapMode = TextureWrapMode.Clamp;

        Graphics.Blit(sourceTex, renderTexture);
        return renderTexture;
    }

    public class ThreadData
    {
        public int start;
        public int end;
        public ThreadData(int s, int e)
        {
            start = s;
            end = e;
        }
    }

    private static Color[] texColors;
    private static Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    private static Mutex mutex;

    public static void ScalePoint(Texture2D tex, int newWidth, int newHeight)
    {
        ThreadedScale(tex, newWidth, newHeight, false);
    }

    public static void ScaleBilinear(Texture2D tex, int newWidth, int newHeight)
    {
        ThreadedScale(tex, newWidth, newHeight, true);
    }

    private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
    {
        texColors = tex.GetPixels();
        newColors = new Color[newWidth * newHeight];
        if (useBilinear)
        {
            ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
            ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
        }
        else
        {
            ratioX = ((float)tex.width) / newWidth;
            ratioY = ((float)tex.height) / newHeight;
        }
        w = tex.width;
        w2 = newWidth;
        var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
        var slice = newHeight / cores;

        finishCount = 0;
        if (mutex == null)
        {
            mutex = new Mutex(false);
        }
        if (cores > 1)
        {
            int i = 0;
            ThreadData threadData;
            for (i = 0; i < cores - 1; i++)
            {
                threadData = new ThreadData(slice * i, slice * (i + 1));
                ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                Thread thread = new Thread(ts);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice * i, newHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
            while (finishCount < cores)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            ThreadData threadData = new ThreadData(0, newHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
        }

        tex.Resize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();
    }

    public static void BilinearScale(System.Object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            int yFloor = (int)Mathf.Floor(y * ratioY);
            var y1 = yFloor * w;
            var y2 = (yFloor + 1) * w;
            var yw = y * w2;

            for (var x = 0; x < w2; x++)
            {
                int xFloor = (int)Mathf.Floor(x * ratioX);
                var xLerp = x * ratioX - xFloor;
                newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                       ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                       y * ratioY - yFloor);
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    public static void PointScale(System.Object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            var thisY = (int)(ratioY * y) * w;
            var yw = y * w2;
            for (var x = 0; x < w2; x++)
            {
                newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
    {
        return new Color(c1.r + (c2.r - c1.r) * value,
                          c1.g + (c2.g - c1.g) * value,
                          c1.b + (c2.b - c1.b) * value,
                          c1.a + (c2.a - c1.a) * value);
    }

    public static Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(fileData);
        }
        return tex;
    }

    // Save ScreenShot
    public static IEnumerator SaveScreenShot(string path, Rect rect, Vector2 size = default(Vector2))
    {
        // We should only read the screen bufferafter rendering is complete
        yield return new WaitForEndOfFrame();
        // 要保存图片的大小
        Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        // 截取的区域
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();
        // 把图片数据转换为byte数组
        if (size != default(Vector2))
        {
            ScaleBilinear(tex, (int)size.x, (int)size.y);
        }

        byte[] buffer = tex.EncodeToJPG(100);
        UnityEngine.Object.Destroy(tex);
        // 然后保存为图片
        File.WriteAllBytes(path, buffer);
    }

    public static Texture2D Copy(Texture2D tex)
    {
        return new Texture2D(tex.width, tex.height, tex.format, false);
    }

    public static Texture2D SetTexture(Texture2D tex,PsType psType)
    {
        Texture2D psTex = new Texture2D(tex.width, tex.height);
        switch (psType)
        {
            case PsType.Sepia:
                psTex = SetSepia(tex);
                break;
            case PsType.Grayscale:
                psTex = SetGrayscale(tex);
                break;
            case PsType.Pixelate:
                psTex = SetPixelate(tex);
                break;
            case PsType.Negative:
                psTex = SetNegative(tex);
                break;
            case PsType.Foggy:
                psTex = SetFoggy(tex);
                break;
            case PsType.Soft:
                psTex = SetSoft(tex);
                break;
            case PsType.Sharp:
                psTex = SetSharp(tex);
                break;
            case PsType.Relief:
                psTex = SetRelief(tex);
                break;
            default:
                break;
        }
        return psTex;
    }

    /// <summary>
    /// Applies sepia effect to the texture.发黄老旧照片
    /// </summary>
    /// <param name="tex"> Texture to process.</param>
    public static Texture2D SetSepia(Texture2D tex)
    {
        Texture2D t = Copy(tex);
        Color[] colors = tex.GetPixels();

        for (int i = 0; i < colors.Length; i++)
        {
            float alpha = colors[i].a;
            float grayScale = ((colors[i].r * .299f) + (colors[i].g * .587f) + (colors[i].b * .114f));
            Color c = new Color(grayScale, grayScale, grayScale);
            colors[i] = new Color(c.r * 1, c.g * 0.95f, c.b * 0.82f, alpha);
        }
        t.SetPixels(colors);
        t.Apply();
        return t;
    }

    /// <summary>
    /// Applies grayscale effect to the texture and changes colors to grayscale.置灰
    /// </summary>
    /// <param name="tex"> Texture to process.</param>
    public static Texture2D SetGrayscale(Texture2D tex)
    {
        Texture2D t = Copy(tex);

        Color[] colors = tex.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            float val = (colors[i].r + colors[i].g + colors[i].b) / 3;
            colors[i] = new Color(val, val, val);
        }
        t.SetPixels(colors);
        t.Apply();
        return t;
    }

    /// <summary>
    /// Pixelates the texture.像素化
    /// </summary>
    /// <param name="tex"> Texture to process.</param>
    /// <param name="size"> Size of the pixel.</param>
    public static Texture2D SetPixelate(Texture2D tex, int size = 10)
    {
        Texture2D t = Copy(tex);
        Rect rectangle = new Rect(0, 0, tex.width, tex.height);
        for (int xx = (int)rectangle.x; xx < rectangle.x + rectangle.width && xx < tex.width; xx += size)
        {
            for (int yy = (int)rectangle.y; yy < rectangle.y + rectangle.height && yy < tex.height; yy += size)
            {
                int offsetX = size / 2;
                int offsetY = size / 2;
                while (xx + offsetX >= tex.width) offsetX--;
                while (yy + offsetY >= tex.height) offsetY--;
                Color pixel = tex.GetPixel(xx + offsetX, yy + offsetY);
                for (int x = xx; x < xx + size && x < tex.width; x++)
                    for (int y = yy; y < yy + size && y < tex.height; y++)
                        t.SetPixel(x, y, pixel);
            }
        }

        t.Apply();
        return t;
    }

    /// <summary>
    /// Inverts colors of the texture.图片反色
    /// </summary>
    /// <param name="tex"> Texture to process.</param>
    public static Texture2D SetNegative(Texture2D tex)
    {
        Texture2D t = Copy(tex);
        Color[] colors = tex.GetPixels();
        Color pixel;

        for (int i = 0; i < colors.Length; i++)
        {
            pixel = colors[i];
            colors[i] = new Color(1 - pixel.r, 1 - pixel.g, 1 - pixel.b);
        }
        t.SetPixels(colors);
        t.Apply();
        return t;
    }

    /// <summary>
    /// Sets the foggy effect.毛玻璃效果
    /// </summary>
    /// <returns>texture processed.</returns>
    /// <param name="tex">texture to process.</param>
    public static Texture2D SetFoggy(Texture2D tex)
    {
        Texture2D t = Copy(tex);
        Color pixel;

        for (int x = 1; x < tex.width - 1; x++)
            for (int y = 1; y < tex.height - 1; y++)
            {
                int k = UnityEngine.Random.Range(0, 123456);
                //像素块大小
                int dx = x + k % 19;
                int dy = y + k % 19;
                if (dx >= tex.width)
                    dx = tex.width - 1;
                if (dy >= tex.height)
                    dy = tex.height - 1;
                pixel = tex.GetPixel(dx, dy);
                t.SetPixel(x, y, pixel);
            }

        t.Apply();

        return t;
    }

    /// <summary>
    /// Sets the soft effect.柔化效果
    /// </summary>
    /// <returns>texture processed.</returns>
    /// <param name="tex">texture to process.</param>
    public static Texture2D SetSoft(Texture2D tex)
    {
        Texture2D t = Copy(tex);
        int[] Gauss = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
        for (int x = 1; x < tex.width - 1; x++)
            for (int y = 1; y < tex.height - 1; y++)
            {
                float r = 0, g = 0, b = 0;
                int Index = 0;
                for (int col = -1; col <= 1; col++)
                    for (int row = -1; row <= 1; row++)
                    {
                        Color pixel = tex.GetPixel(x + row, y + col);
                        r += pixel.r * Gauss[Index];
                        g += pixel.g * Gauss[Index];
                        b += pixel.b * Gauss[Index];
                        Index++;
                    }
                r /= 16;
                g /= 16;
                b /= 16;
                //处理颜色值溢出
                r = r > 1 ? 1 : r;
                r = r < 0 ? 0 : r;
                g = g > 1 ? 1 : g;
                g = g < 0 ? 0 : g;
                b = b > 1 ? 1 : b;
                b = b < 0 ? 0 : b;
                t.SetPixel(x - 1, y - 1, new Color(r, g, b));
            }

        t.Apply();

        return t;
    }

    /// <summary>
    /// Sets the sharp.锐化效果
    /// </summary>
    /// <returns>The sharp.</returns>
    /// <param name="tex">Tex.</param>
    public static Texture2D SetSharp(Texture2D tex)
    {
        Texture2D t = Copy(tex);
        Color pixel;
        //拉普拉斯模板
        int[] Laplacian = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
        for (int x = 1; x < tex.width - 1; x++)
            for (int y = 1; y < tex.height - 1; y++)
            {
                float r = 0, g = 0, b = 0;
                int index = 0;
                for (int col = -1; col <= 1; col++)
                    for (int row = -1; row <= 1; row++)
                    {
                        pixel = tex.GetPixel(x + row, y + col); r += pixel.r * Laplacian[index];
                        g += pixel.g * Laplacian[index];
                        b += pixel.b * Laplacian[index];
                        index++;
                    }
                //处理颜色值溢出
                r = r > 1 ? 1 : r;
                r = r < 0 ? 0 : r;
                g = g > 1 ? 1 : g;
                g = g < 0 ? 0 : g;
                b = b > 1 ? 1 : b;
                b = b < 0 ? 0 : b;
                t.SetPixel(x - 1, y - 1, new Color(r, g, b));
            }

        t.Apply();
        return t;
    }

    /// <summary>
    /// Sets the relief.浮雕效果
    /// </summary>
    /// <returns>The relief.</returns>
    /// <param name="tex">Tex.</param>
    public static Texture2D SetRelief(Texture2D tex)
    {
        Texture2D t = Copy(tex);

        for (int x = 0; x < tex.width - 1; x++)
        {
            for (int y = 0; y < tex.height - 1; y++)
            {
                float r = 0, g = 0, b = 0;
                Color pixel1 = tex.GetPixel(x, y);
                Color pixel2 = tex.GetPixel(x + 1, y + 1);
                r = Mathf.Abs(pixel1.r - pixel2.r + 0.5f);
                g = Mathf.Abs(pixel1.g - pixel2.g + 0.5f);
                b = Mathf.Abs(pixel1.b - pixel2.b + 0.5f);
                if (r > 1)
                    r = 1;
                if (r < 0)
                    r = 0;
                if (g > 1)
                    g = 1;
                if (g < 0)
                    g = 0;
                if (b > 1)
                    b = 1;
                if (b < 0)
                    b = 0;
                t.SetPixel(x, y, new Color(r, g, b));
            }
        }

        t.Apply();
        return t;
    }
}
#endregion

#region 颜色拓展
/// <summary>文字颜色</summary>
public enum STRING_COLORS
{
    /// <summary>蓝色</summary>
    Blue,
    /// <summary>绿色</summary>
    Green,
    /// <summary>红色</summary>
    Red,
    /// <summary>灰色</summary>
    Gray,
    /// <summary>橙色</summary>
    Orange,
    /// <summary>淡灰</summary>
    ThinGray,
    /// <summary>深绿</summary>
    DeepGreen,
    /// <summary>深绿ex</summary>
    DeepDGreen,
    /// <summary>浅蓝</summary>
    ThinBule,
    /// <summary>棕色</summary>
    Brown,
    /// <summary>灰白</summary>
    CGray
}

public static class ColorUtil
{
    public static Color SetA(this Color color, float a)
    {
        return new Color(color.r, color.g, color.b, a);
    }

    public static string SetStringColor(string str, STRING_COLOR strColor)
    {
        //基色
        string ColourPrimaries = "";
        StringBuilder sb = new StringBuilder();
        sb.Capacity = 127;
        switch (strColor)
        {
            case STRING_COLOR.Blue:
                ColourPrimaries = "#226DDD";
                break;
            case STRING_COLOR.Green:
                ColourPrimaries = "#11EE11";
                break;
            case STRING_COLOR.CGray:
                ColourPrimaries = "#E4DDBB";
                break;
            case STRING_COLOR.Red:
                ColourPrimaries = "#EE1111";
                break;
            case STRING_COLOR.Gray:
                ColourPrimaries = "#B1ADAA";
                break;
            case STRING_COLOR.Orange:
                ColourPrimaries = "#EEA652";
                break;
            case STRING_COLOR.ThinGray:
                ColourPrimaries = "#D8D6C1";
                break;
            case STRING_COLOR.DeepGreen:
                ColourPrimaries = "#05B142";
                break;
            case STRING_COLOR.DeepDGreen:
                ColourPrimaries = "#1EB01A";
                break;
            case STRING_COLOR.ThinBule:
                ColourPrimaries = "#0096ff";
                break;
            case STRING_COLOR.Brown:
                ColourPrimaries = "#725137";
                break;
        }
        sb.Append("<color=");
        sb.Append(ColourPrimaries);
        sb.Append(">");
        sb.Append(str);
        sb.Append("</color>");
        //string strc =  + ColourPrimaries + ">" + str + "</color>";
        return sb.ToString(0, sb.Length);
    }

    public static string SetStringColor(string str, string strColor)
    {
        string strc = "<color=" + strColor + ">" + str + "</color>";
        return strc;
    }

    /// <summary>半角转全角</summary>
    public static string ToSBC(string input)
    {
        char[] array = input.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == 32)
            {
                array[i] = (char)12288;
                continue;
            }
            if (array[i] < 127)
            {
                array[i] = (char)(array[i] + 65248);
            }
        }
        return new string(array);
    }

    /// <summary>全角转半角</summary>
    public static string ToDBC(string input)
    {
        char[] array = input.ToCharArray();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == 12288)
            {
                array[i] = (char)32;
                continue;
            }
            if (array[i] > 65280 && array[i] < 65375)
            {
                array[i] = (char)(array[i] - 65248);
            }
        }
        return new string(array);
    }

    /// <summary>
    /// 颜色过度
    /// </summary>
    /// <param name="colors"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Color Lerp(Color[] colors, float t)
    {
        if (colors.Length == 1)
            return colors[0];

        int end = colors.Length - 1;

        if (t >= 1.0f)
        {
            return colors[end];
        }
        else if (t <= 0.0f)
        {
            return colors[0];
        }
        else
        {
            float indOfs = t * end;
            int ind = Mathf.FloorToInt(indOfs);

            if (ind == end)
                return colors[ind];

            return Color.Lerp(colors[ind], colors[ind + 1], indOfs - ind);
        }
    }

    public static Color LerpRepeat(Color[] colors, float t)
    {
        if (colors.Length == 1)
            return colors[0];

        int end = colors.Length - 1;

        float indOfs = t * end;
        int ind = Mathf.FloorToInt(indOfs) % end;

        if (ind < 0) ind = end - ind;

        int indNext = (ind + 1) % end;

        return Color.Lerp(colors[ind], colors[indNext], indOfs - ind);
    }

    /// <summary>色码转颜色</summary>
    public static Color hexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public struct ColorHSB
    {
        public float h;
        public float s;
        public float b;
        public float a;

        public ColorHSB(float h, float s, float b, float a)
        {
            this.h = h;
            this.s = s;
            this.b = b;
            this.a = a;
        }

        public ColorHSB(float h, float s, float b)
        {
            this.h = h;
            this.s = s;
            this.b = b;
            this.a = 1f;
        }

        public ColorHSB(Color col)
        {
            ColorHSB temp = FromColor(col);
            h = temp.h;
            s = temp.s;
            b = temp.b;
            a = temp.a;
        }

        public static ColorHSB FromColor(Color color)
        {
            ColorHSB ret = new ColorHSB(0f, 0f, 0f, color.a);

            float r = color.r;
            float g = color.g;
            float b = color.b;

            float max = Mathf.Max(r, Mathf.Max(g, b));

            if (max <= 0)
            {
                return ret;
            }

            float min = Mathf.Min(r, Mathf.Min(g, b));
            float dif = max - min;

            if (max > min)
            {
                if (g == max)
                {
                    ret.h = (b - r) / dif * 60f + 120f;
                }
                else if (b == max)
                {
                    ret.h = (r - g) / dif * 60f + 240f;
                }
                else if (b > g)
                {
                    ret.h = (g - b) / dif * 60f + 360f;
                }
                else
                {
                    ret.h = (g - b) / dif * 60f;
                }
                if (ret.h < 0)
                {
                    ret.h = ret.h + 360f;
                }
            }
            else
            {
                ret.h = 0;
            }

            ret.h *= 1f / 360f;
            ret.s = (dif / max) * 1f;
            ret.b = max;

            return ret;
        }

        public static Color ToColor(ColorHSB hsbColor)
        {
            float r = hsbColor.b;
            float g = hsbColor.b;
            float b = hsbColor.b;
            if (hsbColor.s != 0)
            {
                float max = hsbColor.b;
                float dif = hsbColor.b * hsbColor.s;
                float min = hsbColor.b - dif;

                float h = hsbColor.h * 360f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
        }

        public Color ToColor()
        {
            return ToColor(this);
        }

        public override string ToString()
        {
            return "H:" + h + " S:" + s + " B:" + b;
        }

        public static ColorHSB Lerp(ColorHSB a, ColorHSB b, float t)
        {
            float h, s;

            //check special case black (color.b==0): interpolate neither hue nor saturation!
            //check special case grey (color.s==0): don't interpolate hue!
            if (a.b == 0)
            {
                h = b.h;
                s = b.s;
            }
            else if (b.b == 0)
            {
                h = a.h;
                s = a.s;
            }
            else
            {
                if (a.s == 0)
                {
                    h = b.h;
                }
                else if (b.s == 0)
                {
                    h = a.h;
                }
                else
                {
                    // works around bug with LerpAngle
                    float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                    while (angle < 0f)
                        angle += 360f;
                    while (angle > 360f)
                        angle -= 360f;
                    h = angle / 360f;
                }
                s = Mathf.Lerp(a.s, b.s, t);
            }
            return new ColorHSB(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
        }

        public static string GetColorHSB(Color color)
        {
            ColorHSB colorHsb;
            colorHsb = new ColorHSB(color);
            return colorHsb.ToString();
        }
    }
}
#endregion

#region 字符串拓展
public static class StringUtils
{
    /// <summary>
    /// 获取字符串所表达的函数名
    /// </summary>
    public static string Method(this string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return string.Empty;
        char[] array = new char[pattern.Length];
        int newSize = 0;
        for (int index = pattern.Length - 1; index >= 0; --index)
        {
            char ch = pattern[index];
            if ((int)ch >= 65 && (int)ch <= 90 || (int)ch >= 97 && (int)ch <= 122 || ((int)ch >= 48 && (int)ch <= 57 || (int)ch == 95))
                array[newSize++] = ch;
            else if (newSize > 0)
                break;
        }
        for (int index = newSize - 1; index >= 0 && ((int)array[index] >= 48 && (int)array[index] <= 57); --index)
            --newSize;
        Array.Resize<char>(ref array, newSize);
        Array.Reverse((Array)array);
        return new string(array);
    }

    /// <summary>
    /// 将规定字符串翻译为星号匹配表达式
    /// 即删减正则表达式中除了星号外的所有功能
    /// </summary>
    public static bool Is(this string pattern, string value)
    {
        if (!(pattern == value))
            return Regex.IsMatch(value, "^" + AsteriskWildcard(pattern) + "$");
        return true;
    }

    /// <summary>
    /// 将规定字符串翻译为星号匹配表达式
    /// 即删减正则表达式中除了星号外的所有功能
    /// </summary>
    public static string AsteriskWildcard(this string pattern)
    {
        pattern = Regex.Escape(pattern);
        pattern = pattern.Replace("\\*", ".*?");
        return pattern;
    }

    /// <summary>
    /// 根据长度将字符串分割到数组中
    /// </summary>
    public static string[] Split(this string str, int length = 1)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        Guard.Requires<ArgumentOutOfRangeException>(length > 0);
        string[] strArray = new string[str.Length / length + (str.Length % length == 0 ? 0 : 1)];
        int startIndex = 0;
        while (startIndex < str.Length)
        {
            strArray[startIndex / length] = str.Substring(startIndex, Math.Min(str.Length - startIndex, length));
            startIndex += length;
        }
        return strArray;
    }

    /// <summary>
    /// 将字符串重复指定的次数
    /// </summary>
    public static string Repeat(this string str, int num)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        Guard.Requires<ArgumentOutOfRangeException>(num >= 0);
        if (num == 0)
            return str;
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < num; ++index)
            stringBuilder.Append(str);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 随机打乱字符串中的所有字符
    /// </summary>
    public static string Shuffle(this string str, int? seed = null)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        System.Random random = Util.MakeRandom(seed);
        string[] strArray = new string[str.Length];
        for (int startIndex1 = 0; startIndex1 < str.Length; ++startIndex1)
        {
            int startIndex2 = random.Next(0, str.Length - 1);
            strArray[startIndex1] = strArray[startIndex1] ?? str.Substring(startIndex1, 1);
            strArray[startIndex2] = strArray[startIndex2] ?? str.Substring(startIndex2, 1);
            if (startIndex2 != startIndex1)
            {
                string str1 = strArray[startIndex1];
                strArray[startIndex1] = strArray[startIndex2];
                strArray[startIndex2] = str1;
            }
        }
        string[] source = strArray;
        Func<object, string, string> func = (Func<object, string, string>)((v1, v2) => (string)v1 + (object)v2);
        string str2 = string.Empty;
        Func<object, string, string> callback = null;
        return LinqUtil.Reduce<string>(source, callback, (object)str2);
    }

    /// <summary>
    /// 计算子串在字符串中出现的次数
    /// <para>
    /// 该函数不计数重叠的子串
    /// </para>
    /// </summary>
    /// <param name="str">规定字符串</param><param name="subStr">子字符串</param><param name="start">起始位置</param><param name="length">需要扫描的长度</param><param name="comparison">扫描规则</param>
    /// <returns>
    /// 子字符串出现的次数
    /// </returns>
    public static int SubstringCount(this string str, string subStr, int start = 0, int? length = null, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        Guard.Requires<ArgumentNullException>(subStr != null);
        Util.NormalizationPosition(str.Length, ref start, ref length);
        int num1 = 0;
        int num2;
        for (; length.Value > 0 && (num2 = str.IndexOf(subStr, start, length.Value, comparison)) >= 0; start = num2 + subStr.Length)
        {
            ++num1;
            int? nullable = length;
            int num3 = num2 + subStr.Length - start;
            length = nullable.HasValue ? new int?(nullable.GetValueOrDefault() - num3) : new int?();
        }
        return num1;
    }

    /// <summary>
    /// 反转规定字符串
    /// </summary>
    public static string Reverse(this string str)
    {
        char[] chArray = str.ToCharArray();
        Array.Reverse((Array)chArray);
        return new string(chArray);
    }

    /// <summary>
    /// 把字符串填充为新的长度。
    /// </summary>
    /// <param name="str">规定要填充的字符串</param><param name="length">规定新的字符串长度。如果该值小于字符串的原始长度，则不进行任何操作。</param><param name="padStr">规定供填充使用的字符串。默认是空白。
    /// 如果传入的字符串长度小于等于0那么会使用空白代替。
    /// 注释：空白不是空字符串
    public static string Pad(this string str, int length, string padStr = null, PadTypes type = PadTypes.Right)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        int num1 = length - str.Length;
        if (num1 <= 0)
            return str;
        int length1;
        int length2 = length1 = 0;
        if (type == PadTypes.Both)
        {
            length2 = num1 >> 1;
            length1 = (num1 >> 1) + (num1 % 2 == 0 ? 0 : 1);
        }
        else if (type == PadTypes.Right)
            length1 = num1;
        else
            length2 = num1;
        padStr = padStr ?? " ";
        padStr = padStr.Length <= 0 ? " " : padStr;
        int num2 = length2 / padStr.Length + (length2 % padStr.Length == 0 ? 0 : 1);
        int num3 = length1 / padStr.Length + (length1 % padStr.Length == 0 ? 0 : 1);
        return Repeat(padStr, num2).Substring(0, length2) + str + Repeat(padStr, num3).Substring(0, length1);
    }

    /// <summary>
    /// 在规定字符串中查找在规定搜索值，并在规定搜索值之后返回规定字符串的剩余部分。
    /// 如果没有找到则返回规定字符串本身
    /// </summary>
    public static string After(this string str, string search)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        Guard.Requires<ArgumentNullException>(search != null);
        int num = str.IndexOf(search);
        if (num < 0)
            return str;
        return str.Substring(num + search.Length, str.Length - num - search.Length);
    }

    /// <summary>
    /// 字符串是否包含某个字符忽视大小写
    /// </summary>
    /// <param name="str"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool ContainsIgnoreCase(this string str, string other)
    {
        return str.IndexOf(other, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// 判断规定字符串是否包含规定子字符串
    /// 子字符串是识别大小写的
    /// </summary>
    public static bool Contains(this string str, params string[] needles)
    {
        Guard.Requires<ArgumentNullException>(str != null);
        Guard.Requires<ArgumentNullException>(needles != null);
        foreach (string str1 in needles)
        {
            if (str.Contains(str1))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 在规定字符串中替换匹配项
    /// </summary>
    public static string Replace(string[] matches, string replace,string str)
    {
        Guard.Requires<ArgumentNullException>(matches != null);
        Guard.Requires<ArgumentNullException>(replace != null);
        Guard.Requires<ArgumentNullException>(str != null);
        foreach (string oldValue in matches)
            str = str.Replace(oldValue, replace);
        return str;
    }

    /// <summary>
    /// 替换规定字符串中第一次遇到的匹配项
    /// 该函数对大小写敏感
    /// </summary>
    public static string ReplaceFirst(this string str,string match, string replace)
    {
        Guard.Requires<ArgumentNullException>(match != null);
        Guard.Requires<ArgumentNullException>(replace != null);
        Guard.Requires<ArgumentNullException>(str != null);
        int startIndex = str.IndexOf(match, StringComparison.Ordinal);//StringComparison.OrdinalIgnoreCase 忽略大小写
        if (startIndex >= 0)
            return str.Remove(startIndex, match.Length).Insert(startIndex, replace);
        return str;
    }

    /// <summary>
    /// 替换规定字符串中从后往前第一次遇到的匹配项
    /// 该函数对大小写敏感
    /// </summary>
    public static string ReplaceLast(this string str,string match, string replace)
    {
        Guard.Requires<ArgumentNullException>(match != null);
        Guard.Requires<ArgumentNullException>(replace != null);
        Guard.Requires<ArgumentNullException>(str != null);
        int startIndex = str.LastIndexOf(match, StringComparison.Ordinal);
        if (startIndex >= 0)
            return str.Remove(startIndex, match.Length).Insert(startIndex, replace);
        return str;
    }

    /// <summary>
    /// 生成一个随机字母（含大小写），数字的字符串。
    /// </summary>
    public static string Random(int length = 16, int? seed = null)
    {
        Guard.Requires<ArgumentOutOfRangeException>(length > 0);
        string str1 = string.Empty;
        System.Random random = Util.MakeRandom(seed);
        int length1;
        int val1;
        string str2;
        for (; (length1 = str1.Length) < length; str1 += str2.Substring(0, Math.Min(val1, str2.Length)))
        {
            val1 = length - length1;
            byte[] numArray = new byte[val1];
            random.NextBytes(numArray);
            string[] matches = new string[3];
            int index1 = 0;
            string str3 = "/";
            matches[index1] = str3;
            int index2 = 1;
            string str4 = "+";
            matches[index2] = str4;
            int index3 = 2;
            string str5 = "=";
            matches[index3] = str5;
            string replace = string.Empty;
            string str6 = Convert.ToBase64String(numArray);
            str2 = Replace(matches, replace, str6);
        }
        return str1;
    }
    /// <summary>
    /// 如果长度超过给定的最大字符串长度，则截断字符串。 截断的字符串的最后一个字符将替换为缺省字符串
    /// eg: Truncate("hello world , the sun is shine",15," ") =&gt; hello world...
    /// </summary>
    /// <param name="str">要截断的字符串</param><param name="length">截断长度(含缺省字符长度)</param><param name="separator">临近的分隔符，如果设定则截断长度为截断长度最近的分隔符位置,如果传入的是一个正则表达式那么使用正则匹配。</param><param name="mission">缺省字符</param>
    public static string Truncate(this string str, int length, object separator = null, string mission = null)
    {
        if (str == null || length > str.Length)
            return str;
        mission = mission ?? "...";
        int length1 = length - mission.Length;
        if (length1 < 1)
            return mission;
        string input = str.Substring(0, length1);
        if (separator == null)
            return input + mission;
        Regex regex = separator as Regex;
        string str1 = separator.ToString();
        int length2 = -1;
        if (regex != null)
        {
            if (regex.IsMatch(input))
                length2 = (regex.RightToLeft ? (Capture)regex.Match(input) : (Capture)Regex.Match(input, regex.ToString(), regex.Options | RegexOptions.RightToLeft)).Index;
        }
        else if (!string.IsNullOrEmpty(str1) && str.IndexOf(str1, StringComparison.Ordinal) != length1)
            length2 = input.LastIndexOf(str1, StringComparison.Ordinal);
        if (length2 > -1)
            input = input.Substring(0, length2);
        return input + mission;
    }

    /// <summary>
    /// 填充类型
    /// </summary>
    public enum PadTypes
    {
        Both,
        Left,
        Right,
    }

    /// <summary>获得字符下标</summary>
    public static int GetStringIndex(this string str, char charStr)
    {
        int index = 0;

        foreach (char item in str)
        {
            if (item != charStr)
            {
                index++;
            }
            else
            {
                break;
            }
        }
        return index;
    }

    /// <summary>统计string中的字符类型和个数</summary>
    public static void CollectionChrCount(this string str)
    {
        int iAllChr = 0; //字符总数：不计字符'\n'和'\r'
        int iChineseChr = 0; //中文字符计数
        int iChinesePnct = 0;//中文标点计数
        int iEnglishChr = 0; //英文字符计数
        int iEnglishPnct = 0;//英文标点计数
        int iNumber = 0;  //数字字符：0-9
        foreach (char ch in str)
        {
            if (ch != '\n' && ch != '\r') iAllChr++;
            if ("～！＠＃￥％…＆（）—＋－＝".IndexOf(ch) != -1 ||
             "｛｝【】：“”；‘'《》，。、？｜＼".IndexOf(ch) != -1) iChinesePnct++;
            if (ch >= 0x4e00 && ch <= 0x9fbb) iChineseChr++;
            if ("`~!@#$%^&*()_+-={}[]:\";'<>,.?/\\|".IndexOf(ch) != -1) iEnglishPnct++;
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) iEnglishChr++;
            if (ch >= '0' && ch <= '9') iNumber++;
        }
        string sStats = string.Format(string.Concat(
         "字符总数：{0}\r\n", "中文字符数：{1}\r\n", "中文标点数：{2}\r\n",
         "英文字符数：{3}\r\n", "英文标点数：{4}\r\n", "数字字符数：{5}\r\n"),
         iAllChr.ToString(), iChineseChr.ToString(), iEnglishChr.ToString(),
         iEnglishChr.ToString(), iEnglishPnct.ToString(), iNumber.ToString());
        Debug.Log(sStats);
    }

    /// <summary>
    /// 替换一个字符串的某一段字符为指定字符
    /// </summary>
    /// <param name="Str">被操作的字符串</param>
    /// <param name="StartIndex">要被替换的字符在字符串中的开始标号</param>
    /// <param name="val">替换目标字符串</param>
    /// <returns>会自动根据目标字符串来获取要替换的字符长度</returns>
    public static void ReplaceStr(ref string Str, int StartIndex, string val)
    {
        char[] array = Str.ToCharArray();
        char[] array2 = val.ToCharArray();
        if (array2.Length + StartIndex > array.Length)
        {
            Debug.LogError("超出有效长度");
            return;
        }
        string text = "";
        int num = 0;
        while (array2.Length > num)
        {
            array[StartIndex] = array2[num];
            StartIndex++;
            num++;
        }
        char[] array3 = array;
        for (int i = 0; i < array3.Length; i++)
        {
            char c = array3[i];
            text += c.ToString();
        }
        Str = text;
    }

    /// <summary>
    /// 替换字符为新字符串
    /// </summary>
    /// <param name="Str">被替换的字符串</param>
    /// <param name="oldChar">旧的字符</param>
    /// <param name="newString">要替换的字符串</param>
    public static void ReplaceStr(ref string Str, char oldChar, string newString)
    {
        char[] array = Str.ToCharArray();
        string text = "";
        char[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            char c = array2[i];
            string text2 = "";
            bool flag = c == oldChar;
            if (flag)
            {
                text2 = newString;
            }
            else
            {
                text2 += c.ToString();
            }
            text += text2;
        }
        Str = text;
    }

    /// <summary>
    /// 获取一个字符串的同字符数量
    /// </summary>
    /// <param name="text">被操作的字符串</param>
    /// <param name="pattern">字符分割规则。默认为将该字符串分割每个字符,直接传入一个字符将以那个字符为分割符号，加括弧号可以包含分割字符。支持正则表达式</param>
    /// <returns></returns>
    public static Dictionary<string, int> CountWords(this string text, string pattern = "")
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        string[] array = Regex.Split(text, pattern);
        string[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            string text2 = array2[i];
            bool flag = dictionary.ContainsKey(text2);
            if (flag)
            {
                Dictionary<string, int> arg_30_0 = dictionary;
                string key = text2;
                int num = arg_30_0[key];
                arg_30_0[key] = num + 1;
            }
            else
            {
                dictionary[text2] = 1;
            }
        }
        return dictionary;
    }

    /// <summary>获取字符串中的中文个数</summary>
    public static int GetStringChineseCounts(this string str)
    {
        int count = 0;
        Regex regex = new Regex(@"^[\u4E00-\u9FA5]{0,}$");

        for (int i = 0; i < str.Length; i++)
        {
            if (regex.IsMatch(str[i].ToString()))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>检查index位置是否存在匹配子字符串sub</summary>
    public static bool CheckStringMatch(this string sub, string baseStr, int index)
    {
        int i = 0;
        while (i < sub.Length)
        {
            if (sub[i] != baseStr[index + i])
            {
                return false;
            }
            i++;
        }
        return true;
    }

    /// <summary>
    /// 是否为空或者空白
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhitespace(this string str)
    {
        if (str == null)
            return true;

        for (var i = 0; i < str.Length; i++)
        {
            if (!char.IsWhiteSpace(str[i]))
                return false;
        }
        return true;
    }

    #region 表达式求值
    /// <summary>
    /// 表达式求值
    /// 注!：当前仅支持“+-*/()”(英文的括号)操作符
    /// </summary>
    /// <param name="s">算式表达式</param>
    /// <returns>返回算式的结果</returns>
    public static double StringCompute(this string s)
    {
        char[] array = s.ToCharArray();
        int i = 0;
        Stack<char> stack = new Stack<char>();
        Stack<double> stack2 = new Stack<double>();
        stack.Push('#');
        int num = array.Length;
        bool flag = true;
        i = 0;
        while (i < num)
        {
            bool flag2 = array[i] == '-' & flag;
            if (flag2)
            {
                stack2.Push(0.0);
                stack.Push('-');
                i++;
            }
            else
            {
                bool flag3 = array[i] == ')';
                if (flag3)
                {
                    flag = false;
                    i++;
                    while (stack.Peek() != '(')
                    {
                        double a = stack2.Pop();
                        double a2 = stack2.Pop();
                        char op = stack.Pop();
                        double item = Operate(a2, op, a);
                        stack2.Push(item);
                    }
                    stack.Pop();
                }
                else
                {
                    bool flag4 = array[i] >= '0' && array[i] <= '9';
                    if (flag4)
                    {
                        flag = false;
                        stack2.Push(Translation(ref i, ref array));
                    }
                    else
                    {
                        bool flag5 = array[i] == '(';
                        if (flag5)
                        {
                            flag = true;
                            stack.Push(array[i]);
                            i++;
                        }
                        else
                        {
                            while (GetLevel(array[i]) <= GetLevel(stack.Peek()))
                            {
                                double a3 = stack2.Pop();
                                double a4 = stack2.Pop();
                                char op2 = stack.Pop();
                                double item2 = Operate(a4, op2, a3);
                                stack2.Push(item2);
                            }
                            stack.Push(array[i]);
                            i++;
                        }
                    }
                }
            }
        }
        while (stack.Peek() != '#')
        {
            double a5 = stack2.Peek();
            stack2.Pop();
            double a6 = stack2.Peek();
            stack2.Pop();
            char op3 = stack.Peek();
            stack.Pop();
            double item3 = Operate(a6, op3, a5);
            stack2.Push(item3);
        }
        return stack2.Pop();
    }

    /// <summary>
    /// 字符转数字
    /// </summary>
    private static double Translation(ref int pos, ref char[] s)
    {
        double num = 0.0;
        double num2 = 0.0;
        while (s[pos] >= '0' && s[pos] <= '9')
        {
            num *= 10.0;
            num += (double)(s[pos] - '0');
            pos++;
        }
        bool flag = s[pos] == '.';
        if (flag)
        {
            pos++;
            int num3 = 1;
            while (s[pos] >= '0' && s[pos] <= '9')
            {
                double num4 = (double)(s[pos] - '0');
                num4 *= Math.Pow(0.1, (double)num3);
                num3++;
                num2 += num4;
                pos++;
            }
        }
        return num + num2;
    }

    /// <summary>
    /// 返回运算符级别
    /// </summary>
    private static int GetLevel(char ch)
    {
        int result;
        if (ch != '#')
        {
            switch (ch)
            {
                case '(':
                    result = 0;
                    return result;
                case '*':
                case '/':
                    result = 2;
                    return result;
                case '+':
                case '-':
                    result = 1;
                    return result;
            }
            result = -1;
        }
        else
        {
            result = -1;
        }
        return result;
    }

    /// <summary>
    /// 对两个数进行运算
    /// </summary>
    private static double Operate(double a1, char op, double a2)
    {
        double result;
        switch (op)
        {
            case '*':
                result = a1 * a2;
                return result;
            case '+':
                result = a1 + a2;
                return result;
            case '-':
                result = a1 - a2;
                return result;
            case '/':
                {
                    double num = 0.0;
                    try
                    {
                        num = a1 / a2;
                    }
                    catch
                    {
                        num = 0.0;
                    }
                    result = num;
                    return result;
                }
        }
        result = 0.0;
        return result;
    }
    #endregion
}
#endregion

#region 时间拓展
/// <summary>
/// 闰年平年
/// </summary>
public enum Leap_Common_Year
{
    Leap_Year = 29,
    Common_Year = 28
}

public sealed class TimeUtil
{
    /// <summary>获取润平年</summary>
    public static Leap_Common_Year GetLerpCommonYaer()
    {
        if (DateTime.IsLeapYear(DateTime.Now.Date.Year))
        {
            return Leap_Common_Year.Leap_Year;
        }
        return Leap_Common_Year.Common_Year;
    }

    /// <summary>获取 0天00时00分</summary>
    public static string GetTimeDdHhmmss2(int leftTime)
    {
        int day = leftTime / 86400;
        int hour = (leftTime - day * 86400) / 3600;
        int min = (leftTime - day * 86400 - hour * 3600) / 60;
        return day + "天" + (hour < 10 ? "0" + hour : hour + "") +
            "时" + (min < 10 ? "0" + min : min + "") + "分";
    }

    /// <summary>获取 0时00分</summary>
    public static string GetTimeHhmm2(uint leftTime)
    {
        uint hour = leftTime / 3600;
        uint min = (leftTime - hour * 3600) / 60;
        return (hour > 0 ? hour + "时" : "") + min + "分";
    }

    /// <summary>大于一天显示天数，小于一天显示 时：分</summary>
    public static string GetTimeDdHhmm(int leftTime)
    {
        int day = leftTime / 86400;
        if (day > 0) return day + "天";
        leftTime = leftTime - day * 86400;
        int hour = leftTime / 3600;
        int min = (leftTime - hour * 3600) / 60;
        return (hour < 10 ? "0" + hour : hour + "") + ":" + (min < 10 ? "0" + min : min + "");
    }

    /// <summary>00时00分00秒 格式的时间字符串  leftTime(秒)</summary>
    public static string GetTimeHhmmss4(int leftTime)
    {
        int day = leftTime / 86400;
        int hour = (leftTime - day * 86400) / 3600;
        int min = (leftTime - day * 86400 - hour * 3600) / 60;
        int sec = (leftTime - day * 86400 - hour * 3600 - min * 60);
        return (hour < 10 ? "0" + hour : hour + "") +
            "时" + (min < 10 ? "0" + min : min + "") + "分" + (sec < 10 ? "0" + sec : sec + "") + "秒";
    }

    /// <summary>获取  00:00   00时00分 格式的时间字符串  leftTime(秒)</summary>
    public static string GetTimeHhmm(int leftTime)
    {
        int hour = leftTime / 3600;
        int min = leftTime / 60;
        int sec = (leftTime - min * 60);
        min -= hour * 60;
        if (sec > 0)
        {
            min += 1;
            if (min > 59)
            {
                min = 59;
            }
        }
        return (hour < 10 ? "0" + hour : hour + "") + ":" + (min < 10 ? "0" + min : min + "");
    }

    /// <summary>获取  00:00   00分00秒 格式的时间字符串  leftTime(秒)</summary>
    public static string GetTimeMmss(int leftTime)
    {
        int min = leftTime / 60;
        int sec = (leftTime - min * 60);
        return (min < 10 ? "0" + min : min + "") + ":" + (sec < 10 ? "0" + sec : sec + "");
    }

    /// <summary>获取  00:00:00   00时00分00秒 格式的时间字符串  leftTime(秒)</summary>
    public static string GetTimeHhmmss(int leftTime)
    {
        int hour = leftTime / 3600;
        int min = leftTime / 60;
        int sec = (leftTime - min * 60);
        min -= hour * 60;
        return (hour < 10 ? "0" + hour : hour + "") + ":" + (min < 10 ? "0" + min : min + "") + ":" + (sec < 10 ? "0" + sec : sec + "");
    }

    /// <summary>显示xx时xx分xx秒</summary>
    public static string GetTimeHhmmss2(int leftTime)
    {
        int hour = leftTime / 3600;
        int min = (leftTime - hour * 3600) / 60;
        int sec = leftTime % 60;
        if (hour > 0)
        {
            return hour + "时" + (min < 10 ? "0" + min : min + "") + "分" +
(sec < 10 ? "0" + sec : sec + "") + "秒";
        }
        else if (min > 0)
        {
            return (min < 10 ? "0" + min : min + "") + "分" +
(sec < 10 ? "0" + sec : sec + "") + "秒";
        }
        else
        {
            return (sec < 10 ? sec.ToString() : sec + "") + "秒";
        }
    }

    /// <summary>显示XX分XX秒 不足1分只显示XX秒</summary>
    public static string GetTimeHhmmss3(int leftTime)
    {
        int min = leftTime / 60;
        int sec = leftTime % 60;
        if (min > 0)
        {
            return min + "分" +
                (sec < 10 ? "0" + sec : sec + "") + "秒";
        }
        else
        {
            return sec + "秒";
        }
    }

    /// <summary>大于一天显示天数，小于一天显示 时:分:秒  00:00:00  格式的时间字符串  leftTime(秒)</summary>
    public static string GetTimeDdHhmmss(int leftTime)
    {
        int day = leftTime / 86400;
        if (day > 0) return day + "天";
        int hour = leftTime / 3600;
        int min = leftTime / 60;
        int sec = (leftTime - min * 60);
        min -= hour * 60;
        return (hour < 10 ? "0" + hour : hour + "") + ":" + (min < 10 ? "0" + min : min + "") + ":" + (sec < 10 ? "0" + sec : sec + "");
    }

    /// <summary>获取 0天（向上取整）</summary>
    public static string GetDays(int leftTime)
    {
        int day = leftTime / 86400;
        int hour = (leftTime - day * 86400) / 3600;
        int min = (leftTime - day * 86400 - hour * 3600) / 60;
        if (min > 0)
        {
            day += 1;
        }
        return day + "天";
    }

    /// <summary>获取离线时间</summary>
    public static string GetleveTime(uint status)
    {
        string time = "";
        if (status == 0)
        {
            time = "在线";
        }
        else
        {
            uint nowtime = GetSecondTime();
            uint lvtime = 0;
            if (nowtime > status)
                lvtime = nowtime - status;
            if (lvtime < 3600)
            {
                time = (lvtime / 60).ToString() + "分钟前";
            }
            else if (3600 < lvtime && lvtime < 86400)
            {
                time = (lvtime / 3600).ToString() + "小时前";
            }
            else if (lvtime > 86400 && lvtime < 2592000)
            {
                time = (lvtime / 86400).ToString() + "天前";

            }
            else
            {
                time = "30天前";
            }
        }
        return time;
    }

    public static uint GetSecondTime()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return Convert.ToUInt32(ts.TotalSeconds);
    }

    public static uint GetMillisecondsTime()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return (uint)ts.TotalMilliseconds;
    }

    /// <summary>将long转换为时间格式</summary>
    public static DateTime ConvertLongToDateTime(long d)
    {
        DateTime dateTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long ticks = long.Parse(d + "0000");
        TimeSpan value = new TimeSpan(ticks);
        return dateTime.Add(value);
    }

    /// <summary>将时间转换为long</summary>
    public static long ConvertDataTimeToLong(DateTime dt)
    {
        DateTime value = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long ticks = dt.Subtract(value).Ticks;
        return long.Parse(ticks.ToString().Substring(0, ticks.ToString().Length - 4));
    }

    /// <summary>
    /// 返回差距天数
    /// 参数为时间戳
    /// </summary>
    /// <param name="l1">前一天</param>
    /// <param name="l2">后一天</param>
    public static int timeInterval(long l1, long l2)
    {
        DateTime d = ConvertLongToDateTime(l1);
        DateTime d2 = ConvertLongToDateTime(l2);
        return (int)(d2 - d).TotalDays;
    }
}
#endregion

#region 链式拓展

#region GameObject
public static class GameObjectExtension
{
    public static GameObject Show(this GameObject selfObj)
    {
        selfObj.SetActive(true);
        return selfObj;
    }

    public static GameObject Hide(this GameObject selfObj)
    {
        selfObj.SetActive(false);
        return selfObj;
    }

    public static GameObject SetLoclEular(this GameObject selfObj, Vector3 eulerAngle)
    {
        TransformExtensions.SetLocalAngle(selfObj.transform, eulerAngle);
        return selfObj;
    }

    public static GameObject SetLocalPos(this GameObject selfObj, Vector3 pos)
    {
        TransformExtensions.SetLocalPos(selfObj.transform, pos);
        return selfObj;
    }

    public static GameObject SetScale(this GameObject selfObj, Vector3 scale)
    {
        TransformExtensions.SetLocalScale(selfObj.transform, scale);
        return selfObj;
    }

    public static GameObject SetParent(this GameObject selfObj, Transform parent)
    {
        selfObj.transform.SetParent(parent);
        SetScale(selfObj, Vector3.one);
        SetLocalPos(selfObj, Vector3.zero);
        return selfObj;
    }

    public static bool CheckLayerAndTag(GameObject go, int layerMask, string tag)
    {
        return (layerMask & (1 << go.layer)) != 0 && go.CompareTag(tag);
    }

    public static bool CheckTag(GameObject go, params string[] tags)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            if (go.CompareTag(tags[i]))
                return true;
        }
        return false;
    }

    public static bool CheckTag(Component comp, params string[] tags)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            if (comp.CompareTag(tags[i]))
                return true;
        }
        return false;
    }

}
#endregion

#region Transform拓展
public static class TransformExtensions
{
    public static void SetX(this Transform transform, float x)
    {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    public static void SetY(this Transform transform, float y)
    {
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    public static void SetZ(this Transform transform, float z)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    public static void SetXY(this Transform transform, Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    public static void SetXZ(this Transform transform, Vector3 position)
    {
        transform.position = new Vector3(position.x, transform.position.y, position.z);
    }

    public static void SetPos(this Transform transform, Vector3 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, pos.z);
    }

    public static void SetLocalPos(this Transform transform, Vector3 pos)
    {
        transform.localPosition = new Vector3(pos.x, pos.y, pos.z);
    }

    public static void SetLocalX(this Transform transform, float x)
    {
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
    }

    public static void SetLocalY(this Transform transform, float y)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
    }

    public static void SetLocalZ(this Transform transform, float z)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
    }

    public static void SetLocalEulerAngles(this Transform transform, Vector3 eulerAngles)
    {
        transform.localEulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z);
    }

    public static void SetLocalEulerX(this Transform transform, float x)
    {
        transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerY(this Transform transform, float y)
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
    }

    public static void SetLocalEulerZ(this Transform transform, float z)
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
    }

    public static void SetSizeDelta(this RectTransform rectTransfrom, Vector2 size)
    {
        rectTransfrom.sizeDelta = new Vector2(size.x, size.y);
    }

    /// <summary>是否父子级关系</summary>
    public static bool IsChildOf(this Transform transform, Transform parent, bool withoutFinding = true)
    {
        if (withoutFinding)     //不用向上找的时候，直接判断
            return transform.parent == parent;
        else                    //需要向上找的时候，使用自带方法
            return transform.IsChildOf(parent);
    }

    /// <summary>
    /// 对子物体进行倒序排序
    /// </summary>
    /// <param name="transform"></param>
    public static void ReverseChild(this Transform transform)
    {
        if (transform.childCount == 0)
            return;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).SetSiblingIndex(0);
    }

    /// <summary>
    /// UGUI转向目标物体
    /// </summary>
    public static void RotateToTarget(this RectTransform rectTransfrom, Vector3 target)
    {
        //这里转向默认改变的是X轴的值
        rectTransfrom.LookAt(target);
        Vector3 eulerRotate = rectTransfrom.rotation.eulerAngles;
        float offsetX = target.x - rectTransfrom.position.x;

        //修正欧拉角转向
        if (offsetX >= 0)
        {
            if ((eulerRotate.x >= 0 && eulerRotate.x < 90) || (eulerRotate.x > 270 && eulerRotate.x <= 360))
                eulerRotate.z = -eulerRotate.x;
        }
        else
        {
            if ((eulerRotate.x >= 0 && eulerRotate.x < 90) || (eulerRotate.x > 270 && eulerRotate.x <= 360))
                eulerRotate.z = eulerRotate.x - 180;
        }
        eulerRotate.x = eulerRotate.y = 0;
        rectTransfrom.rotation = Quaternion.Euler(eulerRotate);
    }

    public static void SetLocalScale(this Transform transform, Vector3 scale)
    {
        transform.localScale = new Vector3(scale.x, scale.y, scale.z);
    }

    public static void SetLocalScaleX(this Transform transform, float x)
    {
        transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
    }

    public static void SetLocalScaleY(this Transform transform, float y)
    {
        transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
    }

    public static void SetLocalScaleZ(this Transform transform, float z)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
    }

    public static void SetLocalAngle(this Transform transform, Vector3 eulerAngle)
    {
        transform.localEulerAngles = new Vector3(eulerAngle.x, eulerAngle.y, eulerAngle.z);
    }

    public static void SetLocalAngleX(this Transform transform, float x)
    {
        transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    public static void SetLocalAngleY(this Transform transform, float y)
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, transform.localEulerAngles.z);
    }

    public static void SetLocalAngleZ(this Transform transform, float z)
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, z);
    }

    /// <summary>
    /// 增加X
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="dx"></param>
    public static void IncX(this Transform transform, float dx)
    {
        SetX(transform, transform.position.x + dx);
    }

    public static void IncLocalX(this Transform transform, float dx)
    {
        SetLocalX(transform, transform.localPosition.x + dx);
    }

    public static void IncY(this Transform transform, float dy)
    {
        SetY(transform, transform.position.y + dy);
    }

    public static void IncLocalY(this Transform transform, float dy)
    {
        SetLocalY(transform, transform.localPosition.y + dy);
    }

    public static void IncZ(this Transform transform, float dz)
    {
        SetZ(transform, transform.position.z + dz);
    }

    public static void IncLocalZ(this Transform transform, float dz)
    {
        SetLocalZ(transform, transform.localPosition.z + dz);
    }

    /// <summary>获得一级子物体</summary>
    public static Transform[] GetFirstChildren(this Transform transform)
    {
        int childCount = transform.childCount;
        Transform[] result = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
            result[i] = transform.GetChild(i);
        return result;
    }

   

    /// <summary>
    /// 移动本地坐标到某一坐标
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="target"></param>
    public static void LocalMove(this Transform transform,Transform target)
    {
        Vector3 pos = target.TransformPoint(transform.localPosition);
        transform.localPosition = transform.InverseTransformPoint(pos);
    }

    /// <summary>
    /// DFS查找，获得Transform的子物体（及其后代）中第一个含有该名字的物体。适用于需要深度搜索的情况
    /// </summary>
    private static Transform GetChildByNameInDFS(this Transform transform, string childObjName)
    {
        //深度优先（DFS）找到该名字的Transform 
        //使用一个栈进行遍历 =_=
        System.Collections.Generic.Stack<Transform> stack =
            new System.Collections.Generic.Stack<Transform>();
        //--将parent入栈
        stack.Push(transform);
        Transform tmp;
        //--DFS开始
        while (stack.Count > 0)
        {
            tmp = stack.Pop();
            if (tmp.name.Equals(childObjName))
            {
                //Great, got it
                return (Transform)tmp;
            }
            //--push back the children
            for (int i = tmp.childCount - 1; i >= 0; i--)
            {
                stack.Push(tmp.GetChild(i));
            }
        }
        //Not found
        return null;
    }

    /// <summary>
    /// 采用广度优先的方式查找Transform的子物体（及其后代）中第一个含有该名字的物体。适用于无需太多深度搜索的情况；
    /// </summary>
    private static Transform GetChildByNameInBFS(this Transform transform, string childObjName)
    {
        //深度优先（BFS）找到该名字的Transform 
        //使用一个栈进行遍历 =_=
        System.Collections.Generic.Queue<Transform> stack =
            new System.Collections.Generic.Queue<Transform>();
        //--将parent入栈
        stack.Enqueue(transform);
        Transform tmp;
        //--BFS开始
        while (stack.Count > 0)
        {
            tmp = stack.Dequeue();
            if (tmp.name.Equals(childObjName))
            {
                //Great, got it
                return (Transform)tmp;
            }
            //--push back the children
            for (int i = 0; i < tmp.childCount; i++)
            {
                stack.Enqueue(tmp.GetChild(i));
            }
        }
        //Not found
        return null;
    }


    /// <summary>
    /// 获得一个物体（或者其组件），可采用BFS（或DFS）的查找方法
    /// </summary>
    /// <param name="useBFS">适用于层级较低的内容，避免较深查找</param>
    public static T GetChildByName<T>(this Transform transform, string childObjName, bool useBFS = true)
    {
        Transform childGot;
        if (useBFS)
            childGot = GetChildByNameInBFS(transform, childObjName);
        else
            childGot = GetChildByNameInDFS(transform, childObjName);
        if (childGot == null)
            //Not found
            return default(T);
        else
            return childGot.GetComponent<T>();
    }

    /// <summary>
    /// 获得一个Transform，可选择BFS（默认广度优先搜索）或者DFS
    /// </summary>
    /// <param name="useBFS">适用于层级较低的内容，避免较深查找</param>
    public static Transform GetChildByName(this Transform transform, string childObjName, bool useBFS = true)
    {
        if (useBFS)
            return GetChildByNameInBFS(transform, childObjName);
        else
            return GetChildByNameInDFS(transform, childObjName);
    }

    /// <summary>获取激活子物体数量</summary>
    public static int GetActiveChildCount(this Transform transform)
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                count++;
        }
        return count;
    }

    /// <summary>获取子物体目录路径</summary>
    public static string GetPath(this Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    /// <summary>获取子物体下标</summary>
    public static int IndexInParent(this Transform transform)
    {
        if (transform.parent == null)
            return -1;

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i) == transform)
                return i;
        }
        return -1;
    }

    /// <summary>设置子物体激活状态</summary>
    public static void SetActiveInChildren(this Transform transform, bool value)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(value);
        }
    }

    /// <summary>比较是否相等</summary>
    public static bool EqualsInHierarchy(this Transform transform, Transform other)
    {
        if (transform.childCount != other.childCount)
            return false;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name != other.GetChild(i).name)
                return false;
            else if (!EqualsInHierarchy(transform.GetChild(i), other.GetChild(i)))
                return false;
        }
        return true;
    }

    /// <summary>删除所有子物体</summary>
    public static void DestroyChildren(this Transform transform)
    {
        for (int i = transform.childCount; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
    }

    /// <summary>
    /// 对齐屏幕位置
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="camera">一般是Camera.main</param>
    /// <param name="dir"></param>
    /// <param name="offset">图片偏移值，因为是像素对齐，不是图片对齐，所以这个值对于带有透明通道的图有用</param>
    /// <returns></returns>
    public static Transform AlginScreen(this Transform transform,AlginDir dir,float offset, Camera camera)
    {
        camera = camera == null ? Camera.main : camera;
        Vector3 pos = transform.position;
        Vector3 worldPos = Vector3.zero;
        Vector3 screenPos = camera.WorldToScreenPoint(pos);

        switch (dir)
        {
            case AlginDir.Left:
                worldPos = camera.ScreenToWorldPoint(new Vector3(0, screenPos.y, screenPos.z));
                break;
            case AlginDir.Right:
                worldPos = camera.ScreenToWorldPoint(new Vector3(Screen.width, screenPos.y, screenPos.z));
                break;
            case AlginDir.Top:
                worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, Screen.height, screenPos.z));
                break;
            case AlginDir.Down:
                worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, 0, screenPos.z));
                break;
            default:
                break;
        }
        if (dir == AlginDir.Left || dir == AlginDir.Right)
        {
            worldPos.x += offset;
            transform.position = new Vector3(worldPos.x, pos.y, pos.z);
        }
        else
        {
            worldPos.y += offset;
            transform.position = new Vector3(pos.x, worldPos.y, pos.z);
        }
        return transform;
    }
}

public enum AlginDir
{
    Left,
    Right,
    Top,
    Down
}
#endregion

#region RectTransform拓展
public enum AnchorPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottonCenter,
    BottomRight,
    BottomStretch,

    VertStretchLeft,
    VertStretchRight,
    VertStretchCenter,

    HorStretchTop,
    HorStretchMiddle,
    HorStretchBottom,

    StretchAll
}

public enum PivotPresets
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,
}

public static class RectTransformExtensions
{
    public static void SetAnchor(this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0)
    {
        source.anchoredPosition = new Vector3(offsetX, offsetY, 0);

        switch (allign)
        {
            case (AnchorPresets.TopLeft):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.TopCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 1);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.TopRight):
                {
                    source.anchorMin = new Vector2(1, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.MiddleLeft):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(0, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0.5f);
                    source.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (AnchorPresets.MiddleRight):
                {
                    source.anchorMin = new Vector2(1, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }

            case (AnchorPresets.BottomLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 0);
                    break;
                }
            case (AnchorPresets.BottonCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 0);
                    break;
                }
            case (AnchorPresets.BottomRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.HorStretchTop):
                {
                    source.anchorMin = new Vector2(0, 1);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
            case (AnchorPresets.HorStretchMiddle):
                {
                    source.anchorMin = new Vector2(0, 0.5f);
                    source.anchorMax = new Vector2(1, 0.5f);
                    break;
                }
            case (AnchorPresets.HorStretchBottom):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 0);
                    break;
                }

            case (AnchorPresets.VertStretchLeft):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(0, 1);
                    break;
                }
            case (AnchorPresets.VertStretchCenter):
                {
                    source.anchorMin = new Vector2(0.5f, 0);
                    source.anchorMax = new Vector2(0.5f, 1);
                    break;
                }
            case (AnchorPresets.VertStretchRight):
                {
                    source.anchorMin = new Vector2(1, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }

            case (AnchorPresets.StretchAll):
                {
                    source.anchorMin = new Vector2(0, 0);
                    source.anchorMax = new Vector2(1, 1);
                    break;
                }
        }
    }

    public static void SetPivot(this RectTransform source, PivotPresets preset)
    {
        switch (preset)
        {
            case (PivotPresets.TopLeft):
                {
                    source.pivot = new Vector2(0, 1);
                    break;
                }
            case (PivotPresets.TopCenter):
                {
                    source.pivot = new Vector2(0.5f, 1);
                    break;
                }
            case (PivotPresets.TopRight):
                {
                    source.pivot = new Vector2(1, 1);
                    break;
                }

            case (PivotPresets.MiddleLeft):
                {
                    source.pivot = new Vector2(0, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleCenter):
                {
                    source.pivot = new Vector2(0.5f, 0.5f);
                    break;
                }
            case (PivotPresets.MiddleRight):
                {
                    source.pivot = new Vector2(1, 0.5f);
                    break;
                }

            case (PivotPresets.BottomLeft):
                {
                    source.pivot = new Vector2(0, 0);
                    break;
                }
            case (PivotPresets.BottomCenter):
                {
                    source.pivot = new Vector2(0.5f, 0);
                    break;
                }
            case (PivotPresets.BottomRight):
                {
                    source.pivot = new Vector2(1, 0);
                    break;
                }
        }
    }

    /// <summary>
    /// 得到根目录canvas
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Canvas GetRootCanvas(this Transform transform)
    {
        Canvas rootCanvas = null;
        Transform parent = transform;
        while (parent)
        {
            rootCanvas = parent.GetComponentInParent<Canvas>();
            if (rootCanvas && !rootCanvas.isRootCanvas)
                parent = parent.parent;
            else
                break;
        }
        return rootCanvas;
    }
}
#endregion

#region Graphic
public static class MaskableGraphicExtension
{
    public static MaskableGraphic SetSizeDelta(this MaskableGraphic graphic, Vector2 sizeDelta)
    {
        TransformExtensions.SetSizeDelta(graphic.rectTransform, sizeDelta);
        return graphic;
    }

    #region 坐标转换
    /// <summary>
    /// 世界坐标转UGUI坐标 
    /// </summary>
    /// <param name="graphic"></param>
    /// <param name="transform"></param>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static MaskableGraphic GetPosByWorldPos(this MaskableGraphic graphic, Transform transform, Canvas canvas)
    {
        Vector2 uisize = canvas.GetComponent<RectTransform>().sizeDelta;//得到画布的尺寸
        Vector2 screenpos = Camera.main.WorldToScreenPoint(transform.position);//将世界坐标转换为屏幕坐标
        Vector2 screenpos2;
        screenpos2.x = screenpos.x - (Screen.width / 2);//转换为以屏幕中心为原点的屏幕坐标
        screenpos2.y = screenpos.y - (Screen.height / 2);
        Vector2 uipos;
        uipos.x = (screenpos2.x / Screen.width) * uisize.x;
        uipos.y = (screenpos2.y / Screen.height) * uisize.y;//得到UGUI的anchoredPosition
        graphic.rectTransform.anchoredPosition = uipos;
        return graphic;
    }

    /// <summary>
    /// 获取rect屏幕坐标
    /// </summary>
    public static MaskableGraphic GetScreenPos(this MaskableGraphic graphic, out Vector3 screenPosition, Canvas canvas = null, Camera camera = null)
    {
        RectTransform rect = graphic.rectTransform;
        graphic.rectTransform.GetScreenPos(out screenPosition, canvas, camera);
        return graphic;
    }
    #endregion

    public static RectTransform GetScreenPos(this RectTransform rect, out Vector3 screenPosition, Canvas canvas = null, Camera camera = null)
    {
        screenPosition = Vector3.zero;
        if (rect == null)
        {
            Debug.LogError("rect is Null");
            return null;
        }
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            screenPosition = rect.position;
        else
            screenPosition = camera.WorldToScreenPoint(rect.position);
        return rect;
    }

    /// <summary>
    /// 获取rect四个角屏幕坐标
    /// </summary>
    public static MaskableGraphic GetSpaceCorners(this MaskableGraphic graphic, out Vector3[] corners, Canvas canvas = null, Camera camera = null)
    {
        corners = new Vector3[4];
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Vector3.zero;

        if (graphic == null)
        {
            Debug.LogError("graphic is Null");
            return null;
        }
        RectTransform rect = graphic.rectTransform;
        if (camera == null)
            camera = Camera.main;
        rect.GetWorldCorners(corners);
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay) { }
        else
        {
            for (int i = 0; i < corners.Length; i++)
                corners[i] = camera.WorldToScreenPoint(corners[i]);
        }
        return graphic;
    }

    #region World positions

    private static Vector3[] _fourCorners = new Vector3[4];//start bottom left and clockwise

    public static Vector2 GetWorldCenter(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Vector2((_fourCorners[0].x + _fourCorners[3].x) / 2f, (_fourCorners[0].y + _fourCorners[1].y) / 2f);
    }

    public static float GetWorldLeft(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return _fourCorners[0].x;
    }

    public static float GetWorldRight(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return _fourCorners[2].x;
    }

    public static float GetWorldTop(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return _fourCorners[1].y;
    }

    public static float GetWorldBottom(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return _fourCorners[0].y;
    }

    public static Vector2 GetWorldTopLeft(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Vector2(_fourCorners[0].x, _fourCorners[1].y);
    }

    public static Vector2 GetWorldTopRight(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Vector2(_fourCorners[2].x, _fourCorners[1].y);
    }

    public static Vector2 GetWorldBottomLeft(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Vector2(_fourCorners[0].x, _fourCorners[0].y);
    }

    public static Vector2 GetWorldBottomRight(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Vector2(_fourCorners[2].x, _fourCorners[0].y);
    }

    public static Rect GetWorldRect(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(_fourCorners);
        return new Rect(_fourCorners[0].x, _fourCorners[0].y, Mathf.Abs(_fourCorners[3].x - _fourCorners[0].x), Mathf.Abs(_fourCorners[1].y - _fourCorners[0].y));
    }

    #endregion

    #region 获取鼠标点中的图片像素点的颜色
    //https://blog.csdn.net/hany3000/article/details/46385005
    //image.GetRectPixelColor(out pixelColor);
    //_resultImage.color = pixelColor;

    public static MaskableGraphic GetRectPixelColor(this MaskableGraphic graphic, out Color color, Canvas canvas = null, Camera camera = null)
    {
        color = Color.black;
        if (graphic == null)
        {
            Debug.LogError("graphic is Null");
            return null;
        }
        color = GetColor(Input.mousePosition, graphic, canvas, camera);
        return graphic;
    }

    private static Color GetColor(Vector3 mousePos, MaskableGraphic graphic, Canvas canvas = null, Camera camera = null)
    {
        Image image = graphic as Image;
        if (RectContainsScreenPoint(mousePos, canvas, graphic.rectTransform, Camera.main))
        {
            var spaceRect = GetSpaceRect(canvas, graphic.rectTransform, Camera.main);
            var localPos = Input.mousePosition - new Vector3(spaceRect.x, spaceRect.y);
            var realPos = new Vector2(localPos.x, localPos.y);
            var imageToTextre = new Vector2(image.sprite.textureRect.width / spaceRect.width,
                image.sprite.textureRect.height / spaceRect.height);
            return image.sprite.texture.GetPixel((int)(realPos.x * imageToTextre.x), (int)(realPos.y * imageToTextre.y));
        }
        return Color.black;
    }

    public static bool RectContainsScreenPoint(Vector3 point, Canvas canvas, RectTransform rect, Camera camera)
    {
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return RectTransformUtility.RectangleContainsScreenPoint(rect, point, camera);
        return GetSpaceRect(canvas, rect, camera).Contains(point);
    }

    public static Rect GetSpaceRect(Canvas canvas, RectTransform rect, Camera camera)
    {
        Rect spaceRect = rect.rect;
        Vector3 spacePos;
        GetScreenPos(rect, out spacePos, canvas, camera);
        //lossyScale
        spaceRect.x = spaceRect.x * rect.lossyScale.x + spacePos.x;
        spaceRect.y = spaceRect.y * rect.lossyScale.y + spacePos.y;
        spaceRect.width = spaceRect.width * rect.lossyScale.x;
        spaceRect.height = spaceRect.height * rect.lossyScale.y;
        return spaceRect;
    }
    #endregion
}
#endregion

#region Component
public static class ComponentExtension
{
    /// <summary>获取第一级子物体的组件</summary>
    public static T[] GetComponentsInFirstChildren<T>(this Transform selfTrans) where T : Component
    {
        List<T> list = new List<T>();
        for (int i = 0; i < selfTrans.childCount; i++)
        {
            T t = selfTrans.GetChild(i).GetComponent<T>();
            if (t != null)
                list.Add(selfTrans.GetChild(i).GetComponent<T>());
        }
        return list.ToArray();
    }

    /// <summary>
    /// 找到父对象组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="inclusive"></param>
    /// <returns></returns>
    public static T GetComponentInParentNotInCludSelf<T>(this Component component) where T : Component
    {
        Transform parent = component.transform.parent;
        if (parent != null)
        {
            T t = parent.GetComponent<T>();
            if (t != null)
                return t;
        }
        return null;
    }

    /// <summary>
    /// 找到父对象组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <param name="inclusive"></param>
    /// <returns></returns>
    public static T GetComponentUpwards<T>(this Transform t, bool inclusive = false) where T : Component
    {
        T ret = null;

        Transform parent = inclusive ? t : t.parent;
        while (parent != null)
        {
            ret = parent.GetComponent<T>();
            if (ret != null)
                break;
            parent = parent.parent;
        }
        return ret;
    }

    /// <summary>获取第一层子物体的组件(是否包括非激活的)</summary>
    public static T[] GetComponentsInFirstHierarchyChildren<T>(this Component component, bool includeInactive = true) where T : Component
    {
        List<T> components = new List<T>();
        foreach (Transform child in component.transform)
        {
            if (!includeInactive && !child.gameObject.activeSelf)
                continue;

            T tComponent = child.GetComponent<T>();
            if (tComponent != null)
                components.Add(tComponent);
        }
        return components.ToArray();
    }

    /// <summary>
    /// 获取子物体并排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="c"></param>
    /// <param name="includeInactive"></param>
    /// <returns></returns>
    public static T[] GetComponentsInChildrenAlphaSort<T>(this Component c, bool includeInactive) where T : Component
    {
        T[] items = c.GetComponentsInChildren<T>(includeInactive);
        Array.Sort<T>(items, GameTool.IndexSort);

        return items;
    }

    /// <summary>
    /// 获取子物体并排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="c"></param>
    /// <param name="includeInactive"></param>
    /// <returns></returns>
    public static T[] GetComponentsInChildrenAlphaSort<T>(this GameObject go, bool includeInactive) where T : Component
    {
        T[] items = go.GetComponentsInChildren<T>(includeInactive);
        Array.Sort<T>(items, GameTool.IndexSort);

        return items;
    }

    public static Component SetParent(this Component component, Component parent)
    {
        Transform trans = component.transform;
        Transform parentTrans = parent.transform;
        trans.SetParent(parentTrans);
        return component;
    }
}
#endregion

#region Action
public static class ActionExtension
{
    //        this.InvokeRepeating(()=>{ AA("",2,3); },0.5f,5);
    /// <summary>
    /// 重复执行某一方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="action"></param>
    /// <param name="delayTime"></param>
    /// <param name="repeatCount"></param>
    /// <returns></returns>
    public static T InvokeRepeating<T>(this T t, Action action, float delayTime, int repeatCount = 0) where T : MonoBehaviour
    {
        t.StartCoroutine(WaitExcuteAction(t, action, delayTime, repeatCount));
        return t;
    }

    public static IEnumerator WaitExcuteAction(MonoBehaviour mono, Action action, float delayTime, int repeatCount)
    {
        if (repeatCount == -1)
        {
            yield return new WaitForSeconds(delayTime);
            action();
            mono.StartCoroutine(WaitExcuteAction(mono, action, delayTime, repeatCount));
        }
        else
        {
            for (int i = 0; i < repeatCount; i++)
            {
                yield return new WaitForSeconds(delayTime);
                action();
            }
        }
    }
}
#endregion

#endregion

#region 异常检测
public static class Guard
{
    /// <summary>
    /// 验证一个条件,并在该协定的条件失败时引发异常。
    /// </summary>
    /// <typeparam name="TException">异常</typeparam><param name="condition">条件</param>
    public static void Requires<TException>(bool condition) where TException : Exception, new()
    {
        //if (!condition)
        //    throw (object)Activator.CreateInstance<TException>();
    }

    /// <summary>不为空或者null</summary>
    public static void NotEmptyOrNull(string argumentValue, string argumentName)
    {
        if (string.IsNullOrEmpty(argumentValue))
            throw new ArgumentNullException(argumentName);
    }

    /// <summary>长度大于0</summary>
    public static void CountGreaterZero<T>(IList<T> argumentValue, string argumentName)
    {
        if (argumentValue.Count <= 0)
            throw new ArgumentNullException(argumentName);
    }

    /// <summary>元素部位空或者null</summary>
    public static void ElementNotEmptyOrNull(IList<string> argumentValue, string argumentName)
    {
        foreach (string str in (IEnumerable<string>)argumentValue)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(argumentName, "Argument element can not be Empty or Null.");
        }
    }

    /// <summary>内容不为空</summary>
    public static void NotNull(object argumentValue, string argumentName)
    {
        if (argumentValue == null)
            throw new ArgumentNullException(argumentName);
    }
}
#endregion

#region 通用支持
public static class Util
{
    /// <summary>
    /// 构建一个随机生成器
    /// </summary>
    /// <param name="seed">种子</param>
    /// <returns>
    /// 随机生成器
    /// </returns>
    public static System.Random MakeRandom(int? seed = null)
    {
        return new System.Random(seed.GetValueOrDefault(Util.MakeSeed()));
    }

    /// <summary>生成种子</summary>
    public static int MakeSeed()
    {
        return Environment.TickCount ^ Guid.NewGuid().GetHashCode();
    }

    /// <summary>
    /// 标准化位置
    /// </summary>
    /// <param name="sourceLength">源长度</param><param name="start">起始位置</param><param name="length">作用长度</param>
    internal static void NormalizationPosition(int sourceLength, ref int start, ref int? length)
    {
        start = start >= 0 ? Math.Min(start, sourceLength) : Math.Max(sourceLength + start, 0);
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
        int?local = @length;
        int num1;
        if (length.HasValue)
        {
            int? nullable = length;
            int num2 = 0;
            num1 = (nullable.GetValueOrDefault() >= num2 ? (nullable.HasValue ? 1 : 0) : 0) != 0 ? Math.Min(length.Value, sourceLength - start) : Math.Max(sourceLength + length.Value - start, 0);
        }
        else
            num1 = Math.Max(sourceLength - start, 0);
        int? nullable1 = new int?(num1);
      // ISSUE: explicit reference operation
        local = nullable1;
    }
}

public static class DebugUtil
{
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    public static void LogError(object message)
    {
        Debug.LogError(message);
    }
}
#endregion