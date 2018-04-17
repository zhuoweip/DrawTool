using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckImgRayCast
{
    /// <summary>
    /// 关闭没有button的Img的射线检测
    /// </summary>
    [MenuItem("Assets/------CheckRayCast------")]
    public static void CheckRayCast()
    {
        Image[] imgs =  GameObject.FindObjectsOfType<Image>();
        for (int i = 0; i < imgs.Length; i++)
        {
            imgs[i].SetNativeSize();
            if (imgs[i].transform.GetComponent<Button>() == null)
                imgs[i].raycastTarget = false;
            else
            {
                if (!imgs[i].raycastTarget)
                    imgs[i].raycastTarget = true;
            }
        }
    }
}
