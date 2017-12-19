using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CowStats))]
public class HumanShooting : MonoBehaviour, IShooting {
    CowStats stats;
    void Start()
    {
        stats = GetComponent<CowStats>();
    }
    /// <summary>
    /// Check is the cow has shot one of her shots depending on the player's number and the shot type.
    /// </summary>
    /// <param name="shotType"></param>
    /// <returns>Whether or not we have fired rhe shot.</returns>
    public bool HasShot()
    {
        string fireButton = string.Concat("Fire", stats.playerNumber);
        return Input.GetButtonDown(fireButton);
    }
}
