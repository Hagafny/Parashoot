using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CowStats))]
public class AIMovement : Movement
{
    public Transform[] Points;

    CowStats stats;
    private IEnumerator<Transform> _currentPoint;

    // Mirrors the human's acceleration ramp so the AI can't out-manoeuvre a player who now has
    // inertia. Without this the AI would reverse direction instantly and dodge shots the player
    // could never dodge.
    private float _currentSpeed;

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

        // Ramp up to the tuned speed rather than starting at full pelt, matching HumanMovement.
        // A waypoint switch (SwitchMovementSide) therefore costs the AI the same turnaround time
        // the player pays, which is what keeps a well-led shot punishing.
        float accelRate = stats.movementSpeed / Mathf.Max(GameplayTuning.AccelerationTime, 0.0001f);
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, stats.movementSpeed, accelRate * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * _currentSpeed);

        var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;

        //Need to raise this action for the Flame animation. CowAnimation will listen to it.
        if (GoingUp != null)
            GoingUp(IsGoingUp());

        if (distanceSquared < 0.1 )
            SwitchToNextPoint();
    }

    /// Advances the patrol waypoint and drops the speed ramp back to zero, so reversing costs the
    /// AI a real turnaround instead of an instant flip.
    private void SwitchToNextPoint()
    {
        _currentPoint.MoveNext();
        _currentSpeed = 0f;
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
            // Widened from 1-2s: with the slower pacing, flipping direction every second read as
            // jitter rather than as a decision the player could observe and shoot against.
            yield return new WaitForSeconds(Random.Range(1.8f, 3.4f));
            SwitchToNextPoint();
        }
    }

    private bool IsGoingUp()
    {
        return _currentPoint.Current.position.y == stats.yMaxBounadry;
    }
}