using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central helper for reading gamepad input via the new Input System.
/// Because it uses the standard Gamepad layout, ANY supported controller
/// (Xbox, PlayStation DualShock4 / DualSense, Switch Pro, generic HID pads)
/// is normalized to the same buttons/sticks, so no per-brand mapping is needed.
///
/// Gamepads are assigned to players by connection order:
///   Player 1 -> first connected gamepad, Player 2 -> second connected gamepad.
/// </summary>
public static class ControllerInput
{
    /// <summary>
    /// Returns the Gamepad assigned to the given player number (1 or 2),
    /// or null if no controller is connected for that slot.
    /// </summary>
    public static Gamepad GetGamepad(int playerNumber)
    {
        int index = playerNumber - 1;
        var pads = Gamepad.all;
        if (index >= 0 && index < pads.Count)
            return pads[index];
        return null;
    }

    /// <summary>
    /// Combined vertical movement from left stick and D-pad (-1..1).
    /// </summary>
    public static float GetVertical(int playerNumber)
    {
        Gamepad pad = GetGamepad(playerNumber);
        if (pad == null) return 0f;
        return Mathf.Clamp(pad.leftStick.y.ReadValue() + pad.dpad.y.ReadValue(), -1f, 1f);
    }

    /// <summary>
    /// Combined rotation input from left stick X, right stick X and D-pad (-1..1).
    /// Either stick can aim the gun.
    /// </summary>
    public static float GetRotation(int playerNumber)
    {
        Gamepad pad = GetGamepad(playerNumber);
        if (pad == null) return 0f;
        return Mathf.Clamp(
            pad.leftStick.x.ReadValue() + pad.rightStick.x.ReadValue() + pad.dpad.x.ReadValue(),
            -1f, 1f);
    }

    /// <summary>
    /// True on the frame a fire input was pressed (face button, right trigger or right bumper).
    /// </summary>
    public static bool FirePressedThisFrame(int playerNumber)
    {
        Gamepad pad = GetGamepad(playerNumber);
        if (pad == null) return false;
        return pad.buttonSouth.wasPressedThisFrame
            || pad.rightTrigger.wasPressedThisFrame
            || pad.rightShoulder.wasPressedThisFrame;
    }
}
