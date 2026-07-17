using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds controller (and keyboard) navigation to every UGUI menu WITHOUT any per-scene
/// editor setup. It bootstraps itself once at startup, survives scene loads, and on each
/// scene:
///   1. Swaps the legacy StandaloneInputModule for InputSystemUIInputModule so gamepads
///      (Xbox / PlayStation / Switch / generic) drive the UI navigation & submit/cancel.
///   2. Enables Automatic navigation on any Selectable that had it turned off.
///   3. Highlights the first button so the controller always has a starting point.
///   4. Re-highlights a button if the mouse cleared the selection and the player then
///      uses the stick / D-pad (lets mouse and controller coexist).
///
/// Scenes with no Selectables (e.g. the Play scene, intro movie) are skipped.
/// </summary>
public class MenuNavigation : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("MenuNavigation");
        DontDestroyOnLoad(go);
        go.AddComponent<MenuNavigation>();
    }

    private GameObject lastSelected;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(SetupNextFrame());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(SetupNextFrame());
    }

    // Wait one frame so the newly loaded scene's UI (EventSystem + buttons) exists.
    private IEnumerator SetupNextFrame()
    {
        yield return null;

        List<Selectable> selectables = GetActiveSelectables();
        if (selectables.Count == 0)
            yield break; // Not a menu scene — nothing to navigate.

        EnsureGamepadModule();
        EnableNavigation(selectables);
        SelectFirst(selectables);
    }

    private void Update()
    {
        HandleCancel();

        EventSystem es = EventSystem.current;
        if (es == null)
            return;

        // Track the last valid selection.
        if (es.currentSelectedGameObject != null)
        {
            lastSelected = es.currentSelectedGameObject;
            return;
        }

        // Selection was lost (usually a mouse click on empty space). Restore it the moment
        // the player nudges a stick or the D-pad, so controller navigation keeps working.
        if (GamepadNavigating())
        {
            if (lastSelected != null && lastSelected.activeInHierarchy)
                es.SetSelectedGameObject(lastSelected);
            else
                SelectFirst(GetActiveSelectables());
        }
    }

    /// <summary>
    /// East button (Circle on PlayStation, B on Xbox) = "back" in the menus.
    /// Sub-menus (Instructions / Credits / Options) all return to the main menu (scene 1).
    /// The main menu itself is identified by the presence of an ExitGame component and is
    /// left alone (its ESC-to-quit stays keyboard-only, so Circle can't accidentally quit).
    /// The Play scene has no Selectables and is handled by GameManager instead.
    /// </summary>
    private void HandleCancel()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null || !pad.buttonEast.wasPressedThisFrame)
            return;

        // Not a menu scene (no buttons) — ignore.
        if (GetActiveSelectables().Count == 0)
            return;

        // Main menu: no "back" to go to.
        if (FindFirstObjectByType<ExitGame>() != null)
            return;

        SceneManager.LoadScene(1);
    }

    private bool GamepadNavigating()
    {
        Gamepad pad = Gamepad.current;
        if (pad == null)
            return false;
        return Mathf.Abs(pad.leftStick.y.ReadValue()) > 0.5f
            || Mathf.Abs(pad.leftStick.x.ReadValue()) > 0.5f
            || pad.dpad.up.wasPressedThisFrame
            || pad.dpad.down.wasPressedThisFrame
            || pad.dpad.left.wasPressedThisFrame
            || pad.dpad.right.wasPressedThisFrame;
    }

    /// <summary>
    /// Make sure the EventSystem uses the Input System UI module (gamepad-capable) rather
    /// than the legacy StandaloneInputModule (keyboard/mouse only in this project).
    /// </summary>
    private void EnsureGamepadModule()
    {
        EventSystem es = EventSystem.current;
        if (es == null)
        {
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
        }

        StandaloneInputModule legacy = es.GetComponent<StandaloneInputModule>();
        if (legacy != null)
        {
            legacy.enabled = false;
            Destroy(legacy);
        }

        InputSystemUIInputModule module = es.GetComponent<InputSystemUIInputModule>();
        if (module == null)
            module = es.gameObject.AddComponent<InputSystemUIInputModule>();

        // Give a freshly-added module the default navigate/submit/cancel/point/click bindings.
        if (module.actionsAsset == null)
            module.AssignDefaultActions();
    }

    /// <summary>
    /// Turn on Automatic navigation for any Selectable that had navigation disabled, so the
    /// controller can move between them. Automatic handles both vertical and horizontal layouts.
    /// </summary>
    private void EnableNavigation(List<Selectable> selectables)
    {
        foreach (Selectable s in selectables)
        {
            if (s.navigation.mode == Navigation.Mode.None)
            {
                Navigation nav = s.navigation;
                nav.mode = Navigation.Mode.Automatic;
                s.navigation = nav;
            }
        }
    }

    private void SelectFirst(List<Selectable> selectables)
    {
        if (selectables.Count == 0 || EventSystem.current == null)
            return;
        GameObject target = selectables[0].gameObject;
        EventSystem.current.SetSelectedGameObject(target);
        lastSelected = target;
    }

    // Active, interactable Selectables sorted top-to-bottom so "first" is the top button.
    private List<Selectable> GetActiveSelectables()
    {
        var list = new List<Selectable>();
        foreach (Selectable s in FindObjectsByType<Selectable>(FindObjectsSortMode.None))
        {
            if (s.isActiveAndEnabled && s.interactable)
                list.Add(s);
        }
        list.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        return list;
    }
}
