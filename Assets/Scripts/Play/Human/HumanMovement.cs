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
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //Get the x position of the spawn point.
        float spawnPointX = m_CowEndingPoint.position.x;

        //Calcaulte Vertical movement based on our axis. Multiply by speed and deltatime to add speed and smoothness respectively.
        float VerticalMovementAxis = Input.GetAxis(GetPlayerAxis("Vertical"));
        //  float VerticalMovementAxis = movementController.GetVerticalMovement();
        float VerticalMovement = VerticalMovementAxis * stats.movementSpeed * Time.deltaTime * InvertedFactor;

        Vector2 pos = m_Rigidbody.position;

        float RotationMovementAxis = Input.GetAxis(GetPlayerAxis("Rotation"));
        float RotationMovement = RotationMovementAxis * stats.rotationSpeed * Time.deltaTime * 15 * InvertedFactor;
        transform.Rotate(Vector3.forward * RotationMovement);

        //Clamp the y position so the cow will have its bounderies.
        pos.y = Mathf.Clamp(pos.y + VerticalMovement, stats.yMinBounadry, stats.yMaxBounadry);
        pos.x = Mathf.Clamp(pos.x, spawnPointX, spawnPointX);
        m_Rigidbody.position = pos;

        //Need to raise this action for the Flame animation. CowAnimation will listen to it.
        if (GoingUp != null)
            GoingUp(IsGoingUp());
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

    private bool IsGoingUp()
    {
        return (!stats.invertedMovement && Input.GetButton(GetPlayerAxis("Up"))) || (stats.invertedMovement          && Input.GetButton(GetPlayerAxis("Down")));
    }
}