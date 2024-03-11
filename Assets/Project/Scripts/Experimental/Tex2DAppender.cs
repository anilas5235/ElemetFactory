using Project.Scripts.ItemSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Project.Scripts.Experimental
{
    public class Tex2DAppender : MonoBehaviour
    {
        private const string BasePath = "Assets/Project/Textures";
        public const int RowSize = 8;

        public string fileName = "test";

        public void MakeAtlas()
        {
            var itemScriptableData = Resources.LoadAll<ItemScriptableData>("Data/Items");
            var tex2DArray = new Sprite[itemScriptableData.Length];
            for (var i = 0; i < tex2DArray.Length; i++)
            {
                tex2DArray[i] = itemScriptableData[i].sprite;
            }

            var tex = AppendToAtlas(tex2DArray);
            CreateTex2DAsset(tex,fileName);
            GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Atlas", tex);
        }

        private Texture2D AppendToAtlas(Sprite[] sprites)
        {
            var tex0 = sprites[0];
            var size = new Vector2Int((int)tex0.rect.width, (int)tex0.rect.height);
            var rows = Mathf.CeilToInt(sprites.Length / (float)RowSize);
            var atlas = new Texture2D(RowSize * size.x, rows * size.y)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                alphaIsTransparency = true,
            };

            for (var x = 0; x < atlas.width; x++)
            {
                for (var y = 0; y < atlas.height; y++)
                {
                    atlas.SetPixel(x,y,Color.clear);
                }
            }


            var currentLeftTop = new Vector2Int(0, 0);
            var count = 0;
            foreach (var sprite in sprites)
            {
                var spriteTex = sprite.texture;
                for (var x = 0; x < size.x; x++)
                {
                    for (var y = 0; y < size.y; y++)
                    {
                        atlas.SetPixel(x + currentLeftTop.x, y + currentLeftTop.y,
                            spriteTex.GetPixel(x + (int)sprite.rect.x, y + (int)sprite.rect.y));
                    }
                }
               
                ++count;
                if (count >= RowSize)
                {
                    currentLeftTop.x = 0;
                    currentLeftTop.y += size.y;
                }
                else
                {
                    currentLeftTop.x += size.x;
                }
            }
            
            atlas.Apply();

            return atlas;
        }
        private void CreateTex2DAsset(Texture2D tex, string usedAssetName)
        {
#if UNITY_EDITOR
            
            AssetDatabase.CreateAsset(tex, $"{BasePath}/{usedAssetName}.mesh");
            AssetDatabase.SaveAssets();
#endif
        }
    }

}
