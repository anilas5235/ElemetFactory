using System;
using System.Collections;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuilding
    {
        private static Action ConveyorTick;
        private static float itemsPerSecond = 1;
        public static float ItemsPerSecond => itemsPerSecond;
        private static Coroutine runningTickClock;
        private static SlotValidationHandler[] SlotValidationHandlers;

        private static IEnumerator TickClock()
        {
            while (true)
            {
                ConveyorTick?.Invoke();
                yield return new WaitForSeconds(1f/itemsPerSecond);
            }
        }

        [SerializeField]private Slot flowSlot1, flowSlot2,slotToPullForm, slotToPushTo;
        protected override void StartWorking()
        {
            ConveyorTick += Tick;
            runningTickClock ??= StartCoroutine(TickClock());

            CheckForInputs();
            CheckForOutputs();
        }

        public override Slot GetInputSlot(GridObject callerPosition, Slot destination)
        {
            if (slotToPullForm != null) return null;
            slotToPullForm = destination;
            return flowSlot1;
        }

        public override Slot GetOutputSlot(GridObject callerPosition, Slot destination)
        {
            if (slotToPushTo!= null) return null;
            slotToPushTo = destination;
            return flowSlot2;
        }

        protected override void SetUpSlots(BuildingScriptableData.Directions direction)
        {
        }

        public override void CheckForInputs()
        {
            BuildingScriptableData.Directions direction = BuildingScriptableData.Directions.Right;
            foreach (Vector2Int validInputPosition in GeneralConstants.NeighbourOffsets2D4)
            {
                direction = BuildingScriptableData.GetNextDirectionClockwise(direction);
                Vector2Int targetPos = validInputPosition + MyGridObject.Position;
                GridObject cell = MyChunk.ChunkBuildingGrid.IsValidPosition(targetPos) ? MyChunk.ChunkBuildingGrid.GetCellData(targetPos) :
                    MyChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, MyChunk);
                PlacedBuilding cellBuild = cell.Building;
                if (!cellBuild) continue;
                if(cellBuild.MyPlacedBuildingData.directionID != (int)direction) continue;
                Slot next = cellBuild.GetOutputSlot(MyGridObject,flowSlot1);
                if (next == null) continue;
                slotToPullForm = next;
                break;
            }
        }

        public override void CheckForOutputs()
        {
            BuildingScriptableData.Directions direction = BuildingScriptableData.Directions.Left;
            foreach (Vector2Int validOutputPosition in GeneralConstants.NeighbourOffsets2D4)
            {
                Vector2Int targetPos = validOutputPosition + MyGridObject.Position;
                var cell = MyChunk.ChunkBuildingGrid.IsValidPosition(targetPos) ? MyChunk.ChunkBuildingGrid.GetCellData(targetPos) :
                    MyChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, MyChunk);
                PlacedBuilding cellBuild = cell.Building;
                if (!cellBuild) continue;
                if(cellBuild.MyPlacedBuildingData.directionID != (int)direction) continue;
                Slot next = cellBuild.GetOutputSlot(MyGridObject,flowSlot2);
                if (next == null) continue;
                slotToPullForm = next;
                break;
            }
        }

        private void Tick()
        {
            if (flowSlot1.IsOccupied && !flowSlot2.IsOccupied)
                flowSlot2.PutIntoSlot(flowSlot1.ExtractFromSlot());

            if (slotToPullForm is { IsOccupied: true } && !flowSlot1.IsOccupied)
                flowSlot1.PutIntoSlot(slotToPullForm.ExtractFromSlot());
        }

        public override void Destroy()
        {
            ConveyorTick -= Tick;
            base.Destroy();
        }
    }
}
