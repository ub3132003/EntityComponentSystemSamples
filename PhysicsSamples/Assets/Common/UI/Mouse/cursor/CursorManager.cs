using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTextureOnUI;//要替换的光标图片1
    public Texture2D cursorTextureOnPress;//要替换的光标图片2
    public Texture2D cursorTexture3;//要替换的光标图片3

    Texture2D t1;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.SetCursor(cursorTextureOnUI, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
