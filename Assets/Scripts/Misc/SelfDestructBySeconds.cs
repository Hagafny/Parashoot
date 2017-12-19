using UnityEngine;
using System.Collections;

public class SelfDestructBySeconds : MonoBehaviour
{

    public int SecondsToLive = 3;
    void Start()
    {
        Destroy(gameObject, SecondsToLive);
    }

}
