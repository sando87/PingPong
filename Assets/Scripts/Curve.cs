using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve
{
    // y = a(t - t1)^2 + y1;
    private static float CurveT1 = 0;
    private static float CurveY1 = 0;
    private static float CurveA = 0;
    public static void UpdateCurve(Vector2 basePt, Vector2 passPt)
    {
        double dx = (passPt.x - basePt.x);
        if (Math.Abs(dx) < 0.0001)
            Debug.Log(dx + "[[[asdf");

        CurveT1 = basePt.x;
        CurveY1 = basePt.y;
        CurveA = (passPt.y - basePt.y) / (float)(dx * dx);
    }
    public static float CalcBaseT(double height, double t, double y, bool whichOne)
    {
        if (Math.Abs(y) < 0.0001)
            return (float)t * 0.5f;

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
    public static float GetCurveY(float t)
    {
        double a = (t - CurveT1);
        double b = CurveA * a * a;
        float c = (float)b + CurveY1;
        return c;
    }
    public static float GetCurveT(float h, float timePerBar)
    {
        float a = CurveA;
        float b = -CurveA * timePerBar;
        float c = -h;
        float d = Mathf.Sqrt(b * b - 4 * a * c);
        return (-b - d) / (2 * a);
    }
    
    
    // x = at;
    private static float LinearA = 0;
    public static void UpdateLinear(float dist, float time)
    {
        LinearA = dist / time;
    }
    public static float GetLinearX(float time)
    {
        return LinearA * time;
    }
    public static float GetLinearT(float dist)
    {
        return dist / LinearA;
    }
}
