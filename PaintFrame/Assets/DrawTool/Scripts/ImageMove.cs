using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ImageMove : MonoBehaviour {
    int i = 0;
    private void Start()
    {
        i = GameObject.FindObjectOfType<GetCamera>().suncount;
    }

    // Update is called once per frame
    void LateUpdate() {
        
        if(transform.localPosition.x< -(i * 490) /1.9f)
        {
            transform.localPosition = new Vector3((i * 490) / 3.1f, transform.localPosition.y);
        }
        //transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition+new Vector3(-1,0,0), Time.deltaTime*2000);
        Vector3 target = transform.localPosition + new Vector3(-1f, 0, 0);
        //GetComponent<RectTransform>().DOLocalMove(target, Time.deltaTime);
        transform.localPosition = new Vector3(Mathf.Round(target.x), Mathf.Round(target.y), 0);
        //transform.Translate(-0.01f, 0, 0);
    }
}
