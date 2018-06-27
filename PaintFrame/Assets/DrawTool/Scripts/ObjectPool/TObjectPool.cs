/*===========================
 *Purpose:对象池
============================*/
using System;
using System.Collections.Generic;
using UnityEngine;

public class TObjectPool<T> where T : class, new()
{
    public static int Index;

    public delegate T Creat(params object[] _params);    //创建委托
    public delegate void Destroy(T t);                   //销毁委托

    private Creat pCreat = null;
    private Destroy pDestroy = null;

    private Queue<T> m_inactitveGameObjects;
    private List<T> m_actitveGameObjects;
    private object[] _params;

    //构建池
    public TObjectPool(int count, Creat pcreat, Destroy pdestroy, params object[] _vparams)
    {
        if (pcreat == null)
        {
            Debug.LogError("TObjectPool 对象池,委托为空");
            return;
        }
        pCreat = pcreat;
        pDestroy = pdestroy;
        _params = _vparams;

        count = Mathf.Clamp(count, 0, 100);
        m_inactitveGameObjects = new Queue<T>();
        for (int i = 0; i < count; ++i)
        {
            T t = pCreat(_params);
            m_inactitveGameObjects.Enqueue(t);
        }
        m_actitveGameObjects = new List<T>();
    }

    //销毁
    public void Release()
    {
        using (var enumerator = m_inactitveGameObjects.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (pDestroy != null)
                    pDestroy.Invoke(enumerator.Current);
            }
        }
        for (int i = 0; i < m_actitveGameObjects.Count; i++)
        {
            if (pDestroy != null)
                pDestroy.Invoke(m_actitveGameObjects[i]);
        }

        m_inactitveGameObjects.Clear();
        m_actitveGameObjects.Clear();
    }

    //获取池中激活数量
    public int GetActiveObjectCount()
    {
        return m_actitveGameObjects.Count;
    }

    public T GetActiveObject(int index)
    {
        return m_actitveGameObjects[index];
    }

    //获得一个激活的
    public T OnActiveGameObject()
    {
        T ret = null;
        if (m_inactitveGameObjects.Count > 0)
        {
            ret = m_inactitveGameObjects.Dequeue();
        }
        else
        {

            Debug.LogFormat("RoleId = {0} 实例化数量不够新加一个，当前正在使用数量 ：{1}", _params[0],m_actitveGameObjects.Count);
            ret = pCreat(_params);
        }
        m_actitveGameObjects.Add(ret);
        return ret;
    }

    public void OnActiveGameObject(Queue<T> quene)
    {
        T data = OnActiveGameObject();
        if (typeof(T).Equals(typeof(GameObjData)))
        {
            (data as GameObjData).gameObject.SetActive(true);
        }
        quene.Enqueue(data);
    }

    //转换激活对象转换为非激活对象
    public bool InActiveGameObject(T t)
    {
        if (t != null)
        {
            if (m_actitveGameObjects.Contains(t))
            {
                m_actitveGameObjects.Remove(t);
            }
            if (!m_inactitveGameObjects.Contains(t))
            {
                m_inactitveGameObjects.Enqueue(t);
            }
            return true;
        }
        return false;
    }

    //删除一个对象，不保存
    public bool DeleOneGameObject(T t)
    {
        if (t != null)
        {
            if (m_actitveGameObjects.Contains(t))
            {
                m_actitveGameObjects.Remove(t);
            }
            else if (m_inactitveGameObjects.Count > 0)
            {
                m_inactitveGameObjects.Dequeue();
            }
            if (pDestroy != null)
                pDestroy.Invoke(t);
            return true;
        }
        return false;
    }


    // 回收全部非激活
    public void InActiveGameAllObjects()
    {
        List<T> activeAll = new List<T>(m_actitveGameObjects);
        for (int i = 0; i < activeAll.Count; i++)
        {
            InActiveGameObject(activeAll[i]);
        }
    }
}

