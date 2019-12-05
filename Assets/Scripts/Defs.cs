using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PP
{
    public enum CubeState { None, Ready, Jump, Fail, Success };
    public enum SystemState { None, UIMode, Standby, WaitJump, Playing, Finish };
    public enum TabType { None, Main, PreHalf, Half, PostHalf };
    public class PathInfo
    {
        public static string DefaultMusics = "DefaultMusics";// Application.dataPath + " / Resources/DefaultMusics/";
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
    public class Defs
    {
        //Default Music을 추가하고 싶으면 userID만 맞춰서 노래를 만들고 Resource/DefaultMusics폴더 하위에 bytes, mp3파일 2개를 추가
        public const string ADMIN_USERNAME = "user#0";
    }

    public struct stIQ
    {
        public short I;
        public short Q;
    }


}
