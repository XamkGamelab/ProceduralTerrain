using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /// <summary>
    /// Generate new perlin noise texture.
    /// </summary>
    /// <param name="width">Pixel width of the texture</param>
    /// <param name="height">Pixel height of the texture</param>
    /// <param name="xOrigin">The x origin of the sampled area in the plane.</param>
    /// <param name="yOrigin">The y origin of the sampled area in the plane.</param>
    /// <param name="scale">The number of cycles of the basic noise pattern that are repeated over the width and height of the texture.</param>
    public static Color[] SimplePerlinNoise(int width, int height, int seed, float scale, Vector2 offset)
    {
        Color[]  colorArray = new Color[width * height];

        System.Random prng = new System.Random(seed);        
        float offsetX = prng.Next(-100000, 100000) + offset.x;
        float offsetY = prng.Next(-100000, 100000) + offset.y;
            
        float y = 0f;        
        while (y < height)
        {
            float x = 0;
            while (x < width)
            {
                float sampleX = offsetX + x / width * scale;
                float sampleY = offsetY + y / height * scale;
                float sample = Mathf.PerlinNoise(sampleX, sampleY);
                colorArray[(int)y * width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;            
        }
        return colorArray;
    }
    
    public static Color[] PerlinNoiseWithOctaves(int width, int height, int seed, float scale, Vector2 offset, int octaves, float persistance, float lacunarity)
    {
        //float[,] noiseMap = new float[mapWidth, mapHeight];
        Color[] colorArray = new Color[width * height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                
                colorArray[(int)y * width + (int)x] = new Color(noiseHeight, noiseHeight, noiseHeight);                
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float inverseLerped = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, colorArray[(int)y * width + (int)x].r);
                colorArray[(int)y * width + (int)x] = new Color(inverseLerped, inverseLerped, inverseLerped);
                //noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, colorArray[(int)y * width + (int)x].r);
            }
        }

        return colorArray;
    }

    public static Texture2D ColorArrayToTexture(Color[] colorArray, int textureWidth, int textureHeight)
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // Reverse each pixel in the color array to texture coordinates
        int y = 0;
        while (y < textureHeight)
        {
            int x = 0;
            while (x < textureWidth)
            {
                texture.SetPixel(x, y, colorArray[(int)y * textureWidth + (int)x]);
                x++;
            }
            y++;
        }
        
        texture.Apply();

        return texture;
    }
}
