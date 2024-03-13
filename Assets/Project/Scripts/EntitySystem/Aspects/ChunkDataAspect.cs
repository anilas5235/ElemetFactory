using System;
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
        public readonly RefRW<ChunkDataComponent> chunkData;

        private readonly DynamicBuffer<CellObject> _cellObjects;

        public readonly DynamicBuffer<EntityRefBufferElement> buildings;
        private static int ChunkSize => ChunkDataComponent.ChunkSize;
        private static int HalfChunkSize => ChunkDataComponent.HalfChunkSize;
        private static int CellSize => GenerationSystem.WorldScale;
        private static int ChunkUnitSize => ChunkDataComponent.ChunkUnitSize;
        public int2 ChunksPosition => chunkData.ValueRO.ChunkPosition;
        public float3 WorldPosition => chunkData.ValueRO.WorldPosition;

        public bool InView
        {
            get => chunkData.ValueRO.InView;
            set => chunkData.ValueRW.InView = value;
        }

        public CellObject GetCell(int2 position,int2 chunkPosition)
        {
           return IsValidPositionInChunk(position) ?
               _cellObjects[GetAryIndex(position)]:
               GetCellFormPseudoPosition(position,chunkPosition);
        }

        private static CellObject GetCellFormPseudoPosition(int2 position, int2 chunkPosition)
        {
            var newChunkPos = GetChunkAndCellPositionFromPseudoPosition(position, chunkPosition, out var newPos);
            GenerationSystem.Instance.TryGetChunk(newChunkPos, out var tempChunkData);
            return tempChunkData.GetCell(newPos, newChunkPos);
        }

        private static int2 GetChunkAndCellPositionFromPseudoPosition(int2 position, int2 chunkPosition, out int2 cellPosition)
        {
            var chunkOffset = new int2(Mathf.FloorToInt((float)position.x / ChunkSize),
                Mathf.FloorToInt((float)position.y / ChunkSize));
            cellPosition = position - chunkOffset * ChunkSize;
            return chunkPosition + chunkOffset;
        }

        public bool TryToPlaceBuilding(int buildingID, int2 cellPosition, FacingDirection facingDirection)
        {
            var placedBuildingData = new PlacedBuildingData()
            {
                directionID = (byte)facingDirection,
                buildingDataID = buildingID,
                origin = cellPosition,
            };

            var offsets = ResourcesUtility.GetGridPositionList(placedBuildingData);

            foreach (var offset in offsets)
            {
                var position = offset + cellPosition;

                if (GetCell(position, ChunksPosition).IsOccupied) return false;
            }

            var originCell =  _cellObjects[GetAryIndex(cellPosition)];
            var entity = PlacingSystem.CreateBuildingEntity(originCell, placedBuildingData);
            
            var myBuildingAspect = GenerationSystem.entityManager.GetAspect<BuildingAspect>(entity);
            
            foreach (var posOffset in offsets)
            {
                var position = posOffset + cellPosition;
                if (IsValidPositionInChunk(position)) BlockCell(position, entity);
                else
                {
                    if (GenerationSystem.Instance.TryGetChunk(
                            GetChunkAndCellPositionFromPseudoPosition(position, ChunksPosition, out var cellPos)
                            , out var chunk))
                    {
                        chunk.BlockCell(cellPos, entity);
                    }
                }
            }

            if (ResourcesUtility.GetBuildingData(buildingID, out var data))
            {
                //set port Positions
                var inputOffsets = data.GetInputOffsets(facingDirection);
                float2 relativeOffset;
                switch (facingDirection)
                {
                    case FacingDirection.Up:
                    case FacingDirection.Down:
                        relativeOffset = new float2(1f,buildingID == 1 ? .25f : .5f);
                        break;
                    case FacingDirection.Right:
                    case FacingDirection.Left:
                        relativeOffset = new float2(buildingID == 1 ? .25f : .5f,1f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(facingDirection), facingDirection, null);
                }
                
                const float zOffset = 0;
                for (var i = 0; i < myBuildingAspect.inputSlots.Length; i++)
                {
                    var portOffset = inputOffsets[i] * relativeOffset * GenerationSystem.WorldScale;
                    myBuildingAspect.inputSlots.ElementAt(i).SetWorldPosition(originCell.WorldPosition + new float3(portOffset,zOffset));
                    myBuildingAspect.inputSlots.ElementAt(i).SetOwnIndex(i);
                    //myBuildingAspect.inputSlots.ElementAt(i).SetFacingDirection();
                }

                var outputOffsets = data.GetOutputOffsets(facingDirection);
                for (var i = 0; i < myBuildingAspect.outputSlots.Length; i++)
                {
                    var portOffset = outputOffsets[i] * relativeOffset * GenerationSystem.WorldScale;
                    myBuildingAspect.outputSlots.ElementAt(i).SetWorldPosition(originCell.WorldPosition + new float3(portOffset,zOffset)); 
                    myBuildingAspect.outputSlots.ElementAt(i).SetOwnIndex(i);
                    //myBuildingAspect.outputSlots.ElementAt(i).SetFacingDirection();
                }
                
                //connect with neighbours 
                var aspects = new List<BuildingAspect>();
                var offsetsData = inputOffsets.Union(outputOffsets);
                foreach (int2 direction in offsetsData)
                {
                    var cell = GetCell(cellPosition + direction, ChunksPosition);
                    if (!cell.IsOccupied) continue;

                    var otherBuildingAspect = GenerationSystem.entityManager.GetAspect<BuildingAspect>(cell.Building);

                    if (aspects.Contains(otherBuildingAspect)) continue;
                    myBuildingAspect.TryToConnectBuildings(otherBuildingAspect, direction);
                    aspects.Add(otherBuildingAspect);
                }
                
                if(buildingID ==1) HandelConveyors(GenerationSystem.entityManager.GetAspect<ConveyorAspect>(entity));
            }

            buildings.Add(new EntityRefBufferElement()
            {
                Entity = entity,
            });
            
            return true;
        }
        
        private static void HandelConveyors(ConveyorAspect conveyorAspect)
        {
            var inputSlots = conveyorAspect.inputSlots;
            var outputSlots = conveyorAspect.outputSlots;
            var entity = conveyorAspect.entity;
            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer();

            Entity head = default;

            if (inputSlots[0].IsConnected && GenerationSystem.entityManager
                    .GetComponentData<BuildingDataComponent>(inputSlots[0].ConnectedEntity)
                    .BuildingData.buildingDataID == 1)
            {
                var sourceBuilding =
                    GenerationSystem.entityManager.GetAspect<ConveyorAspect>(inputSlots[0].ConnectedEntity);
                head = sourceBuilding.conveyorDataComponent.ValueRO.head;

                var buffer = GenerationSystem.entityManager.GetBuffer<EntityRefBufferElement>(head);
                buffer.Add(new EntityRefBufferElement()
                {
                    Entity = entity,
                });

                ecb.SetComponent(entity, new ConveyorDataComponent()
                {
                    head = head,
                });
            }

            if (outputSlots[0].IsConnected && GenerationSystem.entityManager
                    .GetComponentData<BuildingDataComponent>(outputSlots[0].ConnectedEntity)
                    .BuildingData.buildingDataID == 1)
            {
                var destinationBuilding =
                    GenerationSystem.entityManager.GetAspect<ConveyorAspect>(outputSlots[0].ConnectedEntity);

                if (head != default)
                {
                    //Connect two chains
                    var bufferA = GenerationSystem.entityManager.GetBuffer<EntityRefBufferElement>(head);
                    var head2 = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var bufferB = GenerationSystem.entityManager.GetBuffer<EntityRefBufferElement>(head2);
                    head = CreateChainHead(ecb, bufferA, bufferB,
                        out DynamicBuffer<EntityRefBufferElement> newChain);

                    ecb.DestroyEntity(head);
                    ecb.DestroyEntity(head2);

                    foreach (var chainLink in newChain)
                    {

                        ecb.SetComponent(chainLink.Entity,
                            new ConveyorDataComponent()
                            {
                                head = head,
                            });
                    }
                }
                else
                {
                    head = destinationBuilding.conveyorDataComponent.ValueRO.head;
                    var buffer = GenerationSystem.entityManager.GetBuffer<EntityRefBufferElement>(head);
                    buffer.Insert(0, new EntityRefBufferElement()
                    {
                        Entity = entity,
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
                buffer.Add(new EntityRefBufferElement()
                {
                    Entity = entity,
                });
                ecb.SetComponent(entity, new ConveyorDataComponent()
                {
                    head = head,
                });
            }
        }

        private static Entity CreateChainHead(EntityCommandBuffer ecb,
            DynamicBuffer<EntityRefBufferElement>chainA,DynamicBuffer<EntityRefBufferElement> chainB,
            out DynamicBuffer<EntityRefBufferElement> buffer)
        {
            var entity = CreateChainHead(ecb, out buffer);
            buffer.AddRange(chainA.AsNativeArray());
            buffer.AddRange(chainB.AsNativeArray());
            return entity;
        }
        public static Entity CreateChainHead(EntityCommandBuffer ecb,
            out DynamicBuffer<EntityRefBufferElement> buffer)
        {
            var entity = ecb.CreateEntity();
            buffer = ecb.AddBuffer<EntityRefBufferElement>(entity);
            ecb.AddComponent(entity,new ConveyorChainDataComponent());
            ecb.SetName(entity,"ChainHead");
            return entity;
        }

        public bool BlockCell(int2 cellPosition, Entity entity)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;
            var index = GetAryIndex(cellPosition);
            _cellObjects.ElementAt(index).PlaceBuilding(entity);
            return true;
        }
        
        public bool FreeCell(int2 cellPosition)
        {
            if (!IsValidPositionInChunk(cellPosition)) return false;
            var index = GetAryIndex(cellPosition);
            _cellObjects.ElementAt(index).DeleteBuilding();
            return true;
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
            
            var ecb = PlacingSystem.beginSimulationEntityCommandBuffer.CreateCommandBuffer();

            Entity entity = GetCell(cellPosition, ChunksPosition).Building;
            if (entity == default|| entity == Entity.Null) return false;

            var offsets = ResourcesUtility.GetGridPositionList(
                GenerationSystem.entityManager.GetComponentData<BuildingDataComponent>(entity).BuildingData);
            
            //free previously blocked cells
            foreach (int2 posOffset in offsets)
            {
                int2 position = posOffset + cellPosition;
                if (IsValidPositionInChunk(position)) FreeCell(position);
                else
                {
                    if (GenerationSystem.Instance.TryGetChunk(
                            GetChunkAndCellPositionFromPseudoPosition(position, ChunksPosition, out var cellPos)
                            , out var chunk))
                        chunk.FreeCell(cellPos);
                }
            }

            //disconnect Building
            var buildingAspect = GenerationSystem.entityManager.GetAspect<BuildingAspect>(entity);

            for (var i = 0; i < buildingAspect.inputSlots.Length; i++)
            {
                if (buildingAspect.inputSlots[i].IsOccupied)
                {
                    ecb.DestroyEntity(buildingAspect.inputSlots[i].SlotContent);
                }
                if(!buildingAspect.inputSlots[i].IsConnected)continue;

                var otherBuildingAspect =
                    GenerationSystem.entityManager.GetAspect<BuildingAspect>(buildingAspect.inputSlots[i].ConnectedEntity);
                var index = buildingAspect.inputSlots[i].ConnectedIndex;
                otherBuildingAspect.outputSlots.ElementAt(index).SetConnectedEntity();
                otherBuildingAspect.outputSlots.ElementAt(index).SetConnectedIndex();

                buildingAspect.inputSlots.ElementAt(i).SetConnectedEntity();
                buildingAspect.inputSlots.ElementAt(i).SetConnectedIndex();
            }

            for (var i = 0; i < buildingAspect.outputSlots.Length; i++)
            {
                if (buildingAspect.outputSlots[i].IsOccupied)
                {
                    ecb.DestroyEntity(buildingAspect.outputSlots[i].SlotContent);
                }
                if(!buildingAspect.outputSlots[i].IsConnected)continue;
                var otherBuildingAspect =
                    GenerationSystem.entityManager.GetAspect<BuildingAspect>(buildingAspect.outputSlots[i]
                        .ConnectedEntity);
                var index = buildingAspect.outputSlots[i].ConnectedIndex;
                otherBuildingAspect.inputSlots.ElementAt(index).SetConnectedEntity();
                otherBuildingAspect.inputSlots.ElementAt(index).SetConnectedIndex();

                buildingAspect.outputSlots.ElementAt(i).SetConnectedEntity();
                buildingAspect.outputSlots.ElementAt(i).SetConnectedIndex();
            }

            //destroy Building entity

            if (buildingAspect.MyBuildingData.buildingDataID == 1)
            {
                var headAspect = GenerationSystem.entityManager.GetAspect<ConveyorChainHeadAspect>(GenerationSystem.entityManager
                    .GetComponentData<ConveyorDataComponent>(buildingAspect.entity).head);
                headAspect.RemoveConveyor(buildingAspect.entity);
            }

            for (var index = 0; index < buildings.Length; index++)
            {
                if (buildings[index].Entity != entity) continue;
                buildings.RemoveAt(index);
                break;
            }

            ecb.DestroyEntity(entity);
            return true;
        }
    }
}
