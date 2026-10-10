# Obstacle Dodge

A small 3D obstacle course made in Unity for a university Game Development course. You control a capsule called **Dodgy** and try to cross a walled arena from the start corner to the finish without bumping into anything: static walls and buildings, spinning bars, objects that drop from the sky, and traps that fire projectiles at you.

The game runs on desktop and on Android, with on-screen touch controls and ads through **Unity LevelPlay** (rewarded, interstitial and banner).

![Unity](https://img.shields.io/badge/Unity-6000.6.2f1-black?logo=unity)
![Language](https://img.shields.io/badge/C%23-scripts-239120)
![Render pipeline](https://img.shields.io/badge/URP-3D-blue)
![Platform](https://img.shields.io/badge/Android-API%2026%2B-3DDC84?logo=android)
![Ads](https://img.shields.io/badge/Unity%20LevelPlay-9.5.1-black)

## How to play

| Input                         | Action |
| ----------------------------- | ------ |
| `W A S D` or arrow keys       | Move   |
| On-screen arrows (Android)    | Move — hold two at once to go diagonally |

- You have **5 lives**. Every obstacle you touch costs one life, turns black and is "spent", so it can't hurt you again. The count is shown in the top-left corner (`Hits: 0 / 5`).
- Reach the green **finish** in the north-east corner to win.
- Lose all 5 lives and it's **Game Over**. You can then:
  - **Watch ad: +3 lives**: watch a rewarded ad and keep playing with 3 lives left. Once per round, and only when an ad has loaded.
  - **Play again**: restart the level. Every 3rd restart shows an interstitial ad first.
- On Android a banner ad is shown at the bottom of the screen.

## The level

The arena is split into three zones by two divider walls, and the route snakes through them from the south-west start to the north-east finish.

| Zone        | What's in it                                                                                                                                    |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| 1. Warm-up  | Three long walls to weave between and a field of buildings of different heights. Nothing moves yet.                                             |
| 2. Maze     | Four baffle walls that force a zigzag path, a spinning bar in the middle, and a projectile trap at the entrance.                                |
| 3. Gauntlet | Three spinners in a row that get faster and alternate direction, pillars blocking the safe lanes, and a second projectile trap at the entrance. |

Dropping objects are timed to fall along the route roughly when you get there, so standing still doesn't keep you safe.

Each obstacle type has its own colour, so you can read the level at a glance:

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

All gameplay code lives in [`Assets/scripts`](Assets/scripts).

### Game loop and UI

| Script            | Attached to   | What it does |
| ----------------- | ------------- | ------------ |
| `GameManager.cs`  | `GameManager` | The "brain" of a round: counts hits, ends the game after `maxHits` (5), handles winning, the "+3 lives" continue after a rewarded ad, and restarts (with an interstitial every 3rd restart). Freezes the game with `Time.timeScale = 0` on Game Over / win. |
| `GameUI.cs`       | `GameManager` | Updates the TextMeshPro UI: the hit counter, the Game Over panel, the win panel, and shows the **Watch ad** button only when a rewarded ad is ready. Its `OnWatchAdPressed` / `OnPlayAgainPressed` methods are hooked to the buttons. |
| `TouchControls.cs`| `GameManager` | Draws four arrow buttons (bottom-left) on phones and tablets and exposes `Horizontal` / `Vertical` values that `Mover` reads. Supports multi-touch; hidden on desktop and on the end screens. |
| `FinishLine.cs`   | `Finish`      | A trigger on the finish pad that tells `GameManager` the player won. |

### Ads

| Script         | Attached to | What it does |
| -------------- | ----------- | ------------ |
| `AdManager.cs` | `AdManager` | One entry point for all ads (`IsRewardedReady`, `ShowRewarded`, `ShowInterstitial`). Uses `#if` directives to pick the platform: **Unity LevelPlay** on Android, a JavaScript bridge for WebGL, and a **stub** in the editor/PC that skips ads and gives the reward instantly, so the whole game loop can be tested without real ads. Survives scene reloads (`DontDestroyOnLoad`). Has a **Show Ad Status** debug overlay that prints the SDK state in the top-right corner. |

### Obstacles and player

| Script                 | Attached to      | What it does |
| ---------------------- | ---------------- | ------------ |
| `Mover.cs`             | Player           | Moves the player with keyboard input plus the touch arrows from `TouchControls`. |
| `ObjectHit.cs`         | Every obstacle   | On the first touch by the player, reports a hit to `GameManager`, turns the obstacle black and tags it `Hit` so it isn't counted twice. Only real obstacles count — the ground and walls without this script don't. |
| `Spinner.cs`           | Spinning bars    | Rotates the object every frame by the X/Y/Z angles set in the Inspector. |
| `Dropper.cs`           | Dropping objects | Keeps the object invisible and gravity-free until `timeToWait` seconds into the round, then lets it fall. Uses `Time.timeSinceLevelLoad`, so it also works after a restart. |
| `FlyAtPlayer.cs`       | Projectiles      | Starts disabled. When enabled, flies to where the player was standing at that moment and destroys itself on arrival. |
| `TriggerProjectile.cs` | Trigger volumes  | An invisible zone that wakes up its 5 projectiles when the player walks in, then destroys itself so it only fires once. |

### How a round works

1. `Mover` moves the player every frame (keyboard or touch arrows).
2. The player runs into an obstacle → `ObjectHit` calls `GameManager.RegisterHit()`, then turns the obstacle black.
3. `GameUI` shows the new count. On the 5th hit `GameManager` ends the round and the Game Over panel appears.
4. **Watch ad** → `AdManager.ShowRewarded(...)` → after the ad, `GameManager.ContinueAfterReward()` sets the hits to 2 / 5 and unfreezes the game.
5. **Play again** → `GameManager.Restart()` → every 3rd time `AdManager.ShowInterstitial(...)` first, then the scene reloads.
6. Touching the finish → `FinishLine` → `GameManager.Win()` → the win panel appears.

## Project structure

```
Assets/
├── Scenes/      MainScene.unity — the level
├── scripts/     gameplay, UI and ad scripts (see above)
├── prefabs/     Wall, Small building Variant, SpinningThing, DroppingObject
├── materials/   one material per obstacle type
├── LevelPlay/   LevelPlay (Ads Mediation) settings
├── Plugins/     Android Gradle templates used by the ad SDK
└── Settings/    URP render pipeline and post-processing settings
```

## Running it in the editor

1. Install **Unity 6000.6.2f1** (any Unity 6 version should work) through Unity Hub.
2. Clone the repository:
   ```bash
   git clone https://github.com/abdurakhmanovjohn/obstacle_game.git
   ```
3. In Unity Hub, click **Add → Add project from disk** and choose the cloned folder. The first import takes a few minutes.
4. Open `Assets/Scenes/MainScene` and press **Play**.

In the editor no real ads are shown: the `AdManager` stub gives the "+3 lives" reward right away and skips interstitials, so you can test the full loop. To try the touch arrows with the mouse, tick **Show On PC** on `GameManager → TouchControls`.

## Building for Android

**Requirements:** Unity Hub → Installs → *Manage* → **Add modules** → **Android Build Support** (with OpenJDK and Android SDK & NDK Tools).

1. **File → Build Profiles → Android → Switch Platform.**
2. **Edit → Project Settings → Player → Android:**
   - Package Name: `com.abdurakhmanovjohn.obstacledodge`. This must match the app on the LevelPlay dashboard.
   - Scripting Backend **IL2CPP**, target architecture **ARM64**.
   - Internet Access **Require**.
   - Default Orientation **Landscape Left**.
   - Active Input Handling **Both**. The scripts use the old Input Manager, and the UI uses the Input System UI module.
3. **Assets → External Dependency Manager → Android Resolver → Force Resolve**. This downloads the native ad SDK. Without it LevelPlay never initialises.
4. **Build** (or **Build And Run** with the device connected and USB debugging on).

### Ads setup (Unity LevelPlay)

1. On the [LevelPlay dashboard](https://platform.ironsrc.com): add an Android app, then create three ad units: **Rewarded**, **Interstitial** and **Banner**. The *ironSource Ads* network is enabled by default.
2. Paste the **App Key** and the three **Ad Unit IDs** into the `AdManager` object in the scene.
3. Add your phone/tablet under **Setup → Test devices** (its Advertising ID, network *ironSource Ads*). A brand-new app has no real ads yet (error `509 No fill`), so test devices are needed to get test ads.
4. Before release, untick **Show Ad Status** on `AdManager` and remove the `LevelPlay.SetAdaptersDebug(true)` line.

### Troubleshooting

| Problem | Cause and fix |
| ------- | ------------- |
| Status overlay stuck on `init: starting` / "no answer after 20s" | The device can't reach `i-sdk.mediation.unity3d.com`. Check with **Android Logcat** (filter `LevelPlay`). In our case an **ad-blocking Private DNS** on the tablet was blocking it. Turn Private DNS off. |
| `rewarded load failed: 509 No fill` | SDK works, but there's no ad for a new app. Register the device as a **test device**. |
| No "Watch ad" button | It only appears when a rewarded ad has actually loaded. Check the status overlay. |
| UI buttons don't react on the device | The EventSystem's input module doesn't match Active Input Handling. Use **Both** + *Input System UI Input Module*. |
| `INSTALL_FAILED_USER_RESTRICTED` on Xiaomi | Turn on **Install via USB** in Developer options, or install the APK by hand. |
| "Blocked by Play Protect" | Normal for self-built APKs. Tap *More details → Install anyway*. |

## Known limitations

- Movement uses `transform.Translate`, so the player can jitter against walls.
- `Spinner` doesn't use `Time.deltaTime`, so spin speed depends on frame rate.
- `TriggerProjectile` uses five separate fields instead of an array.
- Active Input Handling is set to **Both**, which Unity warns about on Android. It works, but a full move to the new Input System would be cleaner.
- Ads only work in Android builds. The WebGL route from the course guide (Monetag/Adsterra) is prepared in `AdManager` but not set up.
- The game isn't published on Google Play, so it only gets test ads.

## Credits

Made by Johnibek Abdurakhmanov for a university Game Development course. Based on the course guides _Building Obstacle Dodge From Scratch_ and _Ads in Obstacle Dodge_.
