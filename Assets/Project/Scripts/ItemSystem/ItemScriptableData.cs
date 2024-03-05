using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemScriptableData", menuName = "Data/Item", order = 0)]
    public class ItemScriptableData : ScriptableObject
    {
        public string trivialName;
        public int uniqueID;
    }
}