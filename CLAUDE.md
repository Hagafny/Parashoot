# Parashoot — Claude Context

## What this is
A 2-player competitive 2D shooter built in Unity 6.2 (6000.5.4f1). Originally created in 2016 (Unity 2017), upgraded to Unity 6. Two cows on opposite sides of a vertical arena shoot at each other. First to lose all lives loses. Supports Human vs Human or Human vs AI (4 difficulty levels).

## Scene Flow
1. **Intro video** (scene 0) — `PlayMovie.cs` plays a VideoClip, ESC skips. Loads scene 1 when done.
2. **Main Menu** (scene 1) — Start, Instructions, Credits, Exit. Music toggle.
3. **Options / Character Select** — Players set names, bandana color (6 options), cow type (Human / AIEasy / AINormal / AIHard / AIInsane).
4. **Play Scene** — the actual gameplay.
5. **Instructions** — accessible from the main menu.

## Controls (default keyboard)
| Action | Player 1 | Player 2 |
|---|---|---|
| Move up/down | Arrow keys | W / S |
| Rotate gun | Left/Right arrows | A / D |
| Shoot | P | Q |

- Movement is **vertical only** — X position is locked to spawn column
- Y clamped: -12 to 9.2. Rotation clamped: ±45 degrees.
- Input uses Unity **Legacy Input Manager** with named axes: `Vertical1`, `Rotation1`, `Fire1` etc.
- `InputManager.asset` is in **binary format** — edit via Unity UI (Edit → Project Settings → Input Manager), not as a text file.

## Architecture — How It All Connects

### Startup (GameManager.cs)
- Reads `GameOptions` (a DontDestroyOnLoad GameObject set in the Options scene)
- Spawns both cows, wires up AI if needed, runs 3-second countdown, then enables controls

### Core Interfaces
- **`IShooting`** — `HasShot()` returns bool. `HumanShooting` checks the Fire button; `AIShooting` always returns true.
- **`Movement` (abstract)** — base for `HumanMovement` and `AIMovement`. Defines `GoingUp` event (triggers flame particle).
- **`IPowerUp`** — `Use(cow)`, `PowerUpEffect()` (Good/Bad), `GetPowerUpType()`. Implemented by `HealthPowerUp`, `ShieldPowerUp`, `MadCowPowerUp`.
- **`IBalloon`** — `Pop(bullet)`. Implemented by `BalloonMagnfyingGlass` and `BalloonStrainer`.

### Event Flow (event-driven architecture)
`CowShooting` fires events that other systems subscribe to:
- `BulletHitPlayer` → `CowHealth.TakeDamage()`
- `BulletHitPowerUp` → `PowerUpSpawner` opens the box
- `BulletHitBalloon` → `BaloonSpawner` pops the balloon
- `BulletHitShield` / `BulletHitShieldExit` → ricochet logic in `BulletMovement`

`GameManager` wires all events at startup. Nothing talks directly to another system — everything goes through events.

### Health System
- Start: 3 lives. Max: 5. One damage per bullet hit.
- 0.2s invincibility after each hit (prevents multi-hit in same frame).
- Shield (`ShieldEffect.cs`) blocks all damage for 5s, increases Rigidbody2D mass by 100.
- `LifeBar.cs` displays 5 heart slots per player.

### Bullet System (BulletMovement.cs)
- Speed: 24 units/s. Mass used for bullet-vs-bullet: heavier wins.
- Collision outcomes: damages player / ricochets off shield / pops balloon / opens power-up box / explodes vs heavier bullet.
- Destroyed after 0.1s post-collision (allows ricochet physics to play out).

### Power-Up System (PowerUpSpawner.cs)
- Spawns a parachute box from the top every 15s.
- Bullet hits box → box destroyed → item detaches and flies toward the **shooter** (Good) or **enemy** (Bad) via `MoveTowards.cs`.
- Types: **Health** (+1 life), **Shield** (5s immunity), **MadCow** (5s inverted controls).

### Balloon System (BaloonSpawner.cs)
- Balloons float up from the bottom every 8s.
- Types: **Magnifying Glass** (enlarges bullet mass/scale), **Strainer** (splits into 2 bullets at ±20°, inheriting all event handlers).

### AI System
- `AIMovement`: patrols between top/bottom waypoints, randomly switches direction every 1-2s.
- `AIRotation`: always aims at P1 using Atan2.
- `AIShooting`: always returns true (fires as fast as fireDelay allows).
- Difficulty scales `movementSpeed`, `rotationSpeed`, `fireDelay` in `CowStats`.

