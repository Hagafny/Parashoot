# Scripts — Architecture Notes

See root `/CLAUDE.md` for full game overview. This file covers scripting conventions and patterns specific to the Scripts folder.

## Folder Structure
```
Scripts/
  Exit/         — ExitGame.cs (quit on ESC)
  Menu/         — ButtonController, EscToReturn, ToggleSound, MenuNavigation (controller UI nav)
  Misc/         — Shared utilities: GameStats, LifeBar, Music, Players enum, MoveTowards, etc.
  Movie/        — PlayMovie.cs (intro video)
  Options/      — Character select UI + data classes (CowOptions, CowColor, CowType, etc.)
  Play/
    AI/         — AIMovement, AIRotation, AIShooting
    Balloons/   — IBalloon, BalloonMagnfyingGlass, BalloonStrainer, BaloonOptions
    Bullet/     — BulletMovement
    Camera/     — CameraRatio (enforces 4:3)
    Cheats/     — GameCheats ("moo" cheat)
    Cow/        — CowStats, CowHealth, CowShooting, CowAnimation, CowSounds, CowClampRotation, CowAnimationsHolder
    Human/      — HumanMovement, HumanShooting
    Interfaces/ — Movement (abstract), IShooting (interface)
    Managers/   — GameManager, CowManager, PowerUpSpawner, BaloonSpawner, ScoreManager, CloudSpawner, CloudOptions, SlowMotionDirector
    PowerUps/   — IPowerUp, HealthPowerUp, ShieldPowerUp, MadCowPowerUp, ShieldEffect, MadCowEffect, PowerUpCollision, PowerUpSpawner, PowerUpEffect, PowerUpManager
```

## Key Patterns

### Interface + event separation
Human and AI never talk to each other directly. All communication goes through events on `CowShooting`:
- Bullet collisions raise events
- `GameManager` subscribes and routes to the correct handler
- Adding a new effect = implement the right interface + subscribe to the right event

### Adding a new power-up
1. Create a class implementing `IPowerUp` (like `HealthPowerUp.cs`)
2. Add a `PowerUpType` enum value in `PowerUpManager.cs`
3. Add the type to `PowerUpManager.InjectBehavior()` switch
4. Wire up a sprite and parachute color in the Unity Inspector on the PowerUpSpawner

### Adding a new balloon effect
1. Create a class implementing `IBalloon` (like `BalloonStrainer.cs`)
2. Add a `BaloonEffect` enum value in `BaloonOptions.cs`
3. Add the type to `BaloonOptions.Setup()` switch
4. Wire up a sprite in the Unity Inspector on the BaloonSpawner

### Controller support (all types incl. PlayStation)
Implemented via the **New Input System package** (`com.unity.inputsystem`), running
alongside Legacy Input Manager (`activeInputHandler: 2` = "Both" in ProjectSettings).
- `ControllerInput.cs` (Play/Human/) reads gamepads through the standard `Gamepad`
  layout, so Xbox, PS4 (DualShock4), PS5 (DualSense), Switch Pro and generic HID pads
  all map identically — no per-brand axis numbers.
- Gamepads are assigned by connection order: P1 = first pad, P2 = second pad.
- `HumanMovement.cs` sums keyboard axis + `ControllerInput.GetVertical/GetRotation`.
- `HumanShooting.cs` fires on keyboard OR `ControllerInput.FirePressedThisFrame`.
- Default pad mapping: Left stick Y = move, left/right stick X = aim, D-pad also works;
  Fire = South button (A / ✕) or right trigger or right bumper.

### Menu navigation with a controller
`MenuNavigation.cs` (Menu/) makes every UGUI menu controller-navigable with **zero per-scene
editor setup**. It self-bootstraps via `[RuntimeInitializeOnLoadMethod]`, is `DontDestroyOnLoad`,
and on each `sceneLoaded`:
- Swaps the legacy `StandaloneInputModule` for `InputSystemUIInputModule` (gamepad-capable),
  calling `AssignDefaultActions()` for navigate/submit/cancel/point/click bindings.
- Sets any `Selectable` with `Navigation.Mode.None` to `Automatic` so it can be reached.
- Highlights the top button, and re-highlights after a mouse click clears selection once the
  stick/D-pad is used (mouse + controller coexist).
- Scenes with no `Selectable` (Play, intro movie) are skipped.
- **East button = back**: in sub-menus (Instructions / Credits / Options) it loads the main
  menu (scene 1). The main menu is detected by an `ExitGame` component and is left alone so
  Circle can't accidentally quit. The Play scene's East→menu is handled in `GameManager` (mirrors ESC).
Submit = South button. Keyboard text entry into name `InputField`s still works because legacy
input stays active in "Both" mode. `EscToReturn.cs` also gained East support but is currently
unused (not attached to any scene).

## CowStats.cs — Tunable Values
All per-cow balance values live here. Key fields:
- `startingLives` = 3, `maximumeLives` = 5
- `movementSpeed` = 12, `rotationSpeed` = 10
- `fireDelay` = 0.75s
- `shieldTime` = 5s, `madCowTime` = 5s
- `yMaxBounadry` = 9.2, `yMinBounadry` = -12
- `rotationAngleLimit` = 45 degrees
- AI difficulty modifies these values at runtime in `GameManager.cs`

## Players Enum (Players.cs)
```csharp
enum Players { Player1 = 1, Player2 = 2 }
```
Used throughout to identify which cow is which. `CowStats.playerNumber` is set to 1 or 2 by `GameManager`.
