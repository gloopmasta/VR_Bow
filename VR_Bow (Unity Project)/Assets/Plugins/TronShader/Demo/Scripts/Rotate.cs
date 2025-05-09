using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // This script lets the loot rotate around Y axis. You can set the speed
    public float RotationSpeed = 180f; // in angle for second
    public float maxHeight = 2;
    public float moveSpeed = 2;
    private float startH;
    void Start()
    {
        startH = transform.position.y;
    }

    void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * RotationSpeed);
        transform.position = new Vector3(transform.position.x,  startH  + 1 + Mathf.PingPong(Time.time * moveSpeed, maxHeight) - maxHeight/moveSpeed, transform.position.z);
    }

}
