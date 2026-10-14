# Obstacle Dodge

A small 3D obstacle course made in Unity for a university Game Development course. You steer a capsule called **Dodgy** across a walled arena, from the start corner to the finish, past static walls and buildings, spinning bars, objects that drop from the sky, and traps that fire projectiles at you.

The game runs on **desktop, Android and iOS**. It has on-screen touch controls and ads through **Unity LevelPlay** (rewarded, interstitial and banner) on both mobile platforms.

![Unity](https://img.shields.io/badge/Unity-6000.6.2f1-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-scripts-239120)
![URP](https://img.shields.io/badge/URP-3D-blue)
![Android](https://img.shields.io/badge/Android-API%2026%2B-3DDC84?logo=android)
![iOS](https://img.shields.io/badge/iOS-15.0%2B-black?logo=apple)
![LevelPlay](https://img.shields.io/badge/Unity%20LevelPlay-9.5.1-black)

---

## Contents

- [How to play](#how-to-play)
- [The level](#the-level)
- [Scripts](#scripts)
- [Project structure](#project-structure)
- [Running in the editor](#running-in-the-editor)
- [Building for Android](#building-for-android)
- [Building for iOS](#building-for-ios)
- [Ads setup (LevelPlay)](#ads-setup-levelplay)
- [Troubleshooting](#troubleshooting)
- [Known limitations](#known-limitations)
- [Credits](#credits)

---

## How to play

| Input                     | Action                                  |
| ------------------------- | --------------------------------------- |
| `W A S D` / arrow keys    | Move                                    |
| On-screen arrows (mobile) | Move. Hold two at once to go diagonally |

- You have **5 lives**. Each obstacle you touch costs one life, then turns black and is "spent", so it can't hurt you again. The counter is in the top-left corner (`Hits: 0 / 5`).
- Reach the green **finish** in the north-east corner to **win**.
- Lose all 5 lives and it's **Game Over**:
  - **Watch ad: +3 lives** shows a rewarded ad, and you keep playing with 3 lives left. This works once per round, and only when an ad has loaded.
  - **Play again** restarts the level. Every 3rd restart shows an interstitial ad first.
- On mobile, a banner ad is shown at the bottom of the screen.

## The level

The arena is split into three zones by two divider walls. The route snakes from the south-west start to the north-east finish.

| Zone        | What's in it                                                                                                             |
| ----------- | ------------------------------------------------------------------------------------------------------------------------ |
| 1. Warm-up  | Three long walls to weave between and a field of buildings of different heights. Nothing moves yet.                      |
| 2. Maze     | Four baffle walls that force a zigzag path, a spinning bar in the middle, and a projectile trap at the entrance.         |
| 3. Gauntlet | Three spinners that get faster and alternate direction, pillars that block the safe lanes, and a second projectile trap. |

Dropping objects are timed to fall along the route roughly when you get there, so standing still doesn't keep you safe.

| Colour     | Meaning                 |
| ---------- | ----------------------- |
| Grey       | Boundary and maze walls |
| Sand       | Free-standing walls     |
| Terracotta | Buildings and pillars   |
| Orange-red | Spinners                |
| Purple     | Dropping objects        |
| Red        | Projectiles             |
| Green glow | Finish                  |

## Scripts

All code lives in [`Assets/scripts`](Assets/scripts).

### Game loop and UI

| Script             | Attached to   | What it does                                                                                                                                                                                                                                                      |
| ------------------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `GameManager.cs`   | `GameManager` | The "brain" of a round. It counts hits, ends the game after `maxHits` (5), handles the win, gives "+3 lives" after a rewarded ad, and restarts the level (with an interstitial every 3rd time). Freezes the game with `Time.timeScale = 0` on Game Over or a win. |
| `GameUI.cs`        | `GameManager` | Updates the TextMeshPro UI: the hit counter, the Game Over panel and the win panel. It shows the **Watch ad** button only when a rewarded ad is ready. `OnWatchAdPressed` and `OnPlayAgainPressed` are hooked to the buttons.                                     |
| `TouchControls.cs` | `GameManager` | Draws four arrow buttons (bottom-left) on phones and tablets, with multi-touch. Exposes `Horizontal` / `Vertical`, which `Mover` reads. Hidden on desktop and on the end screens.                                                                                 |
| `FinishLine.cs`    | `Finish`      | A trigger on the finish pad that tells `GameManager` the player won.                                                                                                                                                                                              |

### Ads

| Script                    | What it does                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AdManager.cs`            | One entry point for all ads (`IsRewardedReady`, `ShowRewarded`, `ShowInterstitial`). Picks the platform with `#if`: **LevelPlay on Android and iOS** (separate keys for each), a WebGL bridge (prepared but not set up), and an **editor stub** that skips ads and gives the reward instantly. On iOS it first asks for tracking permission (ATT). If an ad fails to load (e.g. _No fill_), it **retries every 15 s**. A **Show Ad Status** overlay prints the SDK state in the top-right corner. |
| `Editor/IosBuildFixes.cs` | Runs automatically on every iOS build. It adds the text for the tracking popup to `Info.plist`, and sets the iOS deployment target of all CocoaPods to 15.0. Without that, the ad SDK pods fail to build in recent Xcode versions.                                                                                                                                                                                                                                                                |

### Obstacles and player

| Script                 | Attached to      | What it does                                                                                                                                                                            |
| ---------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Mover.cs`             | Player           | Moves the player using keyboard input plus the touch arrows.                                                                                                                            |
| `ObjectHit.cs`         | Every obstacle   | On first contact with the player it reports a hit to `GameManager`, turns black and is tagged `Hit` so it's only counted once. Walls and the ground don't have it, so they don't count. |
| `Spinner.cs`           | Spinning bars    | Rotates the object every frame by the X/Y/Z angles set in the Inspector.                                                                                                                |
| `Dropper.cs`           | Dropping objects | Invisible and gravity-free until `timeToWait` seconds into the round, then falls. Uses `Time.timeSinceLevelLoad`, so it also works after a restart.                                     |
| `FlyAtPlayer.cs`       | Projectiles      | Starts disabled. When enabled, it flies to where the player was standing at that moment and destroys itself on arrival.                                                                 |
| `TriggerProjectile.cs` | Trigger volumes  | An invisible zone that wakes up its 5 projectiles when the player walks in, then destroys itself so it fires only once.                                                                 |

### How a round works

1. `Mover` moves the player (keyboard or touch arrows).
2. The player hits an obstacle → `ObjectHit` calls `GameManager.RegisterHit()` and turns the obstacle black.
3. `GameUI` updates the counter. On the 5th hit, `GameManager` ends the round and the Game Over panel appears.
4. **Watch ad** → `AdManager.ShowRewarded(...)` → after the ad, `GameManager.ContinueAfterReward()` sets hits to 2 / 5 and unfreezes the game.
5. **Play again** → `GameManager.Restart()` → every 3rd time `AdManager.ShowInterstitial(...)` first → the scene reloads.
6. Touch the finish → `FinishLine` → `GameManager.Win()` → the win panel appears.

## Project structure

```
Assets/
├── Scenes/            MainScene.unity — the level
├── scripts/           gameplay, UI and ad scripts
│   └── Editor/        IosBuildFixes.cs (iOS post-build step)
├── prefabs/           Wall, Small building Variant, SpinningThing, DroppingObject
├── materials/         one material per obstacle type
├── LevelPlay/         LevelPlay (Ads Mediation) settings
├── Plugins/           Android Gradle templates used by the ad SDK
└── Settings/          URP render pipeline and post-processing settings
```

Packages: **Ads Mediation** (`com.unity.services.levelplay` 9.5.1) and **iOS 14 Advertising Support** (`com.unity.ads.ios-support`).

## Running in the editor

1. Install **Unity 6000.6.2f1** (any Unity 6 version should work) through Unity Hub.
2. Clone the repository:
   ```bash
   git clone https://github.com/abdurakhmanovjohn/obstacle_game.git
   ```
3. In Unity Hub: **Add → Add project from disk**, then pick the folder. The first import takes a few minutes.
4. Open `Assets/Scenes/MainScene` and press **Play**.

No real ads are shown in the editor. The `AdManager` stub gives the "+3 lives" reward instantly and skips interstitials, so the whole loop can be tested. To try the touch arrows with a mouse, tick **Show On PC** on `GameManager → TouchControls`.

## Building for Android

**Requirement:** Unity Hub → Installs → _Manage_ → **Add modules** → **Android Build Support** (with OpenJDK and Android SDK & NDK Tools).

1. **File → Build Profiles → Android → Switch Profile.**
2. **Project Settings → Player → Android:**
   - Package Name `com.abdurakhmanovjohn.obstacledodge`. It must match the LevelPlay app.
   - Scripting Backend **IL2CPP**, target **ARM64**, Internet Access **Require**.
   - Default Orientation **Landscape Left**.
   - Active Input Handling **Both**. The scripts use the old Input Manager, and the UI uses the Input System UI module.
3. **Assets → External Dependency Manager → Android Resolver → Force Resolve.** This downloads the native ad SDK.
4. **Build**, then install the `.apk` on the device. Or use **Build And Run** with USB debugging on.

## Building for iOS

**Requirements:** a Mac with **Xcode**, the **iOS Build Support** module in Unity Hub, **CocoaPods** (`brew install cocoapods`), and an Apple ID. A free account is enough for your own device.

1. **File → Build Profiles → iOS → Switch Profile.**
2. **Project Settings → Player → iOS:** Bundle Identifier `com.abdurakhmanovjohn.obstacledodge`, Target minimum iOS Version **15.0**, Default Orientation **Landscape Left**.
3. **Build** into an **empty folder**. Unity generates an Xcode project and runs `pod install`. If `pod install` fails, run it yourself in the build folder.
4. Open **`Unity-iPhone.xcworkspace`**, not the `.xcodeproj`.
5. **Unity-iPhone target → Signing & Capabilities:** tick _Automatically manage signing_ and choose your **Personal Team**.
6. Connect the iPhone, turn on **Developer Mode** on it, select it in Xcode, and press **▶**.
7. On first launch, trust the developer under _Settings → General → VPN & Device Management_, and tap **Allow** in the tracking popup.

With a free Apple ID, the app stays installed for 7 days and can only be installed from Xcode. It can't be shared like an APK.

## Ads setup (LevelPlay)

The [LevelPlay dashboard](https://platform.ironsrc.com) has **two apps**, one for Android and one for iOS, each with its own App Key and three ad units: **Rewarded**, **Interstitial** and **Banner**.

1. Paste the keys into the `AdManager` object, in the **LevelPlay - Android** and **LevelPlay - iOS** sections. The right set is chosen automatically per platform.
2. Register test devices under **Setup → Test devices**, with network _ironSource Ads_:
   - Android: the device's **Advertising ID** (Settings → Google → Ads).
   - iOS: the **IDFA**, printed in the Xcode console as `[Ads] IDFA: ...`. It's only available after tapping _Allow_ in the tracking popup.
3. A brand-new app gets little or no real ad fill (`509 Mediation No fill`). Test devices and adding **Unity Ads** as a second network help with that.
4. Before release, untick **Show Ad Status** on `AdManager` and remove the `LevelPlay.SetAdaptersDebug(true)` line.

## Troubleshooting

| Problem                                                    | Cause and fix                                                                                                                                                                             |
| ---------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Overlay stuck on `init: starting` / "no answer after 20s"  | The device can't reach `i-sdk.mediation.unity3d.com`. Check with **Android Logcat** (filter `LevelPlay`). An **ad-blocking Private DNS** on the device was blocking it here. Turn it off. |
| `load failed ... 509 No fill`                              | The SDK works, but there's no ad available. Register a **test device**. The game retries every 15 s automatically.                                                                        |
| No "Watch ad" button                                       | It only appears when a rewarded ad has actually loaded. Check the overlay.                                                                                                                |
| UI buttons don't react on Android                          | The EventSystem input module doesn't match Active Input Handling. Use **Both** + _Input System UI Input Module_.                                                                          |
| Gradle: "Could not resolve all files"                      | Network or ad blocker on the Mac blocked the library download. Turn it off and **Force Resolve** again.                                                                                   |
| `INSTALL_FAILED_USER_RESTRICTED` (Xiaomi)                  | Turn on **Install via USB** in Developer options, or install the APK by hand.                                                                                                             |
| "Blocked by Play Protect"                                  | Normal for self-built APKs. Tap _More details → Install anyway_.                                                                                                                          |
| iOS: no `.xcworkspace` / `getcwd: Operation not permitted` | `pod install` didn't run. Give Terminal access to the Desktop folder (_Privacy & Security → Files & Folders_), then run `pod install`.                                                    |
| Xcode: "IPHONEOS_DEPLOYMENT_TARGET is set to 10.0…"        | Old pod deployment targets. Handled by `IosBuildFixes.cs`. If you build with _Append_, check that the Podfile has the `post_install` block.                                               |
| Unity iOS build: `IOException: Directory not empty`        | Xcode or Terminal still had the old build folder open. Close them, or build into a new folder.                                                                                            |

## Known limitations

- Movement uses `transform.Translate`, so the player can jitter against walls.
- `Spinner` doesn't use `Time.deltaTime`, so the spin speed depends on frame rate.
- `TriggerProjectile` uses five separate fields instead of an array.
- Active Input Handling is set to **Both**, which Unity warns about on Android. It works, but a full move to the new Input System would be cleaner.
- The WebGL ad route (Monetag / Adsterra) is prepared in `AdManager` but not set up.
- The game isn't published on Google Play or the App Store, so it mostly gets test ads.

## Credits

Made by Johnibek Abdurakhmanov for a university Game Development course. Based on the course guides _Building Obstacle Dodge From Scratch_ and _Ads in Obstacle Dodge_.
