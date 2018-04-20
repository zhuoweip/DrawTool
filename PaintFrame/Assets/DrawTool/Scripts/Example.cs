using UnityEngine;

[ExecuteInEditMode]
public class Example : MonoBehaviour {

    public string Text;

    private GUIStyle style;
    private Color color = new Color(0, 0, 0, .9f);

    private void OnGUI()
    {
        style = new GUIStyle(GUI.skin.label);
        style.fontSize = 50;
        style.normal.background = new Texture2D(1, 1);
        style.normal.background.SetPixel(0, 0, Color.black);
        style.padding = new RectOffset(5, 5, 5, 5);
        style.wordWrap = true;

        var c = GUI.backgroundColor;
        GUI.backgroundColor = color;
        var rect = GUI.skin.label.CalcSize(new GUIContent(Text));
        GUILayout.BeginArea(new Rect(50, 50, Screen.width/2, Screen.height));
        GUILayout.Label(Text, style);
        GUILayout.EndArea();
        GUI.backgroundColor = c;
    }

}
