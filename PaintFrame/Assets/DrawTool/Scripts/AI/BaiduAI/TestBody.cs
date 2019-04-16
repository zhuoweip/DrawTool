using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBody : MonoBehaviour {
    public Texture2D tex;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.frameCount % 10 == 0)//Input.GetKeyDown(KeyCode.A)
        {
            byte[] bytes = tex.EncodeToJPG();
            Debug.LogError(AIManager.Instance.AIBodyAnalysisDetector(bytes));
            
        }
	}
}
