using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class MadCowEffect : MonoBehaviour
{
    public Action<bool> MadCow;
    int madCowInstances = 0;

    CowStats stats;

    void Awake()
    {
        stats = GetComponent<CowStats>();
    }

    void Start()
    {
        MadCow += MadCowAction;
    }
    // the coroutine makes the player be in mad cow status for the input seconds
    IEnumerator MadCowCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        madCowInstances--;
        if (MadCow != null && madCowInstances == 0)
            MadCow(false);
    }


    public void activateMadCow()
    {
        if (MadCow != null && madCowInstances == 0)
            MadCow(true);

        madCowInstances++;
        StartCoroutine(MadCowCoroutine(stats.madCowTime));
    }

    private void MadCowAction(bool madCowStatus)
    {
        stats.invertedMovement = madCowStatus;
    }
}
