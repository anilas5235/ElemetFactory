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

        private void Start()
        {
            //CreateMesh(Resources.Load<Material>("Materials/Excavator"),new float2(.5f, .4f));
            
            //CreateMesh(Resources.Load<Material>("Materials/Combiner"),new float2(.25f, .5f));
            AssetDatabase.SaveAssets();
        }

        private void CreateMesh(Material material,float2 center)
        {
            string path = BasePath;
            
            Texture tex = material.GetTexture(BaseTexture);
            Mesh mesh = MeshUtils.CreateQuad(center,(float)tex.width / PixelsPerUnit, (float)tex.height / PixelsPerUnit);
            
            AssetDatabase.CreateAsset(mesh, $"{path}/{material.name}.mesh");
        }
    }
}
