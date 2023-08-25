using System;
using System.Collections;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuilding,IHaveOutput,IHaveInput
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

        public Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            if (slotToPullForm != null) return null;
            slotToPullForm = destination;
            return flowSlot1;
        }

        public Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            if (slotToPushTo!= null) return null;
            slotToPushTo = destination;
            return flowSlot2;
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
        }

        public override void CheckForSlotToPullForm()
        {
            if (slotToPullForm) return;
            Vector2Int[] offsets = new Vector2Int[]
            {
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetOppositeDirection(MyPlacedBuildingData.directionID)) ,
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetNextDirectionClockwise(MyPlacedBuildingData.directionID)) ,
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetNextDirectionCounterClockwise(MyPlacedBuildingData.directionID)) ,
            };

            foreach (var offset in offsets)
            {
                if (!PlacedBuildingUtility.CheckForBuilding(offset + MyGridObject.Position, MyChunk,
                        out PlacedBuilding cellBuild)) continue;
                IHaveOutput buildingOut = cellBuild.GetComponent<IHaveOutput>();
                if(buildingOut == null)continue;
                if (cellBuild.MyPlacedBuildingData.directionID ==
                    (int)PlacedBuildingUtility.GetOppositeDirection(MyPlacedBuildingData.directionID)) continue;
                Slot next = buildingOut.GetOutputSlot(MyPlacedBuildingData, flowSlot1);
                if (next == null) continue;
                slotToPullForm = next;
                break;
            }
        }

        public override void CheckForSlotsToPushTo()
        {
            if(slotToPullForm) return;
            Vector2Int targetPos = PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID)+ MyGridObject.Position;
            if(!PlacedBuildingUtility.CheckForBuilding(targetPos,MyChunk,out PlacedBuilding cellBuild)) return;
            IHaveInput buildingIn = cellBuild.GetComponent<IHaveInput>();
            if(buildingIn == null)return;
            Slot next = buildingIn.GetInputSlot(MyPlacedBuildingData, flowSlot2);
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
