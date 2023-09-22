using System;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    public enum PossibleBuildings : byte
    {
        Extractor,
        Conveyor,
        Combiner,
        TrashCan,
        Separator,
    }
    
    public abstract class PlacedBuildingEntity
    {
        /// <summary>
        /// Creates a specified Building Entity with the input parameters  
        /// </summary>
        /// <param name="gridObject">corresponding gridObject the Building will be placed on</param>
        /// <param name="worldPosition">3D Position the Entity will be set</param>
        /// <param name="origin">Coordinates of the cell in the Chunk that the building is placed on</param>
        /// <param name="facingDirection">The facing direction of the building</param>
        /// <param name="buildingData">The typeData of the building</param>
        /// <returns>Reference to the newly created PlacedBuildingEntity</returns>
        public  static PlacedBuildingEntity CreateBuilding<T>(GridObject gridObject, Vector3 worldPosition,
            Vector2Int origin, FacingDirection facingDirection, PossibleBuildings buildingData) where T : PlacedBuildingEntity,new()
        {
            PlacedBuildingEntity placedBuilding = new T()
            {
                MyPlacedBuildingData = new PlacedBuildingData()
                {
                    origin = origin,
                    buildingDataID = (int)buildingData,
                    directionID = (int)facingDirection,
                },
                MyGridObject = gridObject
            };
            placedBuilding.BuildingEntity =
                BuildingGridEntityUtilities.CreateBuildingEntity(worldPosition, placedBuilding.MyPlacedBuildingData);
            placedBuilding.SetUpSlots(facingDirection);
            placedBuilding.OnCreate();
            return placedBuilding;
        }
        
        protected static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        public Entity BuildingEntity { get; private set; }
        public PlacedBuildingData MyPlacedBuildingData { get; private set; }
        public GridObject MyGridObject { get; private set; }

        protected SlotValidationHandler mySlotValidationHandler;

        protected virtual void OnCreate()
        {
            
        }

        /// <summary>
        /// Give back a list of positions, that this building occupies
        /// </summary>
        /// <returns>Ary of Vector2Int pseudo positions, not all maybe in the same Chunk</returns>
        public Vector2Int[] GetGridPositionList()
        {
            return ResourcesUtility.GetBuildingDataBase(MyPlacedBuildingData.buildingDataID)
                .GetGridPositionList(MyPlacedBuildingData);
        }

        public virtual void Destroy()
        {
            _entityManager.DestroyEntity(BuildingEntity);
        }

        protected abstract void SetUpSlots(FacingDirection facingDirection);

        protected virtual void CheckForSlotToPullForm()
        {
            DynamicBuffer<InputDataComponent> buffer = _entityManager.GetBuffer<InputDataComponent>(BuildingEntity);
            
            for (int i = 0; i < mySlotValidationHandler.ValidInputPositions.Length; i++)
            {
                if(buffer.Length -1 < i) break;
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidInputPositions[i],
                        MyGridObject.Chunk, out PlacedBuildingEntity building))
                {
                    InputDataComponent inputDataComponent = buffer[i];
                    IEntityOutput entityInput = (IEntityOutput)building;
                    if(entityInput == null) continue;
                    if(!entityInput.GetOutput(this, out Entity entity, out int index)) continue;
                    buffer[i] = new InputDataComponent(inputDataComponent.Position, inputDataComponent.MySlotBehaviour,
                        entity,(byte)index, inputDataComponent.SlotContent);
                }
            }
        }

        protected virtual void CheckForSlotsToPushTo()
        {
            DynamicBuffer<OutputDataComponent> buffer = _entityManager.GetBuffer<OutputDataComponent>(BuildingEntity);
            for (int i = 0; i < mySlotValidationHandler.ValidOutputPositions.Length; i++)
            {
                if(buffer.Length -1 < i) break;
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidOutputPositions[i],
                        MyGridObject.Chunk, out PlacedBuildingEntity building))
                {
                    OutputDataComponent outputDataComponent = buffer[i];
                    IEntityInput entityInput = (IEntityInput) building;
                    if(entityInput == null) continue;
                    if(!entityInput.GetInput(this, out Entity entity, out int index)) continue;
                    buffer[i] = new OutputDataComponent(outputDataComponent.Position, outputDataComponent.MySlotBehaviour,
                        entity,(byte)index, outputDataComponent.SlotContent);
                }
            }
        }
        public override string ToString()
        {
            return ((PossibleBuildings) MyPlacedBuildingData.buildingDataID).ToString();
        }
    }
}
