﻿using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    [Serializable]
    public struct ResourcePatch
    {
        public NativeArray<int2> Positions;
        public int itemID;
    }
}