using System;
using System.Collections;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.Utilities
{
    public static class ItemEntityUtility
    {
        public static Entity CombineItemEntities(int index,Entity building, OutputSlot outputSlot,
            NativeArray<ResourceDataPoint> resourceDataPointsA, NativeArray<ResourceDataPoint> resourceDataPointsB
            , EntityCommandBuffer.ParallelWriter ECB, NativeArray<Entity> prefabs,
            int worldScale, NativeArray<ResourceLookUpData> resourceLookUpData)
        {
            using var itemBuffer = new NativeList<ResourceDataPoint>(Allocator.TempJob);
            itemBuffer.AddRange(resourceDataPointsA);
            itemBuffer.AddRange(resourceDataPointsB);
            
            CalculateItemAppearance(itemBuffer,resourceLookUpData,out var color,out var form);

            return CreateItemEntity(index,building, outputSlot, itemBuffer.AsArray(), form,
                color, ECB, prefabs, worldScale);
        }

        public static void CalculateItemAppearance(NativeList<ResourceDataPoint> itemBuffer,
            NativeArray<ResourceLookUpData> resourceLookUpData, out float4 color, out ItemForm itemForm)

        {
            color = float4.zero;
            float form = 0;
            foreach (var id in itemBuffer)
            {
                ResourceLookUpData lookUpData = resourceLookUpData[(int)id.id];
                color += new float4(lookUpData.color.r, lookUpData.color.g, lookUpData.color.b, 0);
                form += (int)lookUpData.form;
            }

            color *= 1f / itemBuffer.Length;
            color.w = 1f;
            form *= 1f / itemBuffer.Length;
            form = Mathf.RoundToInt(form);
            itemForm = (ItemForm)form;
        }

        public static Entity CreateItemEntity(int index, Entity building, OutputSlot outputSlot, NativeArray<ResourceDataPoint> resourceDataPoints,
          ItemForm itemForm, float4 itemColor ,EntityCommandBuffer.ParallelWriter ECB, NativeArray<Entity> prefabs, int worldScale)
        {

            var itemEntity = itemForm switch
            {
                ItemForm.Gas => ECB.Instantiate(index, prefabs[0]),
                ItemForm.Fluid => ECB.Instantiate(index, prefabs[1]),
                ItemForm.Solid => ECB.Instantiate(index, prefabs[2]),
                _ => throw new ArgumentOutOfRangeException()
            };

            ECB.SetComponent(index, itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = outputSlot.Position,
                PreviousPos = outputSlot.Position,
            });


            DynamicBuffer<ResourceDataPoint> bufferResource = ECB.SetBuffer<ResourceDataPoint>(index, itemEntity);
          
            bufferResource.AddRange(resourceDataPoints);

            ECB.SetComponent(index, itemEntity, new LocalTransform()
            {
                Position = outputSlot.Position,
                Scale = worldScale * .7f,
            });

            ECB.SetComponent(index, itemEntity, new ItemColor()
            {
                Value = itemColor,
            });

            ECB.SetComponent(index, itemEntity, new ItemDataComponent()
            {
                itemForm = itemForm,
                itemColor = itemColor,
            });

            ECB.SetName(index, itemEntity, "Item");
            
            ECB.AddComponent(index, building, new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = outputSlot.OwnIndex,
            });
            return itemEntity;
        }


        public static bool SplitItemEntity(int index,Entity building, OutputSlot outputSlotA,OutputSlot outputSlotB,
            NativeArray<ResourceDataPoint> resourceDataPoints, EntityCommandBuffer.ParallelWriter ECB,
            NativeArray<Entity> prefabs, int worldScale, NativeArray<ResourceLookUpData> resourceLookUpData,
            out Entity itemEntityA, out Entity itemEntityB, int splitIndex =-1)
        {
            itemEntityA = default;
            itemEntityB = default;
            if (resourceDataPoints.Length < 2) return false;

            if (splitIndex < 0) splitIndex = (int)math.ceil(resourceDataPoints.Length / 2f);
           
            if (splitIndex < 0 || !(splitIndex < resourceDataPoints.Length)) return false;
            
            using var itemBufferA = new NativeList<ResourceDataPoint>(Allocator.TempJob);
            using var itemBufferB = new NativeList<ResourceDataPoint>(Allocator.TempJob);

            bool addToA = true;

            for (int i = 0; i < resourceDataPoints.Length; i++)
            {
                if (i > splitIndex) addToA = false;
                if(addToA) itemBufferA.Add(new ResourceDataPoint()
                {
                    id = resourceDataPoints[i].id,
                });
                else itemBufferB.Add(new ResourceDataPoint()
                {
                    id = resourceDataPoints[i].id,
                });
            }
            
            CalculateItemAppearance(itemBufferA,resourceLookUpData,out var colorA,out var formA);
            CalculateItemAppearance(itemBufferB,resourceLookUpData,out var colorB,out var formB);
            
            itemEntityA = CreateItemEntity(index,building, outputSlotA, itemBufferA.AsArray(), formA,
                colorA, ECB, prefabs, worldScale);
            itemEntityB = CreateItemEntity(index,building ,outputSlotB, itemBufferB.AsArray(), formB,
                colorB, ECB, prefabs, worldScale);

            return true;
        }
    }
}