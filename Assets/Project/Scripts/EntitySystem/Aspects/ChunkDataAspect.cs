using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Buildings;
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
            int2 newChunkPos = GetChunkAndCellPositionFromPseudoPosition(position, chunkPosition, out int2 newPos);
            GenerationSystem.TryGetChunk(newChunkPos, out ChunkDataAspect chunkData);
            return chunkData.GetCell(newPos, newChunkPos);
        }

        public static int2 GetChunkAndCellPositionFromPseudoPosition(int2 position, int2 chunkPosition, out int2 cellPosition)
        {
            int2 chunkOffset = new int2(Mathf.FloorToInt((float)position.x / ChunkSize),
                Mathf.FloorToInt((float)position.y / ChunkSize));
            cellPosition = position - chunkOffset * ChunkSize;
            return chunkPosition + chunkOffset;
        }

        public bool TryToPlaceBuilding(int buildingID, int2 cellPosition, FacingDirection facingDirection)
        {
            PlacedBuildingData placedBuildingData = new PlacedBuildingData()
            {
                directionID = (byte)facingDirection,
                buildingDataID = buildingID,
                origin = cellPosition,
            };

            var offsets = ResourcesUtility.GetGridPositionList(placedBuildingData);

            foreach (int2 offset in offsets)
            {
                int2 position = offset + cellPosition;

                if (GetCell(position, ChunksPosition).IsOccupied) return false;
            }

            Entity entity = PlacingSystem.CreateBuildingEntity(
                _chunkData.ValueRO.CellObjects[GetAryIndex(cellPosition)].WorldPosition,
                buildingID, facingDirection, placedBuildingData);
            
            BuildingAspect myBuildingAspect = GenerationSystem._entityManager.GetAspect<BuildingAspect>(entity);
            
            foreach (int2 posOffset in offsets)
            {
                int2 position = posOffset + cellPosition;
                if (IsValidPositionInChunk(position)) BlockCell(position, entity);
                else
                {
                    if (GenerationSystem.TryGetChunk(
                            GetChunkAndCellPositionFromPseudoPosition(position, ChunksPosition, out int2 cellPos)
                            , out ChunkDataAspect chunk)) ;
                    chunk.BlockCell(cellPos, entity);
                }
            }



            if (ResourcesUtility.GetBuildingData(buildingID, out BuildingLookUpData data))
            {
                List<BuildingAspect> aspects = new List<BuildingAspect>();
                IEnumerable<int2> offsetsData = data.GetInputOffsets(facingDirection).Union(data.GetOutputOffsets(facingDirection));
                foreach (int2 inputDirection in offsetsData)
                {
                    CellObject cell = GetCell(cellPosition + inputDirection, ChunksPosition);
                    if (!cell.IsOccupied) continue;

                    BuildingAspect otherBuildingAspect =
                        GenerationSystem._entityManager.GetAspect<BuildingAspect>(cell.Building);

                    if (aspects.Contains(otherBuildingAspect)) continue;
                    myBuildingAspect.TryToConnect(otherBuildingAspect);
                    aspects.Add(otherBuildingAspect);
                }
            }

            return true;
        }

        public bool BlockCell(int2 cellPosition, Entity entity)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;
            ref var cellObjs = ref _chunkData.ValueRW.CellObjects;
            int index = GetAryIndex(cellPosition);
            var ob = cellObjs[index];
            ob.PlaceBuilding(entity);
            cellObjs[index] = ob;
            return true;
        }
        
        public bool FreeCell(int2 cellPosition)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;
            ref var cellObjs = ref _chunkData.ValueRW.CellObjects;
            int index = GetAryIndex(cellPosition);
            CellObject ob = cellObjs[index];
            ob.DeleteBuilding();
            cellObjs[index] = ob;
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
            chunkPosition = GetChunkPositionFromWorldPosition(worldPosition);

            float2 cellFloatPos = worldPosition.xy - chunkPosition * ChunkUnitSize;
            
            int2 cellPos =new int2(Mathf.FloorToInt(cellFloatPos.x/CellSize) + HalfChunkSize,
                                         Mathf.FloorToInt(cellFloatPos.y/CellSize)+HalfChunkSize);
            return cellPos;
        }

        public static int2 GetChunkPositionFromWorldPosition(float3 worldPosition)
        {
            return new int2(Mathf.RoundToInt(worldPosition.x / ChunkUnitSize),
                Mathf.RoundToInt(worldPosition.y / ChunkUnitSize));
        }

        public static bool IsValidPositionInChunk(int2 position)
        {
            return position.x >= 0 && position.x < ChunkSize &&
                   position.y >= 0 && position.y < ChunkSize;
        }

        public bool TryToDeleteBuilding(int2 cellPosition)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;

            Entity entity = GetCell(cellPosition, ChunksPosition).Building;
            if (entity == default) return false;

            var offsets = ResourcesUtility.GetGridPositionList(
                GenerationSystem._entityManager.GetComponentData<BuildingDataComponent>(entity).BuildingData);

            foreach (int2 posOffset in offsets)
            {
                int2 position = posOffset + cellPosition;
                if (IsValidPositionInChunk(position)) FreeCell(cellPosition);
                else
                {
                    if (GenerationSystem.TryGetChunk(
                            GetChunkAndCellPositionFromPseudoPosition(position, ChunksPosition, out int2 cellPos)
                            , out ChunkDataAspect chunk))
                        chunk.FreeCell(cellPos);
                }
            }

            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);
            ecb.DestroyEntity(entity);
            return true;
        }
    }
}
