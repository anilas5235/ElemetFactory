using System;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    public class PlacedBuildingEntity
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
        public static PlacedBuildingEntity CreateBuilding(GridObject gridObject, Vector3 worldPosition,
            Vector2Int origin, FacingDirection facingDirection, PossibleBuildings buildingData)
        {
            PlacedBuildingEntity placedBuilding = new PlacedBuildingEntity
            {
                MyPlacedBuildingData = new PlacedBuildingData()
                {
                    origin = origin,
                    buildingDataID = (int)buildingData,
                    directionID = (int)facingDirection,
                },
                MyGridObject = gridObject
            };
            placedBuilding.SetUpSlots(facingDirection);

            placedBuilding.StartWorking();
            return placedBuilding;
        }
        
        protected static EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        protected static int NumberOfBuildings;
        public Entity BuildingEntity { get; private set; }
        public PlacedBuildingData MyPlacedBuildingData { get; private set; }
        public GridObject MyGridObject { get; private set; }

        protected SlotValidationHandler mySlotValidationHandler;


        /// <summary>
        /// Give back a list of positions, that this building occupies
        /// </summary>
        /// <returns>Ary of Vector2Int pseudo positions, not all maybe in the same Chunk</returns>
        public Vector2Int[] GetGridPositionList()
        {
            return BuildingGridResources.GetBuildingDataBase(MyPlacedBuildingData.buildingDataID)
                .GetGridPositionList(MyPlacedBuildingData);
        }

        public virtual void Destroy()
        {
            EntityManager.DestroyEntity(BuildingEntity);
        }

        protected virtual void StartWorking()
        {
        }

        protected virtual void SetUpSlots(FacingDirection facingDirection)
        {
        }

        public virtual void CheckForSlotToPullForm()
        {
            DynamicBuffer<InputDataComponent> buffer = EntityManager.GetBuffer<InputDataComponent>(BuildingEntity);
            
            for (int i = 0; i < mySlotValidationHandler.ValidInputPositions.Length; i++)
            {
                if(buffer.Length -1 < i) break;
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidInputPositions[i],
                        MyGridObject.Chunk, out PlacedBuilding building))
                {
                    InputDataComponent inputDataComponent = buffer[i];
                    IEntityOutput entityInput = building.GetComponent<IEntityOutput>();
                    if(entityInput == null) continue;
                    if(!entityInput.GetOutput(this, out Entity entity, out int index)) continue;
                    buffer[i] = new InputDataComponent(inputDataComponent.Position, inputDataComponent.MySlotBehaviour,
                        entity,index, inputDataComponent.SlotContent);
                }
            }
        }

        public virtual void CheckForSlotsToPushTo()
        {
            DynamicBuffer<OutputDataComponent> buffer = EntityManager.GetBuffer<OutputDataComponent>(BuildingEntity);
            for (int i = 0; i < mySlotValidationHandler.ValidOutputPositions.Length; i++)
            {
                if(buffer.Length -1 < i) break;
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidOutputPositions[i],
                        MyGridObject.Chunk, out PlacedBuilding building))
                {
                    OutputDataComponent outputDataComponent = buffer[i];
                    IEntityInput entityInput = building.GetComponent<IEntityInput>();
                    if(entityInput == null) continue;
                    if(!entityInput.GetInput(this, out Entity entity, out int index)) continue;
                    buffer[i] = new OutputDataComponent(outputDataComponent.Position, outputDataComponent.MySlotBehaviour,
                        entity,index, outputDataComponent.SlotContent);
                }
            }
        }

        public override string ToString()
        {
            return ((PossibleBuildings) MyPlacedBuildingData.buildingDataID).ToString();
        }
    }
}
