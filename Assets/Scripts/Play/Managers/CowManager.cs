using UnityEngine;
using System;
/// <summary>
/// The CowManager is individual to each cow. It is managed by the global GameManager. This class does not inherit from MonoDevelop (This is why we have an attrivute of Serializable)
/// Notice we have some public fields that are use "HideInInspector". This is because I want them to be global so I could reference them from other scripts (I.E GameManager) but I don't want 
/// the user to control them from the Unity UI.
/// </summary>
[Serializable]
public class CowManager
{
    public string m_CowName;

    public Color m_PlayerColor; //The player's color. This affects both the Parachute and the "PLAYER X WON THE ROUND" color;
    public Transform m_StartingPoint; // The spawn point on the screen from which the Cow will spawn. Note that the cow will spawn in the same position and rotation as the spawnPoint.
    public Transform m_EndPoint;
    public Sprite m_CowHead;
    public Sprite m_CowTorso;
    public CowAnimationsHolder Animations;

    [HideInInspector]
    public int m_PlayerNumber; // We set this field from the GameManager after we instantiate the cows.

    [HideInInspector]
    public GameObject m_Instance; // We set this instance on the GameManager after we called the "Instantiate" method.
    [HideInInspector]
    public CowStats stats;

    [HideInInspector]
    public CowHealth m_Health;
    [HideInInspector]
    private Movement m_Movement;
    [HideInInspector]
    public CowShooting m_Shooting; // Reference to the Shooting script of the cow.  We want this so we could Enable/Disable it. This is public because I want to register to tht CowShooting Action that let's me know when a powerup was hit so I could shout to the PowerUpManager.

    /// <summary>
    /// This is called from GameManager after we instantiate the cow prefab. Here we do all the initial setup we need.
    /// </summary>
    public void Setup(CowOptions cowOptions)
    {
        m_PlayerColor = cowOptions.color;

        m_CowName = cowOptions.name;



        if (cowOptions.type == CowType.Human)
        {
            m_Instance.AddComponent<HumanShooting>();
            m_Instance.AddComponent<HumanMovement>();
        }
        else
        {
            m_Instance.AddComponent<AIShooting>();
            m_Instance.AddComponent<AIRotation>();
            m_Instance.AddComponent<AIMovement>();
        }
        

        stats = m_Instance.GetComponent<CowStats>();
        stats.playerNumber = m_PlayerNumber;

        m_Health = m_Instance.GetComponent<CowHealth>();
        m_Movement = m_Instance.GetComponent<Movement>();
        m_Shooting = m_Instance.GetComponent<CowShooting>();

        m_Instance.transform.name = "Cow" + m_PlayerNumber;

        AdjustTheCow();
    }

    /// <summary>
    /// Disables the movement and shooting of the cow. Called from GameManager
    /// </summary>
    public void DisableControl()
    {
        m_Health.enabled = false;
        m_Movement.enabled = false;
        m_Shooting.enabled = false;
    }

    /// <summary>
    /// Enable the movement and shooting of the cow. Called from GameManager
    /// </summary>
    public void EnableControl()
    {
        m_Health.enabled = true;
        m_Movement.enabled = true;
        m_Shooting.enabled = true;
    }


    //  Iterate through all the SpriteRenderer children of the cow and look for Sprite renderers that contain the string "Color". <- color them the player's color.
    // Also this is where we attach the cow head.
    private void AdjustTheCow()
    {
        SpriteRenderer[] renderers = m_Instance.GetComponentsInChildren<SpriteRenderer>();
        int renderersLength = renderers.Length;
        for (int i = 0; i < renderersLength; i++)
        {
            if (renderers[i].transform.name == "CowHead")
            {
                renderers[i].sprite = m_CowHead;

                //Override the animation controller with the specific cow's animations.
                Animator headAnimator = renderers[i].GetComponent<Animator>();

                if (headAnimator == null)
                    throw new MissingComponentException("Cow head must have an animation component");

                AnimatorOverrideController specificCowHeadAnimationController = new AnimatorOverrideController();
                specificCowHeadAnimationController.runtimeAnimatorController = headAnimator.runtimeAnimatorController;
                specificCowHeadAnimationController.name = "Player" + m_PlayerNumber + "HeadController";
                specificCowHeadAnimationController["e_hurt"] = Animations.GettingHurt;
                specificCowHeadAnimationController["e_dumb"] = Animations.GettingPowerUp;
                specificCowHeadAnimationController["e_mad"] = Animations.MadCow;
                specificCowHeadAnimationController["e_dead"] = Animations.Dying;
                specificCowHeadAnimationController["e_celebrating"] = Animations.Winning;
                headAnimator.runtimeAnimatorController = specificCowHeadAnimationController;

            }
            if (renderers[i].transform.name == "CowTorso")
            {
                renderers[i].sprite = m_CowTorso;

                //Override the animation controller with the specific cow's animations.
                Animator bodyAnimator = renderers[i].GetComponent<Animator>();

                if (bodyAnimator == null)
                    throw new MissingComponentException("Cow head must have an animation component");


                AnimatorOverrideController specificCowBodyAnimationController = new AnimatorOverrideController();
                specificCowBodyAnimationController.runtimeAnimatorController = bodyAnimator.runtimeAnimatorController;
                specificCowBodyAnimationController.name = "Player" + m_PlayerNumber + "BodyController";
                specificCowBodyAnimationController["e_bodyBlood"] = Animations.BodyDeathAnimation;

                bodyAnimator.runtimeAnimatorController = specificCowBodyAnimationController;
            }

            if (renderers[i].transform.name.Contains("Color"))
                renderers[i].color = m_PlayerColor;

        }
    }

}
