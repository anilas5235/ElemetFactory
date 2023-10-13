using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct ChunkDataAspect : IAspect
    {
        private readonly RefRW<ChunkDataComponent> _chunkData;
        private static int ChunkSize => ChunkDataComponent.ChunkSize;
        private static int HalfChunkSize => ChunkDataComponent.HalfChunkSize;
        private static int CellSize => GenerationSystem.WorldScale;
        private static int ChunkUnitSize => ChunkDataComponent.ChunkUnitSize;
        public int2 ChunksPosition => _chunkData.ValueRO.ChunkPosition;
        public float3 WorldPosition => _chunkData.ValueRO.WorldPosition;
        public int NumPatches => _chunkData.ValueRO.ResourcePatches.Length;

        public bool InView
        {
            get => _chunkData.ValueRO.InView;
            set => _chunkData.ValueRW.InView = value;
        }

        public CellObject GetCell(int2 position,int2 chunkPosition)
        {
           return IsValidPositionInChunk(position) ?
               _chunkData.ValueRO.CellObjects[GetAryIndex(position)]:
               GetCellFormPseudoPosition(position,chunkPosition);
        }

        private CellObject GetCellFormPseudoPosition(int2 position, int2 chunkPosition)
        {
            int2 chunkOffset = new int2(Mathf.FloorToInt((float)position.x / ChunkSize),
                Mathf.FloorToInt((float)position.y / ChunkSize));
            int2 newPos = position - chunkOffset * ChunkSize;
            int2 newChunkPos = chunkPosition + chunkOffset;
            GenerationSystem.TryGetChunk(newChunkPos, out ChunkDataAspect chunkData);
            return chunkData.GetCell(newPos, newChunkPos);
        }
        
        public bool TryToPlaceBuilding( int buildingID, int2 cellPosition, FacingDirection facingDirection)
        {
            CellObject cell = _chunkData.ValueRO.CellObjects[GetAryIndex(cellPosition)];
            if (cell.IsOccupied) return false;

            Entity entity = PlacingSystem.PlaceBuilding(cell,buildingID,facingDirection);
            
            PlacedBuildingData placedBuildingData = new PlacedBuildingData() //TODO: Put this on a component on the entity
            {
                directionID = buildingID,
                buildingDataID = (int)facingDirection,
                origin = cell.Position,
            };

            var cellObjs = _chunkData.ValueRW.CellObjects;
            foreach (int2 pos in ResourcesUtility.GetGridPositionList(placedBuildingData))
            { 
                int index = GetAryIndex(pos);
               var ob = cellObjs[index];
               ob.PlaceBuilding(entity);
               cellObjs[index] = ob;
            }
            _chunkData.ValueRW.CellObjects = cellObjs;
            return true;
        }

        public static int2 GetPseudoPosition(int2 myChunkPosition, int2 otherChunkPosition, int2 position)
        {
            int2 chunkOffset = otherChunkPosition - myChunkPosition;
            return position + chunkOffset * ChunkSize;
        }

        public static int GetAryIndex(int2 position)
        {
            return position.y * ChunkSize + position.x;
        }
        public static float3 GetCellWorldPosition(int2 cellPos, float3 chunkWorldPosition)
        {
            return chunkWorldPosition + new float3((cellPos.x - HalfChunkSize)+.5f,
                (cellPos.y - HalfChunkSize)+.5f, 0)* CellSize;
        }

        public static int2 GetCellPositionFormWorldPosition(float3 worldPosition, out int2 chunkPosition)
        {
            chunkPosition = new int2(Mathf.RoundToInt(worldPosition.x / ChunkUnitSize),
                Mathf.RoundToInt(worldPosition.y / ChunkUnitSize));

            float2 cellFloatPos = worldPosition.xy - chunkPosition * ChunkUnitSize;
            
            int2 cellPos =new int2(Mathf.FloorToInt(cellFloatPos.x/CellSize) + HalfChunkSize,
                                         Mathf.FloorToInt(cellFloatPos.y/CellSize)+HalfChunkSize);
            return cellPos;
        }

        public static bool IsValidPositionInChunk(int2 position)
        {
            return position.x >= 0 && position.x < ChunkSize &&
                   position.y >= 0 && position.y < ChunkSize;
        }
    }
}
