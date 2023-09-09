using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class MaterialCreator : MonoBehaviour
    {
        private const string BasePath = "Assets/Project/Resources/Materials/ItemMaterials/";

        public Shader[] shaders;
        private static readonly int ContentColor = Shader.PropertyToID("_ContentColor");

        public void CreateMaterials()
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    for (int k = 0; k < 11; k++)
                    {
                        CreateMaterialBatch(new Vector3Int(i,j,k));
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
        }

        private void CreateMaterialBatch(Vector3Int color)
        {
            string fileName = TurnToHex(color.x) + TurnToHex(color.y) + TurnToHex(color.z);
            string path = BasePath + fileName;
            
            if (AssetDatabase.IsValidFolder(path)) return;

            AssetDatabase.CreateFolder(BasePath, fileName);

            foreach (var shader in shaders)
            {
                Material material = new Material(shader);
                material.SetColor(ContentColor, new Color(color.x / 10f, color.y / 10f, color.z / 10f, 1f));
                material.name = fileName;

                AssetDatabase.CreateAsset(material, $"Assets/Project/Resources/Materials/Gas_Bottle/fileName/{shader.name}_{fileName}.mat");
            }
        }

        private string TurnToHex(int number)
        {
            if (number < 10) return number.ToString();
            return number switch
            {
                10 => "a",
                11 => "b",
                _ => "error"
            };
        }
    }
}


