using System;
using UnityEngine;

namespace MRU.InfiniteRunner
{
    [Serializable]
    public sealed class SaveData
    {
        public int coins = 0;
        public int highScore = 0;
        public int selectedSkin = 0;
        public int selectedWorld = 0;
        public int ownedSkins = 1; // Bitmask: skin 0 owned by default
        public int bestDistance = 0;
        public int lifetimeCoins = 0;
        public int totalRuns = 0;
        public int lastDailyUtcDay = -1;
        public int dailyStreak = 0;
        public int missionCoins = 0;
        public int missionRuns = 0;
        public bool missionRewardClaimed = false;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public bool vibrationEnabled = true;
        public int graphicsQualityTier = 2; // 0 = Low, 1 = Medium, 2 = Ultra
        public int achievementsMask = 0;   // Bitfield for unlocked achievements

        private const string Key = "mru_runner_save_v2";
        private const string LegacyKey = "mru_runner_save_v1";

        public static SaveData Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                // Try migration from v1
                json = PlayerPrefs.GetString(LegacyKey, string.Empty);
            }

            if (string.IsNullOrEmpty(json))
            {
                return new SaveData { ownedSkins = 1 };
            }

            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return new SaveData { ownedSkins = 1 };
                if (data.ownedSkins == 0) data.ownedSkins = 1;
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[MRU SaveData] Could not load save file; defaulting. " + exception.Message);
                return new SaveData { ownedSkins = 1 };
            }
        }

        public void Store()
        {
            try
            {
                string json = JsonUtility.ToJson(this);
                PlayerPrefs.SetString(Key, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError("[MRU SaveData] Failed to save player data: " + ex.Message);
            }
        }

        public bool IsAchievementUnlocked(int achievementId)
        {
            return (achievementsMask & (1 << achievementId)) != 0;
        }

        public bool UnlockAchievement(int achievementId)
        {
            if (IsAchievementUnlocked(achievementId)) return false;
            achievementsMask |= (1 << achievementId);
            Store();
            return true;
        }
    }
}
