using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class BuildingMono : MonoBehaviour
    {
        [SerializeField] private string nameString;
        [SerializeField] private int buildingID;
        [SerializeField] private int2[] neededTiles;
        [SerializeField] private PortData[] inputOffsets;
        [SerializeField] private PortData[] outputOffsets;
        public string NameString => nameString;
        public int BuildingID => buildingID;
        public int2[] NeededTiles => neededTiles;
        public PortData[] InputOffsets => inputOffsets;
        public PortData[] OutputOffsets => outputOffsets;
        
        public class BuildingBaker : Baker<BuildingMono>
        {
            public override void Bake(BuildingMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingDataComponent( new PlacedBuildingData()
                {
                    buildingDataID = authoring.buildingID,
                }));
                
                var inputBuffer = AddBuffer<InputSlot>(entity);

                for (var i = 0; i < authoring.inputOffsets.Length; i++){inputBuffer.Add(new InputSlot());}
                
                var outputBuffer =AddBuffer<OutputSlot>(entity);
                
                for (var i = 0; i < authoring.outputOffsets.Length; i++){outputBuffer.Add(new OutputSlot());}
            }
        }
    }
}