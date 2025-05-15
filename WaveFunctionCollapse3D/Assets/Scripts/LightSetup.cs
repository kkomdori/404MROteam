using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class LightVisual
{
    public GameObject parent;
    public Light light;
    public Renderer quad;
    public Renderer bulb;

    public LightVisual(GameObject parent, Light light, Renderer quad, Renderer bulb)
    {
        this.parent = parent;
        this.light = light;
        this.quad = quad;
        this.bulb = bulb;
    }
}


public class LightSetup : MonoBehaviour
{

    [Header("Light options")]
    public bool addCeilingLight;
    public Color lightColor;
    public Vector3 lightObjOffset; // (0f, -0.05f, 0f)
    public float lightIntensity; // 0.2f
    public float lightRange;
    public float lightAngle; // 60f

    [Header("Bulb")]
    public float bulbSize; // 0.03f
    public float bulbIntensity;

    [Header("LightCircleEffect")]
    public Vector3 lightCircleOffset;
    public float lightCircleSize;
    public float lightCircleIntensity;
    public Material glowMat;

    [Header("ResourceManagement")]
    public float activationDistance = 15f;
    public float activationAngle = 60f;
    
    Transform cam;
    List <LightVisual> lightObjects = new List<LightVisual>();
    bool lightPadInit = false;

    #region Singleton
    public static LightSetup ls;
    private void Awake()
    {
        if (ls == null)
            ls = this;
        else
            Destroy(this);
    }
    #endregion

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        // 조명 리소스 관리
        Vector3 camPos = cam.position;
        Vector3 camFwd = cam.forward;

        //if (!lightPadInit)
        //{
        //    foreach (var obj in lightObjects)
        //    {
        //        Vector3Int gridPos = new Vector3Int((int)obj.parent.transform.position.x, (int)obj.parent.transform.position.y - 1, (int)obj.parent.transform.position.z);
        //        WFCGridCell3D cell = WFCgeneratorForCube.wfcGen.grid[gridPos.x, gridPos.y, gridPos.z];
        //        Debug.Log(cell.GetCollapsedCube().y_inverse);

        //        if (Physics.OverlapSphere(obj.quad.transform.position, 0.001f).Length <= 5)
        //            obj.quad.enabled = false;
        //    }
        //    lightPadInit = true; // 한번만 실행
        //}
        
        foreach (var obj in lightObjects)
        {
            Vector3 toLight = obj.parent.transform.position - camPos;
            float dist = toLight.magnitude;
            float angle = Vector3.Angle(camFwd, toLight.normalized);
            //new Vector3 angleLimit = quaternion.AxisAngle(Vector3.forward, activationAngle);

            bool visible = dist < activationDistance;

            obj.light.enabled = visible;
            obj.quad.enabled = !visible;

        }
    }

    public void AddCeilingLight(Vector3 pos)
    {
        if (!addCeilingLight) return;

        GameObject lightParent = new GameObject("RoomLight");
        lightParent.transform.position = pos + lightObjOffset; // 마커 아래

        // 조명 효과
        Light light = lightParent.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = lightColor;
        light.range = lightRange;
        light.intensity = lightIntensity;
        light.spotAngle = lightAngle; // 빛이 퍼지는 각도
        light.shadows = LightShadows.Hard;
        light.lightmapBakeType = LightmapBakeType.Realtime; // Baked or Mixed 적용 불가; 런타임 중 Lightmap 생성 못함

        // 가짜 조명 효과
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        circle.transform.SetParent(lightParent.transform);
        circle.transform.localPosition = lightCircleOffset;
        circle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); // 바닥에 눕힘
        circle.transform.localScale = Vector3.one * lightCircleSize;
        Destroy(circle.GetComponent<Collider>());

        Material m = new Material(glowMat);
        //Color lightColorWithAlpha = new Color(lightColor.r, lightColor.g, lightColor.b, lightCircleAlpha);
        //m.SetColor("_BaseColor", lightColorWithAlpha);
        m.SetColor("_BaseColor", lightColor);
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", lightColor * lightCircleIntensity);
        
        Renderer quadRenderer = circle.GetComponent<Renderer>();
        quadRenderer.material = m;

        // 전구 모양
        GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bulb.transform.SetParent(lightParent.transform);
        bulb.transform.localPosition = Vector3.zero;
        bulb.transform.localScale = Vector3.one * bulbSize;
        Destroy(bulb.GetComponent<Collider>());

        Renderer bulbRenderer = bulb.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", lightColor * bulbIntensity);
        bulbRenderer.material = mat;

        lightParent.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // 아래방향
        
        // 저장
        lightObjects.Add( new LightVisual (lightParent, light, quadRenderer, bulbRenderer));
    }
}
