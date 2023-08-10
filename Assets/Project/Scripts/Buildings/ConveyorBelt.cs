using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuilding
    {
        private static Action ConveyorTick;
        private static float itemsPerSecond = 1;
        private static Coroutine runningTickClock;

        private static IEnumerator TickClock()
        {
            while (true)
            {
                ConveyorTick?.Invoke();
                yield return new WaitForSeconds(1f/itemsPerSecond);
            }
        }
        
        public Slot flowSlot1, flowSlot2;
        public Slot sourceSlot;
        protected override void StartWorking()
        {
            flowSlot1 = new Slot();
            flowSlot2 = new Slot();
            ConveyorTick += Tick;
            runningTickClock ??= StartCoroutine(TickClock());
            validOutputSorource = new List<Vector2Int>() 
                { GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin };

            validInputSorource = new List<Vector2Int>() 
                { -1*GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin };
            
            CheckForSource();
        }

        protected override Slot GetInputSlot(GridObject callerPosition)
        {
            return validOutputSorource.Contains(callerPosition.Position) ? flowSlot1 : null;
        }

        protected override Slot GetOutputSlot(GridObject callerPosition)
        {
            return validInputSorource.Contains(callerPosition.Position) ? flowSlot2 : null;
        }

        public void CheckForSource()
        {
            GridObject cell = MyChunk.ChunkBuildingGrid.GetCellData(validInputSorource[0]);
            PlacedBuilding cellBuild = cell.Building;
            if(!cellBuild) return;
            if (cellBuild.MyPlacedBuildingData.buildingDataID == 1)
            {
                ConveyorBelt next = cellBuild as ConveyorBelt;
                if (next != null) sourceSlot = next.GetInputSlot(MyGridObject);
            }
        }

        private void Tick()
        {
            Item item;
            if (sourceSlot.ExtractFromSlot(out item) && flowSlot1 is { IsOccupied: false })
                flowSlot1.PutIntoSlot(item);
            if (flowSlot1.ExtractFromSlot(out item) && !flowSlot2.IsOccupied)
                flowSlot2.PutIntoSlot(item);
        }
    }
}
