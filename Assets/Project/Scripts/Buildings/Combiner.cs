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

        public override Slot GetInputSlot(GridObject callerPosition)
        {
            throw new System.NotImplementedException();
        }

        public override Slot GetOutputSlot(GridObject callerPosition)
        {
            throw new System.NotImplementedException();
        }

        protected override void SetUpSlots(BuildingScriptableData.Directions direction)
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForInputs()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForOutputs()
        {
            throw new System.NotImplementedException();
        }
    }
}
