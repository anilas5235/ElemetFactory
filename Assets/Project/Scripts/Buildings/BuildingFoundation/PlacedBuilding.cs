using System;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
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
        public static PlacedBuilding CreateBuilding(GridChunk chunk, GridObject gridObject, Vector3 localPosition,
            Vector2Int origin,
            FacingDirection facingDirection, PossibleBuildings buildingData,
            Transform transformParent, float cellSize)
        {
            Transform buildingTransform = Instantiate(BuildingGridResources.GetBuildingDataBase(buildingData).prefab,
                localPosition,
                Quaternion.Euler(0, 0, PlacedBuildingUtility.GetRotation(facingDirection))).transform;
            buildingTransform.name = $"{buildingData}({numberOfBuildings++})";

            PlacedBuilding placedBuilding = buildingTransform.GetComponent<PlacedBuilding>();

            placedBuilding.MyPlacedBuildingData = new PlacedBuildingData()
            {
                origin = origin,
                buildingDataID = (int)buildingData,
                directionID = (int)facingDirection,
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

        protected static int numberOfBuildings;

        public PlacedBuildingData MyPlacedBuildingData { get; private set; }
        public GridChunk MyChunk { get; private set; }

        public GridObject MyGridObject { get; private set; }

        [SerializeField] private GameObject visualParent;

        [SerializeField] protected Slot[] inputs;

        [SerializeField] protected Slot[] outputs;

        [SerializeField] protected Slot[] slotsToPullFrom;
        [SerializeField] protected Slot[] slotsToPushTo;

        [SerializeField] protected bool subedToConveyorTick = false;

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
            if (subedToConveyorTick)
            {
                foreach (Slot slot in slotsToPullFrom)
                {
                    if (!slot) continue;
                    if (slot.transform.parent.TryGetComponent(out ConveyorBelt belt)) belt.SubToConveyorTickEvent();
                }
            }

            Destroy(gameObject);
        }

        protected virtual void StartWorking()
        {
            slotsToPullFrom = new Slot[inputs.Length];
            slotsToPushTo = new Slot[outputs.Length];
        }

        protected virtual void SetUpSlots(FacingDirection facingDirection)
        {
        }

        public virtual void CheckForSlotToPullForm()
        {
            bool foundSlot = false;
            for (int i = 0; i < mySlotValidationHandler.ValidInputPositions.Length; i++)
            {
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidInputPositions[i],
                        MyChunk, out PlacedBuilding building))
                {
                    Slot slot = building.GetComponent<IHaveOutput>()?.GetOutputSlot(this, inputs[i]);
                    if (!slot) continue;
                    slotsToPullFrom[i] = slot;
                    foundSlot = true;
                }
            }

            if (!foundSlot) return;
        }

        public virtual void CheckForSlotsToPushTo()
        {
            for (int i = 0; i < mySlotValidationHandler.ValidOutputPositions.Length; i++)
            {
                if (PlacedBuildingUtility.CheckForBuilding(
                        MyPlacedBuildingData.origin + mySlotValidationHandler.ValidOutputPositions[i],
                        MyChunk, out PlacedBuilding building))
                {
                    slotsToPushTo[i] = building.GetComponent<IHaveInput>()?.GetInputSlot(this, outputs[i]);
                }
            }
        }

        public override string ToString()
        {
            return MyPlacedBuildingData.buildingDataID.ToString();
        }

        protected void PullItem()
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                if (!slotsToPullFrom[i]) continue;
                if (!inputs[i].IsOccupied && slotsToPullFrom[i].IsOccupied)
                    inputs[i].PutIntoSlot(slotsToPullFrom[i].ExtractFromSlot());
            }
        }
    }

    public struct PlacedBuildingsDataComponent : IComponentData
    {
        public static int NumberOfBuildings;

        public PlacedBuildingData MyPlacedBuildingData { get; }
        public GridChunk MyChunk { get; }

        public GridObject MyGridObject { get; }

        public bool SubedToConveyorTick;

        public SlotValidationHandler MySlotValidationHandler;

        public PlacedBuildingsDataComponent(GridChunk myChunk, GridObject myGridObject,
            PlacedBuildingData myPlacedBuildingData)
        {
            MyChunk = myChunk;
            MyGridObject = myGridObject;
            MyPlacedBuildingData = myPlacedBuildingData;
            SubedToConveyorTick = false;
            MySlotValidationHandler = null;
        }
    }
}
