using Project.Scripts.Buildings.BuildingFoundation;
using UnityEngine;

namespace Project.Scripts.ScriptableData
{
    [CreateAssetMenu(fileName = "ItemRecipeScriptableData", menuName = "Factory/Data/ItemRecipe", order = 0)]
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