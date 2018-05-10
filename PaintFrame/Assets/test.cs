using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour 
{

    public int count;

    public Button btn;

    public GameObject GO;

    private float time;


    public void AA()
    {
        time = Time.realtimeSinceStartup;
        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject();
        }
        Debug.LogError(Time.realtimeSinceStartup - time);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //AA();
            GO.SetActive(true);
        }
    }

    private void Start()
    {
        btn.onClick.AddListener(()=>
        {
            Debug.LogError("AA");
        });
    }
}
