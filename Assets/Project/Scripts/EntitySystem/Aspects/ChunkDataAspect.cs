using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
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

            var originCell = _chunkData.ValueRO.CellObjects[GetAryIndex(cellPosition)];
            Entity entity = PlacingSystem.CreateBuildingEntity(originCell, placedBuildingData);
            
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
                //set port Positions
                int2[] inputOffsets = data.GetInputOffsets(facingDirection);
                float relativeOffset = (buildingID == 1 ? .25f : .5f);
                float zOffset = 0;
                for (int i = 0; i < myBuildingAspect.inputSlots.Length; i++)
                {
                    float2 portOffset = (float2)inputOffsets[i] * relativeOffset * GenerationSystem.WorldScale;
                    myBuildingAspect.inputSlots.ElementAt(i).Position = originCell.WorldPosition + new float3(portOffset,zOffset);
                }

                int2[] outputOffsets = data.GetOutputOffsets(facingDirection);
                for (int i = 0; i < myBuildingAspect.outputSlots.Length; i++)
                {
                    float2 portOffset = (float2)outputOffsets[i] * relativeOffset * GenerationSystem.WorldScale;
                    myBuildingAspect.outputSlots.ElementAt(i).Position = originCell.WorldPosition + new float3(portOffset,zOffset);
                }
                
                //connect with neighbours 
                List<BuildingAspect> aspects = new List<BuildingAspect>();
                IEnumerable<int2> offsetsData = inputOffsets.Union(outputOffsets);
                foreach (int2 direction in offsetsData)
                {
                    CellObject cell = GetCell(cellPosition + direction, ChunksPosition);
                    if (!cell.IsOccupied) continue;

                    BuildingAspect otherBuildingAspect =
                        GenerationSystem._entityManager.GetAspect<BuildingAspect>(cell.Building);

                    if (aspects.Contains(otherBuildingAspect)) continue;
                    myBuildingAspect.TryToConnectBuildings(otherBuildingAspect);
                    aspects.Add(otherBuildingAspect);
                }
                
                if(buildingID ==1) HandelConveyors(GenerationSystem._entityManager.GetAspect<ConveyorAspect>(entity));
            }

            return true;
        }
        
        private void HandelConveyors(ConveyorAspect conveyorAspect)
        {
            var inputSlots = conveyorAspect.inputSlots;
            var outputSlots = conveyorAspect.outputSlots;
            var entity = conveyorAspect.entity;
            EntityCommandBuffer ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);

            Entity head = default;

            if (inputSlots[0].IsConnected && GenerationSystem._entityManager
                    .GetComponentData<BuildingDataComponent>(inputSlots[0].EntityToPullFrom)
                    .BuildingData.buildingDataID == 1)
            {
                var sourceBuilding =
                    GenerationSystem._entityManager.GetAspect<ConveyorAspect>(inputSlots[0].EntityToPullFrom);
                head = sourceBuilding.conveyorDataComponent.ValueRO.head;

                var buffer = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                buffer.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity,
                });

                ecb.SetComponent(entity, new ConveyorDataComponent()
                {
                    head = head,
                });
            }

            if (outputSlots[0].IsConnected && GenerationSystem._entityManager
                    .GetComponentData<BuildingDataComponent>(outputSlots[0].EntityToPushTo)
                    .BuildingData.buildingDataID == 1)
            {
                var destinationBuilding =
                    GenerationSystem._entityManager.GetAspect<ConveyorAspect>(outputSlots[0].EntityToPushTo);

                if (head != default)
                {
                    //Connect two chains
                    var bufferA = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                    Entity head2 = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var bufferB = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head2);
                    head = CreateChainHead(ecb, bufferA, bufferB,
                        out DynamicBuffer<ConveyorChainDataPoint> newChain);

                    ecb.DestroyEntity(head);
                    ecb.DestroyEntity(head2);

                    foreach (ConveyorChainDataPoint chainLink in newChain)
                    {

                        ecb.SetComponent(chainLink.ConveyorEntity,
                            new ConveyorDataComponent()
                            {
                                head = head,
                            });
                    }
                }
                else
                {
                    head = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var buffer = GenerationSystem._entityManager.GetBuffer<ConveyorChainDataPoint>(head);
                    buffer.Insert(0, new ConveyorChainDataPoint()
                    {
                        ConveyorEntity = entity,
                    });
                    ecb.SetComponent(entity, new ConveyorDataComponent()
                    {
                        head = head,
                    });
                }
            }

            if (head == default)
            {
                head = CreateChainHead(ecb, out var buffer);
                buffer.Add(new ConveyorChainDataPoint()
                {
                    ConveyorEntity = entity,
                });
                ecb.SetComponent(entity, new ConveyorDataComponent()
                {
                    head = head,
                });
            }
        }
        public static Entity CreateChainHead(EntityCommandBuffer ecb,
            DynamicBuffer<ConveyorChainDataPoint>chainA,DynamicBuffer<ConveyorChainDataPoint> chainB,
            out DynamicBuffer<ConveyorChainDataPoint> buffer)
        {
            var entity = CreateChainHead(ecb, out buffer);
            buffer.AddRange(chainA.AsNativeArray());
            buffer.AddRange(chainB.AsNativeArray());
            return entity;
        }
        public static Entity CreateChainHead(EntityCommandBuffer ecb,
            out DynamicBuffer<ConveyorChainDataPoint> buffer)
        {
            var entity = ecb.CreateEntity();
            buffer = ecb.AddBuffer<ConveyorChainDataPoint>(entity);
            ecb.AddComponent(entity,new ConveyorChainDataComponent());
            ecb.SetName(entity,"ChainHead");
            return entity;
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
            chunkPosition = GenerationSystem.GetChunkPosition(worldPosition);

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

        public bool TryToDeleteBuilding(int2 cellPosition)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;

            Entity entity = GetCell(cellPosition, ChunksPosition).Building;
            if (entity == default) return false;

            var offsets = ResourcesUtility.GetGridPositionList(
                GenerationSystem._entityManager.GetComponentData<BuildingDataComponent>(entity).BuildingData);
            
            //free previously blocked cells
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

            //disconnect Building
            var buildingAspect = GenerationSystem._entityManager.GetAspect<BuildingAspect>(entity);

            for (int i = 0; i < buildingAspect.inputSlots.Length; i++)
            {
                if(!buildingAspect.inputSlots[i].IsConnected)continue;

                var otherBuildingAspect =
                    GenerationSystem._entityManager.GetAspect<BuildingAspect>(buildingAspect.inputSlots[i]
                        .EntityToPullFrom);
                int index = buildingAspect.inputSlots[i].outputIndex;
                otherBuildingAspect.outputSlots.ElementAt(index).EntityToPushTo = default;
                otherBuildingAspect.outputSlots.ElementAt(index).InputIndex = 0;

                buildingAspect.inputSlots.ElementAt(i).EntityToPullFrom = default;
                buildingAspect.inputSlots.ElementAt(i).outputIndex =0;
            }

            for (int i = 0; i < buildingAspect.outputSlots.Length; i++)
            {
                if(!buildingAspect.outputSlots[i].IsConnected)continue;
                var otherBuildingAspect =
                    GenerationSystem._entityManager.GetAspect<BuildingAspect>(buildingAspect.outputSlots[i]
                        .EntityToPushTo);
                int index = buildingAspect.outputSlots[i].InputIndex;
                otherBuildingAspect.inputSlots.ElementAt(index).EntityToPullFrom = default;
                otherBuildingAspect.inputSlots.ElementAt(index).outputIndex = default;

                buildingAspect.outputSlots.ElementAt(i).EntityToPushTo = default;
                buildingAspect.outputSlots.ElementAt(i).InputIndex = default;
            }

            //destroy Building entity
            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer(
                World.DefaultGameObjectInjectionWorld.Unmanaged);

            if (buildingAspect.MyBuildingData.buildingDataID == 1)
            {
                var headAspect = GenerationSystem._entityManager.GetAspect<ConveyorChainHeadAspect>(GenerationSystem._entityManager
                    .GetComponentData<ConveyorDataComponent>(buildingAspect.entity).head);
                headAspect.RemoveConveyor(buildingAspect.entity);
            }
            
            
            ecb.DestroyEntity(entity);
            return true;
        }
    }
}
