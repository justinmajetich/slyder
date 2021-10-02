using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        FollowTarget();
    }

    void LateUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}
