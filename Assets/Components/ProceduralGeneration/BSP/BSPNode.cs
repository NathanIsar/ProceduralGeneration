using UnityEngine;
using VTools.RandomService;
using VTools.Grid;
using Grid = VTools.Grid.Grid;

public class BSPNode
{
    private Grid _grid;
    private readonly RandomService _randomService;
    private Vector2Int _minSize;
    private BSPNode _parent;
    private BSPNode _firstChild;
    private BSPNode _secondChild;
    private RectInt _area;
    private RectInt _room;

    public BSPNode(Grid grid, RandomService randomService, Vector2Int minSize, RectInt area, BSPNode parent = null)
    {
        _grid = grid;
        _randomService = randomService;
        _minSize = minSize;
        _area = area;
        _parent = parent;
    }

    public bool CanSplit()
    {
        return _area.width > _minSize.x * 2 && _area.height > _minSize.y * 2;
    }

    public void Split()
    {
        bool splitHorizontally = _area.width < _area.height;

        if (_area.width > _minSize.x * 2 && _area.height > _minSize.y * 2)
        {
            splitHorizontally = _randomService.Range(0, 2) == 0;
        }

        if (splitHorizontally)
        {
            int splitY = _randomService.Range(_area.yMin + _minSize.y, _area.yMax - _minSize.y);
            _firstChild = new BSPNode(_grid, _randomService, _minSize,
                new RectInt(_area.xMin, _area.yMin, _area.width, splitY - _area.yMin), this);
            _secondChild = new BSPNode(_grid, _randomService, _minSize,
                new RectInt(_area.xMin, splitY, _area.width, _area.yMax - splitY), this);
        }
        else
        {
            int splitX = _randomService.Range(_area.xMin + _minSize.x, _area.xMax - _minSize.x);
            _firstChild = new BSPNode(_grid, _randomService, _minSize,
                new RectInt(_area.xMin, _area.yMin, splitX - _area.xMin, _area.height), this);
            _secondChild = new BSPNode(_grid, _randomService, _minSize,
                new RectInt(splitX, _area.yMin, _area.xMax - splitX, _area.height), this);
        }
    }

    public void SetRoom(RectInt room)
    {
        _room = room;
    }

    public RectInt GetRoom()
    {
        return _room;
    }

    public bool IsLeaf()
    {
        return _firstChild == null && _secondChild == null;
    }

    public BSPNode GetFirstChild()
    {
        return _firstChild;
    }

    public BSPNode GetSecondChild()
    {
        return _secondChild;
    }

    public RectInt GetArea()
    {
        return _area;
    }

    public BSPNode GetParent()
    {
        return _parent;
    }
}