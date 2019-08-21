using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Touch2DEvent : MonoBehaviour
{
    [Serializable]
    public class Touch2D : UnityEvent<Vector2> { }
    public Touch2D mTap = null;
    public Touch2D mDrag = null;
    public Touch2D mDragEnd = null;

    private bool mIsActive = false;
    private Vector3 mMousePrePos;
    private Vector3 mMouseDownPt;
    private bool mIsMouseDown;
    private bool mIsMouseDrag;
    private const int mRangeClickTol = 1; //pixel unit

    private Canvas canvas;
    private PointerEventData ped;
    private GraphicRaycaster gr;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        gr = canvas.GetComponent<GraphicRaycaster>();
        ped = new PointerEventData(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ped.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            gr.Raycast(ped, results);
            foreach (var result in results)
            {
                if (gameObject == result.gameObject)
                {
                    mIsActive = true;
                    mIsMouseDown = true;
                    mIsMouseDrag = false;
                    mMouseDownPt = Input.mousePosition;
                }
            }
        }

        if (mIsActive)
        {
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
                    OnDragChart(new Vector2(diff.x, diff.y));
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 diff = mMouseDownPt - Input.mousePosition;
                if(mIsMouseDrag)
                    OnDragChartEnd(Input.mousePosition);
                else if (diff.magnitude < mRangeClickTol)
                    OnClickChart(Input.mousePosition);

                mIsMouseDown = false;
                mIsMouseDrag = false;
                mIsActive = false;
            }
        }

    }
    private void OnClickChart(Vector2 point)
    {
        mTap.Invoke(point);
    }
    private void OnDragChart(Vector2 delta)
    {
        mDrag.Invoke(delta);
    }
    private void OnDragChartEnd(Vector2 point)
    {
        mDragEnd.Invoke(point);
    }
}
