using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameObjData
{
    public GameObject gameObject;     //物体
    public int Id;

    public GameObjData()
    {

    }

    public GameObjData(GameObject gameObject)
    {
        Id = GetCamera.Index;
        this.gameObject = gameObject;
        gameObject.name = "Obj_" + Id.ToString();
        GetCamera.Index++;
    }

    public static GameObjData CreateObj(params object[] param)
    {
        GameObject Object;
        if (string.IsNullOrEmpty((string)param[0]))     
            Object = new GameObject();                     //创建
        else                                          
        {
            string path = param[0] as string;
            if (string.IsNullOrEmpty(path))
                return null;
            Object = Resources.Load(path) as GameObject;   //加载
            if (Object == null)
                    return null;
            Object = GameObject.Instantiate(Object);
        }
        Transform parent = (Transform)param[1];
        Object.SetActive(false);
        Object.transform.SetParent(parent);
        GameObjData gameObjDate = new GameObjData(Object);
        return gameObjDate;
    }

    public static void ReleaseObj(GameObjData Data)
    {
        if (Data != null)
            Data.Release();
    }
    public void Release()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        }
    }
}
