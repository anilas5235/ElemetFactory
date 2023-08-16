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
    }
}
