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
- Movement **accelerates and decelerates** (`GameplayTuning.AccelerationTime` /
  `DecelerationTime`) rather than snapping to full speed — reversing pays a braking cost first,
  so a direction change is a commitment. Applies to both `HumanMovement` and `AIMovement`.
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

### Gameplay Pacing (GameplayTuning.cs)
- **All pacing values live in `GameplayTuning.cs`** — fire delay, bullet speed, movement speed,
  acceleration, rotation rate, invincibility, spawn cadences, AI difficulty multipliers.
- **Why a central file:** the project uses **binary serialization**, so serialized values on the
  binary Cow/Bullet prefabs and Play scene *override* C# field initializers. Editing
  `CowStats.fireDelay = 0.75f` in source changes nothing at runtime. Values are therefore
  **assigned at runtime**: `CowStats.Awake()`, `BulletMovement.Start()`, both spawners' `Start()`.
- **Keystone ratio:** cows are locked to their spawn columns, so every shot crosses the full arena
  and bullet travel time is a constant. `shotsInFlight = travelTime / fireDelay` must stay **≤ 1.0**
  so a cow cannot fire again before its previous shot resolves. That is what makes a miss cost
  something and stops the shoot-shoot-shoot loop.
- `PacingDiagnostics` (self-bootstrapping) logs the **measured** arena width and resulting ratios on
  round start — arena width lives in the binary scene and can't be read from source.
- **Balloon rise / crate fall** are driven by Rigidbody `gravityScale` authored on the binary
  prefabs, so there is no absolute speed readable from source. `BaloonSpawner.ScaleRise` and
  `PowerUpSpawner.ScaleFall` **multiply** that value at spawn (`BalloonRiseMultiplier`,
  `CrateFallMultiplier`) — scaled down, because at the prefab's authored gravity a balloon
  accelerates past bullet speed before clearing the screen.
- **These multipliers are not linear.** The motion accelerates, so speed goes with the *square
  root* of the multiplier: to halve crossing speed, use a **quarter** of the multiplier.
- Both remain accelerating rather than constant-speed — slowest at the spawn edge, fastest on exit.
  If that ramp itself becomes a problem, the fix is assigning a constant velocity at spawn rather
  than a smaller multiplier.
- **Lifetime is measured, not hardcoded** (`OffscreenLifetime.cs`). A fixed `SelfDestructBySeconds`
  is authored against one particular speed, so retuning the multipliers used to make balloons
  vanish in mid-screen. Both spawners now strip the prefab's root timer and attach
  `OffscreenLifetime`, which destroys the object `OffscreenGraceSeconds` after it actually clears
  the camera. Retuning speed can no longer strand or orphan one.
  - The grace window is deliberately non-zero: shooting a balloon just after it leaves the top of
    the screen is a valid play, so it stays alive and shootable for a beat.
  - Objects spawn *offscreen* (y = ±19), so the component waits until it has been seen once before
    arming the timer, plus a `MaxLifetimeSeconds` backstop for anything never visible.
  - Only the **root** timer is stripped; children keep theirs for effect cleanup (pop, box explosion).
- To retune: edit `GameplayTuning.cs`, re-enter Play mode, read the `[Pacing]` console lines.

### Health System
- Start: 3 lives. Max: 5. One damage per bullet hit.
- Invincibility after each hit (`GameplayTuning.InvincibilitySeconds`) — long enough to be a
  reposition beat, not just a same-frame multi-hit guard.
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
- While MadCow is active, `CowAnimation` smoothly cross-fades the head between its normal face and
  the "crazy" face: the base head stays normal while an overlay `SpriteRenderer` (drawn just above
  the head) fades the crazy face in/out on a cosine curve shaped by smootherstep (so it dwells at
  full-normal and full-mad and passes quickly through the mid-blend), easing back and forth
  `MadCycles` (3) times across `CowStats.madCowTime`. Done in `LateUpdate` so it overrides the static mad clip; it
  captures the normal face at `Start` and the crazy face from the animator at runtime — no asset wiring.

### Balloon System (BaloonSpawner.cs)
- Balloons float up from the bottom every 8s.
- Types: **Magnifying Glass** (enlarges bullet mass/scale), **Strainer** (splits into 2 bullets at ±20°, inheriting all event handlers).

### AI System
- `AIMovement`: patrols between top/bottom waypoints, randomly switches direction every 1.8-3.4s.
  Accelerates from rest and resets its ramp on each waypoint switch, mirroring the human's inertia.
- `AIRotation`: aims at P1 using Atan2, rate-capped via `Quaternion.RotateTowards` at
  `rotationSpeed * 15` deg/s. (Was a `Slerp` whose factor exceeded 1 every frame, i.e. instant aim.)
- `AIShooting`: always returns true (fires as fast as fireDelay allows).
- Difficulty scales `movementSpeed`, `rotationSpeed`, `fireDelay` by **multipliers** from
  `GameplayTuning.ScaleFor()`. These were additive offsets (`-= 7`) hard-coupled to the old base
  values — against the retuned baselines AIEasy would have ended up frozen with negative rotation.

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
| `GameplayTuning.cs` | **Single source of truth for pacing** — applied at runtime because binary prefabs override field initializers. Also `PacingDiagnostics` |
| `CowStats.cs` | Per-cow values; `Awake()` pulls pacing from `GameplayTuning` |
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
- **The whole project uses binary serialization** (`EditorSettings.asset`): every `.unity` scene and
  `.prefab` is a binary blob, not YAML. Consequences: they can't be diffed, grepped, or edited
  outside the Editor, and **serialized values override C# field initializers** — changing a default
  in a script does *not* change the value the game runs with. Gameplay values that need to be
  tunable from source must be assigned at runtime (see `GameplayTuning.cs`)
- All assets use 4:3 aspect ratio (enforced by `CameraRatio.cs`)
- `DontDestroyOnLoad` objects: Game Music, GameOptions
