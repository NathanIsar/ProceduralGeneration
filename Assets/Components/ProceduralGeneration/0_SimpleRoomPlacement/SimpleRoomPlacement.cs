using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2Int _roomSizeMin = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int _roomSizeMax = new Vector2Int(7, 7);
      
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            int roomCount = 0;
            List<RectInt> placedRooms = new List<RectInt>();
            List<RectInt> disconnectedRooms = new List<RectInt>();
            List<RectInt> connectedRooms = new List<RectInt>();

            for (int i = 0; i < _maxSteps; i++)
            {

                cancellationToken.ThrowIfCancellationRequested();

                if (roomCount >= _maxRooms)
                    break;

                int roomWidth = RandomService.Range(_roomSizeMin.x, _roomSizeMax.x + 1);
                int roomLength = RandomService.Range(_roomSizeMin.y, _roomSizeMax.y + 1);

                if (roomWidth > Grid.Width || roomLength > Grid.Lenght)
                {
                    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
                    continue;
                }

                int posX = RandomService.Range(0, Grid.Width - roomWidth);
                int posZ = RandomService.Range(0, Grid.Lenght - roomLength);

                var roomRect = new RectInt(posX, posZ, roomWidth, roomLength);

                const int spacing = 1;
                if (!CanPlaceRoom(roomRect, spacing))
                {
                    await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
                    continue;
                }

                for( int x = roomRect.xMin; x < roomRect.xMax; x++)
                {
                    for (int z = roomRect.yMin; z < roomRect.yMax; z++)
                    {
                        if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                        {
                            Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                            continue;
                        }

                        AddTileToCell(chosenCell, "Room", true);
                    }
                }
                roomCount++;

                placedRooms.Add(roomRect);
                disconnectedRooms = new List<RectInt>(placedRooms);
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            RectInt startRoom = disconnectedRooms[0];
            connectedRooms.Add(startRoom);
            disconnectedRooms.Remove(startRoom);

            while(disconnectedRooms.Count >0)
            {
                float minDistance = float.MaxValue;
                RectInt roomToConnect = default;
                RectInt closestConnectedRoom = default;

                foreach( RectInt disconnectedRoom in disconnectedRooms)
                {
                    foreach( RectInt connectedRoom in connectedRooms)
                    {
                        float distance = Vector2.Distance(disconnectedRoom.center, connectedRoom.center);

                        if( distance < minDistance )
                        {
                            minDistance = distance;
                            roomToConnect = disconnectedRoom;
                            closestConnectedRoom = connectedRoom;
                        }
                    }
                }

                var startX = roomToConnect.center.x;
                var startZ = roomToConnect.center.y;
                var endX = closestConnectedRoom.center.x;
                var endZ = closestConnectedRoom.center.y;

                // Horizontal corridor
                for (int x = Mathf.Min((int)startX, (int)endX); x <= Mathf.Max(startX, endX); x++)
                {
                    if (Grid.TryGetCellByCoordinates(x, (int)startZ, out var corridorCell))
                    {
                        AddTileToCell(corridorCell, "Corridor", true);
                    }
                }

                // Vertical corridor
                for (int z = Mathf.Min((int)startZ, (int)endZ); z <= Mathf.Max(startZ, endZ); z++)
                {
                    if (Grid.TryGetCellByCoordinates((int)endX, z, out var corridorCell))
                    {
                        AddTileToCell(corridorCell, "Corridor", true);
                    }
                }


                connectedRooms.Add(roomToConnect);
                disconnectedRooms.Remove(roomToConnect);
            }

            BuildGround();
        }
        
        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}