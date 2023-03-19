using UnityEngine;

/// <summary>
/// Class for generating simple and more complex perlin noise "maps".
/// Noise methods return just 1d float arrays, but they are organized
/// 2-dimensionally in order to create textures and terrain data from noise.
/// Use helper methods FloatArrayToBWColorArray and ColorArrayToTexture to
/// create color data and texture from generated noise.
/// </summary>
public static class Noise
{
    /// <summary>
    /// Generate simple noise without octaves.
    /// Basically this: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
    /// </summary>
    /// <param name="width">Color array width.</param>
    /// <param name="height">Color array height.</param>
    /// <param name="seed">Seed for random generation.</param>
    /// <param name="scale">Noise scale.</param>
    /// <param name="offset">Offset sampled area.</param>
    /// <returns>1-dimensional float array with noise.</returns>
    public static float[] SimplePerlinNoise(int width, int height, int seed, float scale, Vector2 offset)
    {
        float[] floatArray = new float[width * height];

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
                floatArray[(int)y * width + (int)x] = sample;
                x++;
            }
            y++;            
        }
        return floatArray;
    }

    /// <summary>
    /// This is a more complex perlin noise implementation, that mixes several "layers" of noise (octaves) to produce more
    /// chaotic noise. 
    /// This implementation is part of excellent series of tutorials by Sebastian Lange: https://www.youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3 (E03: Octaves).
    /// You can download the source codes for the tutorials from this repository: https://github.com/SebLague/Procedural-Landmass-Generation
    /// Check out also a nice C++ library called libnoise. They have good theory break down and glossary explaining the coherent noise: https://libnoise.sourceforge.net/glossary/
    /// </summary>
    /// <param name="width">Color array width.</param>
    /// <param name="height">Color array height.</param>
    /// <param name="seed">Seed for random generation.</param>
    /// <param name="scale">Noise scale.</param>
    /// <param name="offset">Offset sampled area.</param>
    /// <param name="octaves">One of the coherent-noise functions in a series of coherent-noise functions that are added together to form Perlin noise.</param>
    /// <param name="persistance">A multiplier that determines how quickly the amplitudes diminish for each successive octave in a Perlin-noise function.</param>
    /// <param name="lacunarity">A multiplier that determines how quickly the frequency increases for each successive octave in a Perlin-noise function. Id est: how much there are "lakes" (lat. lacuna).</param>
    /// <returns>1-dimensional float array with noise.</returns>
    public static float[] PerlinNoiseWithOctaves(int width, int height, int seed, float scale, Vector2 offset, int octaves, float persistance, float lacunarity)
    {
        float[] floatArray = new float[width * height];

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
                    maxNoiseHeight = noiseHeight;                
                else if (noiseHeight < minNoiseHeight)                
                    minNoiseHeight = noiseHeight;
                
                floatArray[(int)y * width + (int)x] = noiseHeight;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float inverseLerped = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, floatArray[(int)y * width + (int)x]);
                floatArray[(int)y * width + (int)x] = inverseLerped;
            }
        }

        return floatArray;
    }

    /// <summary>
    /// Convert array of floats to Color type array
    /// </summary>
    /// <param name="floatArray">Float array.</param>
    /// <param name="width">Width of the "map"</param>
    /// <param name="height">Height of the "map"</param>
    /// <returns>Color array.</returns>
    public static Color[] FloatArrayToBWColorArray(float[] floatArray, int width, int height)
    {
        Color[] colors = new Color[width * height];

        //Float to color
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float f = floatArray[y * width + x];
                colors[y * width + x] = new Color(f, f, f);
            }
        }

        return colors;
    }

    /// <summary>
    /// Create Texture2D from color array.
    /// </summary>
    /// <param name="colorArray">Color array.</param>
    /// <param name="textureWidth">Array and texture width.</param>
    /// <param name="textureHeight">Array and texture height.</param>
    /// <returns></returns>
    public static Texture2D ColorArrayToTexture(Color[] colorArray, int textureWidth, int textureHeight)
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // Each pixel in the color array to texture coordinates
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
