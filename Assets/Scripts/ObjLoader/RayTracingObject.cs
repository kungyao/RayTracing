using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingObject : MonoBehaviour
{
    public ObjectType o_type = ObjectType.none;
    public LightType l_type = LightType.none;

    public MaterialType m_type = MaterialType.none;

    public Vector3 pos;
    public Vector3 rot;
    public Vector3 scale;

    // width, height, radius
    public Vector3 whrInfo;

    //for cylinder ymin, ymax
    public Vector2 yminmax;

    // for material attribute
    public MaterialAttribute mattr;

    //for light
    public Vector3 lightColor;

    //for area
    public int nsample;

    public RayTracingObject()
    {
        o_type = ObjectType.none;
        l_type = LightType.none;
        m_type = MaterialType.none;
        pos = Vector3.zero;
        rot = Vector3.zero;
        scale = Vector3.one;
        whrInfo = Vector3.zero;
        yminmax = Vector2.zero;
        mattr = new MaterialAttribute();
        lightColor = Vector3.zero;
        nsample = 0;
    }

}
