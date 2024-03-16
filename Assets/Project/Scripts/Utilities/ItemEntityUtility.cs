using Project.Scripts.EntitySystem.BlobAssets;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.Utilities
{
    public static class ItemEntityUtility
    {
        public static Entity CreateItemEntity(int index, Entity building, OutputSlot outputSlot, int itemID,
            EntityCommandBuffer.ParallelWriter ecb, int worldScale, BlobAssetReference<BlobGamePrefabData> blobPrefs)
        {

            var itemEntity = ecb.Instantiate(index, blobPrefs.Value.ItemPrefab);
            
            ecb.SetComponent(index,itemEntity,new AtlasModifier()
            {
                Value = blobPrefs.Value.GetAtlasPositionForItem(itemID),
            });

            ecb.SetComponent(index, itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = outputSlot.WorldPosition,
                PreviousPos = outputSlot.WorldPosition,
            });

            ecb.SetComponent(index, itemEntity, new LocalTransform()
            {
                Position = outputSlot.WorldPosition,
                Scale = worldScale * .7f,
            });

            ecb.SetComponent(index, itemEntity, new ItemDataComponent()
            {
                ItemID = itemID,
            });

            ecb.SetName(index, itemEntity, "Item");

            ecb.AddComponent(index, building, new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = outputSlot.OwnIndex,
            });
            return itemEntity;
        }
    }
}