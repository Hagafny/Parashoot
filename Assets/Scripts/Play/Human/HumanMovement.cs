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

        // Arena X bounds are derived from both players' EndingPoint anchors so either cow can
        // roam the full width between them (and cross into the other's side), not just its own column.
        int otherPlayerNumber = stats.playerNumber == 1 ? 2 : 1;
        float myX = m_CowEndingPoint.position.x;
        float otherX = GameObject.Find("EndingPoint" + otherPlayerNumber).transform.position.x;
        stats.xMinBoundary = Mathf.Min(myX, otherX);
        stats.xMaxBoundary = Mathf.Max(myX, otherX);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Calcaulte Vertical/Horizontal movement based on our axes. Multiply by speed and deltatime to add speed and smoothness respectively.
        //Keyboard (legacy axis) and gamepad (new Input System) are summed so either can drive the cow.
        float VerticalMovementAxis = Mathf.Clamp(
            Input.GetAxis(GetPlayerAxis("Vertical")) + ControllerInput.GetVertical(stats.playerNumber), -1f, 1f);
        //  float VerticalMovementAxis = movementController.GetVerticalMovement();
        float VerticalMovement = VerticalMovementAxis * stats.movementSpeed * Time.deltaTime * InvertedFactor;

        float HorizontalMovementAxis = Mathf.Clamp(
            Input.GetAxis(GetPlayerAxis("Horizontal")) + ControllerInput.GetHorizontal(stats.playerNumber), -1f, 1f);
        float HorizontalMovement = HorizontalMovementAxis * stats.movementSpeed * Time.deltaTime * InvertedFactor;

        Vector2 pos = m_Rigidbody.position;

        float RotationMovementAxis = Mathf.Clamp(
            Input.GetAxis(GetPlayerAxis("Rotation")) + ControllerInput.GetRotation(stats.playerNumber), -1f, 1f);
        float RotationMovement = RotationMovementAxis * stats.rotationSpeed * Time.deltaTime * 15 * InvertedFactor;
        transform.Rotate(Vector3.forward * RotationMovement);

        //Clamp the position so the cow stays within the arena bounds.
        pos.y = Mathf.Clamp(pos.y + VerticalMovement, stats.yMinBounadry, stats.yMaxBounadry);
        pos.x = Mathf.Clamp(pos.x + HorizontalMovement, stats.xMinBoundary, stats.xMaxBoundary);
        m_Rigidbody.position = pos;

        //Need to raise this action for the Flame animation. CowAnimation will listen to it.
        if (GoingUp != null)
            GoingUp(IsGoingUp(VerticalMovementAxis));
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

    private bool IsGoingUp(float verticalAxis)
    {
        // Effective direction after accounting for the mad-cow inverted controls.
        bool axisUp = verticalAxis * InvertedFactor > 0.1f;
        bool buttonUp = (!stats.invertedMovement && Input.GetButton(GetPlayerAxis("Up"))) || (stats.invertedMovement && Input.GetButton(GetPlayerAxis("Down")));
        return axisUp || buttonUp;
    }
}