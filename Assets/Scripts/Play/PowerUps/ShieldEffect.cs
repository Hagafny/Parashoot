using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class ShieldEffect : MonoBehaviour
{
    public GameObject ShieldTexture;
    public Action<bool> Shield;
    int shieldInstances = 0;
    string shieldGameObjectName = "Shield";
    Rigidbody2D rb;
    CowStats stats;
    void Awake()
    {
        stats = GetComponent<CowStats>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            throw new MissingComponentException("Shield scripts must be located inside a gameobject with a rigidbody2D");
    }
    void Start()
    {
        Shield += ShieldAction;
    }
    // the coroutine makes the player be in shield status
    IEnumerator ShieldCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        shieldInstances--;
        if (Shield != null && shieldInstances == 0)
        {
            Shield(false);
            DestroyShieldInstance();
        }
    }

    public void activateShield()
    {
        if (shieldInstances == 0)
        {
            instantiateShield();
            if (Shield != null)
                Shield(true);
        }

        shieldInstances++;
        StartCoroutine(ShieldCoroutine(stats.shieldTime));
    }

    private void instantiateShield()
    {
        GameObject shield = Instantiate(ShieldTexture, transform.position, transform.rotation) as GameObject;
        shield.transform.localPosition += Vector3.zero;
        shield.transform.name = shieldGameObjectName;
        shield.gameObject.transform.parent = transform;
    }

    private void DestroyShieldInstance()
    {
        Transform shield = transform.FindChild(shieldGameObjectName);
        Destroy(shield.gameObject);
    }

    private void ShieldAction(bool hasShield)
    {
        float mass = hasShield ? rb.mass + 100 : rb.mass - 100;
        rb.mass = mass;
    }
}
