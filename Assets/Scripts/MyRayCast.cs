using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class MyRayCast : MonoBehaviour
{
    public Camera cam;
    public Vector2 myRes = new Vector2(800, 800);

    public List<Light> lights = new List<Light>();

    public bool showRayFromCam = false;

    public bool bakeToImage = false;
    public bool save = false;

    private bool rayFinished = false;
    private Texture2D rayTex;

    private void Update()
    {
        if (cam)
        {
            if (bakeToImage)
            {
                print("Start");
                rayFinished = false;
                bakeToImage = false;
                rayTex = new Texture2D((int)myRes.x, (int)myRes.y);

                Matrix4x4 c2w = cam.cameraToWorldMatrix;
                float imgAspect = myRes.x / myRes.y;
                float tanAlpha = Mathf.Tan(cam.fieldOfView / 2 * Mathf.PI / 180.0f);
                Vector3 eyePos = cam.transform.position;
                for (int i = 0; i < myRes.x; i++)
                {
                    // screen x
                    float rx = (((i + 0.5f) / myRes.x) * 2 - 1) * tanAlpha * imgAspect;
                    for (int j = 0; j < myRes.y; j++)
                    {
                        // screen x
                        float ry = -1 * (((j + 0.5f) / myRes.y) * 2 - 1) * tanAlpha;
                        // ray direction
                        // camera to world
                        Vector3 rd = c2w.MultiplyVector(new Vector3(rx, ry, -1));

                        // ray color
                        Color color = ColorFromRay(eyePos ,rd, iterationCount);
                        rayTex.SetPixel(i, j, color);
                        rayTex.Apply();
                    }
                }

                rayFinished = true;
                print("Done");
            }
            if (save && rayFinished)
            {
                SaveTexture();
            }
        }   
    }

    static public int iterationCount = 5;
    static Color RayToLight(Vector3 hitObjectPos, Vector3 lightPos)
    {
        RaycastHit hit;
        Vector3 hitToLight = (lightPos - hitObjectPos).normalized;
        if (Physics.Raycast(hitObjectPos, hitToLight, out hit, 1000.0f))
        {
            if ((lightPos - hit.point).magnitude < 1e-5)
                return Color.white;
        }
        return Color.black;
    }
    /*static*/ Color ColorFromRay(Vector3 eyePos, Vector3 rayDirection, int iterCount)
    {
        // should be ambient light?
        Color color = Color.black;
        if (iterCount-- == 0)
            return color;
        RaycastHit hit;
        if (Physics.Raycast(eyePos, rayDirection, out hit, 1000.0f))
        {
            // test
            // original obj color on hit point
            bool getHitColor = false;
            Color hitColor = Color.black;
            //return color;
            // shadow ray
            // hit point ray to every light
            foreach (Light light in lights)
            {
                // light.transform.position
                Color lightColor = light.color * RayToLight(hit.point, light.transform.position);
                if (lightColor != Color.black)
                {
                    if (!getHitColor)
                    {
                        hitColor = ColorFromHitPoint.ColorFromHit(hit);
                    }
                    color += lightColor * hitColor;
                }
            }
            // if hit object is mirror
            bool mirror = false;
            if (mirror)
            {
                // 鏡子
                int newIter = iterationCount;
                Color mirrorColor = Color.white;
                Vector3 reflectRayDirection = RotationFromNormal.VectorRotateAroundAxis(-rayDirection, hit.normal, 180);
                color += mirrorColor * ColorFromRay(hit.point, reflectRayDirection, iterCount);
            }
            // if hit object is transparent
            bool transparent = false;
            if (transparent)
            {
                // 透明
                Color transColor = Color.white;
                // 需計算折射ray方向
                Vector3 refractRayDirection = rayDirection;
                color += transColor * ColorFromRay(hit.point, refractRayDirection, iterCount);
            }
        }
        return color;
    }

    public void SaveTexture()
    {
        if (rayTex)
        {
            byte[] _bytes = rayTex.EncodeToPNG();
            string dirPath = Application.dataPath + "/Out/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            System.IO.File.WriteAllBytes(dirPath + "result.png", _bytes);
            rayTex = null;
            save = false;
        }
    }

    List<Vector3> RayDirection()
    {
        List<Vector3> rd = new List<Vector3>();
        Matrix4x4 c2w = cam.cameraToWorldMatrix;
        float imgAspect = myRes.x / myRes.y;
        float tanAlpha = Mathf.Tan(cam.fieldOfView / 2 * Mathf.PI / 180.0f);
        Vector3 eyePos = cam.transform.position;
        for (int i = 0; i < myRes.x; i++)
        {
            float rx = (((i + 0.5f) / myRes.x) * 2 - 1) * tanAlpha * imgAspect;
            for (int j = 0; j < myRes.y; j++)
            {
                float ry = -1 * (((j + 0.5f) / myRes.y) * 2 - 1) * tanAlpha;
                Vector3 w = c2w.MultiplyVector(new Vector3(rx, ry, -1));
                rd.Add(w.normalized);
            }
        }
        return rd;
    }

    private void OnDrawGizmos()
    {
        if (cam)
        {
            if (showRayFromCam)
            {
                Gizmos.color = Color.white;
                List<Vector3> rds = RayDirection();
                Vector3 eyePos = cam.transform.position;
                foreach (Vector3 rd in rds)
                {
                    Vector3 rayTo = eyePos + rd;
                    Gizmos.DrawLine(eyePos, rayTo);
                }
            }
        }
    }
}
