using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera cam;

    public UIManager um;

    //強制讀prefab 讀檔路徑需改
    public GameObject model;
    public GameObject cone;

    //private List<GameObject> _lightObjects = new List<GameObject>();
    //private List<GameObject> _objects = new List<GameObject>();
    private Dictionary<GameObject, RayTracingObject> _lightObjects = new Dictionary<GameObject, RayTracingObject>();
    private Dictionary<GameObject, RayTracingObject> _objects = new Dictionary<GameObject, RayTracingObject>();

    Texture2D tex;
    Rect rect = new Rect(0, 0, 0, 0);

    private bool _hidePanel = true;

    public Transform customScene;

    private void Start()
    {
        if (customScene)
        {
            AddCustomToList();
        }
    }

    private void AddCustomToList()
    {
        OutputInfo outputInfo = customScene.GetComponent<OutputInfo>();
        um._parser.outputInfo.Resolution = outputInfo.Resolution;
        um._parser.outputInfo.ImageName = outputInfo.ImageName;
        int childCount = customScene.childCount;
        for(int i = 0; i < childCount; i++)
        {
            Transform child = customScene.GetChild(i);
            if (child.gameObject.activeInHierarchy == false)
                continue;
            RayTracingObject rto = child.GetComponent<RayTracingObject>();
            if(rto.l_type != LightType.none)
            {
                Light tmpLight = child.GetComponent<Light>();
                rto.pos = tmpLight.transform.position;
                rto.lightColor = new Vector3(tmpLight.color.r, tmpLight.color.g, tmpLight.color.b);
                if (tmpLight.type == UnityEngine.LightType.Area)
                {
                    rto.whrInfo = tmpLight.areaSize;
                    rto.nsample = 3;
                    rto.l_type = LightType.area;
                }
                else if (tmpLight.type == UnityEngine.LightType.Point)
                    rto.l_type = LightType.point;
                else
                    print("None Light Type !");
                _lightObjects[child.gameObject] = rto;
            }
            else
            {
                _objects[child.gameObject] = rto;
            }
        }
    }

    private void OnGUI()
    {
        if (rect.width != 0 && rect.height != 0)
            GUI.DrawTexture(rect, tex);
    }

    private void Update()
    {
        if (rect.width != 0 && rect.height != 0)
            tex.Apply();
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _hidePanel = !_hidePanel;
            um.gameObject.SetActive(_hidePanel);
        }
    }

    private void CreateLight(RayTracingObject robj)
    {
        GameObject lightObj = new GameObject();
        lightObj.transform.position = robj.pos;
        
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.color = new Color(robj.lightColor.x, robj.lightColor.y, robj.lightColor.z);

        if(robj.l_type == LightType.point)
        {
            lightComp.type = UnityEngine.LightType.Point;
            lightObj.name = "PointLight";
            _lightObjects[lightObj] = robj;
        }
        else if (robj.l_type == LightType.area)
        {
            lightComp.type = UnityEngine.LightType.Area;
            lightObj.name = "AreaLight";
            lightComp.areaSize = new Vector2(robj.whrInfo.x, robj.whrInfo.y);
            _lightObjects[lightObj] = robj;
        }
        else
            print("Create Light Object Failed!");
    }

    private Texture2D LoadImage(string filepath)
    {
        Texture2D tex = null;
        byte[] fileData;
        if (File.Exists(filepath))
        {
            fileData = File.ReadAllBytes(filepath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        else
            tex = Texture2D.blackTexture;
        return tex;
    }

    private void CreateObject(RayTracingObject robj)
    {
        GameObject obj = null;
        if (robj.o_type == ObjectType.sphere)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = robj.pos;
            obj.transform.eulerAngles = robj.rot;
            //default radius 0.5
            float mult = robj.whrInfo.z / 0.5f;
            obj.transform.localScale = robj.scale * mult;
        }
        else if (robj.o_type == ObjectType.cylinder)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.transform.position = robj.pos;
            obj.transform.eulerAngles = robj.rot;
            //delete default collider
            Destroy(obj.GetComponent<SphereCollider>());
            obj.AddComponent<MeshCollider>();
            obj.GetComponent<MeshCollider>().convex = true;
            //default yminmax +-1 radius 0.5
            float mult = robj.whrInfo.z / 0.5f;
            obj.transform.localScale = new Vector3(robj.scale.x * mult, robj.scale.y * robj.yminmax.y, robj.scale.z * mult);
        }
        else if (robj.o_type == ObjectType.cone)
        {
            //default radius 1  height 2
            obj = Instantiate(cone, robj.pos, Quaternion.Euler(robj.rot));
            Vector3 mult = new Vector3(robj.whrInfo.z, robj.whrInfo.y / 2.0f, robj.whrInfo.z);
            obj.transform.localScale = new Vector3(robj.scale.x * mult.x, robj.scale.y * mult.y, robj.scale.z * mult.z);
        }
        else if (robj.o_type == ObjectType.plane)
        {
            //default size 10*10
            obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Vector3 mult = new Vector3(robj.whrInfo.x / 10f, 1, robj.whrInfo.y / 10f);
            obj.transform.position = robj.pos;
            obj.transform.eulerAngles = robj.rot;
            obj.transform.localScale = new Vector3(robj.scale.x * mult.x, robj.scale.y, robj.scale.z * mult.z);
        }
        else if (robj.o_type == ObjectType.mesh)
        {
            //string rootFolder = Directory.GetParent(mattr.meshName).FullName;
            //loader.Load(rootFolder, Path.GetFileName(mattr.meshName));

            //blender object
            obj = Instantiate(model, robj.pos, Quaternion.Euler(new Vector3(robj.rot.x, 180, robj.rot.z)));
            obj.transform.localScale = robj.scale;
        }
        if(robj.m_type == MaterialType.map)
        {
            Texture2D c_map = LoadImage(robj.mattr.cmapName);
            Texture2D b_map = LoadImage(robj.mattr.bmapName);
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetTexture("_MainTex", c_map);
            mat.SetTexture("_BumpMap", b_map);
            obj.GetComponent<MeshRenderer>().material = mat;
        }

        //bind raytracingInfo to GameObject
        _objects[obj] = robj;
    }

    public void ClearScene()
    {
        foreach(GameObject obj in _lightObjects.Keys)
            Destroy(obj);
        foreach (GameObject obj in _objects.Keys)
            Destroy(obj);
        _lightObjects.Clear();
        _objects.Clear();
    }

    public void GenerateScene(RayTracingInfo rtInfo, OutputInfo outputInfo)
    {
        //set screen resolution (not work in editing)
        Screen.SetResolution((int)outputInfo.Resolution.x, (int)outputInfo.Resolution.y, false);

        //set camera info
        cam.transform.position = rtInfo.cameraPos;
        cam.transform.LookAt(rtInfo.cameraCenter, rtInfo.cameraUp);
        if (rtInfo.cameraType == 1) //default : perspective
            cam.orthographic = true;
        cam.fieldOfView = rtInfo.cameraFov;

        //create scene objects
        foreach(RayTracingObject robj in rtInfo.objects)
        {
            // light type
            if(robj.o_type == ObjectType.none)
                CreateLight(robj);

            // object type
            if (robj.l_type == LightType.none)
            {
                //CreateObject(robj.o_type, robj.m_type, robj.pos, robj.rot, robj.scale, robj.yminmax, robj.whrInfo, robj.mattr);
                CreateObject(robj);
            }               
        }
    }

    public void StartRender(Parser parser)
    {
        Vector2 res = parser.outputInfo.Resolution;
        int sampleCount = parser.rayTracingInfo.sampleCount;
        if (sampleCount == 0)
            sampleCount = 1;
        else
            sampleCount = (int)Mathf.Pow(sampleCount, 0.5f);
        string outPath = Application.dataPath + "/Out/" + parser.outputInfo.ImageName;
        print(outPath);
        StartRayTrace(res, sampleCount, outPath); 
    }

    private List<Vector3> generateRayDirectionWithPixelRange(Vector2 rnageX, Vector2 rnageY, Vector2 res, int nsample)
    {
        List<Vector3> rds = new List<Vector3>();
        Matrix4x4 c2w = cam.cameraToWorldMatrix;
        float imgAspect = res.x / res.y;
        float tanAlpha = Mathf.Tan(cam.fieldOfView / 2 * Mathf.PI / 180.0f);

        if (nsample > 1)
        {
            float difX = (rnageX.y - rnageX.x) / nsample * 0.5f;
            float difY = (rnageY.y - rnageY.x) / nsample * 0.5f;

            for (int i = 0; i < nsample; i++)
            {
                float offsetX = difX * i;
                for (int j = 0; j < nsample; j++)
                {
                    float offsetY = difY * j;
                    // screen x
                    float rx = (((rnageX.x + 0.5f + offsetX) / res.x) * 2 - 1) * tanAlpha * imgAspect;
                    // screen x
                    float ry = -1 * (((rnageY.x + 0.5f + offsetY) / res.y) * 2 - 1) * tanAlpha;
                    // ray direction
                    // camera to world
                    Vector3 rd = c2w.MultiplyVector(new Vector3(rx, ry, -1));
                    rds.Add(rd);
                }
            }
        }
        else
        {
            // screen x
            float rx = (((rnageX.x + 0.5f) / res.x) * 2 - 1) * tanAlpha * imgAspect;
            // screen x
            float ry = -1 * (((rnageY.x + 0.5f) / res.y) * 2 - 1) * tanAlpha;
            // ray direction
            // camera to world
            Vector3 rd = c2w.MultiplyVector(new Vector3(rx, ry, -1));
            rds.Add(rd);
        }
        return rds;
    }

    private void StartRayTrace(Vector2 res, int sampleCount, string outPath)
    {
        Texture2D rayTex = new Texture2D((int)res.x, (int)res.y);
        Vector3 eyePos = cam.transform.position;

        // if update, you should update weights below.!!!!!!!!!!!!!!!!!!!!!!
        int maxIteration = 10;
        int resx = (int)res.x;
        int resy = (int)res.y;
        for (int i = 0; i < resx; i++)
        {
            Vector2 rangeX = new Vector2(i, i + 1);
            for (int j = 0; j < resy; j++)
            {
                Vector2 rangeY = new Vector2(j, j + 1);
                List<Vector3> rds = generateRayDirectionWithPixelRange(rangeX, rangeY, res, sampleCount);
                if (rds.Count == 0)
                    continue;
                Color color = Color.black;
                foreach (Vector3 rd in rds)
                {
                    // ray color
                    color += PixelColor(eyePos, rd, maxIteration, 1, RotationFromNormal.Space.AirTo);
                    //print(color);
                }
                color /= rds.Count;
                rayTex.SetPixel(i, resy - j - 1, color);
                rayTex.Apply();
            }
        }

        TextureHelper.SaveImg(rayTex, outPath);
    }

    static float globalWeight = 0.9f;
    static float refractMediumAirToThing = 0.6666666666f;

    Color PixelColor(Vector3 eyePos, Vector3 ray, int maxIteration, float weight, RotationFromNormal.Space space)
    {
        Color color = Color.black;
        if (maxIteration == -1) return color;

        Color ToColor(Vector3 vec)
        {
            return new Color(vec.x, vec.y, vec.z);
        }

        Color Clamp(Color cc)
        {
            cc.r = Mathf.Min(cc.r, 1.0f);
            cc.g = Mathf.Min(cc.g, 1.0f);
            cc.b = Mathf.Min(cc.b, 1.0f);
            cc.a = Mathf.Min(cc.a, 1.0f);
            return cc;
        }

        RaycastHit hit;
        bool isHit = false;
        if (space == RotationFromNormal.Space.ThingTo)
        {
            if(Physics.Raycast(eyePos, ray, out hit, 1000.0f))
            {
                isHit = Physics.Raycast(hit.point, -ray, out hit, 1000.0f);
            }
        }
        else
        {
            isHit = Physics.Raycast(eyePos, ray, out hit, 1000.0f);
        }

        if (isHit)
        {
            var hitInfo = _objects[hit.transform.gameObject];
            // if hit object is mirror
            bool mirror = hitInfo.m_type == MaterialType.mirror;
            // if hit object is transparent
            bool transparent = hitInfo.m_type == MaterialType.transparent;
            bool shadow = !(mirror || transparent);
            // ambient
            // Color ambient;
            // diffuse and specular
            Color diffuse_specular_color = Color.black;
            Color mirrorColor = Color.black;
            Color transColor = Color.black;
            Vector3 kd = Vector3.one;
            Vector3 ks = Vector3.one;
            Vector3 normal = hit.normal;
            Vector3 hitPoint = hit.point;
            Vector3 rRay = ray;

            if (space == RotationFromNormal.Space.ThingTo)
            {
                // rRay = -ray;
                normal = -normal;
            }
            hitPoint = hitPoint + normal * 0.01f;
            if (hitInfo.m_type == MaterialType.kdks)
            {
                kd = hitInfo.mattr.kd;
                ks = hitInfo.mattr.ks;
            }

            if (shadow)
            {
                Color hitColor = ColorFromHitPoint.ColorFromHit(hit);
                // color = hitColor;
                // no use
                // int hitCount = 0;
                foreach (var lightPair in _lightObjects)
                {
                    // bool isHit = false;
                    Light light = lightPair.Key.GetComponent<Light>();
                    RayTracingObject rInfo = lightPair.Value;
                    if (rInfo.l_type == LightType.point)
                    {
                        Vector3 lightPos = rInfo.pos;
                        diffuse_specular_color += hitColor * light.color * ToColor(PointLightCoefficient(eyePos, hitPoint, normal, rInfo.pos, kd, ks/*, out isHit*/));
                    }
                    else if (rInfo.l_type == LightType.area)
                    {
                        Vector3 lightPos = rInfo.pos;
                        diffuse_specular_color += hitColor * light.color * ToColor(AreaLightCoefficient(eyePos, hitPoint, normal, rInfo.pos, kd, ks/*, out isHit*/, light.areaSize, rInfo.nsample));
                    }
                }
                //print(diffuse_specular_color);
                //diffuse_specular_color.r = Mathf.Min(diffuse_specular_color.r, 1.0f);
                //diffuse_specular_color.g = Mathf.Min(diffuse_specular_color.g, 1.0f);
                //diffuse_specular_color.b = Mathf.Min(diffuse_specular_color.b, 1.0f);
                //if (hitCount != 0)
                //    diffuse_specular_color /= hitCount;
            }

            if (mirror || transparent)
                maxIteration--;
            if (mirror || transparent)
            {
                Vector3 reflectDir = RotationFromNormal.VectorRotateAroundAxis(-rRay, normal, 180);
                mirrorColor = PixelColor(hitPoint, reflectDir, maxIteration, weight, space);
            }
            if (transparent)
            {
                Vector3 refractDir = RotationFromNormal.RefractRayDirectino(-rRay, normal, refractMediumAirToThing, space);
                if (space == RotationFromNormal.Space.AirTo)
                    space = RotationFromNormal.Space.ThingTo;
                else
                    space = RotationFromNormal.Space.AirTo;
                //weight *= globalWeight;
                transColor = PixelColor(hitPoint, refractDir, maxIteration, weight, space);
                // mirrorColor *= 
            }
            color = diffuse_specular_color + mirrorColor + transColor;
            color = Clamp(color);
            color *= weight;
        }
        return color;
    }

    Vector3 AreaLightCoefficient(Vector3 eye, Vector3 hitPos, Vector3 hitNormal, Vector3 lightPos, Vector3 kd, Vector3 ks/*, out bool isHit*/, Vector2 area, int nsample)
    {
        Vector3 co = Vector3.zero;
        //bool areaHit = false;
        if (nsample == 0)
            nsample = 1;
        Vector2 halfArea = area / (2 * nsample);
        Vector3 startPos = lightPos + new Vector3(-halfArea.x, -halfArea.y, 0);
        Vector2 randRange = halfArea / nsample;
        for (int i = 0; i < nsample; i++)
        {
            Vector2 randX = new Vector2(randRange.x * i, randRange.y * (i + 1));
            for (int j = 0; j < nsample; j++)
            {
                Vector2 randY = new Vector2(randRange.x * j, randRange.y * (j + 1));
                Vector3 rand = startPos + new Vector3(Random.Range(randX.x, randX.y), Random.Range(randY.x, randY.y), 0);
                co += PointLightCoefficient(eye, hitPos, hitNormal, rand, kd, ks/*, out areaHit*/);
            }
        }
        co = co / (nsample * nsample);
        //isHit = areaHit;
        return co;
    }

    Vector3 PointLightCoefficient(Vector3 eye, Vector3 hitPos, Vector3 hitNormal, Vector3 lightPos, Vector3 kd, Vector3 ks/*, out bool isHit*/)
    {
        hitNormal.Normalize();
        Vector3 lightDir = (lightPos - hitPos).normalized;
        Vector3 viewDir = (eye - hitPos).normalized;

        RaycastHit hit;
        float disTolight = (hitPos - lightPos).magnitude;
        if (Physics.Raycast(hitPos, lightDir, out hit, disTolight))
        {
            //isHit = false;
            return Vector3.zero;
        }

        float diffuse = Mathf.Max(Vector3.Dot(hitNormal, lightDir), 0.0f);

        float specularStrength = 0.5f;
        Vector3 lightReflectDir = RotationFromNormal.VectorRotateAroundAxis(-lightDir, hitNormal, 180);
        float specular = specularStrength * Mathf.Pow(Mathf.Max(Vector3.Dot(viewDir, lightReflectDir), 0.0f), 2);
        //isHit = true;

        return kd * diffuse + ks * specular;
    }

    //recursive get color
    private Color ShootRay(Ray r)
    {
        return new Color(1,1,0);
    }

    public void StartRender(RayTracingInfo rtInfo, OutputInfo outputInfo)
    {
        int sample = rtInfo.sampleCount;
        int sq_sample = (int)Mathf.Sqrt(sample);

        // for test
        Vector2 resol = new Vector2(20, 20);
        //Vector2 resol = outputInfo.Resolution;
        Vector2 scResol = new Vector2(Screen.width, Screen.height);

        Vector2 ratio = new Vector2(scResol.x / resol.x, scResol.y / resol.y);
        Vector2 s_ratio = new Vector2(ratio.x / sq_sample, ratio.y / sq_sample);

        rect = new Rect(0, 0, resol.x, resol.y);
        tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        for (int w = 0; w < resol.x; w++)
        {
            for (int h = 0; h < resol.y; h++)
            {
                Vector2 rayPoint = new Vector2(w * ratio.x, h * ratio.y);
                Color pixelColor = new Color(0, 0, 0);
                for (int sw = 0; sw < sq_sample; sw++)
                {
                    for (int sh = 0; sh < sq_sample; sh++)
                    {
                        Vector2 scRayPoint = new Vector2(rayPoint.x + sw * s_ratio.x, rayPoint.y + sh * s_ratio.y);
                        Ray ray = cam.ScreenPointToRay(scRayPoint);
                        // box filter
                        pixelColor += ShootRay(ray) / sample;
                    }
                }
                TextureHelper.SetPixel(tex, w, h, pixelColor);
                // give current pixel color
                /*
                 
                */
            }
        }
        TextureHelper.SaveImg(tex, "Img/output.png");
    }

    public void StartRenderRect(RayTracingInfo rtInfo, OutputInfo outputInfo)
    {

        int sample = rtInfo.sampleCount;
        int sq_sample = (int)Mathf.Sqrt(sample);

        rect = new Rect(200, 200, 400, 400);
 
        Vector2 s_ratio = new Vector2(1f / sq_sample, 1f / sq_sample);

        tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        for (int w = (int)rect.x; w < (int)rect.x + rect.width; w++) 
        {
            for (int h = (int)rect.y; h < (int)rect.y + rect.height; h++)
            {
                Vector2 rayPoint = new Vector2(w, h);
                Color pixelColor = new Color(0, 0, 0);
                for (int sw = 0; sw < sq_sample; sw++)
                {
                    for (int sh = 0; sh < sq_sample; sh++)
                    {
                        Vector2 scRayPoint = new Vector2(rayPoint.x + sw * s_ratio.x, rayPoint.y + sh * s_ratio.y);
                        Ray ray = cam.ScreenPointToRay(scRayPoint);
                        // box filter
                        pixelColor += ShootRay(ray) / sample;
                    }
                }
                TextureHelper.SetPixel(tex, w, h, pixelColor);
                // give current pixel color
                /*
                 
                */
            }
        }

        TextureHelper.SaveImg(tex, "Img/output.png");
    }
}
