using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class MyRayCast : MonoBehaviour
{
    public Camera cam;

    public Vector2 myRes = new Vector2(800, 800);

    public bool printInfo = true;

    public bool showRayFromCam = false;

    public bool bakeToImage = false;
    public bool showHit = false;
    public bool clearHit = false;
    private List<Vector3> hitPoint = new List<Vector3>();

    public bool save = false;

    private bool rayFinished = false;
    private Texture2D rayTex;

    private void Update()
    {
        if (cam)
        {
            if (clearHit)
            {
                hitPoint.Clear();
                clearHit = false;
            }
            if (printInfo)
            {
                print("eye pos : " + cam.transform.position);
                print("near plane : " + cam.nearClipPlane);
            }
            if (bakeToImage)
            {
                print("Start");
                rayFinished = false;
                bakeToImage = false;
                rayTex = new Texture2D((int)myRes.x, (int)myRes.y);

                RaycastHit hit;

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
                        Vector3 rd = c2w.MultiplyVector(new Vector3(rx, ry, -1));

                        if (Physics.Raycast(eyePos, rd, out hit, 1000.0f))
                        {
                            hitPoint.Add(hit.point);
                            // get color from hit point
                            Material tmpMat = hit.transform.GetComponent<Renderer>().sharedMaterial;
                            Color color = Color.white;
                            Texture2D texture2D = tmpMat.mainTexture as Texture2D;
                            //print(hit.textureCoord);
                            if (texture2D)
                            {
                                Vector2 pCoord = hit.textureCoord;
                                pCoord.x *= texture2D.width;
                                pCoord.y *= texture2D.height;

                                Vector2 tiling = tmpMat.mainTextureScale;
                                color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));
                            }
                            else
                            {
                                color = tmpMat.color;
                            }
                            rayTex.SetPixel(i, j, color);
                            rayTex.Apply();
                        }
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
            if (showHit)
            {
                Gizmos.color = Color.red;
                Vector3 eyePos = cam.transform.position;
                foreach (Vector3 hp in hitPoint)
                {
                    Gizmos.DrawLine(eyePos, hp);
                }
            }
        }
    }
}
