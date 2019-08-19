using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public float mLineWidth;
    public Color mColor;
    private Chart mParent;
    // Start is called before the first frame update
    void Start()
    {
        mParent = transform.parent.GetComponent<Chart>();
        if (mParent == null)
            return;

        Vector3[] rect = new Vector3[4]
        {
            new Vector3(0, mParent.mChartArea.yMin, 0),
            new Vector3(0, mParent.mChartArea.yMax, 0),
            new Vector3(mLineWidth, mParent.mChartArea.yMin, 0),
            new Vector3(mLineWidth, mParent.mChartArea.yMax, 0)
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
