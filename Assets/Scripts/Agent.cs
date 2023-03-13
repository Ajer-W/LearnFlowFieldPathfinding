using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private List<Grid> _path;
    private bool isMoveing;
    private int _pathIndex;
    private Vector2 forceToApply;

    private Vector2 velocity = Vector2.zero;
    private int maxForce = 150;
    private int maxSpeed = 100;
    private Vector2 position;

    public void SetMvoe(List<Grid> path, Vector2[][] flowField)
    {
        this.flowField = flowField;
        _path = path;
        _pathIndex = 1;
        isMoveing = true;
    }

    private Vector2[][] flowField;

    Vector2 steeringBehaviourFlowField()
    {
        //Work out the force to apply to us based on the flow field grid squares we are on.
        //we apply bilinear interpolation on the 4 grid squares nearest to us to work out our force.
        // http://en.wikipedia.org/wiki/Bilinear_interpolation#Nonlinear

        Vector2Int floor = new Vector2Int((int)Mathf.Floor(position.x), (int)Mathf.Floor(position.y));

        //The 4 weights we'll interpolate, see http://en.wikipedia.org/wiki/File:Bilininterp.png for the coordinates
        var f00 = flowField[floor.x][floor.y];
        var f01 = flowField[floor.x][floor.y + 1];
        var f10 = flowField[floor.x + 1][floor.y];
        var f11 = flowField[floor.x + 1][floor.y + 1];

        //Do the x interpolations
        var xWeight = transform.localPosition.x - floor.x;

        var top = f00 * (1 - xWeight) + (f10 * (xWeight));
        var bottom = f01 * (1 - xWeight) + (f11 * (xWeight));

        //Do the y interpolation
        var yWeight = transform.localPosition.y - floor.y;

        //This is now the direction we want to be travelling in (needs to be normalized)
        var direction = top * (1 - yWeight) + (bottom * (yWeight)).normalized;


        //If we are centered on a grid square with no vector this will happen
        // if (isNaN(direction.length()))
        // {
        //     return Vector2.zero;
        // }

        //Multiply our direction by speed for our desired speed
        var desiredVelocity = direction * maxSpeed;

        //The velocity change we want
        var velocityChange = desiredVelocity - velocity;
        //Convert to a force
        return velocityChange * (maxForce / maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoveing)
        {
            forceToApply = steeringBehaviourFlowField();

            velocity += forceToApply * Time.deltaTime;

            //Cap speed as required
            var speed = velocity.magnitude;
            if (speed > maxSpeed)
            {
                velocity *= maxSpeed / speed;
            }

            position += velocity * Time.deltaTime;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, velocity);
            transform.rotation = rotation;
            transform.position = new Vector3(32 * (position.x + 0.5f), 32 * position.y + 0.5f);
            
            //2.
            // var seek = steeringBehaviourSeek();
            // Debug.Log(seek);
            // velocity += (seek * (Time.deltaTime));
            // Debug.Log("移动中：" + velocity);
            // //Cap speed as required
            // var speed = velocity.magnitude;
            // if (speed > maxSpeed)
            // {
            //     velocity *= (maxSpeed / speed);
            // }
            //
            // //Move a bit
            // transform.localPosition += velocity * Time.deltaTime;
            // velocity.z = 0;
            // Quaternion rotation = Quaternion.FromToRotation(Vector3.up, velocity);
            // transform.rotation = rotation;

            //1.
            // Vector3 targetPos = _path[_pathIndex].position;
            // var distanceToMove = Time.deltaTime * maxSpeed;
            // var vectorToTarget = targetPos - transform.localPosition;
            // var distanceToTarget = vectorToTarget.magnitude;
            // if (distanceToTarget < distanceToMove)
            // {
            //     transform.localPosition = targetPos;
            //     _pathIndex++;
            //     if (_pathIndex >= _path.Count)
            //     {
            //         isMoveing = false;
            //         return;
            //     }
            //
            //     //recalculate for the new destination
            //     distanceToMove -= distanceToTarget;
            //     vectorToTarget = _path[_pathIndex].position - transform.localPosition;
            //     distanceToTarget = vectorToTarget.magnitude;
            // }
            //
            // transform.localPosition += vectorToTarget.normalized * distanceToMove;
            // vectorToTarget.z = 0;
            // Quaternion rotation = Quaternion.FromToRotation(Vector3.up, vectorToTarget);
            // transform.rotation = rotation;
        }
    }
}