# Scripts — Architecture Notes

See root `/CLAUDE.md` for full game overview. This file covers scripting conventions and patterns specific to the Scripts folder.

## Folder Structure
```
Scripts/
  Exit/         — ExitGame.cs (quit on ESC)
  Menu/         — ButtonController, EscToReturn, ToggleSound
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
    Managers/   — GameManager, CowManager, PowerUpSpawner, BaloonSpawner, ScoreManager, CloudSpawner, CloudOptions
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

### Adding a new player control scheme (e.g. Xbox controller)
1. Add new input axes in Unity Editor: Edit → Project Settings → Input Manager
2. `HumanMovement.cs` reads named axes (`Vertical1`, `Rotation1`) — add controller axis as an alternative binding, or modify the script to also check joystick input
3. `HumanShooting.cs` checks `Fire1` button — add joystick button as alternative

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
