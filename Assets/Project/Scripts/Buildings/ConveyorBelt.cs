using System;
using System.Collections;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuilding,IHaveOutput,IHaveInput,IReceiveConveyorChainTickUpdate,IConveyorDestination
    {
        public static Action ConveyorTick;
        private static float itemsPerSecond = 2;
        public static float ItemsPerSecond => itemsPerSecond;
        private static Coroutine runningTickClock;
        private static SlotValidationHandler[] SlotValidationHandlers;
        
        [SerializeField] private bool subedToConveyorTick = false;
        [SerializeField]private Slot slotToPullForm, slotToPushTo;
        
        private static IEnumerator TickClock()
        {
            yield return new WaitForFixedUpdate();
            while (true)
            {
                ConveyorTick?.Invoke();
                yield return new WaitForSeconds(1f/itemsPerSecond);
            }
        }
        protected override void StartWorking()
        {
            runningTickClock ??= StartCoroutine(TickClock());

            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
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
                if (!PlacedBuildingUtility.DoYouPointAtMe(cellBuild.MyPlacedBuildingData.directionID,offset)) continue;
                Slot next = buildingOut.GetOutputSlot(this, inputs[0]);
                if (next == null) continue;
                slotToPullForm = next;
                break;
            }
        }

        public override void CheckForSlotsToPushTo()
        {
            if (!slotToPushTo)
            {
                Vector2Int targetPos = PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID) +
                                       MyGridObject.Position;
                if (PlacedBuildingUtility.CheckForBuilding(targetPos, MyChunk, out PlacedBuilding cellBuild))
                {
                    IHaveInput buildingIn = cellBuild.GetComponent<IHaveInput>();
                    if (buildingIn != null)
                    {
                        Slot next = buildingIn.GetInputSlot(this, outputs[0]);
                        if (next) slotToPushTo = next;
                    }
                }

            }

            UpdateRespConveyorTickEvent();
        }

        private void UpdateRespConveyorTickEvent()
        {
            if (slotToPushTo && subedToConveyorTick)
            {
                ConveyorTick -= StartConveyorChainTickUpdate;
                subedToConveyorTick = false;
                Debug.Log($"Conveyor {name} unsubed");
            }
            else if (!slotToPushTo && !subedToConveyorTick)
            {
                ConveyorTick += StartConveyorChainTickUpdate;
                subedToConveyorTick = true;
                Debug.Log($"Conveyor {name} subed");
            }
        }

        public override void Destroy()
        {
            ConveyorTick -= StartConveyorChainTickUpdate;
            subedToConveyorTick = false;
            base.Destroy();
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            if(slotToPushTo!= null) return null;
            slotToPushTo = destination;
            return outputs[0];
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            if (slotToPullForm != null) return null;
            slotToPullForm = destination;
            return inputs[0];
        }

        public void ConveyorChainTickUpdate()
        {
            if (slotToPushTo)
            {
                if (!slotToPushTo.IsOccupied && outputs[0].IsOccupied)
                {
                    slotToPushTo.PutIntoSlot(outputs[0].ExtractFromSlot());
                }
            }

            if (inputs[0].IsOccupied)
                if (!outputs[0].IsOccupied) outputs[0].PutIntoSlot(inputs[0].ExtractFromSlot());
                else
                {
                    if (slotToPullForm && slotToPullForm.IsOccupied)
                        inputs[0].PutIntoSlot(slotToPullForm.ExtractFromSlot());
                }

            if (slotToPullForm)
                slotToPullForm.GetComponentInParent<IReceiveConveyorChainTickUpdate>()?.ConveyorChainTickUpdate();
        }

        public static IEnumerator ConveyorChainTickUpdateHandler(IReceiveConveyorChainTickUpdate start)
        {
            start.ConveyorChainTickUpdate();
            yield return null;
        }

        public void StartConveyorChainTickUpdate()
        {
            UpdateRespConveyorTickEvent();
            if(!subedToConveyorTick) return;
            StartCoroutine(ConveyorChainTickUpdateHandler(this));
        }
    }
}
