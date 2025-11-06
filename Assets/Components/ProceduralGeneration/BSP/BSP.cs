using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
public class BSP : ProceduralGenerationMethod
{
    [Header("BSP Parameters")]
    [SerializeField] private Vector2Int _minPartitionSize = new Vector2Int(10, 10);
    [SerializeField] private int _maxDepth = 4;

    [Header("Room Parameters")]
    [SerializeField] private Vector2Int _roomSizeMin = new Vector2Int(3, 3);
    [SerializeField] private Vector2Int _roomSizeMax = new Vector2Int(7, 7);

    private BSPNode _rootNode;
    private HashSet<Vector2Int> _corridorCells = new HashSet<Vector2Int>();
    private HashSet<string> _connectedPairs = new HashSet<string>();

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        _corridorCells.Clear();
        _connectedPairs.Clear();

        RectInt initialArea = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        _rootNode = new BSPNode(Grid, RandomService, _minPartitionSize, initialArea);
        await BuildBSPTree(_rootNode, 0, cancellationToken);

        List<BSPNode> leafNodes = new List<BSPNode>();
        GetLeafNodes(_rootNode, leafNodes);

        Debug.Log($"BSP created {leafNodes.Count} leaf nodes (rooms)");

        foreach (BSPNode leaf in leafNodes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await CreateRoomInNode(leaf, cancellationToken);
        }

        await ConnectSiblingNodes(_rootNode, cancellationToken);

