using Project.Scripts.Buildings.BuildingFoundation;
using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class ItemRecipeScriptableData : ScriptableObject
    {
        public string trivialName;
        public BuildingScriptableData building;
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