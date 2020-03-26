using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RotationFromNormal : MonoBehaviour
{
    public Transform nObj;
    public Transform rObj;
    [Range(0,360)]
    public float angle = 180;
    public float reflectionLength = 20;
    public bool show = true;

    private void OnDrawGizmos()
    {
        if (show && nObj && rObj)
        {
            Vector3 n = (transform.position - nObj.position).normalized;
            Vector3 r = (transform.position - rObj.position).normalized;
            Vector3 tmp = VectorRotateAroundAxis(-r, n, angle);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, nObj.position);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, rObj.position);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + tmp * reflectionLength);
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
}
