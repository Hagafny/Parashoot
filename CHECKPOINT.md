# Checkpoint — Controller Support (2026-07-17)

Snapshot of in-progress work so anyone (human or AI) picking this up knows where things stand.

## Goal
Add support for **all controller types — especially PlayStation (DS4 / DualSense)** — for
both gameplay and menu navigation, without breaking the existing keyboard controls.

## Approach chosen
Installed the **New Input System package** (`com.unity.inputsystem`) and run it **alongside**
the Legacy Input Manager (`Active Input Handling = "Both"`, `activeInputHandler: 2`).
The New Input System's standard `Gamepad` layout normalizes Xbox / PS4 / PS5 / Switch Pro /
generic pads to the same buttons & sticks, so there is **no per-brand axis guessing**.
Keyboard keeps using the Legacy Input Manager, unchanged.

## What was done (all committed in this checkpoint)

### 1. Package + project settings
- `Packages/manifest.json` + `packages-lock.json`: added `com.unity.inputsystem` **1.19.0**
  (1.19.0 is the minimum required by this editor, **Unity 6.2 / 6000.5.4f1** — an earlier
  1.11.2 pin caused 89 compile errors because its editor code used a `TreeView` API that
  Unity 6.2 removed).
- `ProjectSettings/ProjectSettings.asset`: `activeInputHandler: 0 → 2` ("Both").

### 2. Gameplay controls
- **NEW** `Assets/Scripts/Play/Human/ControllerInput.cs` — static helper reading gamepads via
  `Gamepad.all`, assigned to players by connection order (P1 = first pad, P2 = second).
  - `GetVertical` (left stick Y + D-pad), `GetRotation` (left/right stick X + D-pad),
    `FirePressedThisFrame` (South button / right trigger / right bumper).
- `HumanMovement.cs` — sums keyboard axis + `ControllerInput` values (either can drive the cow).
- `HumanShooting.cs` — fires on keyboard OR gamepad.

### 3. Menu navigation
- **NEW** `Assets/Scripts/Menu/MenuNavigation.cs` — self-bootstrapping (`RuntimeInitializeOnLoadMethod`,
  `DontDestroyOnLoad`), **no per-scene editor setup**. On each scene load it:
  - Swaps the legacy `StandaloneInputModule` for `InputSystemUIInputModule` (gamepad-capable UI).
  - Sets any `Selectable` with Navigation = None to Automatic.
  - Highlights the first button; re-highlights after a mouse click if the stick/D-pad is used.
  - Skips scenes with no Selectables (Play, intro movie).
- **East button (Circle ○ / B) = back**: in Instructions / Credits / Options it loads the main
  menu (scene 1). Main menu is detected via its `ExitGame` component and left alone so Circle
  can't quit accidentally.
- `GameManager.cs` — its ESC→menu exit also fires on East (exits a match to menu).
- `EscToReturn.cs` — gained East support too, but is **currently unused** (not attached to any scene).

### 4. Docs
- `CLAUDE.md` + `Assets/Scripts/CLAUDE.md` updated (controller support sections; corrected the
  Unity version from the wrong `6000.0.54f1` to `6000.5.4f1`).

## Control mapping (pad)
| Action | Pad |
|---|---|
| Move up/down | Left stick Y / D-pad |
| Aim gun | Left or right stick X / D-pad |
| Shoot | South (✕ / A) · R2 trigger · R1 bumper |
| Menu: navigate | Stick / D-pad |
| Menu: select | South (✕ / A) |
| Menu / match: back | East (○ / B) |

## STATUS: verified working in play mode ✅
Gameplay + fire confirmed on a DualSense (USB). Getting there required, in order:
1. Bumping the package to 1.19.0 (fixed the 89 compile errors).
2. A **full editor restart** to activate the "Both" input backends.
3. Setting **Edit → Project Settings → Input System Package → Play Mode Input Behavior =
   "All Device Input Always Goes To Game View"**. This is the key editor gotcha: with the
   default, gamepad input is withheld from Play mode unless the Game view has focus, so the
   cow read all-zeros while the Console was focused. NOTE: this is a **per-user editor
   setting**, not committed to the repo — and it only matters in the editor. In a built
   player there is no "game view focus", so controllers work regardless.

## Next steps / ideas
- Test menu navigation + Circle-as-back across Instructions/Credits/Options with a pad.
- Consider a proper pause menu (Circle currently exits a match instantly).

## Known caveats / open questions
- Circle in the Play scene exits a match instantly (mirrors ESC, no confirmation) — a proper
  pause menu is still a planned feature. Could add a confirmation if desired.
- Options screen uses Automatic navigation; its exact up/down/left/right flow may feel arbitrary
  and could be tuned with explicit navigation later.
- Player→pad assignment is by connection order; no in-menu pad picker yet.
