using Project.Scripts.Utilities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental
{
#if UNITY_EDITOR
    public class MeshCreator : MonoBehaviour
    {
        private const string BasePath = "Assets/Project/Resources/Mesh";
        
        private const int PixelsPerUnit = 200;
        private static readonly int BaseTexture = Shader.PropertyToID("_BaseTexture");

        public float2 center;
        public float2 size;
        public string assetName;

        private void CreateMesh(Material material,float2 center)
        {
            var tex = material.GetTexture(BaseTexture);
            var mesh = MeshUtils.CreateQuad(center,(float)tex.width / PixelsPerUnit, (float)tex.height / PixelsPerUnit);
            CreateMeshAsset(mesh,material.name);
        }

        public void CreateMesh()
        {
            CreateMeshAsset(MeshUtils.CreateQuad(center,size.x,size.y),assetName);
        }

        private void CreateMeshAsset(Mesh mesh, string usedAssetName)
        {
            AssetDatabase.CreateAsset(mesh, $"{BasePath}/{usedAssetName}.mesh");
            AssetDatabase.SaveAssets();
        }
    }
#endif
}
