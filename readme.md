# Obstacle Dodge

A small 3D obstacle course made in Unity for a university Game Development course. You control a capsule called **Dodgy** and try to cross a walled arena from the start corner to the finish without bumping into anything: static walls and buildings, spinning bars, objects that drop from the sky, and traps that fire projectiles at you.

![Unity](https://img.shields.io/badge/Unity-6000.6.2f1-black?logo=unity)
![Language](https://img.shields.io/badge/C%23-scripts-239120)
![Render pipeline](https://img.shields.io/badge/URP-3D-blue)

## How to play

| Key                     | Action |
| ----------------------- | ------ |
| `W A S D` or arrow keys | Move   |

Every obstacle you touch turns black and counts as one hit. A black obstacle is "spent" and won't count again. The hit count is printed to the Unity **Console**. Try to reach the finish in the north-east corner with as few hits as possible.

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

| Script                 | Attached to      | What it does                                                                                                            |
| ---------------------- | ---------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `Mover.cs`             | Player           | Reads keyboard input and moves the player with `transform.Translate`. Prints the instructions to the Console on start.  |
| `Scorer.cs`            | Player           | Counts collisions with obstacles that haven't been hit yet and logs the total.                                          |
| `ObjectHit.cs`         | Every obstacle   | When the player touches it, turns it black and changes its tag to `Hit` so it isn't counted twice.                      |
| `Spinner.cs`           | Spinning bars    | Rotates the object every frame by the X/Y/Z angles set in the Inspector.                                                |
| `Dropper.cs`           | Dropping objects | Keeps the object invisible and gravity-free until `timeToWait` seconds have passed, then lets it fall.                  |
| `FlyAtPlayer.cs`       | Projectiles      | Starts disabled. When enabled, flies to where the player was standing at that moment and destroys itself on arrival.    |
| `TriggerProjectile.cs` | Trigger volumes  | An invisible zone that wakes up its 5 projectiles when the player walks in, then destroys itself so it only fires once. |

### How they work together

1. `Mover` moves the player every frame.
2. The player runs into an obstacle. `ObjectHit` on the obstacle turns it black and tags it `Hit`.
3. At the same moment `Scorer` on the player sees a collision with something not tagged `Hit` and adds one to the count.
4. Walking into a trigger volume enables five projectiles, which fly at the spot where the player was standing.

## Project structure

```
Assets/
├── Scenes/      MainScene.unity — the level
├── scripts/     gameplay scripts (see above)
├── prefabs/     Wall, Small building Variant, SpinningThing, DroppingObject
├── materials/   one material per obstacle type
└── Settings/    URP render pipeline and post-processing settings
```

## Running it

1. Install **Unity 6000.6.2f1** (any Unity 6 version should work) through Unity Hub.
2. Clone the repository:
   ```bash
   git clone https://github.com/abdurakhmanovjohn/obstacle_game.git
   ```
3. In Unity Hub, click **Add → Add project from disk** and choose the cloned folder. The first import takes a few minutes.
4. Open `Assets/Scenes/MainScene` and press **Play**.
5. Open the **Console** (Window → General → Console) to see the instructions and your hit count.

### Building

File → Build Profiles → choose Windows, Mac or Linux → **Add Open Scenes** → **Build**.

## Known limitations

- There's no on-screen UI yet. The hit count only appears in the Console.
- Reaching the finish doesn't end the game, and there's no lose condition.
- Movement uses `transform.Translate`, so the player can jitter against walls.
- `Spinner` doesn't use `Time.deltaTime`, so spin speed depends on frame rate.
- `Dropper` uses `Time.time`, which doesn't reset when the scene reloads.
- The project uses the legacy Input Manager, which Unity 6 marks as deprecated.

## Credits

Made by Johnibek Abdurakhmanov for a university Game Development course. Based on the course guide _Building Obstacle Dodge From Scratch_.
