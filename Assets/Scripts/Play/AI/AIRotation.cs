using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CowStats))]
public class AIRotation : MonoBehaviour
{
    CowStats stats;
    public Transform targetTransform;

    private int InvertedFactor
    {
        get
        {
            return stats.invertedMovement ? -1 : 1;
        }
    }

    void Start()
    {
        stats = GetComponent<CowStats>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vectorToTarget = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        //We need to reverse the Quaternion because the comwputer will be the player on the left and Q will work perectly for player 1 but needs to be reversed.
        if (stats.playerNumber == 2)
            q = Quaternion.Euler(q.eulerAngles.x, 180, -q.eulerAngles.z * InvertedFactor);

        // Was a Slerp whose factor (deltaTime * rotationSpeed * 15) exceeded 1 every frame, so the
        // AI snapped onto the player instantly and never had to lead a shot. RotateTowards makes
        // rotationSpeed mean the same thing it means for a human — degrees per second — so the AI
        // has to commit to an aim just like the player does, and can be baited off-target.
        float maxDegreesThisFrame = stats.rotationSpeed * 15f * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, q, maxDegreesThisFrame);

    }
}
