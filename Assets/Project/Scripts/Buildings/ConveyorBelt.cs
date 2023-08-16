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
        
        private Slot flowSlot1, flowSlot2;
        private Slot sourceSlot;
        protected override void StartWorking()
        {
            flowSlot1 = new Slot();
            flowSlot2 = new Slot();
            ConveyorTick += Tick;
            runningTickClock ??= StartCoroutine(TickClock());
            validOutputPositions = new List<Vector2Int>() 
                { GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin };

            validInputPositions = new List<Vector2Int>() 
                { -1*GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin };
            
            CheckForSource();
        }

        public override Slot GetInputSlot(GridObject callerPosition)
        {
            return validOutputPositions.Contains(callerPosition.Position) ? flowSlot1 : null;
        }

        public override Slot GetOutputSlot(GridObject callerPosition)
        {
            return validInputPositions.Contains(callerPosition.Position) ? flowSlot2 : null;
        }

        public void CheckForSource()
        {
            GridObject cell = MyChunk.ChunkBuildingGrid.GetCellData(validInputPositions[0]);
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
