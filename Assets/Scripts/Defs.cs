using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PP
{
    public enum TabType { None, Main, PreHalf, Half, PostHalf };
    struct TabInfo
    {
        public float time;
        public TabType type;
        public TabInfo(float _time, TabType _type) { time = _time; type = _type; }
    }
}
