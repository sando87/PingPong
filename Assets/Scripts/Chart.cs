using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chart : MonoBehaviour
{
    MeshFilter mf = null;
    MeshRenderer mr = null;
    float[] mData;
    public float mLineWidth;
    public Color mLineColor;

    private Vector3 mMousePrePos;
    private Vector3 mMouseDownPt;
    private bool mIsMouseDown;
    private bool mIsMouseDrag;
    private const int mRangeClickTol = 1; //pixel unit
    private int mOffData = 100;
    public Rect mChartArea;

    private void Start()
    {
        Rect parentRect = transform.parent.GetComponent<Graph>().mRectArea;
        Vector3 pos = transform.position;
        pos.y = parentRect.y + parentRect.height / 2;
        transform.position = pos;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mData = new float[1024];
        for (int i = 0; i < 1024; ++i)
            mData[i] = Random.Range(mChartArea.yMin, mChartArea.yMax);

        float width = mChartArea.xMax - mChartArea.xMin;
        UpdateData(mOffData, (int)width);
    }
    private void Update()
    {
        Vector3 relativePos = GetRelativeMousePos();
        if (!mChartArea.Contains(relativePos))
            return;

        if(Input.GetMouseButtonDown(0))
        {
            mIsMouseDown = true;
            mIsMouseDrag = false;
            mMouseDownPt = Input.mousePosition;
        }
        if (mIsMouseDown && !mIsMouseDrag)
        {
            Vector3 diff = mMouseDownPt - Input.mousePosition;
            if (diff.magnitude > mRangeClickTol)
            {
                mMousePrePos = Input.mousePosition;
                mIsMouseDrag = true;
            }
        }
        if (mIsMouseDrag)
        {
            Vector3 diff = Input.mousePosition - mMousePrePos;
            if (diff.magnitude > mRangeClickTol)
            {
                mMousePrePos = Input.mousePosition;
                OnDragChart(diff.x, diff.y);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            mIsMouseDown = false;
            mIsMouseDrag = false;
            Vector3 diff = mMouseDownPt - Input.mousePosition;
            if (diff.magnitude < mRangeClickTol)
            {
                OnClickChart(Input.mousePosition);
                return;
            }
        }
    }

    private Vector3 GetRelativeMousePos()
    {
        Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 off = MousePos - transform.position;
        return off;
    }
    private List<Vector3> NewLine(Vector3 p1, Vector3 p2, float lineWidth)
    {
        List<Vector3> ret = new List<Vector3>();
        Vector3 dir = p2 - p1;
        Vector3 nor = Quaternion.AngleAxis(90, new Vector3(0, 0, 1)) * dir;
        dir.Normalize();
        dir *= lineWidth;
        nor.Normalize();
        nor *= lineWidth;

        ret.Add(p1 - dir + nor);
        ret.Add(p1 - dir - nor);
        ret.Add(p2 + dir + nor);
        ret.Add(p2 + dir - nor);
        return ret;
    }
    public bool UpdateData(int offIdx, int length)
    {
        if (mf == null || mData == null)
            return false;

        List<Vector3> verticies = new List<Vector3>();
        List<int> indicies = new List<int>();
        if (offIdx + length > mData.Length)
            length = mData.Length - offIdx;

        int cnt = length - 1;
        for (int idx = 0; idx < cnt; idx++)
        {
            Vector3 pt1 = new Vector3(idx, mData[offIdx + idx], 0);
            Vector3 pt2 = new Vector3(idx + 1, mData[offIdx + idx + 1], 0);
            List<Vector3> line = NewLine(pt1, pt2, mLineWidth);
            int baseIdx = verticies.Count;
            verticies.AddRange(line);
            indicies.AddRange(new int[6] {
                baseIdx + 0, baseIdx + 2, baseIdx + 1,
                baseIdx + 2, baseIdx + 3, baseIdx + 1 });
        }

        mf.mesh.vertices = verticies.ToArray();
        mf.mesh.triangles = indicies.ToArray();
        mr.material.color = mLineColor;
        return true;
    }

    private void OnClickChart(Vector3 mousePt)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(mousePt);
        Debug.Log("Click: " + pos);
    }
    private void OnDragChart(float deltaX, float deltaY)
    {
        Debug.Log("Drag: X," + deltaX + " Y," + deltaY);
    }
    private void OnZoomChart(float deltaX, float deltaY)
    {

    }
}
