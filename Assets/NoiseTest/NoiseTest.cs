using System;
using Project.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoiseTest
{
    public class NoiseTest : MonoBehaviour
    {
        public Vector2Int size = new Vector2Int(256,256);
        public Vector2 positionOffset = Vector2.zero;
        [Range(0.0000001f,100f)]public float zoom = 1f;
        [Range(0f,1f)] public float thresholdA = .8f, thresholdB = .2f;
        public bool autoUpdate;
        public Renderer myRenderer;


        private void Start()
        {
            positionOffset += new Vector2(Random.Range(-10f,10f),Random.Range(-10f,10f));
        }

        public void GenerateMap()
        {
            myRenderer.sharedMaterial.mainTexture = Noise.GenerateNoiseTexture2D(size.x, size.y, zoom, positionOffset,
                thresholdA, thresholdB, Color.red, Color.blue);
        }
    }
}
