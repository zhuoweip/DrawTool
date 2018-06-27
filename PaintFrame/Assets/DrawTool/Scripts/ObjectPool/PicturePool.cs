using System.Collections.Generic;
using UnityEngine;

public class PicturePool : MonoBehaviour
{
    public List<GameObject> poolList = new List<GameObject>();

    public void InitPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddToPool();
        }
    }

    public void AddToPool()
    {
        GameObject obj = new GameObject();
        obj.transform.SetParent(this.transform);
        poolList.Add(obj);
    }

    public void CleanPool()
    {
        if (poolList != null)
        {
            poolList.Clear();
        }
    }
    public int GetPoolSize()
    {
        return poolList.Count;
    }

    public GameObject GetLastElement()
    {
        if (poolList == null || poolList.Count == 0)
            return null;
        return poolList[poolList.Count - 1];
    }

    public void RemoveLastElement()
    {
        if (poolList == null || poolList.Count == 0)
            return;
        poolList.RemoveAt(poolList.Count - 1);
    }


    private void OnDestroy()
    {
        for (int i = 0; i < poolList.Count; i++)
        {
            Destroy(poolList[i]);
        }
        poolList.Clear();
    }
}
