﻿using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.DataObject
{
    public struct ItemPrefabsDataComponent : IComponentData
    {
        public Entity ItemPrefab;
    }
}