﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PP
{
    public enum FileType { None, Basic, Local, Download };
    public enum BeatType { None, B4B4, B3B4, B6B8, B2B2 };
    public enum CubeState { None, Ready, Jump, Fail, Success };
    public enum SystemState { None, UI, Standby, WaitJump, Playing, Finish };
    public enum TabType { None, Main, PreHalf, Half, PostHalf };
    public class PathInfo
    {
        public static string BasicSongs = Application.dataPath + "\\Resources\\MetaInfo\\Basic\\";
        public static string AudioClip = "AudioClips\\";
        public static string Images = "Images\\";
    }
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

    public struct stIQ
    {
        public short I;
        public short Q;
    }


}
