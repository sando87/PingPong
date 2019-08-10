using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PP
{
    public enum CubeState { None, Ready, Jump, Fail, Success };
    public enum TabType { None, Main, PreHalf, Half, PostHalf };
    public struct TabInfo
    {
        public int idxStep;
        public float time;
        public TabType type;
        public Vector2 worldPos;
        public int idxStepToNext;
        public TabPoint script;
        public TabInfo(float _time, TabType _type, int _idxStep)
        { time = _time; type = _type; idxStep = _idxStep; idxStepToNext = -1; worldPos = new Vector2(); script = null; }
    }
    public struct Bar
    {
        public bool Main;
        public bool Half;
        public bool PreHalf;
        public bool PostHalf;
    }
}
