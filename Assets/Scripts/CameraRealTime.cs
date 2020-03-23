using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraRealTime : MonoBehaviour
{
    public Camera cam;

    public Vector3 pos;
    public Vector3 lookAtPos;
    public Vector3 upVec;

    public float fov;

    private void OnValidate()
    {
        if (cam)
        {
            cam.transform.position = pos;
            cam.transform.LookAt(lookAtPos, upVec);
            cam.fieldOfView = fov;
        }
    }
}
