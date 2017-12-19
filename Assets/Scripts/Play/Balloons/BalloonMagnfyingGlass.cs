using UnityEngine;
using System.Collections;
using System;

public class BalloonMagnfyingGlass : MonoBehaviour, IBalloon
{
    public AudioClip startingSound;
    public void Pop(GameObject bullet)
    {
        bullet.GetComponent<Collider2D>().enabled = false;
        bullet.GetComponent<Animator>().SetTrigger("Magnafied");
        
    }

   public void StartBalloonSound()
    {
        GetComponent<AudioSource>().PlayOneShot(startingSound);
    }

}
