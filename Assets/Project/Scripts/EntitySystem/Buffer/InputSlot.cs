using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Buffer
{
    [Serializable]
    public struct InputSlot : IBufferElementData
    {
        public float3 WorldPosition {get; private set;}
        public FacingDirection FacingDirection{get; private set;}
        public Entity SlotContent{get; private set;}
        public Entity ConnectedEntity{get; private set;}
        public int OwnIndex{get; private set;}
        public int ConnectedIndex{get; private set;}
        public bool IsOccupied => SlotContent != default;
        public bool IsConnected => ConnectedEntity != default;

        public void SetWorldPosition(float3 pos) {WorldPosition = pos;}
        public void SetFacingDirection(FacingDirection direction) {FacingDirection = direction;}
        public void SetSlotContent(Entity entity = default) { SlotContent = entity;}
        public void SetConnectedEntity(Entity entity = default) {ConnectedEntity = entity;}
        public void SetOwnIndex(int index = default) { OwnIndex = index; }
        public void SetConnectedIndex(int index = default) { ConnectedIndex = index; }
    }
}