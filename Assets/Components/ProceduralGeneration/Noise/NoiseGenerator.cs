using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;

[CreateAssetMenu(fileName = "NoiseGenerator", menuName = "Scriptable Objects/NoiseGenerator")]
public class NoiseGenerator : ProceduralGenerationMethod
{
    [Header("Noise Settings")]
    [SerializeField] private float _frequency;
    [SerializeField] private FastNoiseLite.NoiseType _noiseType;
    [SerializeField] private FastNoiseLite.FractalType _fractalType;
    [SerializeField] private int _octaves;
    [SerializeField] private float _lacunarity;
    [SerializeField] private float _gain;

    [Header("Terrain Thresholds")]
    [Tooltip("Values below this become water (-1 to 1)")]
    [SerializeField] private float _waterThreshold;

    [Tooltip("Values between water and sand become sand")]
    [SerializeField] private float _sandThreshold;

    [Tooltip("Values between sand and rock become grass")]
    [SerializeField] private float _rockThreshold;

    [Header("Secondary Noise (Optional)")]
    [SerializeField] private bool _useSecondaryNoise;
    [SerializeField] private float _secondaryFrequency;
    [SerializeField] private float _secondaryWeight;

    [Header("Visualization")]
    [SerializeField] private bool _visualizeStepByStep;
    [SerializeField] private int _tilesPerStep;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(_noiseType);
        noise.SetFrequency(_frequency);
        noise.SetFractalType(_fractalType);
        noise.SetFractalOctaves(_octaves);
        noise.SetFractalLacunarity(_lacunarity);
        noise.SetFractalGain(_gain);

        FastNoiseLite secondaryNoise = null;
        if (_useSecondaryNoise)
        {
            secondaryNoise = new FastNoiseLite(RandomService.Seed + 1000);
            secondaryNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            secondaryNoise.SetFrequency(_secondaryFrequency);
        }

        int tileCount = 0;
        int totalTiles = Grid.Width * Grid.Lenght;

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                    continue;

                float noiseValue = noise.GetNoise(x, y);

                if (_useSecondaryNoise && secondaryNoise != null)
                {
                    float secondaryValue = secondaryNoise.GetNoise(x, y);
                    noiseValue = Mathf.Lerp(noiseValue, secondaryValue, _secondaryWeight);
                }

                string tileType = GetTileTypeFromNoise(noiseValue);

                AddTileToCell(cell, tileType, true);

                tileCount++;

                if (_visualizeStepByStep && tileCount % _tilesPerStep == 0)
                {
                    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
                }
            }
        }

        Debug.Log($"Map generation complete! Generated {tileCount} tiles.");
    }

    private string GetTileTypeFromNoise(float noiseValue)
    {
        if (noiseValue < _waterThreshold)
            return WATER_TILE_NAME;

        if (noiseValue < _sandThreshold)
            return SAND_TILE_NAME;

        if (noiseValue < _rockThreshold)
            return GRASS_TILE_NAME;

        return ROCK_TILE_NAME;
    }
}