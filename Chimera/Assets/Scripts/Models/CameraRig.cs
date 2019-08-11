using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    #region Private fields
    private Camera Camera;
    #endregion

    #region Unity methods
    private void Start()
    {
        Camera = GetComponent<Camera>();
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Retrieve a 2D point in the world using current mouse position.
    /// </summary>
    /// <returns></returns>
    public Vector2 PointFromRaycast()
    {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        float dst = ray.origin.y - ray.direction.y;
        Vector3 pos = (ray.origin + ray.direction * dst) + Vector3.up * transform.position.y;

        return new Vector2(pos.x, pos.z);
    }
    #endregion
}