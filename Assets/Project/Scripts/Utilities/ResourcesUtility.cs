using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class ResourcesUtility
    {
        private static BuildingLookUpData[] _buildingsData;

        private static readonly BuildingScriptableData[] BuildingScriptableDataAry = new[]
        {
            Resources.Load<BuildingScriptableData>("Buildings/Data/Extractor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Conveyor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Combiner"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Separator"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/TrashCan"),
        };
        public static void SetUpBuildingData(NativeArray<EntityIDPair> entityIDPairs)
        {
            _buildingsData = new BuildingLookUpData[entityIDPairs.Length];
            for (var i = 0; i < _buildingsData.Length; i++)
            {
                var data = BuildingScriptableDataAry[i];
                var prefab = Entity.Null;

                foreach (var entityIDPair in entityIDPairs)
                {
                    if (entityIDPair.ID != data.buildingID){continue;}
                    
                    prefab = entityIDPair.Entity;
                    break;
                }

                if (prefab == Entity.Null)
                {
                    Debug.LogError($"Prefab entity for building id {data.buildingID} could not be found",data);
                    continue;
                }
                
                _buildingsData[i] = new BuildingLookUpData(data.nameString,prefab,data.InputOffsets,
                    data.OutputOffsets, data.buildingID,data.neededTiles);
            }
        }

        public static bool GetBuildingData(int buildingID, out BuildingLookUpData buildingLookUpData)
        {
            buildingLookUpData = default;
            foreach (var data in _buildingsData)
            {
                if (data.BuildingID != buildingID) continue;
                buildingLookUpData = data;
                return true;
            }
            return false;
        }

        public static int2[] GetGridPositionList(PlacedBuildingData myPlacedBuildingData)
        {
            if (!GetBuildingData(myPlacedBuildingData.buildingDataID, out var data)) return default;
            var positions = new List<int2>();

            foreach (var tileOffset in data.neededTileOffsets)
            {
                positions.Add(tileOffset.GetPortDirection((FacingDirection)myPlacedBuildingData.directionID));
            }

            return positions.ToArray();
        }
    }

    public readonly struct BuildingLookUpData
    {
        public readonly FixedString64Bytes Name;
        public readonly Entity Prefab;
        public readonly int BuildingID;
        public readonly CellOffsetHandler[] neededTileOffsets;
        private readonly PortDataHandler[] _inputPortInfos, _outputPortInfos;
        private readonly CellOffsetHandler[] _inputDirections, _outputDirection;

        public BuildingLookUpData(FixedString64Bytes name, Entity prefab, PortData[] inputPortData,
            PortData[] outputPortData, int buildingID, int2[] neededTiles)
        {
            Name = name;
            Prefab = prefab;
            BuildingID = buildingID;

            neededTileOffsets = new CellOffsetHandler[neededTiles.Length];
            for (var i = 0; i < neededTiles.Length; i++) { neededTileOffsets[i] = new CellOffsetHandler(neededTiles[i]); }

            _inputPortInfos = new PortDataHandler[inputPortData.Length];
            for (var i = 0; i < _inputPortInfos.Length; i++) { _inputPortInfos[i] = new PortDataHandler(inputPortData[i]); }

            _outputPortInfos = new PortDataHandler[outputPortData.Length];
            for (var i = 0; i < _outputPortInfos.Length; i++) { _outputPortInfos[i] = new PortDataHandler(outputPortData[i]); }

            _inputDirections = new CellOffsetHandler[inputPortData.Length];
            for (var i = 0; i < _inputDirections.Length; i++)
            {
                _inputDirections[i] =
                    new CellOffsetHandler(PlacedBuildingUtility.FacingDirectionToVector(inputPortData[i].direction) +
                                       neededTiles[inputPortData[i].bodyPartID]);
            }

            _outputDirection = new CellOffsetHandler[outputPortData.Length];
            for (var i = 0; i < outputPortData.Length; i++)
            {
                _outputDirection[i] = new CellOffsetHandler(
                    PlacedBuildingUtility.FacingDirectionToVector(outputPortData[i].direction) +
                    neededTiles[outputPortData[i].bodyPartID]);
            }
        }

        public int2 GetBodyOffset(byte bodyID,byte directionID)
        {
            return neededTileOffsets[bodyID].GetPortDirection(directionID);
        }

        #region InputInfos
        public PortInstantData[] GetInputPortInfo(FacingDirection facingDirectionOfBuilding)
        {
            return _inputPortInfos.Select(inputPort => inputPort.GetPortInstantData(facingDirectionOfBuilding))
               
                .ToArray();
        }

        public PortInstantData[] GetInputPortInfo(byte directionID)
        {
            return GetInputPortInfo((FacingDirection)directionID);
        }

        public int2[] GetInputOffsets(FacingDirection facingDirection)
        {
            return _inputDirections.Select(data => data.GetPortDirection(facingDirection)).ToArray();
        }

        public int2[] GetInputOffsets(byte directionID)
        {
           return  GetInputOffsets((FacingDirection)directionID);
        }
        
        #endregion

        #region OutputInfos
        public PortInstantData[] GetOutputPortInfo( FacingDirection facingDirectionOfBuilding)
        {
            return _outputPortInfos.Select(outputPort =>
                    outputPort.GetPortInstantData(facingDirectionOfBuilding))
                .ToArray();
        }
        public PortInstantData[] GetOutputPortInfo(byte directionID)
        {
            return GetOutputPortInfo((FacingDirection)directionID);
        }

        public int2[] GetOutputOffsets(FacingDirection facingDirection)
        {
            return _outputDirection.Select(data => data.GetPortDirection(facingDirection)).ToArray();
        }

        public int2[] GetOutputOffsets(byte directionID)
        {
           return GetOutputOffsets((FacingDirection)directionID);
        }
        
        #endregion
    }

    [Serializable]
    public readonly struct PortDataHandler
    {
        private readonly byte _bodyPartID,_portID;
        private readonly FacingDirection _up, _right, _down, _left;
        public PortDataHandler(PortData portData) : this()
        {
            _up = portData.direction;
            _right = PlacedBuildingUtility.GetNextDirectionClockwise(_up);
            _down = PlacedBuildingUtility.GetNextDirectionClockwise(_right);
            _left = PlacedBuildingUtility.GetNextDirectionClockwise(_down);
            _bodyPartID = portData.bodyPartID;
            _portID = portData.portID;
        }

        public PortInstantData GetPortInstantData(FacingDirection facingDirection)
        {
            return new PortInstantData(_bodyPartID, facingDirection switch
            {
                FacingDirection.Up => _up,
                FacingDirection.Right => _right,
                FacingDirection.Down => _down,
                FacingDirection.Left => _left,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirection), facingDirection, null)
            },_portID);
        }
    }

    [Serializable]
    public readonly struct PortInstantData
    {
        public readonly byte bodyPartID,portID;
        public readonly FacingDirection direction;
        public PortInstantData(byte bodyPartID, FacingDirection direction, byte portID)
        {
            this.bodyPartID = bodyPartID;
            this.direction = direction;
            this.portID = portID;
        }
    }

    [Serializable]
    public readonly struct CellOffsetHandler
    {
        private readonly int2 _up, _right, _down, _left;
        
        public CellOffsetHandler(int2 directionOffsetFacingUp) : this()
        {
            _up = directionOffsetFacingUp;
            _right = PlacedBuildingUtility.GetRotatedVectorClockwise(_up);
            _down = PlacedBuildingUtility.GetRotatedVectorClockwise(_right);
            _left = PlacedBuildingUtility.GetRotatedVectorClockwise(_down);
        }

        public int2 GetPortDirection(FacingDirection facingDirectionOfBuilding)
        {
            return facingDirectionOfBuilding switch
            {
                FacingDirection.Up => _up,
                FacingDirection.Right => _right,
                FacingDirection.Down => _down,
                FacingDirection.Left => _left,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirectionOfBuilding), facingDirectionOfBuilding,
                    null)
            };
        }
        public int2 GetPortDirection(byte facingID)
        {
            return GetPortDirection((FacingDirection)facingID);
        }
    }
}
