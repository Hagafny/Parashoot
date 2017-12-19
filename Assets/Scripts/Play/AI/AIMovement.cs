using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CowStats))]
public class AIMovement : Movement
{
    public Transform[] Points;

    CowStats stats;
    private IEnumerator<Transform> _currentPoint;

    public void Start()
    {
        stats = GetComponent<CowStats>();

        _currentPoint = GetPathEnumerator();
        _currentPoint.MoveNext();

        if (_currentPoint.Current == null)
            return;

        StartCoroutine(SwitchMovementSide());
    }

    public void Update()
    {
        if (_currentPoint == null || _currentPoint.Current == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * stats.movementSpeed);

        var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;

        //Need to raise this action for the Flame animation. CowAnimation will listen to it.
        if (GoingUp != null)
            GoingUp(IsGoingUp());

        if (distanceSquared < 0.1 )
            _currentPoint.MoveNext();
    }

    private IEnumerator<Transform> GetPathEnumerator()
    {
        if (Points == null || Points.Length < 1)
            yield break;

        var direction = 1;
        var index = 0;
        while (true)
        {
            yield return Points[index];

            if (Points.Length == 1)
                continue;

            if (index <= 0)
                direction = 1;
            else if (index >= Points.Length - 1)
                direction = -1;

            index = index + direction;
        }
    }

    private IEnumerator SwitchMovementSide()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 2f));
            _currentPoint.MoveNext();
        }
    }

    private bool IsGoingUp()
    {
        return _currentPoint.Current.position.y == stats.yMaxBounadry;
    }
}