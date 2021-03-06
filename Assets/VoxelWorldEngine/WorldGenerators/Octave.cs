﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxelWorldEngine.Attributes;
using VoxelWorldEngine.Enums;
using VoxelWorldEngine.Noise;

namespace VoxelWorldEngine
{
    public class Octave : WorldGenerator
    {
        [Space(10)]
        [Header("Density noise properties")]
        [Range(0, 1.0f)]
        public float Density = 0.45f;
        public bool Inverted = false;
        [Space]
        public float Frequency3D = 2;
        [Range(1, 8)]
        [Tooltip("Number of iterations for the noise, each octave is a new noise sum.")]
        public int Octaves3D = 1;
        [Range(1f, 4f)]
        [Tooltip("Phase between the different noise frequencies when the sum is applied.")]
        public float Lacunarity3D = 1;
        [Range(0f, 1f)]
        [Tooltip("Multiplier for the noise sum, decrease each noise sum")]
        public float Persistence3D = 0.5f;
        public float Width3D = 50f;
        [Space]
        [Range(1, 3)]
        [Tooltip("Noise dimensions, (x,z) as a 2Dplane, y is the up axis.")]
        public int DimensionsDensity = 3;
        [Tooltip("Method to apply.")]
        public NoiseMethodType NoiseTypeDensity;

        //public Dictionary<string, string> NoiseLayers;
        //****************************************************************
        protected override BlockType HeightNoise(Vector3 pos)
        {
            //NoiseMethod_delegate method = NoiseMap.NoiseMethods[(int)NoiseType][Dimensions - 1];
            //float sample = (NoiseMap.Sum(method, pos / WidthMagnitude, Frequency, Octaves, Lacunarity, Persistence) + 1) / 2;

            ////Apply the height magnitude
            ////float h = Mathf.PerlinNoise(pos.x / (WidthMagnitude), pos.z / (WidthMagnitude)) * 200;
            ////Debug.Log(h);
            ////TODO: Control the height, cannot generate mountains without increase the depht
            ////sample *= HeightMagnitude;
            ////sample *= h;

            ////Set the minimum height of the world
            //sample += WorldHeight;

            //if (pos.y < WorldHeight)
            //    return BlockType.SAND;
            //if (pos.y < sample)
            //    return BlockType.STONE;

            return BlockType.NULL;
        }
        protected override BlockType HeightNoise(Vector3 pos, BiomeAttributes attr)
        {
            NoiseMethod_delegate method = NoiseMap.NoiseMethods[(int)attr.NoiseType][attr.Dimensions - 1];
            float sample = (NoiseMap.Sum(method, pos / attr.WidthMagnitude, attr.Frequency, attr.Octaves, attr.Lacunarity, attr.Persistence) + 1) / 2;
            //sample = (float)NoiseLibrary.GetValue(pos.x / attr.WidthMagnitude, 0, pos.z / WidthMagnitude, attr.Frequency, attr.Octaves, attr.Lacunarity, attr.Persistence);

            if (attr.NoiseType == NoiseMethodType.Value)
                sample *= 0.5f;

            sample *= attr.HeightMagnitude;

            //Set the minimum height of the world
            sample += MinWorldHeight;

            if (pos.y < MinWorldHeight)
                return BlockType.SAND;
            if (pos.y < sample)
                if (debug.IsActive)
                    return attr.DebugBlock;
                else
                    return BlockType.STONE;

            return BlockType.NULL;
        }
        protected override BlockType OreNoise(Vector3 pos, OreAttributes attr)
        {
            if (pos.y > attr.HeightLimit)
                return BlockType.NULL;

            NoiseMethod_delegate method = NoiseMap.NoiseMethods[(int)attr.NoiseType][attr.Dimensions - 1];
            Vector3 widthPos = new Vector3(pos.x / attr.XWidth, pos.y / attr.YWidth, pos.z / attr.ZWidth);
            float sample = (NoiseMap.Sum(method, widthPos, attr.Frequency, attr.Octaves, attr.Lacunarity, attr.Persistence) + 1) / 2;

            BlockType ore = BlockType.NULL;

            if (attr.Inverted)
            {
                if (sample > attr.Density)
                {
                    ore = attr.OreBlock;
                }
            }
            else
            {
                if (sample < attr.Density)
                {
                    ore = attr.OreBlock;
                }
            }

            return ore;
        }
        protected override BlockType[] StrataNoise(Vector3 pos)
        {
            //TODO: NEED FIX, crash if there are multiple chunks in Y
            BlockType[] arr = new BlockType[(int)pos.y + 1];

            //Apply a simple noise
            float range = (Mathf.PerlinNoise(pos.x, pos.z) /*+ 1*/) * 5;

            for (int i = 0; i < (int)range; i++)
            {
                if (i == 0)
                    arr[(int)pos.y - i] = BlockType.GRASS;
                else
                    arr[(int)pos.y - i] = BlockType.DIRT;
            }

            return arr;
        }
        /// <summary>
        /// Use to test the noise 3D generation, for ores and caves
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected override BlockType DensityNoise(Vector3 pos)
        {
            //TEST ORE GENERATION: Using to test the ores

            NoiseMethod_delegate method = NoiseMap.NoiseMethods[(int)NoiseTypeDensity][DimensionsDensity - 1];
            //float sample = (NoiseMap.Sum(method, pos / WidthDensity, Frequency, Octaves, Lacunarity, Persistence) + 1) / 2;
            float sample = (NoiseMap.Sum(method, pos / Width3D, Frequency3D, Octaves3D, Lacunarity3D, Persistence3D) + 1) / 2;

            if (Inverted)
            {
                if (sample > Density)
                {
                    return BlockType.BRICK;
                }
            }
            else
            {
                if (sample < Density)
                {
                    return BlockType.BRICK;
                }
            }

            return BlockType.NULL;
        }
    }
}
