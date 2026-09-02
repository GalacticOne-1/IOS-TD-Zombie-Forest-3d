using UnityEngine;

namespace Galactic1.Tools
{
    public static class EditorUtils
    {
        public static void DrawSprite(Sprite sprite, float size = 30f)
        {
            if (sprite == null) return;
            Texture2D tex = sprite.texture;
            Rect texCoords = new Rect(
                sprite.rect.x / tex.width,
                sprite.rect.y / tex.height,
                sprite.rect.width / tex.width,
                sprite.rect.height / tex.height
            );
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
            GUI.DrawTextureWithTexCoords(rect, tex, texCoords);
        }
    }
}