using System;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    [Serializable]
    public struct ResourcePatch
    {
        public NativeArray<float2> Positions;
        public Item Resource;
    }
}