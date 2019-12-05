using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class IEnumDetectBPM : IEnumerator
{
    public int BPM = 0;
    private bool Started = false;
    private bool Done = false;
    private BMPDector.AudioInfo mClipInfo = null;
    public IEnumDetectBPM(AudioClip clip)
    {
        mClipInfo = new BMPDector.AudioInfo();
        mClipInfo.frequency = clip.frequency;
        mClipInfo.length = clip.length;
        mClipInfo.samples = clip.samples;
        mClipInfo.channels = clip.channels;
        long len = clip.samples * clip.channels;
        mClipInfo.buffer = new float[len];
        clip.GetData(mClipInfo.buffer, 0);
    }

    public object Current
    {
        get
        {
            if(!Started)
            {
                Started = true;
                Task th = new Task(() => {
                    Done = false;
                    while(true)
                    {
                        BMPDector dt = new BMPDector();
                        int[] bpms = dt.DetectBPM(mClipInfo);
                        if (bpms != null & bpms.Length > 0)
                            BPM = bpms[0];

                        break;
                    }
                    Done = true;
                });
                th.Start();
            }
            return null;
        }
    }

    public bool MoveNext()
    {
        return !Done;
    }

    public void Reset()
    {
    }
}
