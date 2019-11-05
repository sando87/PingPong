using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;

public class TabPoint : MonoBehaviour
{
    static public float Delay = 1.5f;
    MeshFilter mf = null;
    MeshRenderer mr = null;
    Animator animator = null;
    public float AngleFactor = 1.0f;
    public float FadeOut = 1.0f;
    public float RoundFirstIn = 0.0f;
    public float RoundFirstOut = 0.0f;
    public float RoundSecondIn = 0.0f;
    public float RoundSecondOut = 0.0f;
    int AngleStepCount = 36;
    Vector3[] Circle = null;
    Material mMaterial;

    public TabInfo TapInfo { get; set; }
    private void Start()
    {
        mMaterial = GetComponent<Renderer>().material;
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        animator = GetComponent<Animator>();
        //InitCircleVertices();
        //UpdateCircleMesh();
        InitMesh();
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateCircleMesh();
        UpdateTP();
    }
    public bool IsFinalTab()
    {
        return TapInfo.idxStepToNext < 0 ? true : false;
    }
    public void CleanHit()
    {
        animator.SetTrigger("Touch");
        mr.material.color = Color.white;
    }
    public void OnEventAnimationEnd()
    {
        Destroy(gameObject);
    }

    public void InitCircleVertices()
    {
        List<Vector3> verticies = new List<Vector3>();
        float stepAngle = 360f / AngleStepCount;
        for (int idx = 0; idx <= AngleStepCount; idx++)
        {
            float angle = stepAngle * idx;
            float radian = angle * Mathf.Deg2Rad;
            verticies.Add(new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0));
        }
        Circle = verticies.ToArray();
    }
    public bool UpdateCircleMesh()
    {
        if (mf == null || AngleFactor <= 0)
            return false;

        float In1 = RoundFirstIn;
        float Out1 = RoundFirstOut;
        float In2 = RoundSecondIn;
        float Out2 = RoundSecondOut;

        List<Vector3> circle = new List<Vector3>();
        float stepAngle = 360f / AngleStepCount;
        for (int idx = 0; idx <= AngleStepCount; idx++)
        {
            float angle = stepAngle * idx * AngleFactor;
            float radian = angle * Mathf.Deg2Rad;
            circle.Add(new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0));
        }
        Circle = circle.ToArray();

        List<Vector3> verticies = new List<Vector3>();
        for (int idx = 0; idx < Circle.Length; idx++)
        {
            verticies.Add(Circle[idx] * In1);
        }
        for (int idx = 0; idx < Circle.Length; idx++)
        {
            verticies.Add(Circle[idx] * Out1);
        }
        for (int idx = 0; idx < Circle.Length; idx++)
        {
            verticies.Add(Circle[idx] * In2);
        }
        for (int idx = 0; idx < Circle.Length; idx++)
        {
            verticies.Add(Circle[idx] * Out2);
        }

        List<int> indicies = new List<int>();
        int n = Circle.Length;
        for (int idx = 0; idx < n-1; idx++)
        {
            indicies.AddRange(new int[3] { idx, n + idx + 1, n + idx });
            indicies.AddRange(new int[3] { idx, idx + 1, n + idx + 1 });
        }
        for (int idx = 0; idx < n-1; idx++)
        {
            indicies.AddRange(new int[3] { 2 * n + idx, 3 * n + idx + 1, 3 * n + idx    });
            indicies.AddRange(new int[3] { 2 * n + idx, 2 * n + idx + 1, 3 * n + idx + 1});
        }

        mf.mesh.vertices = verticies.ToArray();
        mf.mesh.triangles = indicies.ToArray();
        Color color = mr.material.color;
        color.a = FadeOut;
        mr.material.color = color;
        return true;
    }
    public bool InitMesh()
    {
        if (mf == null || mMaterial == null)
            return false;


        int stepCount = 36;
        Vector2[] factors = new Vector2[(stepCount + 1) * 4];

        List<Vector3> circleList = new List<Vector3>();
        for (int idx = 0; idx <= stepCount; idx++)
        {
            circleList.Add(new Vector3(1, 0, 0));
            factors[(stepCount + 1) * 0 + idx] = new Vector2((float)idx / stepCount, 0);
            factors[(stepCount + 1) * 1 + idx] = new Vector2((float)idx / stepCount, 1);
            factors[(stepCount + 1) * 2 + idx] = new Vector2((float)idx / stepCount, 2);
            factors[(stepCount + 1) * 3 + idx] = new Vector2((float)idx / stepCount, 3);
        }
        Vector3[] circles = circleList.ToArray();

        List<Vector3> verticies = new List<Vector3>();
        verticies.AddRange(circles);
        verticies.AddRange(circles);
        verticies.AddRange(circles);
        verticies.AddRange(circles);

        List<int> indicies = new List<int>();
        int n = circles.Length;
        for (int idx = 0; idx < n - 1; idx++)
        {
            indicies.AddRange(new int[3] { idx, n + idx + 1, n + idx });
            indicies.AddRange(new int[3] { idx, idx + 1, n + idx + 1 });
        }
        for (int idx = 0; idx < n - 1; idx++)
        {
            indicies.AddRange(new int[3] { 2 * n + idx, 3 * n + idx + 1, 3 * n + idx });
            indicies.AddRange(new int[3] { 2 * n + idx, 2 * n + idx + 1, 3 * n + idx + 1 });
        }


        mf.mesh.vertices = verticies.ToArray();
        mf.mesh.triangles = indicies.ToArray();
        mf.mesh.uv = factors;

        mMaterial.SetColor("_Color", Color.gray);
        //Color color = mr.material.color;
        //color.a = FadeOut;
        //mr.material.color = color;

        return true;
    }
    public void UpdateTP()
    {
        Color color = Color.gray;
        color.a = FadeOut;
        mMaterial.SetColor("_Color", color);
        mMaterial.SetVector("_Rounds", new Vector4(RoundFirstIn, RoundFirstOut, RoundSecondIn, RoundSecondOut));
        mMaterial.SetFloat("_Rotate", AngleFactor * Mathf.PI * 2);
    }
}
