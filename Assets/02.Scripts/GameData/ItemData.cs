using UnityEngine;

namespace GameData
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public int itemID;
    }
}