using UnityEngine;

public enum STRING_COLOR
{
    /// <summary>白色</summary>
    White,
    /// <summary>蓝色</summary>
    Blue,
    /// <summary>绿色</summary>
    Green,
    /// <summary>红色</summary>
    Red,
    /// <summary>灰色</summary>
    Gray,
    /// <summary>橙色</summary>
    Orange,
    /// <summary>淡灰</summary>
    ThinGray,
    /// <summary>深绿</summary>
    DeepGreen,
    /// <summary>深绿ex</summary>
    DeepDGreen,
    /// <summary>浅蓝</summary>
    ThinBule,
    /// <summary>棕色</summary>
    Brown,
    /// <summary>灰白</summary>
    CGray,
    /// <summary>黄色</summary>
    Yellow
}

public enum AIType
{
    /// <summary>百度SDK</summary>
    BaiDu,
    /// <summary>腾讯SDK</summary>
    Tencent
}

public enum OcrType
{
    [Header("通用识别")]
    GENERAL_BASIC,
    [Header("通用文字识别（高精度版）")]
    ACCURATE_BASIC,
    [Header("通用文字识别（含生僻字版）")]
    GENERAL_ENHANCED,
    [Header("手写识别")]
    HANDWRITING,


}