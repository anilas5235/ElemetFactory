using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Combiner : PlacedBuilding
    {
        private static float CombinationsPerSecond = .25f;

        protected override void StartWorking()
        {
            throw new System.NotImplementedException();
        }

        public override Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            throw new System.NotImplementedException();
        }

        public override Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            throw new System.NotImplementedException();
        }

        protected override void SetUpSlots(BuildingScriptableData.FacingDirection facingDirection)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotToPullForm()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotsToPushTo()
        {
            throw new System.NotImplementedException();
        }
    }
}
