using System;
using UnityEngine;

namespace NoiseTest
{
    public static class Noise
    {
        private const float Boarder = 100000f;

        public static float[] GenerateNoiseMap(int width, int height, float scale, Vector2 posOffset)
        {
            if (scale <= 0) scale = .0000001f;
            float[] noiseMap = new float[width * height];
            if (posOffset.x> Boarder) posOffset.x %= Boarder;
            else if (posOffset.x < 0f) posOffset.x = posOffset.x % Boarder + Boarder;
            if (posOffset.y > Boarder) posOffset.y %= Boarder;
            else if (posOffset.y < 0f) posOffset.y = posOffset.y % Boarder + Boarder;
            
            ForAllPixels(width,height,((x, y) =>
            {
                float sampleX = (x + posOffset.x) / scale;
                float sampleY = (y + posOffset.y) / scale;

                noiseMap[x  + y*width] = Mathf.PerlinNoise(sampleX,sampleY);
            } ));

            return noiseMap;
        }

        public static float[] GenerateNoiseMap(int width, int height, float scale)
        {
            return GenerateNoiseMap(width, height, scale, Vector2.zero);
        }

        public static Color[] GenerateColorMap(float[] noiseMap)
        {
            Color[] colorMap = new Color[noiseMap.Length];
            for (int i = 0; i < noiseMap.Length; i++)
            {
                colorMap[i] = Color.Lerp(Color.white, Color.black, noiseMap[i]);
            }
            return colorMap;
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height,Color[] colorMap)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(colorMap);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GenerateNoiseTexture2D(int width, int height,float scale)
        {
            return GenerateNoiseTexture2D(width, height, GenerateColorMap(GenerateNoiseMap(width, height, scale)));
        }
        
        public static Texture2D GenerateNoiseTexture2D(int width, int height,float scale, Vector2 posOffset)
        {
            return GenerateNoiseTexture2D(width, height, GenerateColorMap(GenerateNoiseMap(width, height, scale,posOffset)));
        }
        
        public static Texture2D GenerateNoiseTexture2D(int width, int height,float scale, Vector2 posOffset,float highThreshold, float lowThreshold, Color a, Color b)
        {
            return GenerateNoiseTexture2D(width, height,ColorFilter(highThreshold,lowThreshold,a,b, GenerateColorMap(GenerateNoiseMap(width, height, scale,posOffset))));
        }

        public static Color[] ColorFilter(float highThreshold, float lowThreshold, Color a, Color b,Color[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].r >= highThreshold) colors[i] = a;
                else if (colors[i].r <= lowThreshold) colors[i] = b;
                else colors[i] = Color.black;
            }

            return colors;
        }
        
        public static void ForAllPixels(int width, int height, Action<int,int> doFunction)
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
