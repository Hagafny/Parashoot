using UnityEngine;
using System.Collections;

public class MoveTowards : MonoBehaviour
{
    public Transform Target;

    void Update()
    {
        if (Target != null)
            transform.position = Vector2.MoveTowards(transform.position, Target.position, Time.deltaTime * 35);
    }
}
