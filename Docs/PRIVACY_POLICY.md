# Privacy Policy — MRU Infinite Runner

**Status:** Draft for review and hosting. This policy has not been published. Replace the bracketed contact field and confirm every SDK and Play Console declaration against the exact release build before publishing.

**Effective date:** [enter publication date]  
**Developer / contact:** [developer name and privacy contact email — required before release]

## Information handled by the game

The current source project does not contain an account system, analytics SDK, or remote game backend. Game progress (coin balance, best score, selected outfit, mission progress, and settings) is saved locally on the device using Unity `PlayerPrefs`. The game does not upload this save data in the source configuration.

Google Mobile Ads and the User Messaging Platform are optional integrations and are disabled unless the official SDKs are added and the documented scripting symbols are enabled. If included in a release, Google Mobile Ads may collect or receive device identifiers (including advertising identifiers where available), IP address, ad interaction and diagnostic data, and other information described in Google's current documentation. Google may use data for ad delivery, measurement, fraud prevention, and related services. The game may show interstitial and rewarded ads; native ads are not implemented in this source project.

When UMP is installed and configured, it is used to request/update consent and present a consent form where required. Consent availability and ad requests depend on the user's region, choices, SDK configuration, and network availability. Ads must not be requested where consent or applicable law does not permit them. Offline gameplay remains available without an ad connection.

## Sharing, retention, and choices

Without optional third-party SDKs, the described gameplay save stays on the device and can generally be removed by clearing the app's storage or uninstalling the app. If the Google advertising SDK is included, information may be transmitted to Google and its service providers as described in Google's privacy disclosures and the developer's configured consent messages. Google's retention periods and user controls are governed by Google's policies; the developer does not control Google's independent retention.

You can change applicable ad-consent choices through the in-app consent options when configured and through Android privacy/ad settings. You can disable sound in game settings. Contact the developer at the address above for privacy questions or requests; a valid contact must be supplied before publication.

## Children and changes

The intended audience and age rating have not yet been determined. Do not target children or select a Families designation without completing the relevant legal and Play Console review. The developer must update this notice whenever the release SDKs, data flows, or practices change.

## Third-party documents

For an AdMob-enabled release, review Google's current [Privacy & Terms](https://policies.google.com/privacy) and the official Google Mobile Ads / UMP data disclosures. This draft is not legal advice.
