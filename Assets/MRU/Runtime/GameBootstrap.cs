using UnityEngine;

namespace MRU.InfiniteRunner
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (FindFirstObjectByType<GameManager>() == null)
                new GameObject("MRU Game Manager").AddComponent<GameManager>();
        }
    }
}
