using UnityEngine;
using System.Collections;

public class CowSounds : MonoBehaviour
{

    private AudioSource audioSrc;
    private PowerUpSpawner powerUpSpawner;
    public AudioClip cowHurt;
    public AudioClip cowDead;
    public AudioClip CowMad;
    public AudioClip gotHealth;
    public AudioClip gotShield;
    public AudioClip hitShield;

    CowHealth cowHealth;
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();

        powerUpSpawner = GameObject.Find("PowerUpSpawner").GetComponent<PowerUpSpawner>();
        if (powerUpSpawner == null)
            throw new MissingComponentException("CowAnimation needs a reference to the general PowerUpSpawner so the cow could react to a power up being taken.");

        CowShooting cowShooting = GetComponent<CowShooting>();

        powerUpSpawner.PowerUpUsed += PowerUpUsed;
        //  cowShooting.HasTakenDamage += HasTakenDamage;
        cowShooting.ShooterHitShield += ShooterHitShield;
        GetComponent<MadCowEffect>().MadCow += MadCow;
        cowHealth = GetComponent<CowHealth>();
        cowHealth.LostAHeart += LostALife;
        cowHealth.CowDead += CowDead;
    }

    private void LostALife(GameObject CowBeingHit)
    {
        if (!CowBeingHit.GetComponent<CowHealth>().m_Dead)
        {
            AudioSource enemyAudioSource = CowBeingHit.GetComponent<AudioSource>();
            enemyAudioSource.PlayOneShot(cowHurt);
        }
    }

    private void ShooterHitShield(GameObject CowBeingHit)
    {
        audioSrc.PlayOneShot(hitShield);
    }

    private void MadCow(bool MadCowStatus)
    {
        if (MadCowStatus)
            audioSrc.PlayOneShot(CowMad);
    }

    private void CowDead(GameObject DeadCow)
    {
        audioSrc.PlayOneShot(cowDead);
    }

    private void PowerUpUsed(GameObject PowerUp, GameObject EffectedCow)
    {
        if (EffectedCow.transform.name != gameObject.transform.name)
            return;

        IPowerUp PowerUpBehavior = PowerUp.GetComponent(typeof(IPowerUp)) as IPowerUp;

        if (PowerUpBehavior == null)
            throw new MissingComponentException("The power ups needs to have a script that inherits from IPowerUp");

        if (PowerUpBehavior.PowerUpEffect() == PowerUpEffect.Good)
        {
            switch (PowerUpBehavior.GetPowerUpType())
            {
                case PowerUpType.Health:
                    audioSrc.PlayOneShot(gotHealth);
                    break;
                case PowerUpType.Shield:
                    audioSrc.PlayOneShot(gotShield);
                    break;
                default:
                    break;
            }
        }

    }
}
