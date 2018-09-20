using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool dragBool;
    private float mousePos;
    private float mouseSpeed = 400f;

    public GameObject top;

    int tmpId;
    private void Start()
    {
        tmpId = GameObject.FindObjectOfType<GetCamera>().suncount;
        Debug.Log(tmpId);
    }
    //拖动开始事件
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragBool = true;
        mousePos = Input.mousePosition.x;
        StartCoroutine(TimingDrag());
        Debug.Log("DragStart/mouse" + mousePos);
    }

    //拖动中事件
    public void OnDrag(PointerEventData eventData)
    {
        if (mousePos < Input.mousePosition.x)
        {
            //x = mouseSpeed;
            //StartCoroutine(AstrictImage());
            DragPos(50);
        }
        else
        {
            //x = -mouseSpeed;
            //StartCoroutine(AstrictImage());
            DragPos(-50);
        }
        Debug.Log("OnDrag/DragX");
    }

    //拖动结束事件
    public void OnEndDrag(PointerEventData eventData)
    {
        dragBool = false;
        StopCoroutine(TimingDrag());
        //StopCoroutine(AstrictImage());
        Debug.Log("EndDrag");
    }

    public void DragPos(float x)
    {
        if (top.transform.childCount != 0)
        {
            for (int i = 0; i < top.transform.childCount; i++)
            {
                float tmpX = top.transform.GetChild(i).transform.localPosition.x;
                float tmpY = top.transform.GetChild(i).transform.localPosition.y;
                top.transform.GetChild(i).transform.localPosition = new Vector3(tmpX + x, tmpY);

                if (top.transform.GetChild(i).transform.localPosition.x < -(tmpId * 490) / 1.9f)
                {
                    float offset = top.transform.GetChild(i).transform.localPosition.x-( - ((tmpId * 490) / 1.9f));
                    top.transform.GetChild(i).transform.localPosition = new Vector3((tmpId * 490) / 3.1f + offset, tmpY);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator TimingDrag()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            mousePos = Input.mousePosition.x;
            //Debug.Log("Tming");
        }
    }

    //float x;
    //IEnumerator AstrictImage()
    //{
    //    if (top.transform.childCount != 0)
    //    {
    //        for (int i = 0; i < top.transform.childCount; i++)
    //        {
    //            float tmpX = top.transform.GetChild(i).transform.localPosition.x;
    //            float tmpY = top.transform.GetChild(i).transform.localPosition.y;
    //            top.transform.GetChild(i).transform.localPosition = new Vector3(tmpX + x, tmpY);

    //            if (top.transform.GetChild(i).transform.localPosition.x < -(tmpId * 490) / 1.9f)
    //            {
    //                top.transform.GetChild(i).transform.localPosition = new Vector3((tmpId * 490) / 3.1f, tmpY);
    //            }
    //        }
    //        yield return new WaitForSeconds(10f);
    //    }
    //}
}
