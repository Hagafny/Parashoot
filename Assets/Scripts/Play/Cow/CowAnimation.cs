using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class CowAnimation : MonoBehaviour
{
    [HideInInspector]
    public Animator headAnimator;
    public Animator bodyAnimator;
    public Animator ligterHandAnimator;
    public GameObject Flame;

    PowerUpSpawner powerUpSpawner;


    void Start()
    {
        powerUpSpawner = GameObject.Find("PowerUpSpawner").GetComponent<PowerUpSpawner>();
        if (powerUpSpawner == null)
            throw new MissingComponentException("CowAnimation needs a reference to the general PowerUpSpawner so the cow could react to a power up being taken.");

        GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gameManager == null)
            throw new MissingComponentException("CowAnimation needs a reference to the general GameController so the cow could react to the game being won.");

        gameManager.CowWon += CowWon;
        powerUpSpawner.PowerUpUsed += PowerUpUsed;
        GetComponent<CowShooting>().HasTakenDamage += HasTakenDamage;
        GetComponent<MadCowEffect>().MadCow += MadCow;
        GetComponent<CowHealth>().CowDead += CowDead;
        GetComponent<Movement>().GoingUp += GoingUp;
    }

    private void HasTakenDamage(GameObject CowBeingHit)
    {
        Animator enemyAnimator = CowBeingHit.transform.Find("CowRenderers/CowHead").GetComponent<Animator>();
        if (enemyAnimator == null)
            throw new MissingComponentException("Enemy cow needs the head animator");

        enemyAnimator.SetTrigger("GotHurt");
    }



    private void PowerUpUsed(GameObject PowerUp, GameObject EffectedCow)
    {
        if (EffectedCow.transform.name != gameObject.transform.name)
            return;

        IPowerUp PowerUpBehavior = PowerUp.GetComponent(typeof(IPowerUp)) as IPowerUp;

        if (PowerUpBehavior == null)
            throw new MissingComponentException("The power ups needs to have a script that inherits from IPowerUp");

        if (PowerUpBehavior.PowerUpEffect() == PowerUpEffect.Good)
            headAnimator.SetTrigger("GotPowerUp");
    }

    private void MadCow(bool MadCowStatus)
    {
        headAnimator.SetBool("IsMadCow", MadCowStatus);
    }

    private void CowDead(GameObject DeadCow)
    {
        headAnimator.SetTrigger("Dead");
        ligterHandAnimator.SetTrigger("Dead");
        bodyAnimator.SetTrigger("Dead");

        UnityEngine.Object blood = Resources.Load("DeathBloodFromNeck");
        Transform headTransform = headAnimator.transform;
        Vector3 newHeadPosition = new Vector3(headTransform.position.x, headTransform.position.y - 0.5f, -5);
        Instantiate(blood, newHeadPosition, new Quaternion());

    }

    private void CowWon(GameObject WinningCow)
    {
        if (WinningCow.transform.name != gameObject.transform.name)
            return;

        headAnimator.SetTrigger("Won");
    }

    private void GoingUp(bool flameOn)
    {
        Flame.SetActive(flameOn);
    }
}
