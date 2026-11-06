using UnityEngine;

namespace MRU.InfiniteRunner
{
    [CreateAssetMenu(fileName = "MRUAdConfiguration", menuName = "MRU Runner/Ad Configuration")]
    public sealed class AdConfiguration : ScriptableObject
    {
        public bool useTestAdUnits = true;
        public string productionInterstitialAndroid = "";
        public string productionRewardedAndroid = "";
    }
}
