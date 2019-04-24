using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBody : MonoBehaviour {
    public Texture2D tex;
	// Use this for initialization
	void Start () {
		
	}

    private float time;
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if (time > 2)//Input.GetKeyDown(KeyCode.A)
        {
            byte[] bytes = tex.EncodeToJPG();
            Debug.LogError(AIManager.Instance.AIBodyAnalysisDetector(bytes));
            time = 0;
        }
	}
}
