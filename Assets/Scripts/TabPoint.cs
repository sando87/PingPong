using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;

public class TabPoint : MonoBehaviour
{
    public Setting Setting = null;
    public int idxTabArray = -1;
    public int idxStep = -1;
    public int idxStepToNext = -1;
    public TabType type = TabType.None;
    public float lifetime = 0;
    private float accTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (Setting == null || !Setting.JumpStarted)
            return;

        accTime += Time.deltaTime;
        float diff = Setting.TimePerBar * 1.5f;
        float from = lifetime - diff;
        float to = lifetime;
        if (from <= accTime && accTime <= to)
        {
            float rate = (accTime - from) / diff;
            float angle = -rate * 360.0f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public bool IsFinalTab()
    {
        if (Setting.tabPoints.Count - 1 == idxTabArray)
            return true;
        else
            return false;
    }
}
