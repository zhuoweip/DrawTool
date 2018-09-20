using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ImageMove : MonoBehaviour
{
    int i = 0;
    float tmpInt = 0;

    private Rigidbody2D rig2D;
    private float m_Speed = 100f;

    private void Start()
    {
        rig2D = this.GetComponent<Rigidbody2D>();
        i = GameObject.FindObjectOfType<GetCamera>().suncount;
        //Debug.Log(this.name+"/"+transform.localPosition);
        Debug.Log(-(i * 490) / 1.9f);
        //Debug.Log(i);
        //tmpInt = (i * 490) / 1.9f-2720f;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        MoveStop();
        Speed();
    }

    float offset;
    public void MoveStop()
    {
        if (transform.localPosition.x < -(i * 490) / 1.9f)
        {
            if (-(i * 490) / 1.9f - transform.localPosition.x != 0)
            {
                offset = -(i * 490) / 1.9f - transform.localPosition.x;
            }
            transform.localPosition = new Vector3((i * 490) / 3.1f + offset, transform.localPosition.y);
        }

    }

    public void Speed()
    {
        if (DragImage.dragBool)
        {
            return;
        }

        Vector3 target = transform.localPosition + new Vector3(-1f, 0, 0);
        transform.localPosition = new Vector3(Mathf.Round(target.x), Mathf.Round(target.y), 0);
    }
}
