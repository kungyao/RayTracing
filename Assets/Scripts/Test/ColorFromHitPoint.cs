using UnityEngine;
using System.Collections;

public class ColorFromHitPoint : MonoBehaviour
{
    public Camera cam;

    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Mesh mesh = GetMesh(hit.transform.gameObject);
        if (mesh)
        {
            int[] hittedTriangle = new int[]
            {
                mesh.triangles[hit.triangleIndex * 3],
                mesh.triangles[hit.triangleIndex * 3 + 1],
                mesh.triangles[hit.triangleIndex * 3 + 2]
            };
            bool findSub = false;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subMeshTris = mesh.GetTriangles(i);
                for (int j = 0; j < subMeshTris.Length; j += 3)
                {
                    if (subMeshTris[j] == hittedTriangle[0] &&
                        subMeshTris[j + 1] == hittedTriangle[1] &&
                        subMeshTris[j + 2] == hittedTriangle[2])
                    {
                        findSub = true;
                        Material mat = hit.transform.GetComponent<Renderer>().sharedMaterials[i];
                        Texture2D tex = mat.mainTexture as Texture2D;

                        Color color = mat.color;
                        if (tex)
                        {
                            Vector2 pCoord = hit.textureCoord;
                            pCoord.x *= tex.width;
                            pCoord.y *= tex.height;
                            Vector2 tiling = mat.mainTextureScale;
                            color = tex.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));
                        }
                        print(color);
                        Debug.Log(string.Format("triangle index:{0} submesh index:{1} submesh triangle index:{2}", hit.triangleIndex, i, j / 3));
                    }
                }
                if (findSub)
                    break;
            }
        }
    }

    static public Color ColorFromHit(RaycastHit hit)
    {
        Mesh mesh = GetMesh(hit.transform.gameObject);
        Color color = Color.black;
        if (mesh)
        {
            int[] hittedTriangle = new int[]
{
                mesh.triangles[hit.triangleIndex * 3],
                mesh.triangles[hit.triangleIndex * 3 + 1],
                mesh.triangles[hit.triangleIndex * 3 + 2]
};
            bool findSub = false;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subMeshTris = mesh.GetTriangles(i);
                for (int j = 0; j < subMeshTris.Length; j += 3)
                {
                    if (subMeshTris[j] == hittedTriangle[0] &&
                        subMeshTris[j + 1] == hittedTriangle[1] &&
                        subMeshTris[j + 2] == hittedTriangle[2])
                    {
                        findSub = true;
                        Material mat = hit.transform.GetComponent<Renderer>().sharedMaterials[i];
                        Texture2D tex = mat.mainTexture as Texture2D;

                        color = mat.color;
                        if (tex)
                        {
                            Vector2 pCoord = hit.textureCoord;
                            pCoord.x *= tex.width;
                            pCoord.y *= tex.height;
                            Vector2 tiling = mat.mainTextureScale;
                            color = tex.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));
                        }
                        //print(color);
                        //Debug.Log(string.Format("triangle index:{0} submesh index:{1} submesh triangle index:{2}", hit.triangleIndex, i, j / 3));
                    }
                }
                if (findSub)
                    break;
            }
        }
        return color;
    }

    static public Mesh GetMesh(GameObject go)
    {
        if (go)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf)
            {
                Mesh m = mf.sharedMesh;
                if (!m) { m = mf.mesh; }
                if (m)
                {
                    return m;
                }
            }
        }
        return (Mesh)null;
    }
}