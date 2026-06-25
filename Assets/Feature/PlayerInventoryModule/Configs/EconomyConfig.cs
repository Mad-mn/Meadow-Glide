using UnityEngine;

namespace Feature.PlayerInventoryModule.Configs
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Configs/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        public int LevelWinReward = 50;
    }
}
