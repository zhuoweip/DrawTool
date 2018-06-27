using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    public static void GetAllFiles(DirectoryInfo dir, int maxDay = 30 ,Hashtable ht = null)
    {
        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();   //初始化一个FileSystemInfo类型的实例
        foreach (FileSystemInfo i in fileinfo)              //循环遍历fileinfo下的所有内容
        {
            if (i is DirectoryInfo)             //当在DirectoryInfo中存在i时
            {
                GetAllFiles((DirectoryInfo)i);  //获取i下的所有文件
            }
            else
            {
                string str = i.FullName;        //记录i的绝对路径
                string path = Application.streamingAssetsPath;
                string strType = str.Substring(path.Length);

                string creatTime = i.CreationTime.ToShortDateString();
                DestoryHistoryImgs(maxDay, ht, creatTime, strType, i);
            }
        }
    }

    private static void DestoryHistoryImgs(int maxDay ,Hashtable ht,string _creatTime, string _strType, FileSystemInfo file)
    {
        int lerpTime = 0;

        //用图片名获取日期
        //string[] str = _strType.Split('-');
        //int strYearNum = int.Parse(str[0].Substring(7,str[0].Length - 7));//去除前面Images文件夹路径
        //int strMonthNum = int.Parse(str[1]);
        //int strDayNum = int.Parse(str[2]);

        //获取图片创建日期
        string[] str = _creatTime.Split('/');
        //Debug.LogError(_creatTime);
        int strYearNum = int.Parse(str[2]);
        int strMonthNum = int.Parse(str[0]);
        int strDayNum = int.Parse(str[1]);
        //Debug.LogError(strYearNum + "年 " + strMonthNum + "月 " + strDayNum + "日");

        string nowDate = DateTime.Now.ToString("yyyy-MM-dd");
        string[] nowDateStr = nowDate.Split('-');
        int nowYearNum = int.Parse(nowDateStr[0]);
        int nowMonthNum = int.Parse(nowDateStr[1]);
        int nowDayNum = int.Parse(nowDateStr[2]);
        //Debug.LogError(nowYearNum + "Year " + nowMonthNum + "Month " + nowDayNum + "Day");

        if (nowYearNum == strYearNum)//年份相同
        {
            if (nowMonthNum == strMonthNum)//月份相同
            {
                lerpTime = nowDayNum - strDayNum;
            }
            else if (nowMonthNum > strMonthNum)
            {
                AddMonthLerpTime(ref lerpTime, nowYearNum, strYearNum, nowMonthNum, strMonthNum);
                lerpTime = lerpTime - strDayNum + nowDayNum;
            }
        }
        else if (nowYearNum > strYearNum)//跨年
        {
            AddYearLerpTime(ref lerpTime, nowYearNum, strYearNum, nowMonthNum, strMonthNum);
            lerpTime = lerpTime - strDayNum + nowDayNum;
        }

        if (lerpTime >= maxDay)//删除保存期限之前的图片
        {
            file.Delete();
            file = null;
        }
        else
        {
            AddStrToHash(ht,_strType);
        }
    }

    private static void AddStrToHash(Hashtable ht, string _strType)
    {
        if (_strType.Substring(_strType.Length - 3).ToLower() == "png")
        {
            if (ht.Contains(_strType))
            {
                ht[_strType] = _strType;
            }
            else
            {
                ht.Add(_strType, _strType);
            }
        }
    }

    //每年平年闰年加的总天数不一样
    private static void AddYearLerpTime(ref int lerpTime, int _nowYearNum, int _stryearNum, int _nowMonthNum, int _strMonthNum)
    {
        if (_stryearNum < _nowYearNum)
        {
            if (_strMonthNum <= 12)
            {
                lerpTime += GetMonthLerp(_stryearNum, _strMonthNum);
                _strMonthNum++;
                AddYearLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
            else
            {
                _stryearNum++;
                _strMonthNum = 1;//重置月份
                AddYearLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
        }
        else
        {
            if (_strMonthNum < _nowMonthNum)
            {
                AddMonthLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
            }
        }
    }

    private static void AddMonthLerpTime(ref int lerpTime, int _nowYearNum, int _stryearNum, int _nowMonthNum, int _strMonthNum)
    {
        if (_strMonthNum < _nowMonthNum)
        {
            lerpTime += GetMonthLerp(_stryearNum, _strMonthNum);
            _strMonthNum++;
            AddMonthLerpTime(ref lerpTime, _nowYearNum, _stryearNum, _nowMonthNum, _strMonthNum);
        }
    }

    private static int GetMonthLerp(int yearNum, int monthNum)
    {
        int monthLerp = 0;
        if (monthNum == 2)
        {
            if (DateTime.IsLeapYear(DateTime.Now.Date.Year))//闰年//yearNum % 4 == 0
                monthLerp = 29;
            else
                monthLerp = 28;
        }
        else if (monthNum == 1 || monthNum == 3 || monthNum == 5
            || monthNum == 7 || monthNum == 8 || monthNum == 10 || monthNum == 12)
        {
            monthLerp = 31;
        }
        else
        {
            monthLerp = 30;
        }

        return monthLerp;
    }
}
