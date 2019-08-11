using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    private Camera Camera;

    private void Start()
    {
        Camera = GetComponent<Camera>();
    }

    public Vector2 PointFromRaycast()
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        float dst = ray.origin.y - ray.direction.y;
        Vector3 pos = (ray.origin + ray.direction * dst) + Vector3.up * transform.position.y;

        return new Vector2(pos.x, pos.z);
    }
}