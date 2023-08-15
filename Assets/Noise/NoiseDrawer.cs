/*
 * based on the work of Sebastian Lague https://www.youtube.com/@SebastianLague 
 */
using System;
using UnityEngine;

namespace Noise
{
    public class NoiseDrawer : MonoBehaviour
    {
        public enum DrawMode
        {
            GrayScale,
            Regions,
            Mesh,
            MeshDetailLevel
        }

        public DrawMode currentDrawMode;
        [Header("Display Parameters")] public Vector2Int size = new Vector2Int(256, 256);
        public Vector2 positionOffset = Vector2.zero;

        [Header("Noise Parameters"), Range(4f, 100f)]
        public float zoom = 1f;

        [Range(1, 10)] public int octaves = 4;
        [Range(0f, 1f)] public float persistence = .5f;
        [Range(1f, 10f)] public float lacunarity = 2f;
        public int seed;
        [Header("Region Data")] public TerrainType[] Regions;

        [Header("MeshParameters")] public float heightMultiplier = 10;
        public AnimationCurve meshHeightCurve;
        [Range(0, 6)] public int levelOfDetail;
        
        public bool autoUpdate;
        public Renderer myRenderer;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public void GenerateMap()
        {
            Texture2D texture;
            switch (currentDrawMode)
            {
                case DrawMode.GrayScale:
                    texture = Noise.GenerateNoiseTexture2D(size.x, size.y, zoom, positionOffset, octaves, persistence,
                        lacunarity, seed);
                    transform.localScale = new Vector3(texture.width, 1,texture.height);
                    myRenderer.sharedMaterial.mainTexture = texture;
                    break;
                case DrawMode.Regions:
                    texture = Noise.GenerateNoiseTexture2D(size.x, size.y, zoom, positionOffset, octaves, persistence,
                        lacunarity, seed, Regions);
                    transform.localScale = new Vector3(texture.width, 1,texture.height);
                    myRenderer.sharedMaterial.mainTexture = texture;
                    break;
                case DrawMode.Mesh:
                    float[] noiseMap = Noise.GenerateNoiseMap(size.x, size.y, zoom, positionOffset, seed, octaves,
                        persistence, lacunarity);
                    Texture2D texture2D = Noise.GenerateNoiseTexture2D(size.x, size.y, Noise.GenerateColorMap(noiseMap, Regions));
                    texture2D.filterMode = FilterMode.Bilinear;
                    texture2D.Apply();
                    DrawMesh(MeshUtility.GenerateTerrainMesh(noiseMap, size.x, size.y,heightMultiplier,meshHeightCurve),texture2D);

                    break;
                case DrawMode.MeshDetailLevel:
                    size = new Vector2Int(241, 241);
                    noiseMap = Noise.GenerateNoiseMap(size.x, size.y, zoom, positionOffset, seed, octaves,
                        persistence, lacunarity);
                    texture2D = Noise.GenerateNoiseTexture2D(size.x, size.y, Noise.GenerateColorMap(noiseMap, Regions));
                    texture2D.filterMode = FilterMode.Bilinear;
                    texture2D.Apply();
                    DrawMesh(MeshUtility.GenerateTerrainMesh(noiseMap, size.x, size.y,heightMultiplier,meshHeightCurve,levelOfDetail),texture2D);
                    break;
                default: return;
            }
        }

        private void DrawMesh(MeshData meshData, Texture2D texture)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }

    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
