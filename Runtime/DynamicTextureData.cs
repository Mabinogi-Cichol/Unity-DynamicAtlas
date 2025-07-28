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