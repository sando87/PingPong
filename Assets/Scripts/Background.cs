using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public GameObject MainCube;
    public GameObject prefabBackCube;
    MeshFilter mf;
    MeshRenderer mr;
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        //CreatePlane(0, 0, 64, 256, 16);

        CreateBackgroundCubes(10, -50, -25);
        CreateBackgroundCubes(10, -25, 0);
        CreateBackgroundCubes(10, 0, 25);
        CreateBackgroundCubes(10, 25, 50);
    }

    private void Update()
    {
        Vector3 pos = MainCube.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;
        pos.z = 0;
        transform.position -= pos * 0.05f;
    }

    void CreateBackgroundCubes(int cnt, float minY, float maxY)
    {
        if (prefabBackCube == null)
            return;

        for(int i = 0; i < cnt; ++i)
        {
            Vector3 axis = new Vector3();
            axis.x = Random.Range(-1, 1);
            axis.y = Random.Range(-1, 1);
            axis.z = Random.Range(-1, 1);
            axis.Normalize();
            float angle = Random.Range(0, 360);

            GameObject obj = Instantiate(prefabBackCube, new Vector3(0,0,0), Quaternion.AngleAxis(angle, axis));
            obj.GetComponent<BackCube>().MainCube = MainCube;
            int depth = Random.Range(2, 10);
            obj.GetComponent<BackCube>().Depth = depth;

            Vector3 pos = new Vector3();
            pos.x = Random.Range(-15, 15);
            pos.y = Random.Range(minY, maxY);
            pos.z = 25;
            obj.transform.position = pos;

            float size = depth * 0.1f;
            Vector3 scale = new Vector3(size, size, size);
            obj.transform.localScale = scale;
        }
    }
    void CreatePlane(int centerX, int centerY, int width, int height, int gridSize)
    {
        int startX = centerX - width / 2;
        int startY = centerY - height / 2;
        List<Vector3> verticies = new List<Vector3>();
        for(int y = 0; y <= height; y += gridSize)
        {
            for (int x = 0; x <= width; x += gridSize)
            {
                Vector3 vert = new Vector3();
                vert.x = startX + x;
                vert.y = startY + y;
                vert.z = 0;
                verticies.Add(vert);
            }
        }
        mf.mesh.vertices = verticies.ToArray();

        List<int> indicies = new List<int>();
        for (int y = 0; y < height; y += gridSize)
        {
            int cntStride = width / gridSize + 1;
            int line = y / gridSize;
            for (int x = 0; x < width; x += gridSize)
            {
                int baseX = x / gridSize + cntStride * line;
                int lb = baseX;
                int rb = lb + 1;
                int lt = lb + cntStride;
                int rt = lt + 1;
                indicies.AddRange(new int[3] { lb, lt, rb } );
                indicies.AddRange(new int[3] { rb, lt, rt });
            }
        }
        mf.mesh.triangles = indicies.ToArray();

        List<Color> colors = new List<Color>();
        int cnt = verticies.Count;
        for (int i = 0; i < cnt; ++i)
        {
            colors.Add(Color.red);
        }
        mf.mesh.colors = colors.ToArray();
    }
}
