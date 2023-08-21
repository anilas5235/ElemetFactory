using System;
using System.Collections;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
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

            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
        }

        public override Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            if (slotToPullForm != null) return null;
            slotToPullForm = destination;
            return flowSlot1;
        }

        public override Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            if (slotToPushTo!= null) return null;
            slotToPushTo = destination;
            return flowSlot2;
        }

        protected override void SetUpSlots(BuildingScriptableData.FacingDirection facingDirection)
        {
        }

        public override void CheckForSlotToPullForm()
        {
            if (slotToPushTo) return;
            Vector2Int targetPos =
                BuildingScriptableData.FacingDirectionToVector(
                    BuildingScriptableData.GetOppositeDirection(MyPlacedBuildingData.directionID)) +
                MyGridObject.Position;
            GridObject cell = MyChunk.ChunkBuildingGrid.IsValidPosition(targetPos)
                ? MyChunk.ChunkBuildingGrid.GetCellData(targetPos)
                : MyChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, MyChunk);
            PlacedBuilding cellBuild = cell.Building;
            if (!cellBuild) return;
            if (cellBuild.MyPlacedBuildingData.directionID ==
                (int)BuildingScriptableData.GetOppositeDirection(MyPlacedBuildingData.directionID)) return;
            Slot next = cellBuild.GetOutputSlot(MyPlacedBuildingData, flowSlot1);
            if (next == null) return;
            slotToPullForm = next;
        }

        public override void CheckForSlotsToPushTo()
        {
            if(slotToPullForm) return;
            Vector2Int targetPos = BuildingScriptableData.FacingDirectionToVector(MyPlacedBuildingData.directionID)+ MyGridObject.Position;
            var cell = MyChunk.ChunkBuildingGrid.IsValidPosition(targetPos)
                ? MyChunk.ChunkBuildingGrid.GetCellData(targetPos)
                : MyChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, MyChunk);
            PlacedBuilding cellBuild = cell.Building;
            if (!cellBuild) return;
            Slot next = cellBuild.GetInputSlot(MyPlacedBuildingData, flowSlot2);
            if (next == null) return;
            slotToPushTo = next;
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
