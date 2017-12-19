using UnityEngine;
using System.Collections;
using System;

public class BaloonSpawner : MonoBehaviour
{

    public GameManager gameManager;
    public BaloonOptions[] Baloons;				// Array of powerup options
    public GameObject BaseBaloon;
    public float baloonDeliveryTime = 8f;		// Delay on delivery.
    public float RangeLeft;					// Smallest value of x in world coordinates the delivery can happen at.
    public float RangeRight;                // Largest value of x in world coordinates the delivery can happen at.

    void Start()
    {
        // Start the first delivery.
        StartCoroutine(BaloonSpawn());
        RegisterToEvents();
    }


    public IEnumerator BaloonSpawn()
    {
        while (true) //Go on forever!! (This is temporary until I find a better thing to write here.
        {
            // Wait for the delivery delay.
            yield return new WaitForSeconds(baloonDeliveryTime);

            // Create a random x coordinate for the delivery in the drop range.
            float dropPosX = UnityEngine.Random.Range(RangeLeft, RangeRight);

            // Create a position with the random x coordinate.
            Vector3 dropPos = new Vector3(dropPosX, -19f, 1f);
            int baloonIndex = UnityEngine.Random.Range(0, Baloons.Length);
            // ... instantiate the base cloud at the drop position.
            Baloons[baloonIndex].Instance = Instantiate(BaseBaloon, dropPos, Quaternion.identity) as GameObject;
            Baloons[baloonIndex].Setup();
        }


    }


    void RegisterToEvents()
    {
        gameManager.ACowHasShotABalloon += ACowHasShotABalloon;

    }

    private void ACowHasShotABalloon(GameObject TheBalloon, GameObject TheBullet, GameObject ShootingCow)
    {
        RemoveBallonColliders(TheBalloon);
        MakeBalloonSymbolDisappear(TheBalloon);
        MakeBalloonFall(TheBalloon);
        MakePopSound(TheBalloon);
        AnimateBalloonPop(TheBalloon);
        ImplementBalloonBehaviour(TheBalloon, TheBullet);
        MakeSpesificBalloonSound(TheBalloon);
    }

    private void RemoveBallonColliders(GameObject TheBalloon)
    {
        Collider2D[] colliders = TheBalloon.GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false; 
        }
    }
    private void MakeBalloonSymbolDisappear(GameObject TheBalloon)
    {
        SpriteRenderer balloonSpriteRenderer = TheBalloon.transform.FindChild("Symbol").GetComponent<SpriteRenderer>();
        if (balloonSpriteRenderer == null)
        {
            throw new MissingComponentException("Balloon's symbol needs a sprite renderer");
        }

        balloonSpriteRenderer.enabled = false;
    }
    private void MakeBalloonFall(GameObject TheBalloon)
    {
        Rigidbody2D balloonRB = TheBalloon.GetComponent<Rigidbody2D>();
        if (balloonRB == null)
        {
            throw new MissingComponentException("Ballon needs a Rigidbody");
        }

        balloonRB.gravityScale = 20;
    }

    private void AnimateBalloonPop(GameObject TheBalloon)
    {
        Animator balloonAnimator = TheBalloon.GetComponent<Animator>();
        if (balloonAnimator == null)
        {
            throw new MissingComponentException("Ballon needs an animator");
        }

        balloonAnimator.SetTrigger("BalloonPopped");
    }

    private void ImplementBalloonBehaviour(GameObject TheBalloon, GameObject TheBullet)
    {
        IBalloon balloonBehaviour = TheBalloon.GetComponent(typeof(IBalloon)) as IBalloon;
        if (balloonBehaviour == null)
        {
            throw new MissingComponentException("Balloon needs a script that implements IBalloon");
        }

        balloonBehaviour.Pop(TheBullet);
    }

    private void MakePopSound(GameObject TheBalloon)
    {
        TheBalloon.GetComponent<AudioSource>().Play();
    }

    private void MakeSpesificBalloonSound(GameObject TheBalloon)
    {
        IBalloon balloonBehaviour = TheBalloon.GetComponent(typeof(IBalloon)) as IBalloon;
        if (balloonBehaviour == null)
        {
            throw new MissingComponentException("Balloon needs a script that implements IBalloon");
        }

        balloonBehaviour.StartBalloonSound();
    }
}
