using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
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
        public  static PlacedBuildingEntity CreateBuilding<T>(CellObject gridObject, Vector3 worldPosition,
            int2 origin, FacingDirection facingDirection, PossibleBuildings buildingData) where T : PlacedBuildingEntity,new()
        {
            PlacedBuildingEntity placedBuilding = new T()
            {
                MyPlacedBuildingData = new PlacedBuildingData()
                {
                    origin = origin,
                    buildingDataID = (int)buildingData,
                    directionID = (int)facingDirection,
                },
                MyCellObject = gridObject
            };
            placedBuilding.BuildingEntity =
                BuildingGridEntityUtilities.CreateBuildingEntity(worldPosition, placedBuilding.MyPlacedBuildingData);
            placedBuilding.SetUpSlots(facingDirection);
            placedBuilding.OnCreate();
            return placedBuilding;
        }
        
        protected static EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        public Entity BuildingEntity { get; private set; }
        public PlacedBuildingData MyPlacedBuildingData { get; private set; }
        public CellObject MyCellObject { get; private set; }

        protected SlotValidationHandler mySlotValidationHandler;

        protected virtual void OnCreate()
        {
            
        }

        /// <summary>
        /// Give back a list of positions, that this building occupies
        /// </summary>
        /// <returns>Ary of Vector2Int pseudo positions, not all maybe in the same Chunk</returns>
        public int2[] GetGridPositionList()
        {
            return ResourcesUtility.GetGridPositionList(MyPlacedBuildingData);
        }

        public virtual void Destroy()
        {
            entityManager.DestroyEntity(BuildingEntity);
        }

        protected abstract void SetUpSlots(FacingDirection facingDirection);

        protected abstract void CheckForSlotToPullForm();

        protected abstract void CheckForSlotsToPushTo();
        
        public override string ToString()
        {
            return ((PossibleBuildings) MyPlacedBuildingData.buildingDataID).ToString();
        }
    }
}
