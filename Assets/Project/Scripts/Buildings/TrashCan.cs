using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class TrashCan : PlacedBuilding, IHaveInput, IConveyorDestination
    {
        protected override void StartWorking()
        {
            base.StartWorking();
            CheckForSlotToPullForm();
        }

        public Slot GetInputSlot(PlacedBuilding caller, Slot destination)
        {
            Vector2 offset = caller.MyGridObject.Position - MyGridObject.Position;

            if (!PlacedBuildingUtility.VectorToFacingDirection(offset, out FacingDirection facingDirection)) return null;

            int index = (int)facingDirection;
            if (slotsToPullFrom[index]) return null;
            slotsToPullFrom[index] = destination;
            if (!subedToConveyorTick)
            {
                ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                subedToConveyorTick = true;
            }
            return inputs[index];
        }

        public override void CheckForSlotToPullForm()
        {
            base.CheckForSlotToPullForm();
            if (subedToConveyorTick) return;
            ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
            subedToConveyorTick = true;
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (Slot input in inputs) input.OnSlotContentChanged += Destroyer;
        }

        private void Destroyer(bool filled)
        {
            if(!filled)return;
            foreach (Slot input in inputs)
            {
                input.EmptySlot()?.Destroy();
            }
        }

        public override void Destroy()
        {
            if (subedToConveyorTick) ConveyorBelt.ConveyorTick -= StartConveyorChainTickUpdate;
            base.Destroy();
        }

        public void StartConveyorChainTickUpdate()
        {
            bool resp = false;
            foreach (Slot receiver in slotsToPullFrom)
            {
                if(!receiver) continue;
                IReceiveConveyorChainTickUpdate receive = receiver.gameObject.GetComponentInParent<IReceiveConveyorChainTickUpdate>();
                if(receive ==null) continue;
                StartCoroutine(ConveyorBelt.ConveyorChainTickUpdateHandler(receive));
                resp = true;
            }
            
            PullItem();
            
            switch (resp)
            {
                case false when subedToConveyorTick:
                    ConveyorBelt.ConveyorTick -= StartConveyorChainTickUpdate;
                    subedToConveyorTick = false;
                    break;
                case true when !subedToConveyorTick:
                    ConveyorBelt.ConveyorTick += StartConveyorChainTickUpdate;
                    subedToConveyorTick = true;
                    break;
            }
        }
    }
}
