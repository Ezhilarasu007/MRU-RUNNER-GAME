using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MRU.InfiniteRunner
{
    public sealed class RuntimeUI
    {
        private readonly GameManager game;
        private readonly Canvas canvas;
        private readonly GameObject menu, hud, pause, results, shop, settings, worlds, missions;
        private readonly Text hudScore, hudCoins, hudWorld, hudPower, resultScore, resultHigh, resultCoins;
        private readonly Text skinInfo, worldInfo, settingsInfo, missionInfo, toast;
        private int shopSkin;

        private static readonly Color CyberDark = new Color(0.04f, 0.07f, 0.14f, 0.96f);
        private static readonly Color NeonBlue  = new Color(0.06f, 0.45f, 0.95f);
        private static readonly Color CyberGold = new Color(1.00f, 0.75f, 0.10f);
        private static readonly Color CyberCyan = new Color(0.05f, 0.92f, 0.95f);
        private static readonly Color CyberGreen= new Color(0.10f, 0.85f, 0.35f);
        private static readonly Color CardBg    = new Color(0.08f, 0.13f, 0.24f, 0.90f);

        public RuntimeUI(GameManager owner)
        {
            game = owner;

            // Root Canvas
            GameObject canvasObject = new GameObject("MRU Cyber Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            // Ensure EventSystem exists
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventObject = new GameObject("Event System", typeof(EventSystem), typeof(StandaloneInputModule));
                Object.DontDestroyOnLoad(eventObject);
            }

            // 1. MAIN MENU PANEL
            menu = Panel("Main Menu Panel");
            AddText(menu.transform, "MRU RUNNER", 92, new Vector2(0f, 660f), CyberGold);
            AddText(menu.transform, "INFINITE CYBER RUNNER 3D", 38, new Vector2(0f, 575f), Color.white);
            AddText(menu.transform, "SWIPE TO RUN  |  JUMP  |  SLIDE  |  COLLECT", 28, new Vector2(0f, 510f), CyberCyan);

            AddButton(menu.transform, "START RUN", 330f, 125f, CyberGold, game.StartRun);
            AddButton(menu.transform, "SELECT WORLD (10 WORLDS)", 180f, 115f, NeonBlue, ShowWorlds);
            AddButton(menu.transform, "OUTFIT SHOP", 40f, 115f, NeonBlue, ShowShop);
            AddButton(menu.transform, "MISSIONS & DAILY REWARDS", -100f, 115f, CyberGreen, ShowMissions);
            AddButton(menu.transform, "SETTINGS", -240f, 115f, NeonBlue, ShowSettings);
            AddText(menu.transform, "v1.0.0.0  |  High Score: " + game.HighScore, 28, new Vector2(0f, -690f), new Color(0.70f, 0.78f, 0.90f));

            // 2. HUD PANEL
            hud = Panel("HUD Panel");
            hudScore = AddText(hud.transform, "SCORE  0     BEST  0", 38, new Vector2(0f, 820f), Color.white);
            hudCoins = AddText(hud.transform, "COINS  0", 34, new Vector2(-310f, 735f), CyberGold);
            hudWorld = AddText(hud.transform, "ANCIENT TEMPLE", 28, new Vector2(0f, 735f), CyberCyan);
            hudPower = AddText(hud.transform, "", 28, new Vector2(0f, -720f), CyberCyan);
            AddButton(hud.transform, "||", 820f, 110f, NeonBlue, game.TogglePause, 120f, 420f);

            // 3. PAUSE PANEL
            pause = Panel("Pause Panel");
            AddText(pause.transform, "GAME PAUSED", 68, new Vector2(0f, 420f), Color.white);
            AddButton(pause.transform, "RESUME", 190f, 120f, CyberGold, game.TogglePause);
            AddButton(pause.transform, "RESTART RUN", 40f, 120f, CyberGreen, game.StartRun);
            AddButton(pause.transform, "MAIN MENU", -110f, 120f, NeonBlue, game.ReturnHome);

            // 4. RESULTS / GAME OVER PANEL
            results = Panel("Results Panel");
            AddText(results.transform, "RUN OVER!", 72, new Vector2(0f, 490f), CyberGold);
            resultScore = AddText(results.transform, "SCORE  0", 44, new Vector2(0f, 375f), Color.white);
            resultHigh = AddText(results.transform, "HIGH SCORE  0", 34, new Vector2(0f, 290f), CyberCyan);
            resultCoins = AddText(results.transform, "COINS COLLECTED  +0", 32, new Vector2(0f, 215f), CyberGold);
            AddButton(results.transform, "PLAY AGAIN", 70f, 120f, CyberGreen, game.StartRun);
            AddButton(results.transform, "WATCH AD  |  +200 COINS", -70f, 120f, NeonBlue, game.WatchRewardedAd);
            AddButton(results.transform, "MAIN MENU", -210f, 120f, NeonBlue, game.ReturnHome);

            // 5. CHARACTER SHOP PANEL
            shop = Panel("Shop Panel");
            AddText(shop.transform, "OUTFIT SHOP", 64, new Vector2(0f, 560f), CyberGold);
            skinInfo = AddText(shop.transform, "", 32, new Vector2(0f, 360f), Color.white);
            AddButton(shop.transform, "< PREVIOUS", 230f, 115f, NeonBlue, () => ChangeSkin(-1), 320f);
            AddButton(shop.transform, "EQUIP / UNLOCK", 95f, 115f, CyberGold, () => game.BuyOrSelectSkin(shopSkin));
            AddButton(shop.transform, "NEXT >", -40f, 115f, NeonBlue, () => ChangeSkin(1), 320f);
            AddButton(shop.transform, "BACK TO MENU", -260f, 115f, NeonBlue, ShowMenu);

            // 6. SETTINGS PANEL
            settings = Panel("Settings Panel");
            AddText(settings.transform, "GAME SETTINGS", 64, new Vector2(0f, 480f), CyberGold);
            settingsInfo = AddText(settings.transform, "", 32, new Vector2(0f, 310f), Color.white);
            AddButton(settings.transform, "TOGGLE SOUND FX", 120f, 115f, NeonBlue, () => game.SetSound(!game.Save.soundEnabled));
            AddButton(settings.transform, "TOGGLE VIBRATION", -20f, 115f, NeonBlue, () => game.SetVibration(!game.Save.vibrationEnabled));
            AddButton(settings.transform, "PRIVACY POLICY & TERMS", -160f, 115f, NeonBlue,
                () => ShowMessage("Privacy Policy: www.mruinfiniterunner.com/privacy-policy.html"));
            AddButton(settings.transform, "BACK TO MENU", -310f, 115f, NeonBlue, ShowMenu);

            // 7. WORLD SELECT PANEL
            worlds = Panel("World Select Panel");
            AddText(worlds.transform, "WORLD SELECT", 64, new Vector2(0f, 520f), CyberGold);
            worldInfo = AddText(worlds.transform, "", 34, new Vector2(0f, 280f), Color.white);
            AddButton(worlds.transform, "< PREVIOUS WORLD", 65f, 115f, NeonBlue, () => ChangeWorld(-1), 340f);
            AddButton(worlds.transform, "NEXT WORLD >", -75f, 115f, NeonBlue, () => ChangeWorld(1), 340f);
            AddButton(worlds.transform, "START RUN IN THIS WORLD", -225f, 120f, CyberGold, game.StartRun);
            AddButton(worlds.transform, "BACK TO MENU", -365f, 115f, NeonBlue, ShowMenu);

            // 8. MISSIONS PANEL
            missions = Panel("Missions Panel");
            AddText(missions.transform, "MISSIONS & REWARDS", 60, new Vector2(0f, 500f), CyberGold);
            missionInfo = AddText(missions.transform, "", 32, new Vector2(0f, 270f), Color.white);
            AddButton(missions.transform, "CLAIM DAILY REWARD  (+150 COINS)", 35f, 115f, CyberGold, game.ClaimDailyReward);
            AddButton(missions.transform, "CLAIM MISSION REWARD  (+250 COINS)", -105f, 115f, CyberGreen, game.ClaimMissionReward);
            AddButton(missions.transform, "BACK TO MENU", -245f, 115f, NeonBlue, ShowMenu);

            // 9. TOAST NOTIFICATION
            toast = AddText(canvas.transform, "", 30, new Vector2(0f, -860f), CyberGold);

            ShowOnly(menu);
            ApplySkin(game.Save.selectedSkin);
            Refresh();
        }

        public void ShowMenu() { ShowOnly(menu); Refresh(); }
        public void ShowHud() { ShowOnly(hud); Refresh(); }
        public void ShowPause() { ShowOnly(pause); }

        public void ShowGameOver()
        {
            resultScore.text = "SCORE  " + game.Score;
            resultHigh.text = "HIGH SCORE  " + game.HighScore;
            resultCoins.text = "COINS EARNED  +" + game.RunCoins;
            ShowOnly(results);
        }

        public void ShowShop() { UpdateSkin(); ShowOnly(shop); }
        public void ShowSettings() { UpdateSettings(); ShowOnly(settings); }
        public void ShowWorlds() { UpdateWorld(); ShowOnly(worlds); }
        public void ShowMissions() { UpdateMission(); ShowOnly(missions); }

        public void ShowMessage(string message)
        {
            if (toast != null) toast.text = message;
        }

        public void ApplySkin(int index)
        {
            game.Runner.ApplySkin(index);
            UpdateSkin();
        }

        public void Refresh()
        {
            if (hudScore == null) return;
            hudScore.text = "SCORE  " + game.Score + "     BEST  " + game.HighScore;
            hudCoins.text = "COINS  " + game.Save.coins;
            hudWorld.text = game.CurrentWorldName.ToUpperInvariant() + "  (" + game.DistanceMeters + "m)";
            hudPower.text = game.ActivePowerText;

            if (shop.activeSelf) UpdateSkin();
            if (settings.activeSelf) UpdateSettings();
            if (worlds.activeSelf) UpdateWorld();
            if (missions.activeSelf) UpdateMission();
        }

        private void UpdateSkin()
        {
            if (skinInfo == null) return;
            string[] names = { "MRU CLASSIC (RED)", "BLUE COMET", "JUNGLE SCOUT", "NIGHT SPARK", "DESERT DASH", "NEON DIVER" };
            int[] cost = { 0, 100, 250, 500, 900, 1500 };
            bool owned = (game.Save.ownedSkins & (1 << shopSkin)) != 0;

            skinInfo.text = names[shopSkin] + "\n\nSTATUS: " + (owned ? "UNLOCKED" : cost[shopSkin] + " COINS") +
                "\nYOUR COINS: " + game.Save.coins + "\nOUTFIT " + (shopSkin + 1) + " / " + names.Length;
        }

        private void ChangeSkin(int amount)
        {
            shopSkin = (shopSkin + amount + 6) % 6;
            UpdateSkin();
        }

        private void UpdateSettings()
        {
            settingsInfo.text = "Sound Effects: " + (game.Save.soundEnabled ? "ENABLED" : "MUTED") +
                "\nVibration Haptics: " + (game.Save.vibrationEnabled ? "ENABLED" : "DISABLED") +
                "\n\nOffline Mode Ready. Web & AdMob Enabled.";
        }

        private void UpdateWorld()
        {
            string[] names =
            {
                "Ancient Temple", "Jungle Adventure", "Sky Kingdom", "Desert Adventure", "Snow Mountain",
                "Volcano", "Futuristic Neon City", "Alien Planet", "Underwater Kingdom", "Space Station"
            };
            worldInfo.text = names[game.SelectedWorld] + "\n\nENVIRONMENT " + (game.SelectedWorld + 1) + " / 10" +
                "\n\n" + game.CurrentWorldDesc + "\n(Worlds transition dynamically every 300 meters!)";
        }

        private void ChangeWorld(int amount)
        {
            game.SelectWorld(game.SelectedWorld + amount);
            UpdateWorld();
        }

        private void UpdateMission()
        {
            int progress = Mathf.Min(game.Save.missionCoins, 50);
            missionInfo.text = "Coin Collector Mission\nProgress: " + progress + " / 50 Coins\n\nTotal Coins: " + game.Save.coins +
                "\nMission Reward: " + (game.Save.missionRewardClaimed ? "CLAIMED" : "250 COINS") +
                "\nBest Distance: " + game.Save.bestDistance + " m | Total Runs: " + game.Save.totalRuns;
        }

        private GameObject Panel(string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            Image image = panel.GetComponent<Image>();
            image.color = CyberDark;
            return panel;
        }

        private static Text AddText(Transform parent, string value, int size, Vector2 position, Color color)
        {
            GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1000f, 150f); rect.anchoredPosition = position;

            Text text = obj.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void AddButton(Transform parent, string label, float y, float height, Color color,
            UnityAction action, float width = 740f, float x = 0f)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = new Vector2(x, y);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            Text text = AddText(buttonObject.transform, label, 30, Vector2.zero, Color.white);
            text.rectTransform.sizeDelta = new Vector2(width - 30f, height - 8f);
        }

        private static void ShowOnly(GameObject visible)
        {
            visible.SetActive(true);
            Transform root = visible.transform.parent;
            for (int i = 0; i < root.childCount; i++)
            {
                GameObject child = root.GetChild(i).gameObject;
                if (child != visible && child.GetComponent<Image>() != null && child.GetComponent<RectTransform>().anchorMin == Vector2.zero)
                    child.SetActive(false);
            }
        }
    }
}
