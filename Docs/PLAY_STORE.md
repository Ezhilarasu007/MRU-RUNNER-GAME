# Google Play release packet

## Listing copy

**App name:** MRU Infinite Runner  
**Short description (draft):** Run, jump, slide, and collect coins across ten ever-changing worlds in an endless 3D runner.

**Full description (draft):**  
Take on an endless run through ten changing worlds. Swipe to switch lanes, jump over hazards, slide beneath barriers, and collect coins as the track speeds up. Use temporary power-ups, unlock outfits, and set a new high score.  

Features:
- Three-lane endless runner with touch and keyboard controls.
- Ten color-themed worlds that rotate automatically during a run.
- Locally saved high scores, coins, settings, and outfit unlocks.
- Magnet, shield, double-coin, and score-multiplier pickups.
- Offline gameplay; optional advertising only when separately configured.

Gameplay, graphics, device compatibility, and every listed feature must be verified in the final build before publishing this copy.

## Assets and declarations — not yet supplied

- App icon: deliver original 512×512 PNG compliant with current Play icon requirements.
- Feature graphic: deliver original 1024×500 PNG/JPEG.
- Phone screenshots: capture genuine final-build gameplay/UI on supported device sizes; do not use concept mockups or generated claims as screenshots.
- Promo video: optional; must show actual gameplay.
- App category, support email, developer identity, website, and public privacy-policy URL: developer must supply.
- Ads declaration: answer **contains ads** only if advertising SDK/ads are present in the submitted binary; the source hooks are disabled without the SDK.
- Data Safety: complete from the exact SDK versions and network behavior in the release build. Source-only build has local PlayerPrefs saves and no analytics/backend. An AdMob-enabled build may share advertising/device and diagnostic data with Google; use current SDK disclosures and Play Console's current questionnaire.
- Content rating: complete the IARC questionnaire honestly for the release. The source has stylized obstacle collisions and ads when enabled; do not pre-claim a rating or child-directed status.
- Target audience, Families policy, account deletion, and data collection answers: require developer review; there is no account system in this source.
- Target API: select the current Google Play required API at submission time, rather than relying on an outdated fixed value.

## Release checklist

1. Import and verify official Google Mobile Ads and UMP SDKs (if used); test Google's Android test ad units only during development. Set production IDs only after account setup and consent review. Native ads are not implemented.
2. Supply developer contact and host `PRIVACY_POLICY.html` at a stable public URL. Replace every bracketed draft field. Obtain legal review for privacy and terms.
3. Create and test the URP asset, quality tiers, scenes, safe areas, art, sound, touch UX, background/resume behavior, and performance on low-end and current Android devices.
4. Set final app ID/version/current target API, permissions, orientation, icon, feature graphic, screenshots, store copy, ads declaration, Data Safety answers, and content rating.
5. Configure a protected upload keystore outside source control, enable Play App Signing, build a release AAB, and verify signature, install/update path, and Play pre-launch report.
6. Publish staged testing first. Keep a record of tested SDK versions and the exact binary submitted.

**Release notes (draft):** Initial release: endless three-lane running, world themes, unlockable outfits, pickups, local progress, and settings.

## Play App Signing

Create a new upload key in Unity's Android Publishing Settings (or Android Studio), back up the keystore and credentials in a secure password manager, and never commit them. Enable Play App Signing in Play Console and enroll with the upload certificate. Upload the signed AAB; Google signs installable distribution APKs with the app-signing key. If the upload key is lost, follow Play Console's reset process. Verify the app-signing and upload certificate fingerprints in Play Console.
