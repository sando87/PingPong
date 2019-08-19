using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public Rect mRectArea;
    public Color mColor;
    // Start is called before the first frame update
    void Start()
    {
        Vector3[] rect = new Vector3[4]
        {
            new Vector3(mRectArea.xMin, mRectArea.yMin, 0),
            new Vector3(mRectArea.xMin, mRectArea.yMax, 0),
            new Vector3(mRectArea.xMax, mRectArea.yMin, 0),
            new Vector3(mRectArea.xMax, mRectArea.yMax, 0)
        };
        int[] indicies = new int[6]
        {
            1, 3, 0 , 3 , 2 , 0
        };
        mf.mesh.vertices = rect;
        mf.mesh.triangles = indicies;
        mr.material.color = mColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
