﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;

public class Setting : MonoBehaviour
{
    public float TimePerBar = 0.938f; //Bar : 음악의 한 마디(한박자)
    public float JumpHeight = 10f;
    public float SpeedMoveX = 10f;
    public float SpeedRotate = 210f;
    public float OffTabTiming = 0;
    public float OffMusciStart = 0.25f;
    public float RatePassFail = 0.125f; //0, 0.0625f, 0.125f, 0.25f, 0.5f, 1.0f

    // y = a(t - t1)^2 + y1;
    private float CurveT1 = 0;
    private float CurveY1 = 0;
    private float CurveA = 0;

    // x = at;
    private float LinearA = 0;

    public bool JumpStarted = false;

    public float JumpHeightHalf = 0;

    public GameObject PrefabTapPoint;
    public List<TabPoint> tabPoints = new List<TabPoint>();
    private Bar[] BarArray = null;

    CubePlayer Player;
    AudioSource audioSource;

    void Start()
    {
        Player = GameObject.Find("Cube").GetComponent<CubePlayer>();
        audioSource = GameObject.Find("AudioPlayer").GetComponent<AudioSource>();

        UpdateLinear(SpeedMoveX, 1);
        UpdateCurve(new Vector2(TimePerBar * 0.5f, JumpHeight), new Vector2(0, 0));
        JumpHeightHalf = GetCurveY(TimePerBar * 0.25f);

        InitTapPoints();
    }

    public void OnBtnStart()
    {
        Invoke("StartJump", OffMusciStart);
        audioSource.Play();
    }
    private void StartJump()
    {
        JumpStarted = true;
        Player.StartJump();
    }

    public void UpdateLinear(float dist, float time)
    {
        LinearA = dist / time;
    }
    public float GetLinearX(float time)
    {
        return LinearA * time;
    }
    public float GetLinearT(float dist)
    {
        return dist / LinearA;
    }

    public void UpdateCurve(Vector2 basePt, Vector2 passPt)
    {
        double dx = (passPt.x - basePt.x);
        if (Math.Abs(dx) < 0.0001)
            Debug.Log(dx + "[[[asdf");

        CurveT1 = basePt.x;
        CurveY1 = basePt.y;
        CurveA = (passPt.y - basePt.y) / (float)(dx * dx);
    }
    public float CalcBaseT(double height, double t, double y, bool whichOne)
    {
        if (Math.Abs(y) < 0.0001)
            Debug.Log(y + "[[[zxcv");

        double s = t;
        double y1 = height;
        double y2 = y;
        double a = y2;
        double b = -4f * y1 * s;
        double c = 4f * y1 * s * s;
        double d = Math.Sqrt(b * b - 4 * a * c);
        double t1 = (-b + d) / (2 * a);
        double t2 = (-b - d) / (2 * a);
        float baseT = whichOne ? (float)t1 : (float)t2;
        return baseT * 0.5f;
    }
    public float GetCurveY(float t)
    {
        double a = (t - CurveT1);
        double b = CurveA * a * a;
        float c = (float)b + CurveY1;
        return c;
    }
    public float GetCurveT(float h)
    {
        float a = CurveA;
        float b = -CurveA * TimePerBar;
        float c = -h;
        float d = Mathf.Sqrt(b * b - 4 * a * c);
        return (-b - d) / (2 * a);
    }

    private void InitTapPoints()
    {
        BarArray = new Bar[256];
        for (int i = 0; i < BarArray.Length; ++i)
        {
            BarArray[i].Main = true;
            BarArray[i].Half = UnityEngine.Random.Range(0, 5) == 1 ? true : false;
            BarArray[i].PostHalf = UnityEngine.Random.Range(0, 8) == 1 ? true : false;
            BarArray[i].PreHalf = UnityEngine.Random.Range(0, 8) == 1 ? true : false;
        }

        CreateTapPoints(BarArray);
    }
    private void CreateTapPoints(Bar[] barArray)
    {
        float acctime = 0;
        TabInfo[] times = ToDeltas(barArray);
        Vector2 current = new Vector2(0, 0);
        float dir = 1.0f;
        for (int i = 0; i < times.Length; ++i)
        {
            acctime += times[i].time;
            float t = times[i].time + OffTabTiming;
            current.x += (dir * SpeedMoveX * t);
            current.y += GetCurveY(t);
            dir *= -1;

            GameObject tab = Instantiate(PrefabTapPoint, current, Quaternion.identity);
            TabPoint script = tab.GetComponent<TabPoint>();
            script.idxTabArray = i;
            script.idxStep = times[i].idxStep;
            script.idxStepToNext = i < times.Length-1 ? times[i + 1].idxStep - times[i].idxStep : 4; //한마디에 4개의 step을 의미(한박,반박,반의반x2)
            script.type = times[i].type;
            script.lifetime = acctime;
            script.Setting = this;
            tabPoints.Add(script);
        }
    }
    TabInfo[] ToDeltas(Bar[] bars)
    {
        int stepIndex = 0;
        float stepDelta = TimePerBar * 0.25f;
        float delayTime = TimePerBar;
        List<TabInfo> times = new List<TabInfo>();
        for (int i = 0; i < bars.Length; ++i)
        {
            if (bars[i].Main) { times.Add(new TabInfo(delayTime, TabType.Main, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].PreHalf) { times.Add(new TabInfo(delayTime, TabType.PreHalf, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].Half) { times.Add(new TabInfo(delayTime, TabType.Half, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].PostHalf) { times.Add(new TabInfo(delayTime, TabType.PostHalf, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;
        }
        return times.ToArray();
    }
    public TabPoint GetTapInfo(int index)
    {
        return tabPoints[index];
    }
}