### Slow Motion on the Killing Blow (SlowMotionDirector.cs)
- Self-bootstrapping (`RuntimeInitializeOnLoadMethod` + `DontDestroyOnLoad`), no per-scene setup.
- Each frame it scans `Bullet`-tagged objects and `CircleCastAll`s ~0.2s of travel ahead. If the
  FIRST collider in the path is a `Player` whose `CowHealth.WouldNextHitBeFatal()` is true (last
  life, not shielded/invincible), it slows `Time.timeScale` to 0.12.
- Freezes the doomed cow (disables its `Movement` component) so it can't dodge the guaranteed kill.
- Holds the slow-mo until the target actually dies (or a 1s real-time safety cap), lingers a beat,
  then eases back to full speed. `Time.fixedDeltaTime` is scaled alongside so physics stays smooth.
- Restores `Time.timeScale` on `sceneLoaded` and `OnDisable` so the game never gets stuck slowed.
- Only the killing blow triggers it (per design) — not every life-losing hit.

### Scoring
- `GameStats.cs` — static-like persistent class, tracks P1Score / P2Score across rounds.
- `ScoreManager.cs` — listens to `CowWon` event, updates UI.
- No "best of X" system yet — rounds are indefinite until ESC.

## Key Files
| File | Responsibility |
|---|---|
| `GameManager.cs` | Central orchestrator — spawns cows, wires events, manages game state machine |
| `SlowMotionDirector.cs` | Predicts a guaranteed killing bullet ~0.2s ahead and slows time through the impact (self-bootstrapping) |
| `CowStats.cs` | All tunable values per cow (speed, lives, delays, boundaries) |
| `CowShooting.cs` | Bullet spawning + all bullet collision events |
| `BulletMovement.cs` | Bullet physics, collision outcomes |
| `PowerUpSpawner.cs` | Spawns boxes, opens them, routes power-up to correct cow |
| `CowHealth.cs` | Life tracking, damage, shield, invincibility |
| `CowManager.cs` | Per-player setup: applies color, swaps animator overrides, adds control scripts |
| `HumanMovement.cs` | Keyboard input for movement |
| `HumanShooting.cs` | Keyboard input for shooting |
| `AIMovement.cs` | AI patrol movement |
| `AIRotation.cs` | AI aiming |
| `Options.cs` | Options screen UI — name/color/type selection |
| `GameOptions.cs` | Persistent container for CowOptions, survives scene loads |
| `PlayMovie.cs` | Intro video (uses VideoPlayer — was MovieTexture, fixed for Unity 6) |
| `iTween.cs` | Animation tween library (fixed for Unity 6: removed GUITexture/GUIText) |

## Customization
- **Characters**: Molly and Eva (separate sprite sets, animator overrides per character)
- **Bandana colors**: Red, Blue, Green, Pink, Purple, Yellow (`CowColor` enum, `RibbonImages.cs`)
- **Cow types**: Human=0, AIEasy=1, AINormal=2, AIHard=3, AIInsane=4 (`CowType` enum)

## Cheat Codes
- Options screen: type `insane` → forces AIInsane difficulty
- Play scene: type `moo` → BalloonOverload (spawn interval drops to 0.04s for 3.5s)

## Controller Support
- All gamepad types supported via the **New Input System** package (`com.unity.inputsystem`),
  running alongside Legacy Input (`activeInputHandler: 2` / "Both").
- Xbox, PS4 (DualShock4), PS5 (DualSense), Switch Pro and generic HID pads all normalize
  to the same `Gamepad` layout — see `ControllerInput.cs`.
- P1 = first connected pad, P2 = second. Left stick moves, either stick X aims,
  South button / right trigger / right bumper fires. Keyboard still works simultaneously.
- Menus are controller-navigable too via `MenuNavigation.cs` (self-bootstrapping, no
  per-scene setup): D-pad/stick to move, South = select, East (Circle/B) = back.
  East in the Play scene exits the match to the menu (handled in `GameManager`, mirrors ESC).

## Planned Features
- Screen shake on hit
- Hit-stop (3-4 frame freeze on impact)
- Power-up timer HUD
- Best of 3 / Best of 5 match structure
- Pause menu (ESC currently exits immediately to menu)
- New power-ups: Rapid Fire, Speed Boost, Freeze
- New balloon: Homing bullet, Explosive bullet, Ricochet bullet
- AI bullet dodging

## Unity-Specific Notes
- Input runs in **"Both"** mode: keyboard uses the **Legacy Input Manager**, controllers use the **New Input System** package (see Controller Support above)
- `InputManager.asset` is binary — edit only via Unity Editor UI
- All assets use 4:3 aspect ratio (enforced by `CameraRatio.cs`)
- `DontDestroyOnLoad` objects: Game Music, GameOptions
