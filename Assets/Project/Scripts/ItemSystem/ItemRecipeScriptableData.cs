using Project.Scripts.Buildings.BuildingFoundation;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemRecipeScriptableData", menuName = "ElementFactory/Data/ItemRecipe", order = 0)]
    public class ItemRecipeScriptableData : ScriptableObject
    {
        public string trivialName;
        public int buildingID;
        public ItemScriptableData[] ingredients;
        public ItemScriptableData[] products;
    }

    public struct ItemRecipeCompressed
    {
        public int buildingID;
        public int[] ingredientItemIDs;
        public int[] productItemIDs;
    }
}