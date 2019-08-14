using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;
using System.IO;

public class Setting
{
    public static float TimePerBar = 0;
    public static float JumpHeightHalf = 0;

    public static float JumpHeight = 10f;
    public static float SpeedMoveX = 10f;
    public static float SpeedRotate = 210f;
    public static float RatePassFail = 0.125f; //0, 0.0625f, 0.125f, 0.25f, 0.5f, 1.0f
    public static float RateAccuracy = 0.03125f;

    void Start()
    {
    }

    void Update()
    {
    }


    public void OnBtnStart()
    {
        Song song = new Song();
        song.BPM = 128;
        song.JumpDelay = 0.43f;
        song.Beat = BeatType.B4B4;
        song.SongFileName = "goaway.mp3";
        song.TitleImageName = "2ne1_chart1.jpg";
        song.SongName = "Go Away";
        song.SingerName = "2NE1";
        song.Grade = 0;
        
        song.Bars = new Bar[128];
        for (int i = 0; i < song.Bars.Length; ++i)
        {
            song.Bars[i].Main = true;
            song.Bars[i].Half = UnityEngine.Random.Range(0, 5) == 1 ? true : false;
            song.Bars[i].PostHalf = UnityEngine.Random.Range(0, 8) == 1 ? true : false;
            song.Bars[i].PreHalf = UnityEngine.Random.Range(0, 8) == 1 ? true : false;
        }
        
        byte[] buf = Utils.Serialize(song);
        File.WriteAllBytes(PathInfo.BasicSongs + song.SongName + ".bin", buf);
    }

}
