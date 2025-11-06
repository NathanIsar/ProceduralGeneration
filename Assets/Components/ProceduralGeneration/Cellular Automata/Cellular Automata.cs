using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;

[CreateAssetMenu(fileName = "CellularAutomata", menuName = "Scriptable Objects/CellularAutomata")]
public class CellularAutomata : ProceduralGenerationMethod
{
    [SerializeField] private int _noiseDensity = 50;
    [SerializeField] private int _grassBirthThreshold = 5;
    [SerializeField] private int _grassSurvivalThreshold = 4;
    [SerializeField] private bool _considerBordersAsWater = true;
    [SerializeField] private int _visualUpdateInterval = 5; 

    private bool[,] _currentState;
    private bool[,] _nextState; 

    private static readonly (int dx, int dy)[] NeighborOffsets = new[]
    {
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1),           (0, 1),
        (1, -1),  (1, 0),  (1, 1)
    };

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        int width = Grid.Width;
        int length = Grid.Lenght;

        _currentState = new bool[width, length];
        _nextState = new bool[width, length]; 

        InitializeRandomGrid();
        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

        for (int step = 0; step < _maxSteps; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ApplyCellularAutomataStep();

            if (step % _visualUpdateInterval == 0 || step == _maxSteps - 1)
            {
                ApplyStateToGrid();
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        if (_maxSteps % _visualUpdateInterval != 0)
        {
            ApplyStateToGrid();
        }
    }

    private void InitializeRandomGrid()
    {
        int width = Grid.Width;
        int length = Grid.Lenght;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                bool isGrass = RandomService.Range(0, 100) < _noiseDensity;
                _currentState[x, y] = isGrass;
            }
        }

        ApplyStateToGrid();
    }

    private void ApplyCellularAutomataStep()
    {
        int width = Grid.Width;
        int length = Grid.Lenght;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                int grassNeighbors = CountGrassNeighborsOptimized(x, y, width, length);
                bool isCurrentlyGrass = _currentState[x, y];

                _nextState[x, y] = isCurrentlyGrass
                    ? grassNeighbors >= _grassSurvivalThreshold
                    : grassNeighbors >= _grassBirthThreshold;
            }
        }

        (_currentState, _nextState) = (_nextState, _currentState);
    }

    private int CountGrassNeighborsOptimized(int x, int y, int width, int length)
    {
        int count = 0;

        foreach (var (dx, dy) in NeighborOffsets)
        {
            int neighborX = x + dx;
            int neighborY = y + dy;

            if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= length)
            {
                if (!_considerBordersAsWater)
                    count++;
                continue;
            }

            if (_currentState[neighborX, neighborY])
                count++;
        }

        return count;
    }

    private void ApplyStateToGrid()
    {
        int width = Grid.Width;
        int length = Grid.Lenght;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                {
                    string tileName = _currentState[x, y] ? GRASS_TILE_NAME : WATER_TILE_NAME;
                    AddTileToCell(cell, tileName, true);
                }
            }
        }
    }
}