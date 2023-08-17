using System;
using System.Collections;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuilding
    {
        private static Action ConveyorTick;
        private static float itemsPerSecond = 1;
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

        [SerializeField]private Slot flowSlot1, flowSlot2,sourceSlot;

        private SlotValidationHandler mySlotValidationHandler;

        protected override void StartWorking()
        {
            ConveyorTick += Tick;
            runningTickClock ??= StartCoroutine(TickClock());

            CheckForInputs();
            CheckForOutputs();
        }

        public override Slot GetInputSlot(GridObject callerPosition)
        {
            return mySlotValidationHandler.ValidateInputSlotRequest(MyGridObject.Position,callerPosition.Position) ? flowSlot1 : null;
        }

        public override Slot GetOutputSlot(GridObject callerPosition)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(MyGridObject.Position,callerPosition.Position) ? flowSlot2 : null;
        }

        protected override void SetUpSlots(BuildingScriptableData.Directions direction)
        {
            SlotValidationHandlers ??= new[]
            {
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/1x1StandartUp"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/1x1StandartRight"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/1x1StandartDown"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/1x1StandartLeft"),
            };

            mySlotValidationHandler = SlotValidationHandlers[(int)direction];
        }

        public override void CheckForInputs()
        {
            foreach (Vector2Int validInputPosition in mySlotValidationHandler.ValidInputPositions)
            {
                GridObject cell = MyChunk.ChunkBuildingGrid.GetCellData(validInputPosition + MyGridObject.Position);
                if(cell == null) return;
                PlacedBuilding cellBuild = cell.Building;
                if (!cellBuild) return;

                Slot next = cellBuild.GetOutputSlot(MyGridObject);
                if (next != null) sourceSlot = next;
            }
        }

        public override void CheckForOutputs()
        {
            foreach (Vector2Int validOutputPosition in mySlotValidationHandler.ValidOutputPositions)
            {
                GridObject cell = MyChunk.ChunkBuildingGrid.GetCellData(validOutputPosition + MyGridObject.Position);
                PlacedBuilding cellBuild = cell.Building;
                if (!cellBuild) return;
                cellBuild.CheckForInputs();
            }
        }

        private void Tick()
        {
            if (flowSlot1.IsOccupied && !flowSlot2.IsOccupied)
                flowSlot2.PutIntoSlot(flowSlot1.ExtractFromSlot());

            if (sourceSlot is { IsOccupied: true } && !flowSlot1.IsOccupied)
                flowSlot1.PutIntoSlot(sourceSlot.ExtractFromSlot());
        }

        public override void Destroy()
        {
            ConveyorTick -= Tick;
            base.Destroy();
        }
    }
}
