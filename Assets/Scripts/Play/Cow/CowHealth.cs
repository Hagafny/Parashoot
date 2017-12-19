using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class CowHealth : MonoBehaviour
{
    public Action<GameObject> CowDead;
    public Action<GameObject> LostAHeart;
    CowStats stats;
    LifeBar lifeBar;

    private int m_CurrentLives; // Saves how many lives we currently have

    [HideInInspector]
    public bool m_Dead;
    [HideInInspector]
    public bool m_Shield;
    [HideInInspector]
    public bool m_Invincibe = false;

    void Awake()
    {
        stats = GetComponent<CowStats>();
        GetComponent<CowShooting>().HasTakenDamage += HasTakenDamage;
        GetComponent<ShieldEffect>().Shield += Shield;

    }

    private void OnEnable()
    {
        m_CurrentLives = stats.startingLives; // Sets the current lives we have to the starting lives int.
        m_Dead = false;

        lifeBar = GameObject.Find("GUICanvas").GetComponent<LifeBar>();

        if (lifeBar == null)
            throw new MissingComponentException("Life bar script is missing");

        lifeBar.InitializeBar(stats.startingLives);
    }

    //TODO: Fix this mess.
    private void HasTakenDamage(GameObject PlayerBeingHit)
    {
        PlayerBeingHit.GetComponent<CowHealth>().TakeDamage(1);
    }

    /// <summary>
    /// Remove livesTaken from m_CurrentLives
    /// </summary>
    /// <param name="livesTaken">How many lives to take</param>
    public void TakeDamage(int livesTaken)
    {
        if (m_Shield || m_Invincibe)
            return;

        m_CurrentLives -= livesTaken;
        m_CurrentLives = Mathf.Clamp(m_CurrentLives, 0, stats.maximumeLives); //Can't have less than 0, can't have more than m_MaximumLives.

        //Update our GUI
        RemoveGUIHeart();

        StartCoroutine(toggleInvicinbility());
        //If we have no more lives left and we're not already dead, kill.
        if (m_CurrentLives <= 0 && !m_Dead)
            OnDeath();
        else
            OnHit();
    }

    /// <summary>
    /// Adds livesAdded to m_CurrentLives
    /// </summary>
    /// <param name="livesAdded">How many lives to add</param>
    public void AddHealth(int livesAdded)
    {
        m_CurrentLives += livesAdded;
        m_CurrentLives = Mathf.Clamp(m_CurrentLives, 0, stats.maximumeLives); //Can't have less than 0, can't have more than m_MaximumLives.
        AddGUIHeart();

    }

    private IEnumerator toggleInvicinbility()
    {
        m_Invincibe = true;
        yield return new WaitForSeconds(0.2f);
        m_Invincibe = false;
    }

    private void OnDeath()
    {
        // Play the effects for the death of the cow and deactivate it.
        m_Dead = true;

        if (CowDead != null)
            CowDead(gameObject);
    }

    private void OnHit()
    {
        if (LostAHeart != null)
            LostAHeart(gameObject);
    }

    private void Shield(bool hasShield)
    {
        m_Shield = hasShield;
        m_Invincibe = hasShield;
    }



    #region GUI
    private void AddGUIHeart()
    {
        lifeBar.LifeUp(stats.playerNumber, m_CurrentLives);
    }

    private void RemoveGUIHeart()
    {
        lifeBar.LifeDown(stats.playerNumber, m_CurrentLives);
    }
    #endregion

}
