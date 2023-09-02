using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
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
            slotsToPullFrom = new Slot[inputs.Length];
            slotsToPushTo = new Slot[outputs.Length];

            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
        }

        public override void CheckForSlotToPullForm()
        {
            if (slotsToPullFrom[0]) return;
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
                slotsToPullFrom[0] = next;
                break;
            }
        }

        public override void CheckForSlotsToPushTo()
        {
            if (!slotsToPushTo[0])
            {
                Vector2Int targetPos = PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID) +
                                       MyGridObject.Position;
                if (PlacedBuildingUtility.CheckForBuilding(targetPos, MyChunk, out PlacedBuilding cellBuild))
                {
                    IHaveInput buildingIn = cellBuild.GetComponent<IHaveInput>();
                    if (buildingIn != null)
                    {
                        Slot next = buildingIn.GetInputSlot(this, outputs[0]);
                        if (next) slotsToPushTo[0] = next;
                    }
                }

            }

            UpdateRespConveyorTickEvent();
        }

        private void UpdateRespConveyorTickEvent()
        {
            if (slotsToPushTo[0] && subedToConveyorTick)
            {
                ConveyorTick -= StartConveyorChainTickUpdate;
                subedToConveyorTick = false;
                //Debug.Log($"Conveyor {name} unsubed");
            }
            else if (!slotsToPushTo[0] && !subedToConveyorTick)
            {
                SubToConveyorTickEvent();
            }
        }

        public void SubToConveyorTickEvent()
        {
            ConveyorTick += StartConveyorChainTickUpdate;
            subedToConveyorTick = true;
            //Debug.Log($"Conveyor {name} subed");
        }

        public override void Destroy()
        {
            if (subedToConveyorTick) ConveyorTick -= StartConveyorChainTickUpdate;
            base.Destroy();
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            if(slotsToPushTo[0]) return null;
            slotsToPushTo[0] = destination;
            return outputs[0];
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            if (slotsToPullFrom[0]) return null;
            slotsToPullFrom[0] = destination;
            return inputs[0];
        }

        public void ConveyorChainTickUpdate()
        {
            // try to move ItemContainer: own Output => input of the next building(SlotToPushTo)
            if (slotsToPushTo[0] && !slotsToPushTo[0].IsOccupied && outputs[0].IsOccupied)
            {
                slotsToPushTo[0].PutIntoSlot(outputs[0].ExtractFromSlot());
            }
            
            // try to move ItemContainer: own Input => own Output
            if (inputs[0].IsOccupied && !outputs[0].IsOccupied)
            {
                outputs[0].PutIntoSlot(inputs[0].ExtractFromSlot());
            }

            if (!slotsToPullFrom[0]) return;
            
            IReceiveConveyorChainTickUpdate nextChainLink = slotsToPullFrom[0].GetComponentInParent<IReceiveConveyorChainTickUpdate>();
            if (nextChainLink != null)
            {
                //continue ChainUpdate to next link 
                nextChainLink.ConveyorChainTickUpdate();
            }
            //end of ChainUpdate: try to move ItemContainer: Output of previous building(slotToPullFrom) => own input 
            else if (!inputs[0].IsOccupied && slotsToPullFrom[0].IsOccupied)
            {
                inputs[0].PutIntoSlot(slotsToPullFrom[0].ExtractFromSlot());
            }
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