        BuildGround();
    }

    private async UniTask BuildBSPTree(BSPNode node, int depth, CancellationToken cancellationToken)
    {
        if (depth >= _maxDepth || !node.CanSplit())
            return;

        cancellationToken.ThrowIfCancellationRequested();

        node.Split();

        await BuildBSPTree(node.GetFirstChild(), depth + 1, cancellationToken);
        await BuildBSPTree(node.GetSecondChild(), depth + 1, cancellationToken);
    }

    private void GetLeafNodes(BSPNode node, List<BSPNode> leafNodes)
    {
        if (node == null)
            return;

        if (node.IsLeaf())
        {
            leafNodes.Add(node);
            return;
        }

        GetLeafNodes(node.GetFirstChild(), leafNodes);
        GetLeafNodes(node.GetSecondChild(), leafNodes);
    }

    private async UniTask CreateRoomInNode(BSPNode node, CancellationToken cancellationToken)
    {
        RectInt area = node.GetArea();

        int roomWidth = RandomService.Range(_roomSizeMin.x, Mathf.Min(_roomSizeMax.x, area.width) + 1);
        int roomLength = RandomService.Range(_roomSizeMin.y, Mathf.Min(_roomSizeMax.y, area.height) + 1);

        int posX = RandomService.Range(area.xMin, area.xMax - roomWidth + 1);
        int posZ = RandomService.Range(area.yMin, area.yMax - roomLength + 1);

        RectInt roomRect = new RectInt(posX, posZ, roomWidth, roomLength);
        node.SetRoom(roomRect);

        for (int x = roomRect.xMin; x < roomRect.xMax; x++)
        {
            for (int z = roomRect.yMin; z < roomRect.yMax; z++)
            {
                if (Grid.TryGetCellByCoordinates(x, z, out var cell))
                {
                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
    }

    private async UniTask ConnectSiblingNodes(BSPNode node, CancellationToken cancellationToken)
    {
        if (node == null || node.IsLeaf())
            return;

        cancellationToken.ThrowIfCancellationRequested();

        await ConnectSiblingNodes(node.GetFirstChild(), cancellationToken);
        await ConnectSiblingNodes(node.GetSecondChild(), cancellationToken);

        BSPNode firstChild = node.GetFirstChild();
        BSPNode secondChild = node.GetSecondChild();

        if (firstChild != null && secondChild != null)
        {
            RectInt room1 = GetBestRoomFromSubtree(firstChild, secondChild);
            RectInt room2 = GetBestRoomFromSubtree(secondChild, firstChild);

            if (room1.width > 0 && room2.width > 0)
            {
                string pairKey = GetPairKey(room1, room2);

                if (!_connectedPairs.Contains(pairKey))
                {
                    _connectedPairs.Add(pairKey);
                    await CreateOptimizedCorridor(room1, room2, cancellationToken);
                }
            }
        }
    }

    private string GetPairKey(RectInt room1, RectInt room2)
    {
        int minX = Mathf.Min(room1.xMin, room2.xMin);
        int maxX = Mathf.Max(room1.xMin, room2.xMin);
        int minY = Mathf.Min(room1.yMin, room2.yMin);
        int maxY = Mathf.Max(room1.yMin, room2.yMin);

        return $"{minX},{minY}-{maxX},{maxY}";
    }

    private RectInt GetBestRoomFromSubtree(BSPNode node, BSPNode targetNode)
    {
        if (node == null)
            return new RectInt(0, 0, 0, 0);

        List<BSPNode> leafNodes = new List<BSPNode>();
        GetLeafNodes(node, leafNodes);

        if (leafNodes.Count == 0)
            return new RectInt(0, 0, 0, 0);

        Vector2Int targetCenter = GetSubtreeCenter(targetNode);

        RectInt closestRoom = new RectInt(0, 0, 0, 0);
        float minDistance = float.MaxValue;

        foreach (BSPNode leaf in leafNodes)
        {
            RectInt room = leaf.GetRoom();
            if (room.width > 0)
            {
                Vector2Int roomCenter = new Vector2Int((int)room.center.x, (int)room.center.y);
                float distance = Vector2Int.Distance(roomCenter, targetCenter);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRoom = room;
                }
            }
        }

        return closestRoom;
    }

    private Vector2Int GetSubtreeCenter(BSPNode node)
    {
        List<BSPNode> leafNodes = new List<BSPNode>();
        GetLeafNodes(node, leafNodes);

        if (leafNodes.Count == 0)
            return Vector2Int.zero;

        Vector2 avgCenter = Vector2.zero;
        int validRooms = 0;

        foreach (BSPNode leaf in leafNodes)
        {
            RectInt room = leaf.GetRoom();
            if (room.width > 0)
            {
                avgCenter += room.center;
                validRooms++;
            }
        }

        if (validRooms > 0)
            avgCenter /= validRooms;

        return new Vector2Int((int)avgCenter.x, (int)avgCenter.y);
    }

    private async UniTask CreateOptimizedCorridor(RectInt room1, RectInt room2, CancellationToken cancellationToken)
    {
        Vector2Int center1 = new Vector2Int((int)room1.center.x, (int)room1.center.y);
        Vector2Int center2 = new Vector2Int((int)room2.center.x, (int)room2.center.y);

        List<Vector2Int> corridorPath = new List<Vector2Int>();

        bool horizontalFirst = Mathf.Abs(center2.x - center1.x) > Mathf.Abs(center2.y - center1.y);

        if (horizontalFirst)
        {
            // Segment horizontal
            int startX = center1.x;
            int endX = center2.x;
            int y = center1.y;

            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);

            for (int x = minX; x <= maxX; x++)
            {
                corridorPath.Add(new Vector2Int(x, y));
            }

            int startY = center1.y;
            int endY = center2.y;
            int x2 = center2.x;

            int minY = Mathf.Min(startY, endY);
            int maxY = Mathf.Max(startY, endY);

            for (int y2 = minY; y2 <= maxY; y2++)
            {
                corridorPath.Add(new Vector2Int(x2, y2));
            }
        }
        else
        {
            // Segment vertical
            int startY = center1.y;
            int endY = center2.y;
            int x = center1.x;

            int minY = Mathf.Min(startY, endY);
            int maxY = Mathf.Max(startY, endY);

            for (int y = minY; y <= maxY; y++)
            {
                corridorPath.Add(new Vector2Int(x, y));
            }

            int startX = center1.x;
            int endX = center2.x;
            int y2 = center2.y;

            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);

            for (int x2 = minX; x2 <= maxX; x2++)
            {
                corridorPath.Add(new Vector2Int(x2, y2));
            }
        }

        foreach (Vector2Int pos in corridorPath)
        {
            await PlaceCorridorTile(pos.x, pos.y, cancellationToken);
        }

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
    }

    private async UniTask PlaceCorridorTile(int x, int z, CancellationToken cancellationToken)
    {
        Vector2Int pos = new Vector2Int(x, z);

        if (!Grid.TryGetCellByCoordinates(x, z, out var cell))
            return;

        if (cell.ContainObject && cell.GridObject.Template.Name == ROOM_TILE_NAME)
            return;

        if (_corridorCells.Contains(pos))
            return;

        _corridorCells.Add(pos);
        AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
    }

    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (Grid.TryGetCellByCoordinates(x, z, out var cell))
                {
                    GridGenerator.AddGridObjectToCell(cell, groundTemplate, false);
                }
            }
        }
    }
}