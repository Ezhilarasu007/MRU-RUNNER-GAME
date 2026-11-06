# MRU Infinite Runner 3D — Super Max Edition

Welcome to **MRU Infinite Runner 3D**! This is a complete, production-ready, portrait 3D endless runner scaffold engineered with Unity 6 (URP) and web technology.

---

## 🌟 Key Features & Competitive Enhancements

### 1. 3D Procedural Runner Gameplay & Audio Synthesis
- **10 Dynamic World Themes:** Transitions automatically every 300m with custom procedural visual landmarks (Ancient Temple, Jungle Adventure, Sky Kingdom, Desert, Snow Mountain, Volcano, Cyber Neon City, Alien Planet, Underwater Kingdom, Space Station).
- **Procedural Audio Synthesizer:** Built-in runtime AudioSynthesizer in C# (`GameManager.cs`) generating real sound effects for Jumping, Coin Pickups, Powerups, Hit Collisions, and Button clicks without requiring external audio assets!
- **Smooth Physics & Visual Polish:** Lateral bank tilt on lane switches, jump squish & stretch spring dynamics, crouching slide, shield aura forcefields, magnet suction particle effects, and rotating coin animations.
- **4 Temporary Powerups:** Coin Magnet, Invincibility Shield, 2X Coin Boost, and 2X Score Multiplier.
- **6 Unlockable Outfits:** Character shop with outfit skin selector and local coin economy.
- **Missions & Daily Rewards:** Daily reward streak (+150 Coins) and Coin Collector challenge (+250 Coins).

### 2. Modern HTML5 / WebGL Web Portal & Live Browser Game
- **Interactive Web Landing Page (`index.html` & `www/index.html`):** A responsive cyber-styled portal for `www.mruinfiniterunner.com` containing a live playable HTML5 Canvas runner mini-game!
- **Google Play Submission Pages:**
  - `privacy-policy.html` (Complete, production-grade Privacy Policy compliant with GDPR, CCPA, and UMP disclosures).
  - `terms.html` (Formal Terms of Service for store compliance).
  - `play-store-listing.html` (Google Play Store Listing preview page with ratings, screenshots carousel, and release copy).

### 3. Automated Packaging & Build System
- **Editor Setup Command:** Top menu `MRU Runner -> 1. Generate Main Scene & App Settings` configures scene, camera, player settings, and package identifiers (`com.mru.infiniterunner`).
- **Android APK & AAB Builders:** Top menu items `2. Build Android APK (.apk)` and `3. Build Android App Bundle (.aab)`.
- **Zip Packager Script (`build_package.py`):** Automatically zips the web portal and full project source code into `Build/Release_Packages/`.

---

## 🚀 How to Build Android APK & AAB

### Method A: Direct Unity Editor Build (One-Click)
1. Install **Unity 6 (6000.0.35f1)** or newer with **Android Build Support** (Android SDK & NDK Tools + OpenJDK).
2. Open this folder in Unity.
3. From the menu bar, run:
   - `MRU Runner` -> `1. Generate Main Scene & App Settings`
   - `MRU Runner` -> `2. Build Android APK (.apk)` for a direct `.apk` binary.
   - `MRU Runner` -> `3. Build Android App Bundle (.aab)` for Google Play Console submission.
4. Output binaries will be located in `Build/Android/`.

### Method B: Command-Line Batchmode Build
Run Unity in batchmode via terminal:
```bash
Unity.exe -batchmode -quit -projectPath "D:/MRU RUNNER GAME 1.0.0.0" -executeMethod MRU.InfiniteRunner.Editor.ProjectSetup.BuildAndroidAppBundle
```

---

## 📱 Google Play Console Submission Checklist

1. **App Identity:**
   - Package ID: `com.mru.infiniterunner`
   - App Name: `MRU Infinite Runner 3D`
   - Target API: Current Android API level (configured in Unity Player Settings).
2. **App Signing:**
   - Generate your upload keystore in Unity (*Project Settings > Player > Android > Publishing Settings*).
   - Enroll in Google Play App Signing and upload `MRU-Infinite-Runner-v1.0.0.aab`.
3. **Web Policy & Contact:**
   - Privacy Policy URL: `https://www.mruinfiniterunner.com/privacy-policy.html`
   - Terms of Service: `https://www.mruinfiniterunner.com/terms.html`
4. **AdMob Setup:**
   - Configure your production Ad Unit IDs in `Assets/Resources/MRUAdConfiguration.asset`.

---

## 📦 Zip Bundles

Pre-built zip archives are located in `Build/Release_Packages/`:
- `MRU_Runner_Web_Portal_v1.0.0.zip` (Web landing page & Play Store preview files)
- `MRU_Runner_Full_Project_v1.0.0.zip` (Complete C# source code and project scaffold)

© 2025 MRU Games Studio. All rights reserved.
