using System.Collections.Generic;
using System.Linq;
using Project.Scripts.EntitySystem.BlobAssets;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class TextureUtility
    {
        public static Texture2D CreateAtlas(Sprite[] sprites,Vector2Int tileSize,out Vector2Int[]atlasPosition, int rowSize = 8)
        {
            var positions = new List<Vector2Int>();
            var rows = Mathf.CeilToInt(sprites.Length / (float)rowSize);
            var atlas = new Texture2D(rowSize * tileSize.x, rows * tileSize.y)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                alphaIsTransparency = true,
            };

            for (var x = 0; x < atlas.width; x++)
            {
                for (var y = 0; y < atlas.height; y++) {atlas.SetPixel(x,y,Color.clear);}
            }

            var currentLeftBottom = new Vector2Int(0, 0);
            var count = 0;
            foreach (var sprite in sprites)
            {
                var spriteTex = sprite.texture;
                for (var x = 0; x < tileSize.x; x++)
                {
                    for (var y = 0; y < tileSize.y; y++)
                    {
                        atlas.SetPixel(x + currentLeftBottom.x, y + currentLeftBottom.y,
                            spriteTex.GetPixel(x + (int)sprite.rect.x, y + (int)sprite.rect.y));
                    }
                }
                
                positions.Add(currentLeftBottom);
               
                ++count;
                if (count >= rowSize)
                {
                    currentLeftBottom.x = 0;
                    currentLeftBottom.y += tileSize.y;
                }
                else
                {
                    currentLeftBottom.x += tileSize.x;
                }
            }
            
            atlas.Apply();

            atlasPosition = positions.ToArray();
            return atlas;
        }
        
        public static Texture2D CreateAtlasWithIDs(IDSpritePair[] idSpritePairs,Vector2Int tileSize,out BlobIDInt2Pair[] atlasPosition, int rowSize = 8)
        {
            var tex = CreateAtlas(idSpritePairs.Select(pair => pair.sprite).ToArray(), tileSize, out var positions, rowSize);
            atlasPosition = new BlobIDInt2Pair[idSpritePairs.Length];
            for (var i = 0; i < atlasPosition.Length; i++)
            {
                atlasPosition[i] = new BlobIDInt2Pair()
                {
                    ID = idSpritePairs[i].id,
                    Position = new int2(positions[i].x,positions[i].y),
                };
            }

            return tex;
        }
    }

    public struct IDSpritePair
    {
        public readonly int id;
        public readonly Sprite sprite;

        public IDSpritePair(int id, Sprite sprite)
        {
            this.id = id;
            this.sprite = sprite;
        }
    }
}