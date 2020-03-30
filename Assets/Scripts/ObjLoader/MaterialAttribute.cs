using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAttribute : MonoBehaviour
{
    public Vector3 kd;
    public Vector3 ks;

    //for color, bump map
    public string cmapName;
    public string bmapName;

    //for mesh
    public string meshName;

    public MaterialAttribute()
    {
        kd = Vector3.zero;
        ks = Vector3.zero;
        cmapName = "";
        bmapName = "";
        meshName = "";
    }
}