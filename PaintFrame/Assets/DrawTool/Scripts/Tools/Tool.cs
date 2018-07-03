using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool:MonoBehaviour
{
    public Gradient color;

    public Sprite bgSpr;

    public float width;

    public Texture texture;

    public LineTextureMode textureMode;

    public bool creatPaintLine;

    public Material material;

    public bool repeatTexture;

    public ToolFeature toolFeature = ToolFeature.Line;

    /// <summary>
    /// 绘画模式：线条，填充，印记
    /// </summary>
    public enum ToolFeature
    {
        Line,
        Fill,
        Stamp,
    };
}
