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
    Color[] mBackColors = new Color[10] { Color.green, Color.yellow, Color.red, Color.magenta, Color.blue, Color.gray, Color.black, Color.cyan, Color.magenta, Color.blue };

    void Start()
    {
        mShaderMat = GetComponent<Renderer>().material;
        mBaseSize = GetComponent<MeshRenderer>().bounds.size;
        mOldRect = GetRect(transform.position, mBaseSize * 5);
        CreateBackgroundCubes(50);
        mShaderMat.SetColorArray("_Colors", mBackColors);
    }

    private void Update()
    {
        float off = Mathf.Abs(transform.position.y) * 0.1f;
        mShaderMat.SetFloat("_Off", off);

        Vector3 pos = MainCube.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;

        if(IsOutOfRange())
        {
            Rect newRect = GetRect(transform.position, mOldRect.size);
            Rect randomAreaX = new Rect(
                newRect.xMin < mOldRect.xMin ? newRect.xMin : mOldRect.xMax,
                newRect.yMin,
                Mathf.Abs(newRect.xMin - mOldRect.xMin),
                newRect.height);
            Rect randomAreaY = new Rect(
                newRect.xMin,
                newRect.yMin < mOldRect.yMin ? newRect.yMin : mOldRect.yMax,
                newRect.width,
                Mathf.Abs(newRect.yMin - mOldRect.yMin));
            bool switchArea = false;
            foreach (BackCube cube in mBackCubes)
            {
                if (newRect.Contains(cube.transform.position))
                    continue;

                UpdateRandomCubePos(cube, switchArea ? randomAreaX : randomAreaY);
                switchArea = !switchArea;
            }
            mOldRect = newRect;
        }
    }

    Rect GetRect(Vector2 center, Vector2 size)
    {
        Vector2 pos = center - size / 2;
        return new Rect(pos, size);
    }

    bool IsOutOfRange()
    {
        Rect currentRect = GetRect(transform.position, mBaseSize);
        float refX = mOldRect.size.x / 2 - currentRect.size.x * 1.5f;
        if (Mathf.Abs(mOldRect.center.x - currentRect.center.x) > refX)
            return true;
        float refY = mOldRect.size.y / 2 - currentRect.size.y * 1.5f;
        if (Mathf.Abs(mOldRect.center.y - currentRect.center.y) > refY)
            return true;
        return false;
    }

    private Vector3 GetRandomPos(Rect rect)
    {
        float maxZ = transform.position.z;
        Vector3 pos = new Vector3();
        pos.x = Random.Range(rect.xMin, rect.xMax);
        pos.y = Random.Range(rect.yMin, rect.yMax);
        pos.z = Random.Range(0, maxZ);
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
        cube.transform.position = pos;

        float size = pos.z * 0.1f;
        Vector3 scale = new Vector3(size, size, size);
        cube.transform.localScale = scale;
    }

    void CreateBackgroundCubes(int cnt)
    {
        if (prefabBackCube == null)
            return;

        for (int i = 0; i < cnt; ++i)
        {
            GameObject obj = Instantiate(prefabBackCube, new Vector3(0,0,0), Quaternion.identity);
            BackCube cube = obj.GetComponent<BackCube>();
            cube.MainCube = MainCube;
            UpdateRandomCubePos(cube, mOldRect);
            mBackCubes.Add(cube);
        }
    }
}
