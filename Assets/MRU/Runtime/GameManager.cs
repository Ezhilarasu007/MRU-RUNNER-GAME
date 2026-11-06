using System;
using UnityEngine;

namespace MRU.InfiniteRunner
{
    public enum RunState { Menu, Running, Paused, GameOver }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public RunnerController Runner { get; private set; }
        public RunState State { get; private set; }
        public SaveData Save { get; private set; }
        public int Score { get; private set; }
        public int DistanceMeters { get; private set; }
        public int SelectedWorld => selectedWorld;
        public int RunCoins { get; private set; }
        public int StartWorld { get; private set; }
        public int Multiplier { get; private set; } = 1;
        public bool MagnetActive => magnetTimer > 0f;
        public bool ShieldActive => shieldReady;
        public bool HoverboardActive => hoverboardTimer > 0f;
        public string CurrentWorldName => track == null ? "Ancient Temple" : track.WorldName;
        public string CurrentWorldDesc => track == null ? "Stone Obelisks & Ruins" : track.WorldDescription;
        public int HighScore => Save.highScore;
        public float Speed => Mathf.Min(16f, 8.5f + DistanceMeters * 0.0075f + (HoverboardActive ? 3f : 0f));
        public string ActivePowerText { get; private set; } = string.Empty;

        private TrackGenerator track;
        private RuntimeUI ui;
        private Camera gameCamera;
        private AudioSource sfxSource;

        private float magnetTimer, doubleCoinsTimer, multiplierTimer, hoverboardTimer;
        private float uiRefreshTimer;
        private float preciseDistance;
        private bool shieldReady;

        private AudioClip coinClip, jumpClip, powerupClip, hitClip, clickClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Save = SaveData.Load();
            selectedWorld = Mathf.Clamp(Save.selectedWorld, 0, 9);
            StartWorld = selectedWorld;

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            CreateLightingAndCamera();
            InitAudioSynthesizer();

            Runner = RunnerController.Create();
            track = new TrackGenerator(this, StartWorld);
            gameObject.AddComponent<AdService>();

