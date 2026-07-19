using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CowStats))]
public class HumanMovement : Movement
{

  //  public Action<bool> GoingUp; //We need this for the flame animation

    CowStats stats;
    private Rigidbody2D m_Rigidbody;
    private Transform m_CowEndingPoint; // I need the m_CowEndingPoint to know where to set the boundries to.

    // Current vertical speed in units/sec, already sign-corrected for mad-cow inversion.
    // Movement used to be instantaneous (the cow snapped straight to full speed on keypress),
    // which is what made dodging a pure twitch reflex. Ramping it means a direction change
    // costs time, so good positioning has to be planned ahead instead of corrected on reaction.
    private float m_CurrentVelocity;


    private int InvertedFactor
    {
        get
        {
            return stats.invertedMovement ? -1 : 1;
        }
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        stats = GetComponent<CowStats>();
        m_CowEndingPoint = GameObject.Find("EndingPoint" + stats.playerNumber).transform; //It wouldn't work on Awake so I put it here.
    }

    // Movement is re-enabled between rounds and after the slow-motion freeze; start from rest
    // each time so no momentum carries across a state change.
    private void OnEnable()
    {
        m_CurrentVelocity = 0f;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Get the x position of the spawn point.
        float spawnPointX = m_CowEndingPoint.position.x;

        //Calcaulte Vertical movement based on our axis.
        //Keyboard (legacy axis) and gamepad (new Input System) are summed so either can drive the cow.
        float VerticalMovementAxis = Mathf.Clamp(
            Input.GetAxis(GetPlayerAxis("Vertical")) + ControllerInput.GetVertical(stats.playerNumber), -1f, 1f);

        // Ramp toward the requested speed instead of snapping to it. Slowing down is quicker than
        // speeding up, so the cow still feels responsive to a stop and never slides like it's on ice.
        float targetVelocity = VerticalMovementAxis * stats.movementSpeed * InvertedFactor;

        // Reversing counts as braking, so a direction change pays the (fast) decel to zero and then
        // the (slow) accel back up — that asymmetry is what makes a committed direction feel committed.
        // Note Mathf.Sign(0) is 1 in Unity, hence the explicit non-zero checks.
        bool reversing = targetVelocity != 0f && m_CurrentVelocity != 0f
                         && Mathf.Sign(targetVelocity) != Mathf.Sign(m_CurrentVelocity);
        bool speedingUp = targetVelocity != 0f && !reversing
                          && Mathf.Abs(targetVelocity) > Mathf.Abs(m_CurrentVelocity);
        float rampTime = speedingUp ? GameplayTuning.AccelerationTime : GameplayTuning.DecelerationTime;
        float rampRate = stats.movementSpeed / Mathf.Max(rampTime, 0.0001f);
        m_CurrentVelocity = Mathf.MoveTowards(m_CurrentVelocity, targetVelocity, rampRate * Time.deltaTime);

        float VerticalMovement = m_CurrentVelocity * Time.deltaTime;

        Vector2 pos = m_Rigidbody.position;

        float RotationMovementAxis = Mathf.Clamp(
            Input.GetAxis(GetPlayerAxis("Rotation")) + ControllerInput.GetRotation(stats.playerNumber), -1f, 1f);
        float RotationMovement = RotationMovementAxis * stats.rotationSpeed * Time.deltaTime * 15 * InvertedFactor;
        transform.Rotate(Vector3.forward * RotationMovement);

        //Clamp the y position so the cow will have its bounderies.
        float unclampedY = pos.y + VerticalMovement;
        pos.y = Mathf.Clamp(unclampedY, stats.yMinBounadry, stats.yMaxBounadry);

        // Held against a boundary the ramp would otherwise bank invisible speed, and the cow would
        // shoot away the instant the stick reversed. Dump it so leaving a wall re-pays the ramp cost.
        if (!Mathf.Approximately(unclampedY, pos.y))
            m_CurrentVelocity = 0f;

        pos.x = Mathf.Clamp(pos.x, spawnPointX, spawnPointX);
        m_Rigidbody.position = pos;

        //Need to raise this action for the Flame animation. CowAnimation will listen to it.
        if (GoingUp != null)
            GoingUp(IsGoingUp(m_CurrentVelocity));
    }

    /// <summary>
    /// Helper method to get the axis based on the name and player's number.
    /// </summary>
    /// <param name="AxisName"></param>
    /// <returns></returns>
    private string GetPlayerAxis(string AxisName)
    {
        return AxisName + stats.playerNumber;
    }

    private bool IsGoingUp(float worldVelocity)
    {
        // worldVelocity is the cow's actual motion (mad-cow inversion already applied), so the
        // flame now follows real movement rather than raw input — it ramps in with the cow.
        bool axisUp = worldVelocity > 0.1f;
        bool buttonUp = (!stats.invertedMovement && Input.GetButton(GetPlayerAxis("Up"))) || (stats.invertedMovement && Input.GetButton(GetPlayerAxis("Down")));
        return axisUp || buttonUp;
    }
}