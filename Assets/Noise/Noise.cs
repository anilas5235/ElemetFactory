/*
 * based on the work of Sebastian Lague https://www.youtube.com/@SebastianLague 
 */

using System;
using UnityEngine;

namespace Noise
{
    public static class Noise
    {
        private const float Boarder = 100000f;

        public static float[] GenerateNoiseMap(int width, int height, float scale, Vector2 posOffset, int seed = 0,
            int octaves = 1, float persistence = 1, float lacunarity = 1)
        {
            if (scale <= 0) scale = .0000001f;
            float[] noiseMap = new float[width * height];

            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + posOffset.x;
                float offsetY = prng.Next(-100000, 100000) + posOffset.y;

                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            ForAllPixels(width, height, ((x, y) =>
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    /*
                    if (sampleX> Boarder) sampleX %= Boarder;
                    else if (sampleX < 0f) sampleX = sampleX % Boarder + Boarder;
                    if (sampleY > Boarder) sampleY %= Boarder;
                    else if (sampleY < 0f) sampleY = sampleY % Boarder + Boarder;*/

                    noiseHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x + y * width] = noiseHeight;
            }));

            for (int i = 0; i < noiseMap.Length; i++)
            {
                noiseMap[i] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[i]);
            }

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

        public static Color[] GenerateColorMap(float[] noiseMap, TerrainType[] regions)
        {
            Color[] colorMap = new Color[noiseMap.Length];
            for (int i = 0; i < noiseMap.Length; i++)
            {
                float currentHeight = noiseMap[i];
                for (int j = 0; j < regions.Length; j++)
                {
                    if (!(currentHeight <= regions[j].height)) continue;
                    colorMap[i] = regions[j].color;
                    break;
                }
            }

            return colorMap;
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height, Color[] colorMap)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(colorMap);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height, float scale)
        {
            return GenerateNoiseTexture2D(width, height, GenerateColorMap(GenerateNoiseMap(width, height, scale)));
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height, float scale, Vector2 posOffset)
        {
            return GenerateNoiseTexture2D(width, height,
                GenerateColorMap(GenerateNoiseMap(width, height, scale, posOffset)));
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height, float scale, Vector2 posOffset,
            int octaves, float persistence, float lacunarity, int seed = 0)
        {
            return GenerateNoiseTexture2D(width, height,
                GenerateColorMap(GenerateNoiseMap(width, height, scale, posOffset, seed, octaves, persistence,
                    lacunarity)));
        }

        public static Texture2D GenerateNoiseTexture2D(int width, int height, float scale, Vector2 posOffset,
            int octaves, float persistence, float lacunarity, int seed, TerrainType[] regions)
        {
            return GenerateNoiseTexture2D(width, height,
                GenerateColorMap(
                    GenerateNoiseMap(width, height, scale, posOffset, seed, octaves, persistence, lacunarity),
                    regions));
        }

        public static Color[] ColorFilter(float highThreshold, float lowThreshold, Color a, Color b, Color[] colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].r >= highThreshold) colors[i] = a;
                else if (colors[i].r <= lowThreshold) colors[i] = b;
                else colors[i] = Color.black;
            }

            return colors;
        }

        public static void ForAllPixels(int width, int height, Action<int, int> doFunction)
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
