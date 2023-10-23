using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.ItemSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class ResourcesUtility
    {
        private static BuildingLookUpData[] BuildingsData;

        private static readonly BuildingScriptableData[] BuildingScriptableDataAry = new[]
        {
            Resources.Load<BuildingScriptableData>("Buildings/Data/Extractor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Conveyor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Combiner"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Separator"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/TrashCan"),
        };
        
        private static readonly ResourceLookUpData[] ResourceDataBank = new[]
        {
            new ResourceLookUpData(ResourceType.None,new Color(.7f,.7f,.7f),ItemForm.Solid),
            new ResourceLookUpData(ResourceType.H,new Color(.1f,.8f,1f), ItemForm.Gas),
            new ResourceLookUpData(ResourceType.C,new Color(.1f,.1f,.1f), ItemForm.Solid),
            new ResourceLookUpData(ResourceType.O,new Color(0f,1f,0f), ItemForm.Gas),
            new ResourceLookUpData(ResourceType.N,new Color(1f,0f,0f), ItemForm.Gas),
            new ResourceLookUpData(ResourceType.S,new Color(1f,1f,0f), ItemForm.Solid),
            new ResourceLookUpData(ResourceType.Al,new Color(1f,.5f,0f), ItemForm.Solid),
            new ResourceLookUpData(ResourceType.Fe,new Color(.5f,0f,1f), ItemForm.Solid),
            new ResourceLookUpData(ResourceType.Na,new Color(1f,.2f,1f), ItemForm.Gas),
        };
        
        [BurstDiscard]
        public static void SetUpBuildingData(PrefabsDataComponent ComponentData)
        {
            Entity[] entities = new[]
            {
                ComponentData.Extractor,
                ComponentData.Conveyor,
                ComponentData.Combiner,
                ComponentData.Separator,
                ComponentData.TrashCan,
            };

            BuildingsData = new BuildingLookUpData[entities.Length];
            for (int i = 0; i < BuildingsData.Length; i++)
            {
                BuildingScriptableData data = BuildingScriptableDataAry[i];
                BuildingsData[i] = new BuildingLookUpData(data.nameString,entities[i],data.InputOffsets,
                    data.OutputOffsets, data.buildingID,data.neededTiles);
            }
            Debug.Log("Setup Comp");
        }

        public static bool GetBuildingData(int buildingID, out BuildingLookUpData buildingLookUpData)
        {
            buildingLookUpData = default;
            foreach (BuildingLookUpData data in BuildingsData)
            {
                if (data.BuildingID != buildingID) continue;
                buildingLookUpData = data;
                return true;
            }
            return false;
        }

        public static Item CreateItemData(NativeArray<uint> resourceIDs)
        {
            float4 color = float4.zero;
            float form =0;
            foreach (var id in resourceIDs)
            {
                ResourceLookUpData lookUpData = GetResourceData(id);
                color += new float4(lookUpData.color.r, lookUpData.color.g, lookUpData.color.b,0);
                form += (int)lookUpData.form;
            }
            
            color *= 1f / resourceIDs.Length;
            color.w = 1f;
            form *= 1f / resourceIDs.Length;
            form = Mathf.RoundToInt(form);
            
            return new Item(resourceIDs,(ItemForm)form,color);
        }

        public static Item CreateItemData(uint[] resourceIDs)
        {
            NativeArray<uint> ids = new NativeArray<uint>(resourceIDs, Allocator.TempJob);
            Item item = CreateItemData(ids);
            ids.Dispose();
            return item;
        }
        
        public static ResourceLookUpData GetResourceData(ResourceType resourceType)
        {
            return GetResourceData((uint)resourceType);
        }
        
        public static ResourceLookUpData GetResourceData(uint resourceID)
        {
            return ResourceDataBank[resourceID];
        }

        public static int2[] GetGridPositionList(PlacedBuildingData myPlacedBuildingData)
        {
            if (!GetBuildingData(myPlacedBuildingData.buildingDataID, out BuildingLookUpData data)) return default;
            List<int2> positions = new List<int2>();

            foreach (PortDirections tileOffset in data.neededTileOffsets)
            {
                positions.Add(tileOffset.GetPortDirection((FacingDirection)myPlacedBuildingData.directionID));
            }

            return positions.ToArray();
        }
    }

    [Serializable]
    public readonly struct ResourceLookUpData
    {
        public string Name => resourceType.ToString();
        public readonly ResourceType resourceType;
        public readonly Color color;
        public readonly ItemForm form;

        public ResourceLookUpData(ResourceType resourceType, Color color, ItemForm form)
        {
            this.color = color;
            this.form = form;
            this.resourceType = resourceType;
        }
    }

    public readonly struct BuildingLookUpData
    {
        public readonly FixedString64Bytes Name;
        public readonly Entity Prefab;
        public readonly int BuildingID;
        public readonly PortDirections[] neededTileOffsets;
        private readonly PortDataHandler[] _inputPortInfos, _outputPortInfos;
        private readonly PortDirections[] _inputDirections, _outputDirection;

        public BuildingLookUpData(FixedString64Bytes name, Entity prefab, PortData[] inputPortData,
            PortData[] outputPortData, int buildingID, int2[] neededTiles)
        {
            Name = name;
            Prefab = prefab;
            BuildingID = buildingID;

            neededTileOffsets = new PortDirections[neededTiles.Length];
            for (int i = 0; i < neededTiles.Length; i++)
            {
                neededTileOffsets[i] = new PortDirections(neededTiles[i]);
            }

            _inputPortInfos = new PortDataHandler[inputPortData.Length];
            for (int i = 0; i < _inputPortInfos.Length; i++)
            {
                _inputPortInfos[i] = new PortDataHandler(inputPortData[i]);
            }

            _outputPortInfos = new PortDataHandler[outputPortData.Length];
            for (int i = 0; i < _outputPortInfos.Length; i++)
            {
                _outputPortInfos[i] = new PortDataHandler(outputPortData[i]);
            }

            _inputDirections = new PortDirections[inputPortData.Length];
            for (int i = 0; i < _inputDirections.Length; i++)
            {
                _inputDirections[i] =
                    new PortDirections(PlacedBuildingUtility.FacingDirectionToVector(inputPortData[i].direction) +
                                       neededTiles[inputPortData[i].bodyPartID]);
            }

            _outputDirection = new PortDirections[outputPortData.Length];
            for (int i = 0; i < outputPortData.Length; i++)
            {
                _outputDirection[i] = new PortDirections(
                    PlacedBuildingUtility.FacingDirectionToVector(outputPortData[i].direction) +
                    neededTiles[outputPortData[i].bodyPartID]);
            }
        }

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
    }

    [Serializable]
    public readonly struct PortDataHandler
    {
        private readonly byte _bodyPartID;
        private readonly FacingDirection _up, _right, _down, _left;
        public PortDataHandler(PortData portData) : this()
        {
            _up = portData.direction;
            _right = PlacedBuildingUtility.GetNextDirectionClockwise(_up);
            _down = PlacedBuildingUtility.GetNextDirectionClockwise(_right);
            _left = PlacedBuildingUtility.GetNextDirectionClockwise(_down);
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
            });
        }
    }

    [Serializable]
    public readonly struct PortInstantData
    {
        public readonly byte bodyPartID;
        public readonly FacingDirection direction;
        public PortInstantData(byte bodyPartID, FacingDirection direction)
        {
            this.bodyPartID = bodyPartID;
            this.direction = direction;
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
