using System.Collections.Generic;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public abstract class PlacedBuilding : MonoBehaviour
    {
        /// <summary>
        /// Creates a specified Building with the input parameters  
        /// </summary>
        /// /// <param name="chunk">the Chunk the building is placed in</param>
        /// <param name="localPosition">3D Position in relation to the Chunks Center</param>
        /// <param name="origin">Coordinates of the cell in the Chunk that the building is placed on</param>
        /// <param name="facingDirection">The facing direction of the building</param>
        /// <param name="buildingData">The type Data of the building</param>
        /// <param name="transformParent">Reference to parent in hierarchy</param>
        /// <param name="cellSize">Size of the cells in the Grid for the scaling of the building</param>
        /// <returns>Reference to the newly created PlacedBuilding</returns>
        public static PlacedBuilding CreateBuilding(GridChunk chunk,GridObject gridObject,Vector3 localPosition, Vector2Int origin,
            FacingDirection facingDirection, PossibleBuildings buildingData,
            Transform transformParent, float cellSize)
        {
            Transform buildingTransform = Instantiate(BuildingGridResources.GetBuildingDataBase(buildingData).prefab,
                localPosition,
                Quaternion.Euler(0, 0, PlacedBuildingUtility.GetRotation(facingDirection))).transform;

            PlacedBuilding placedBuilding = buildingTransform.GetComponent<PlacedBuilding>();

            placedBuilding.MyPlacedBuildingData = new PlacedBuildingData()
            {
                origin = origin,
                buildingDataID =(int) buildingData,
                directionID =(int) facingDirection,
            };

            placedBuilding.MyChunk = chunk;
            placedBuilding.MyGridObject = gridObject;
            placedBuilding.visualParent = buildingTransform.GetChild(0).gameObject;
            placedBuilding.SetUpSlots(facingDirection);

            buildingTransform.SetParent(transformParent);
            buildingTransform.localScale = new Vector3(cellSize, cellSize);
            buildingTransform.localPosition = localPosition;

            placedBuilding.StartWorking();
            return placedBuilding;
        }

        public PlacedBuildingData MyPlacedBuildingData { get; private set; }
        protected GridChunk MyChunk { get; private set; }
        
        protected GridObject MyGridObject { get; private set; }

        [SerializeField] private GameObject visualParent;


        /// <summary>
        /// Give back a list of positions, that this building occupies
        /// </summary>
        /// <returns>Ary of Vector2Int pseudo positions, not all maybe in the same Chunk</returns>
        public Vector2Int[] GetGridPositionList()
        {
            return BuildingGridResources.GetBuildingDataBase( MyPlacedBuildingData.buildingDataID)
                .GetGridPositionList(MyPlacedBuildingData);
        }

        /// <summary>
        /// Deactivates the visuals of the building
        /// </summary>
        public virtual void Load()
        {
            visualParent.SetActive(true);
        }

        /// <summary>
        /// Activates the visuals of the building
        /// </summary>
        public virtual void UnLoad()
        {
            visualParent.SetActive(false);
        }

        public virtual void Destroy()
        {
            Destroy(gameObject);
        }

        protected abstract void StartWorking();

        protected abstract void SetUpSlots(FacingDirection facingDirection);

        public abstract void CheckForSlotToPullForm();

        public abstract void CheckForSlotsToPushTo();

        public override string ToString()
        {
            return MyPlacedBuildingData.buildingDataID.ToString();
        }
    }
}
