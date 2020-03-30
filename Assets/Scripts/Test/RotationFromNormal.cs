using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RotationFromNormal : MonoBehaviour
{
    public enum Space{
        AirTo,
        ThingTo,
    }


    public Transform nObj;
    public Transform rObj;
    [Range(0,360)]
    public float angle = 180;
    public float reflectionLength = 20;
    public bool refraction = false;
    public bool show = true;


    public float refractMediumAirToThing = 0.6666666666f;
    private void OnDrawGizmos()
    {
        if (show && nObj && rObj)
        {
            if (!refraction)
            {
                Vector3 n = (transform.position - nObj.position).normalized;
                Vector3 r = (transform.position - rObj.position).normalized;
                Vector3 tmp = VectorRotateAroundAxis(-r, -n, angle);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, nObj.position);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, rObj.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + tmp * reflectionLength);
            }
            else
            {
                Vector3 n = (nObj.position - transform.position).normalized;
                Vector3 r = (transform.position - rObj.position).normalized;
                Vector3 refractDir = RotationFromNormal.RefractRayDirectino(-r, n, refractMediumAirToThing, RotationFromNormal.Space.AirTo);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, nObj.position);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, rObj.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + refractDir * reflectionLength);
            }
        }
    }

    /** @brief 向量依據軸向旋轉，所有向量都會被normalize
     *  @param vec      輸入向量
     *  @param axis     旋轉軸
     *  @param angle    角度，
     */
    static public Vector3 VectorRotateAroundAxis(Vector3 vec, Vector3 axis, float angle) {
        vec.Normalize();
        axis.Normalize();
        return Quaternion.AngleAxis(angle, axis) * vec;
    }

    static public Vector3 RefractRayDirectino(Vector3 inRay, Vector3 normal, float mediumAirTo, Space space)
    {
        inRay.Normalize();
        normal.Normalize();
        float medium = mediumAirTo;
        if (space == Space.ThingTo)
            medium = 1 / medium;
        //float inverseMedium = 1 / medium;
        //if (space == Space.ThingTo) {
        //    float tmp = medium;
        //    medium = inverseMedium;
        //    inverseMedium = tmp;
        //}
        // normal dot inRay
        Vector3 refract = Vector3.zero;
        float cos_theta = Mathf.Min(Vector3.Dot(normal, inRay), 1.0f);
        float sin_theta = Mathf.Pow(1 - cos_theta * cos_theta, 0.5f);
        if(medium* sin_theta > 1.0)
        {
            return VectorRotateAroundAxis(-inRay, normal, 180);
        }
        Vector3 r_out_parallel = medium * (-inRay + cos_theta * normal);
        Vector3 r_out_perp =  - Mathf.Pow(1 - r_out_parallel.sqrMagnitude, 0.5f) * normal;
        // Vector3 tmpo  = (medium * ndi - Mathf.Pow(1 - medium * medium * (1 - ndi * ndi), 0.5f)) * normal - medium * inRay;
        // tmpo = r_out_perp + r_out_parallel;
        //tmpo = medium * ((Mathf.Pow(ndi * ndi + inverseMedium * inverseMedium - 1, 0.5f) - ndi) * normal + inRay);
        return r_out_perp + r_out_parallel;
    }
}
