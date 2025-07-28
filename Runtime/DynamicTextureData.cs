using DaVikingCode.RectanglePacking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicAtlas
{
    public class DynamicTextureData
    {
        public int Id { get { return Rect.id; } }
        public int ReferenceCount { get; private set; }
        public IntegerRectangle Rect { get; private set; }
        public Sprite Sprite { get; private set; }

        public DynamicTextureData(int index, IntegerRectangle rect)
        {
            Rect = rect;
            Rect.id = index;
            ReferenceCount = 0;
        }

        public void SetSprite(Sprite sprite, string sprite_name, int x, int y, int width, int height)
        {
            sprite.name = sprite_name;
            Sprite = sprite;
            Rect.x = x;
            Rect.y = y;
            Rect.width = width;
            Rect.height = height;
        }

        public Color32[] GetPixels32(Color32[] allPixels, int textureWidth, int textureHeight)
        {
            if (Sprite == null)
            {
                Debug.LogError("DynamicTextureData: Sprite is null");
                return null;
            }
            if (Rect.x < 0 || Rect.y < 0 || Rect.width <= 0 || Rect.height <= 0 ||
            Rect.x + Rect.width > textureWidth || Rect.y + Rect.height > textureHeight)
            {
                Debug.LogError("区域参数超出纹理范围！");
                return null;
            }
            Color32[] regionPixels = new Color32[Rect.width * Rect.height];

            for (int row = 0; row < Rect.height; row++)
            {
                int sourceIndex = (Rect.y + row) * textureWidth + Rect.x;
                int targetIndex = row * Rect.width;
                System.Array.Copy(allPixels, sourceIndex, regionPixels, targetIndex, Rect.width);
            }

            return regionPixels;
        }

        public void AddReference()
        {
            ReferenceCount++;
        }

        public void RemoveReference()
        {
            ReferenceCount--;
        }
    }
}