using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public GameObject MainCube;
    public GameObject prefabBackCube;
    List<BackCube> mBackCubes = new List<BackCube>();
    Vector2 mBaseSize; //World : Screen => 1:1 size rate
    Rect mOldRect;
    Material mShaderMat;
    Color[] mBackColors = new Color[]
    {
        new Color(128f/255f,    128f/255f,  128f/255f),
        new Color(39f/255f,     88f/255f,   163f/255f),
        new Color(74f/255f,     70f/255f,   143f/255f),
        new Color(206f/255f,    118f/255f,  114f/255f),
        new Color(218f/255f,    180f/255f,  98f/255f),
        new Color(20f/255f,     134f/255f,  101f/255f),
    };
    //{ Color.white, Color.black, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.gray, Color.black };

    public Color CurrentColor { get; set; }

    void Start()
    {
        mShaderMat = GetComponent<Renderer>().material;
        mBaseSize = GetComponent<MeshRenderer>().bounds.size;
        mOldRect = GetRect(transform.position, mBaseSize * 3);
        CreateBackgroundCubes(100, 0);

        mShaderMat.SetColor("_Color", mBackColors[0]);
        mShaderMat.SetFloat("_Height", Screen.height);
    }

    private void Update()
    {
        CurrentColor = LerpColor(transform.position.y);
        mShaderMat.SetColor("_Color", CurrentColor);

        Vector3 pos = MainCube.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;

        if(IsOutOfRange())
        {
            Rect newRect = GetRect(transform.position, mOldRect.size);
            foreach (BackCube cube in mBackCubes)
            {
                if (cube.gameObject.activeSelf && mOldRect.Contains(cube.transform.position))
                    continue;
        
                UpdateRandomCubePos(cube, newRect);
        
                if (mOldRect.Contains(cube.transform.position))
                    cube.gameObject.SetActive(false);
                else
                    cube.gameObject.SetActive(true);
            }
            mOldRect = newRect;
        }
    }

    Color LerpColor(float _factor)
    {
        int span = 250;
        float factor = Mathf.Abs(_factor);
        int seq = (int)(factor / span);
        if (seq % 2 == 0)
        {
            int idx = (seq / 2) % mBackColors.Length;
            return mBackColors[idx];
        }

        int idxA = (seq / 2) % mBackColors.Length;
        int idxB = (idxA + 1) % mBackColors.Length;
        Color colA = mBackColors[idxA];
        Color colB = mBackColors[idxB];
        float rate = (factor % span) / (float)span;
        Color colC = colA * (1 - rate) + colB * rate;
        return colC;
    }

    Rect GetRect(Vector2 center, Vector2 size)
    {
        Vector2 pos = center - size / 2;
        return new Rect(pos, size);
    }

    bool IsOutOfRange()
    {
        Vector2 pos = transform.position;
        float dist = (mOldRect.center - pos).magnitude;
        if (dist > mBaseSize.y)
            return true;
        return false;
    }

    private Vector3 GetRandomPos(Rect rect)
    {
        float maxZ = transform.position.z;
        Vector3 pos = new Vector3();
        pos.x = Random.Range(rect.xMin, rect.xMax);
        pos.y = Random.Range(rect.yMin, rect.yMax);
        int ran = Random.Range(0, 100);
        if(ran > 40)
            pos.z = Random.Range(0, maxZ * 0.5f);
        else if(ran > 10)
            pos.z = Random.Range(maxZ * 0.5f, maxZ * 0.75f);
        else
            pos.z = Random.Range(maxZ * 0.75f, maxZ);

        return pos;
    }

    void UpdateRandomCubePos(BackCube cube, Rect range)
    {
        Vector3 axis = new Vector3();
        axis.x = Random.Range(-1, 1);
        axis.y = Random.Range(-1, 1);
        axis.z = Random.Range(-1, 1);
        axis.Normalize();
        float angle = Random.Range(0, 360);
        cube.transform.rotation = Quaternion.AngleAxis(angle, axis);

        Vector3 pos = GetRandomPos(range);
        cube.SetPosition(MainCube, pos);
    }

    void CreateBackgroundCubes(int cnt, int disabledCount)
    {
        if (prefabBackCube == null)
            return;

        for (int i = 0; i < cnt; ++i)
        {
            GameObject obj = Instantiate(prefabBackCube, new Vector3(0,0,0), Quaternion.identity);
            BackCube cube = obj.GetComponent<BackCube>();
            cube.mBackground = this;
            UpdateRandomCubePos(cube, mOldRect);
            mBackCubes.Add(cube);
        }

        for (int i = 0; i < disabledCount; ++i)
        {
            GameObject obj = Instantiate(prefabBackCube, new Vector3(0, 0, 0), Quaternion.identity);
            BackCube cube = obj.GetComponent<BackCube>();
            UpdateRandomCubePos(cube, mOldRect);
            cube.gameObject.SetActive(false);
            mBackCubes.Add(cube);
        }
    }
}
