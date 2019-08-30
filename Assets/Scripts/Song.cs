using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;
using System;

[Serializable]
public class Song
{
    public int BPM;
    public float JumpDelay;
    public BeatType Beat;
    public string FullName;
    public string SongFileName;
    public string TitleImageName;
    public string SongName;
    public string SingerName;
    public int Grade;
    public Bar[] Bars;
}
