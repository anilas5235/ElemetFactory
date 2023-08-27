using Project.Scripts.General;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    public class ItemPool : ObjectPooling<ItemPool>
    {
        protected override GameObject CreateNewPoolObject()
        {
            return ItemContainer.CreateNewContainer().gameObject;
        }
    }
}
