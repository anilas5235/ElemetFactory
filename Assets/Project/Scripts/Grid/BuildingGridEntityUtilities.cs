using System;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public static class BuildingGridEntityUtilities
    {
    }
      [Serializable]
    public readonly struct BuildingData
    {
        public readonly FixedString64Bytes Name;
        public readonly Entity Prefab;
        public readonly int BuildingID;
        private readonly PortDirections[] _inputPortDirections, _outputPortDirections;
        
        public BuildingData(FixedString64Bytes name, Entity prefab, int2[] inputDirections, int2[] outputDirections, int buildingID)
        {
            Name = name;
            Prefab = prefab;
            BuildingID = buildingID;
            _inputPortDirections = new PortDirections[inputDirections.Length];
            for (int i = 0; i < _inputPortDirections.Length; i++)
            {
                _inputPortDirections[i] = new PortDirections(inputDirections[i]);
            }
            _outputPortDirections = new PortDirections[outputDirections.Length];
            for (int i = 0; i < _outputPortDirections.Length; i++)
            {
                _outputPortDirections[i] = new PortDirections(outputDirections[i]);
            }
        }

        public int2[] GetInputPortDirections(FacingDirection facingDirectionOfBuilding)
        {
            return _inputPortDirections.Select(inputPort => inputPort.GetPortDirection(facingDirectionOfBuilding)).ToArray();
        }
        
        public int2[] GetOutputPortDirections(FacingDirection facingDirectionOfBuilding)
        {
            return _outputPortDirections.Select(outputPort => outputPort.GetPortDirection(facingDirectionOfBuilding)).ToArray();
        }
    }

    [Serializable]
    public readonly struct PortDirections
    {
        private readonly int2 _up, _right, _down, _left;
        public PortDirections(int2 directionOffsetFacingUp) : this()
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
                FacingDirection.Down => _left,
                FacingDirection.Left => _down,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirectionOfBuilding), facingDirectionOfBuilding,
                    null)
            };
        }
    }
}
