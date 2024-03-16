using UnityEngine;

namespace Project.Scripts.ScriptableData
{
    [CreateAssetMenu(fileName = "ItemScriptableData", menuName = "Factory/Data/Item", order = 0)]
    public class ItemScriptableData : ScriptableObject
    {
        public string nameString;
        public Sprite sprite;
        public int uniqueID;
    }
}