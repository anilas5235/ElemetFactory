using System;
using System.Linq;
using Project.Scripts.Utilities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental
{
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
            Texture tex = material.GetTexture(BaseTexture);
            Mesh mesh = MeshUtils.CreateQuad(center,(float)tex.width / PixelsPerUnit, (float)tex.height / PixelsPerUnit);
            CreateMeshAsset(mesh,material.name);
        }

        public void CreateMesh()
        {
            CreateMeshAsset(MeshUtils.CreateQuad(center,size.x,size.y),assetName);
        }

        private void CreateMeshAsset(Mesh mesh, string usedAssetName)
        {
            string path = BasePath;
            AssetDatabase.CreateAsset(mesh, $"{path}/{usedAssetName}.mesh");
            AssetDatabase.SaveAssets();
        }
    }
}
