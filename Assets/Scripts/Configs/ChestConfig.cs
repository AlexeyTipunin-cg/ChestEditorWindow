using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Chest")]
public class ChestConfig : ScriptableObject
{

    public enum RewardType
    {
        Soft,
        Hard,
        Chest,
    }

    [Serializable]
    public class RewardInfo
    {
        public float randomWeight;
        public RewardType type;
        public int softMin;
        public int softMax;
        public float hardMin;
        public float hardMax;
        public ChestConfig chest;
        public int chestCount;
    }

    [SerializeField] private RewardInfo[] _rewards;
    public RewardInfo[] Rewards => _rewards;
}
