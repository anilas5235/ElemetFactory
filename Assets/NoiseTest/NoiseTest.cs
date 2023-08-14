using System;
using Project.Scripts.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NoiseTest
{
    public class NoiseTest : MonoBehaviour
    {
        public Vector2Int Size = new Vector2Int(256,256);
        public Vector2 PositionOffset = Vector2.zero;
        public float zoom = 1f;
        [Range(0f,1f)] public float thresholdA = .8f, thresholdB = .2f;
        private Material myMaterial;

        private Vector2Int oldSize;
        private Vector2 oldPositionOffset;
        private float oldZoom,oldThresholdA, oldThresholdB ;

        private void Start()
        {
            myMaterial = GetComponent<Renderer>().material;
            PositionOffset += new Vector2(Random.Range(-10f,10f),Random.Range(-10f,10f));
        }

        private void FixedUpdate()
        {
            if(oldSize != Size || oldZoom != zoom||oldPositionOffset != PositionOffset||
               oldThresholdA != thresholdA|| oldThresholdB != thresholdB)
            {
                myMaterial.mainTexture = GenerateMap(Size.x, Size.y);
                oldSize = Size;
                oldPositionOffset = PositionOffset;
                oldZoom = zoom;
                oldThresholdA = thresholdA;
                oldThresholdB = thresholdB;
            }
        }

        Texture2D GenerateMap(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            ForAllCells(width, height, ((x, y) =>
            {
                float sample = Mathf.PerlinNoise((float)x / width * zoom + PositionOffset.x,
                    (float)y / height * zoom + PositionOffset.y);

                Color color = Color.white;
                if (sample >= thresholdA) color = Color.red;
                else if (sample <= thresholdB) color = Color.blue;

                //Color color = new Color(sample, sample, sample);

                texture.SetPixel(x, y, color);
            }));

            texture.Apply();
            return texture;
        }

        private void ForAllCells(int width, int height, Action<int,int> doFunction)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    doFunction(x, y);
                }
            }
        }
    }
}
