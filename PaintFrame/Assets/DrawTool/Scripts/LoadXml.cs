using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Xml;

public class LoadXml {                                      //读取Xml文本的工具

    //读取数值的方法
    public static string ReadXml(string id, string name)
    {
        string read = null;
        //创建文档
        XmlDocument xml = new XmlDocument();
        XmlReaderSettings set = new XmlReaderSettings();

        set.IgnoreComments = true;

        //Xml.Load加载的路径在StreamingAssets的脚本下
        xml.Load(XmlReader.Create((Application.streamingAssetsPath + "/Config.xml"), set));

        //得到Config节点下的所有子节点
        XmlNodeList xmlNodeList = xml.SelectSingleNode("Table").ChildNodes;
        //遍历所有的子节点
        foreach (XmlElement xnode in xmlNodeList)
        {
            //Table子节点下面，命名的id的属性
            if (xnode.GetAttribute("id") == id)
            {
                //遍历id的子节点
                foreach (XmlElement xChild in xnode)
                {
                    //"id"节点下面，子节点name
                    if (xChild.GetAttribute("name") == name) 
                    {
                        read = xChild.InnerText;
                        //print(xChild.InnerText);
                    }
                }
            }
        }
        return read;
    }
}