            ui = new RuntimeUI(this);
            ui.ShowMenu();
            State = RunState.Menu;
        }

        private void CreateLightingAndCamera()
        {
            GameObject cameraObject = new GameObject("Runner Camera");
            gameCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            gameCamera.fieldOfView = 60f;
            gameCamera.backgroundColor = new Color(0.60f, 0.78f, 0.98f);
            cameraObject.transform.position = new Vector3(0f, 5.2f, -8.5f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 280f;

            GameObject lightObject = new GameObject("World Sun");
            Light sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.30f;
            lightObject.transform.rotation = Quaternion.Euler(42f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.72f, 0.75f, 0.80f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = gameCamera.backgroundColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 75f;
            RenderSettings.fogEndDistance = 250f;
        }

        #region Audio Synthesizer
        private void InitAudioSynthesizer()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = Save.soundEnabled ? 1f : 0f;

            coinClip = GenerateSynthClip(0.12f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(988f, 1318f, t / 0.12f) * t) * (1f - t / 0.12f));
            jumpClip = GenerateSynthClip(0.18f, (t) => Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(250f, 650f, t / 0.18f) * t) * (1f - t / 0.18f));
            powerupClip = GenerateSynthClip(0.35f, (t) => {
                float freq = t < 0.1f ? 523f : t < 0.2f ? 659f : t < 0.3f ? 784f : 1046f;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * (1f - t / 0.35f);
            });
            hitClip = GenerateSynthClip(0.28f, (t) => (UnityEngine.Random.value * 2f - 1f) * (1f - t / 0.28f) * 0.7f + Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.3f);
            clickClip = GenerateSynthClip(0.06f, (t) => Mathf.Sin(2f * Mathf.PI * 1200f * t) * (1f - t / 0.06f));
        }

        private AudioClip GenerateSynthClip(float duration, Func<float, float> sampleFunc)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                samples[i] = Mathf.Clamp(sampleFunc(time), -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("SynthClip", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void PlaySound(AudioClip clip)
        {
            if (Save.soundEnabled && sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, 0.85f);
            }
        }
        #endregion

        private void Update()
        {
            if (State != RunState.Running) return;

            Runner.ReadInput();
            float dt = Time.deltaTime;
            preciseDistance += Speed * dt;
            DistanceMeters = Mathf.FloorToInt(preciseDistance);
            Score = DistanceMeters * Multiplier + RunCoins * 15;

            // FOV Speed Kick (Subway Surfers feel)
            if (gameCamera != null)
            {
                float targetFOV = 60f + (Speed - 8.5f) * 1.5f;
                gameCamera.fieldOfView = Mathf.Lerp(gameCamera.fieldOfView, targetFOV, dt * 3f);
            }

            TickPowerups(dt);
            track.Tick(Speed, dt);

            uiRefreshTimer += dt;
            if (uiRefreshTimer >= 0.08f)
            {
                uiRefreshTimer = 0f;
                ui.Refresh();
            }
        }

        private void TickPowerups(float dt)
        {
            magnetTimer = Mathf.Max(0f, magnetTimer - dt);
            doubleCoinsTimer = Mathf.Max(0f, doubleCoinsTimer - dt);
            multiplierTimer = Mathf.Max(0f, multiplierTimer - dt);
            hoverboardTimer = Mathf.Max(0f, hoverboardTimer - dt);

            if (multiplierTimer <= 0f) Multiplier = 1;

            Runner.SetShieldVisual(shieldReady);
            Runner.SetMagnetVisual(magnetTimer > 0f);
            Runner.SetHoverboard(hoverboardTimer > 0f);

            string active = string.Empty;
            if (hoverboardTimer > 0f) active += "HOVERBOARD (" + Mathf.CeilToInt(hoverboardTimer) + "s)  ";
            if (magnetTimer > 0f) active += "MAGNET (" + Mathf.CeilToInt(magnetTimer) + "s)  ";
            if (shieldReady) active += "SHIELD  ";
            if (doubleCoinsTimer > 0f) active += "2X COINS (" + Mathf.CeilToInt(doubleCoinsTimer) + "s)  ";
            if (multiplierTimer > 0f) active += "2X SCORE (" + Mathf.CeilToInt(multiplierTimer) + "s)";
            ActivePowerText = active.Trim();
        }

        public void StartRun()
        {
            PlaySound(clickClip);
            Runner.transform.position = Vector3.zero;
            Score = 0;
            DistanceMeters = 0;
            preciseDistance = 0f;
            RunCoins = 0;
            magnetTimer = doubleCoinsTimer = multiplierTimer = hoverboardTimer = 0f;
            shieldReady = false;
            Multiplier = 1;

            StartWorld = selectedWorld;
            track.Reset(StartWorld);
            Runner.ApplySkin(Save.selectedSkin);

            State = RunState.Running;
            ui.ShowHud();
            ui.Refresh();
        }

        private int selectedWorld;

        public void SelectWorld(int world)
        {
            PlaySound(clickClip);
            selectedWorld = (world + 10) % 10;
            StartWorld = selectedWorld;
            Save.selectedWorld = selectedWorld;
            Save.Store();
        }

        public void TogglePause()
        {
            PlaySound(clickClip);
            if (State == RunState.Running)
            {
                State = RunState.Paused;
                ui.ShowPause();
            }
            else if (State == RunState.Paused)
            {
                State = RunState.Running;
                ui.ShowHud();
            }
        }

        public void ReturnHome()
        {
            PlaySound(clickClip);
            if (State == RunState.Running || State == RunState.Paused) SaveRun();
            State = RunState.Menu;
            ui.ShowMenu();
        }

        public void Collect(PickupKind kind)
        {
            if (kind == PickupKind.Coin)
            {
                int amount = doubleCoinsTimer > 0f ? 2 : 1;
                RunCoins += amount;
                Save.coins += amount;
                Save.lifetimeCoins += amount;
                Save.missionCoins += amount;
                Save.Store();
                PlaySound(coinClip);
            }
            else if (kind == PickupKind.Magnet)
            {
                magnetTimer = 10f;
                PlaySound(powerupClip);
                ui.ShowMessage("MAGNET ACTIVATED!");
            }
            else if (kind == PickupKind.Shield)
            {
                shieldReady = true;
                PlaySound(powerupClip);
                ui.ShowMessage("SHIELD READY!");
            }
            else if (kind == PickupKind.DoubleCoins)
            {
                doubleCoinsTimer = 14f;
                PlaySound(powerupClip);
                ui.ShowMessage("2X COINS ACTIVE!");
            }
            else if (kind == PickupKind.Multiplier)
            {
                multiplierTimer = 14f;
                Multiplier = 2;
                PlaySound(powerupClip);
                ui.ShowMessage("2X SCORE MULTIPLIER!");
            }
            ui.Refresh();
        }

        public void HitHazard()
        {
            if (hoverboardTimer > 0f) return; // Hoverboard immunity!

            PlaySound(hitClip);
            if (Save.vibrationEnabled) Handheld.Vibrate();

            if (shieldReady)
            {
                shieldReady = false;
                Runner.SetShieldVisual(false);
                ui.ShowMessage("SHIELD SAVED YOU!");
                return;
            }
            EndRun();
        }

        private void EndRun()
        {
            State = RunState.GameOver;
            Save.totalRuns++;
            SaveRun();
            ui.ShowGameOver();

            AdService ads = FindFirstObjectByType<AdService>();
            if (ads != null) ads.TryShowInterstitialAtResults();
        }

        private void SaveRun()
        {
            Save.highScore = Mathf.Max(Save.highScore, Score);
            Save.bestDistance = Mathf.Max(Save.bestDistance, DistanceMeters);
            Save.missionRuns++;

            // Check Achievements
            if (Save.highScore >= 5000) Save.UnlockAchievement(0);
            if (Save.bestDistance >= 1000) Save.UnlockAchievement(1);
            if (Save.lifetimeCoins >= 500) Save.UnlockAchievement(2);

            Save.Store();
        }

        public void BuyOrSelectSkin(int index)
        {
            PlaySound(clickClip);
            index = Mathf.Clamp(index, 0, 5);
            bool owned = (Save.ownedSkins & (1 << index)) != 0;
            int[] prices = { 0, 100, 250, 500, 900, 1500 };

            if (!owned)
            {
                if (Save.coins < prices[index])
                {
                    ui.ShowMessage("Need " + (prices[index] - Save.coins) + " more coins!");
                    return;
                }
                Save.coins -= prices[index];
                Save.ownedSkins |= 1 << index;
            }

            Save.selectedSkin = index;
            Save.Store();
            ui.ApplySkin(index);
            ui.ShowMessage(owned ? "Outfit equipped!" : "Outfit unlocked & equipped!");
        }

        public void ClaimDailyReward()
        {
            PlaySound(clickClip);
            int utcDay = System.DateTime.UtcNow.DayOfYear + System.DateTime.UtcNow.Year * 366;
            if (Save.lastDailyUtcDay == utcDay)
            {
                ui.ShowMessage("Daily reward already claimed today!");
                return;
            }
            Save.lastDailyUtcDay = utcDay;
            Save.coins += 150;
            Save.Store();
            PlaySound(powerupClip);
            ui.ShowMessage("Daily Reward Claimed: +150 Coins!");
        }

        public void ClaimMissionReward()
        {
            PlaySound(clickClip);
            if (Save.missionRewardClaimed || Save.missionCoins < 50)
            {
                ui.ShowMessage(Save.missionRewardClaimed ? "Mission reward already claimed" : "Collect 50 coins in runs to unlock");
                return;
            }
            Save.missionRewardClaimed = true;
            Save.coins += 250;
            Save.Store();
            PlaySound(powerupClip);
            ui.ShowMessage("Mission Complete: +250 Coins!");
        }

        public void GrantRewardedCoins()
        {
            Save.coins += 200;
            Save.Store();
            PlaySound(powerupClip);
            ui.ShowMessage("Ad Reward: +200 Coins Added!");
        }

        public void WatchRewardedAd()
        {
            PlaySound(clickClip);
            AdService ads = FindFirstObjectByType<AdService>();
            if (ads == null)
            {
                ui.ShowMessage("Ad service offline - try again later");
                return;
            }
            ads.ShowRewarded(success =>
            {
                if (success) GrantRewardedCoins();
                else ui.ShowMessage("Ad unavailable - showing bonus +50 coins");
            });
        }

        public void SetSound(bool enabled)
        {
            Save.soundEnabled = enabled;
            if (sfxSource != null) sfxSource.volume = enabled ? 1f : 0f;
            Save.Store();
            PlaySound(clickClip);
        }

        public void SetVibration(bool enabled)
        {
            Save.vibrationEnabled = enabled;
            Save.Store();
            PlaySound(clickClip);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && State == RunState.Running) TogglePause();
        }
    }
}
