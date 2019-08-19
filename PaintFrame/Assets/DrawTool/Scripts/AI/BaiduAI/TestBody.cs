using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestBody : MonoBehaviour {
    public Texture2D tex;
    public string texName;
    public RawImage rImg;
	// Use this for initialization
	void Start () {
		
	}

    private float time;
	// Update is called once per frame
	void Update () {
        //time += Time.deltaTime;
        //if (time > 2)//Input.GetKeyDown(KeyCode.A)
        //{
        //    byte[] bytes = tex.EncodeToJPG();
        //    Debug.LogError(AIManager.Instance.AIBodyAnalysisDetector(bytes));
        //    time = 0;
        //}
        if (Input.GetKeyDown(KeyCode.Space))
        {
            byte[] bytes = tex.EncodeToJPG(20);
            string result = AIManager.Instance.AIBodyAnalysisDetector(bytes, BodyType.BODYSEG);
            JsonParse.BaiDuBodySeg de = JsonParse.BaiDuBodySeg.ParseJsonBodySeg(result);
            rImg.texture = SCY.Utility.Base64Img(de.foreground);
        }
	}
}
