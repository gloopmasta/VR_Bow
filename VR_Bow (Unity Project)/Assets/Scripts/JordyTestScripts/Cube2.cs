using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube2 : MonoBehaviour
{
    public Vector3 pointA = new Vector3(-5, 0, 0);
    public Vector3 pointB = new Vector3(5, 0, 0);
    public float speed = 1f;

    private float t = 0f;

    void Update()
    {
        t += Time.unscaledDeltaTime * speed;
        float lerpValue = Mathf.PingPong(t, 1f);
        transform.position = Vector3.Lerp(pointA, pointB, lerpValue);
    }
}
