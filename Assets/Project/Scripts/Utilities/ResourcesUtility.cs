using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class ResourcesUtility
    {
        private static BuildingData[] BuildingsData;

        private static readonly BuildingScriptableData[] BuildingScriptableDataAry = new[]
        {
            Resources.Load<BuildingScriptableData>("Buildings/Data/Extractor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Conveyor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Combiner"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Separator"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/TrashCan"),
        };
        
        private static readonly ResourceData[] ResourceDataBank = new[]
        {
            new ResourceData(ResourceType.None,new Color(.7f,.7f,.7f),ItemForm.Solid),
            new ResourceData(ResourceType.H,new Color(.1f,.8f,1f), ItemForm.Gas),
            new ResourceData(ResourceType.C,new Color(.1f,.1f,.1f), ItemForm.Solid),
            new ResourceData(ResourceType.O,new Color(0f,1f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.N,new Color(1f,0f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.S,new Color(1f,1f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Al,new Color(1f,.5f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Fe,new Color(.5f,0f,1f), ItemForm.Solid),
            new ResourceData(ResourceType.Na,new Color(1f,.2f,1f), ItemForm.Gas),
        };
        
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

            BuildingsData = new BuildingData[entities.Length];
            for (int i = 0; i < BuildingsData.Length; i++)
            {
                BuildingScriptableData data = BuildingScriptableDataAry[i];
                BuildingsData[i] = new BuildingData(data.nameString,entities[i],data.InputOffsets,
                    data.OutputOffsets, data.buildingID,data.neededTiles);
            }
            Debug.Log("Setup Comp");
        }

        public static bool GetBuildingData(int buildingID, out BuildingData buildingData)
        {
            buildingData = default;
            foreach (BuildingData data in BuildingsData)
            {
                if (data.BuildingID != buildingID) continue;
                buildingData = data;
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
                ResourceData data = GetResourceData(id);
                color += new float4(data.color.r, data.color.g, data.color.b,0);
                form += (int)data.form;
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
        
        public static ResourceData GetResourceData(ResourceType resourceType)
        {
            return GetResourceData((uint)resourceType);
        }
        
        public static ResourceData GetResourceData(uint resourceID)
        {
            return ResourceDataBank[resourceID];
        }

        public static int2[] GetGridPositionList(PlacedBuildingData myPlacedBuildingData)
        {
            if (!GetBuildingData(myPlacedBuildingData.buildingDataID, out BuildingData data)) return default;
            List<int2> positions = new List<int2>();

            foreach (PortDirections tileOffset in data.neededTileOffsets)
            {
                positions.Add(tileOffset.GetPortDirection((FacingDirection)myPlacedBuildingData.directionID));
            }

            return positions.ToArray();
        }
    }

    [Serializable]
    public readonly struct ResourceData
    {
        public string Name => resourceType.ToString();
        public readonly ResourceType resourceType;
        public readonly Color color;
        public readonly ItemForm form;

        public ResourceData(ResourceType resourceType, Color color, ItemForm form)
        {
            this.color = color;
            this.form = form;
            this.resourceType = resourceType;
        }
    }
    public readonly struct BuildingData
    {
        public readonly FixedString64Bytes Name;
        public readonly Entity Prefab;
        public readonly int BuildingID;
        public readonly PortDirections[] neededTileOffsets;
        private readonly PortDirections[] _inputPortDirections, _outputPortDirections;

        public BuildingData(FixedString64Bytes name, Entity prefab, int2[] inputDirections, int2[] outputDirections,
            int buildingID, int2[] neededTiles)
        {
            Name = name;
            Prefab = prefab;
            BuildingID = buildingID;

            neededTileOffsets = new PortDirections[neededTiles.Length];
            for (int i = 0; i < neededTiles.Length; i++)
            {
                neededTileOffsets[i] = new PortDirections(neededTiles[i]);
            }
            
            _inputPortDirections = new PortDirections[inputDirections.Length];
            for (int i = 0; i < _inputPortDirections.Length; i++)
            {
                _inputPortDirections[i] = new PortDirections(inputDirections[i]);
            }

            _outputPortDirections =new PortDirections[ outputDirections.Length];
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
                FacingDirection.Down => _down,
                FacingDirection.Left => _left,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirectionOfBuilding), facingDirectionOfBuilding,
                    null)
            };
        }
    }
}
