using PP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BMPDector
{
    private string path = "C:\\Users\\lee\\Desktop\\FFX_GUI_Music - 복사본\\musics\\sample3.mp3";
    private AudioClip mClip;
    private const int mDivFactor = 4; //44100 -> 11025Hz로 샘플 다운해서 계산(계산 속도를 빠르게 하기 위해)
    private const int mFFTCount = 256; //FFT 샘플링 개수 : 
    private const double mDetectSpan = 1.0; //200ms
    private double mSpectrumStep = 0; // 1 / ((44100/4) / 256) = 0.02322s
    private double mStartTime = 0; //첫 Beat를 감지할때까지의 시간
    private double mThresholdAmp = 1500; //Peak포인트 감지를 위한 기준값
    private double mDT = 0; // 1.0 / (44100hz / mDivFactor);
    private double[] mSamples = null; //해상도를 낮춰서 실제 알고리즘에 적용되는 샘플링 데이터들
    private List<double[]> mFFTs_FreqAmp = new List<double[]>(); //한요소는 FFT결과(freq-amp축)이고 시간에 따른 결과를 List로 담음
    private List<double[]> mFreqs_TimeAmp = new List<double[]>(); //한요소는 (time-amp축)이고 주파수대역별로 List로 담음
    private double[] mFreqs_Sum = null; //저주파수 대역의 값을 합산한 결과(time-amp축)

    public AudioClip GetAudioClip() { return mClip; }
    public double GetFirstBeat() { return mStartTime; }
    public int[] DetectBPM(AudioClip clip)
    {
        Reset();

        //LoadAudioClip();
        mClip = clip;

        mDT = 1.0 / (mClip.frequency / mDivFactor);
        mSpectrumStep = 1.0 / ((mClip.frequency / (double)mDivFactor) / mFFTCount);

        LoadSamples(mDivFactor);

        CalcFFTs();

        TransAxis();

        SumFreqs(10);

        mThresholdAmp = CalcThreshold();
        mStartTime = DetectFirstBeat();

        List<Tuple<int, int>> bpms = new List<Tuple<int, int>>();
        for (int bpm = 90; bpm < 180; ++bpm)
        {
            int hitCount = SyncBPM(mStartTime, bpm);
            Tuple<int, int> tuple = new Tuple<int, int>(bpm, hitCount);
            bpms.Add(tuple);
            string log = bpm.ToString() + ", " + hitCount.ToString();
        }

        bpms.Sort((x, y) =>
        {
            return x.Item2 > y.Item2 ? -1 : 1;
        });

        List<int> rets = new List<int>();
        foreach (var item in bpms)
            rets.Add(item.Item1);

        return rets.ToArray();
    }
    public double[] TestBPM(double off, double startTime, double endTime, double bpm)
    {
        double[] rets = new double[mFreqs_Sum.Length];
        double stepTime = 120.0 / bpm;
        int startIdx = (int)(startTime / stepTime);
        int endIdx = (int)(endTime / stepTime);
        double deltaFreqsTime = mSpectrumStep;
        for (int bpmIdx = startIdx; bpmIdx < endIdx; bpmIdx++)
        {
            double curTime = off + stepTime * bpmIdx;
            int idx = (int)(curTime / deltaFreqsTime);
            if (idx >= rets.Length)
                break;

            rets[idx] = 5000.0;
        }
        return rets;
    }

    private void LoadAudioClip()
    {
        string url = string.Format("file://{0}", path);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            www.SendWebRequest();

            while (true)
            {
                if (!www.isNetworkError)
                    break;
            }

            mClip = DownloadHandlerAudioClip.GetContent(www);
        }
    }
    private double DetectFirstBeat()
    {
        if (mFreqs_Sum == null)
            return -1;

        int detectSpanIdx = (int)(mDetectSpan / mSpectrumStep);
        for (int i = 0; i < mFreqs_Sum.Length; ++i)
        {
            int cen = DetectBeat(mFreqs_Sum, i, detectSpanIdx, mThresholdAmp);
            if (cen > 0)
                return (cen * mSpectrumStep);
        }
        return -1;
    }
    private void SumFreqs(int count)
    {
        if (mFreqs_TimeAmp.Count == 0)
            return;

        int totalCnt = mFreqs_TimeAmp[0].Length;
        mFreqs_Sum = new double[totalCnt];
        for (int timeIdx = 0; timeIdx < totalCnt; ++timeIdx)
        {
            double sum = 0;
            for (int freqIdx = 0; freqIdx < count; ++freqIdx)
            {
                sum += mFreqs_TimeAmp[freqIdx][timeIdx];
            }
            mFreqs_Sum[timeIdx] = sum / count;
        }
    }
    private int SyncBPM(double off, int bpm)
    {
        double stepTime = 120.0 / bpm;
        int endIdx = (int)(mClip.length / stepTime);
        double deltaFreqsTime = mSpectrumStep;
        int spanIdx = (int)(mDetectSpan / mSpectrumStep) / 2;
        int[] peaks = new int[spanIdx * 2 + 1];
        for (int bpmIdx = 0; bpmIdx < endIdx; bpmIdx++)
        {
            double curTime = off + stepTime * bpmIdx;
            int idx = (int)(curTime / deltaFreqsTime);
            int cen = FindNearPeak(mFreqs_Sum, idx - spanIdx, idx + spanIdx);
            if (cen > 0)
            {
                int peakIdx = cen - idx + spanIdx;
                peaks[peakIdx]++;
            }
        }
        Array.Sort(peaks);
        //return peaks[peaks.Length - 1];
        double acc = peaks[peaks.Length - 1] / (double)endIdx;
        return (int)(acc * 100.0);
    }
    private int FindNearPeak(double[] freqs, int _from, int _to)
    {
        int from = _from < 0 ? 0 : _from;
        int to = _to >= freqs.Length ? freqs.Length - 1 : _to;
        double deltaFreqsTime = mSpectrumStep;
        int detectSpan = (int)(mDetectSpan / deltaFreqsTime);
        int detectSpanhalf = detectSpan / 2;
        for (int cenIdx = from; cenIdx < to; ++cenIdx)
        {
            int cen = DetectBeat(freqs, cenIdx - detectSpanhalf, detectSpan, mThresholdAmp);
            if (cen > 0)
                return cen;
        }
        return -1;
    }
    private void LoadSamples(double divSampleRateFactor, double startTime = 0, double spanTime = 0)
    {
        int div = (int)divSampleRateFactor;
        long len = mClip.samples * mClip.channels;
        float[] buf = new float[len];
        mClip.GetData(buf, 0);
        int sampleCnt = mClip.samples;
        double dt = 1 / (double)mClip.frequency;
        int startOffIdx = (int)(startTime / dt);
        int spanIdx = startOffIdx + (int)(spanTime / dt);
        spanIdx = spanTime == 0 ? sampleCnt : spanIdx;
        List<double> list = new List<double>();
        for (int idx = startOffIdx; idx < spanIdx; idx += div)
        {
            float val = ReadSample(buf, idx);
            list.Add(val);
        }
        mSamples = list.ToArray();
    }
    private float ReadSample(float[] buf, int idx)
    {
        float ret = 0;
        for (int i = 0; i < mClip.channels; ++i)
            ret += buf[mClip.channels * idx + i];
        return ret;
    }
    private double[] FFT(int off, int count)
    {
        if (off + count >= mSamples.Length)
            return null;

        stIQ[] tmp = new stIQ[count];
        float aa = 1f / ((float)mDT * tmp.Length);
        int bb = (int)aa;
        float time = 0;
        for (int ii = 0; ii < tmp.Length; ++ii)
        {
            tmp[ii].I = (Int16)(mSamples[off + ii] * Math.Cos(bb * time));
            tmp[ii].Q = (Int16)(mSamples[off + ii] * Math.Sin(bb * time));
            time += (float)mDT;
        }

        double[] fft = Utils.FFT(tmp);
        return fft;
    }
    private void Reset()
    {
        mClip = null;
        mSamples = null;
        mDT = 0;
        mFFTs_FreqAmp.Clear();
        mFreqs_TimeAmp.Clear();
    }
    private void CalcFFTs()
    {
        int cnt = mFFTCount;
        double curTime = 0;
        while (curTime < mClip.length)
        {
            int offIdx = (int)(curTime / mDT) - cnt;
            if (offIdx >= 0)
            {
                double[] ret = FFT(offIdx, cnt);
                mFFTs_FreqAmp.Add(ret);
            }
            else
            {
                mFFTs_FreqAmp.Add(new double[cnt]);
            }
            curTime += mSpectrumStep;
        }
    }
    private void TransAxis()
    {
        if (mFFTs_FreqAmp.Count == 0)
            return;

        int cntElements = mFFTs_FreqAmp[0].Length;
        int cnt = mFFTs_FreqAmp.Count;
        for (int frq = 0; frq < cntElements; ++frq)
        {
            double[] freq = new double[cnt];
            for (int tt = 0; tt < cnt; ++tt)
            {
                freq[tt] = mFFTs_FreqAmp[tt][frq];
            }

            mFreqs_TimeAmp.Add(freq);
        }
    }
    private int DetectBeat(double[] freqs, int offIdx, int count, double amp)
    {
        int startIdx = offIdx < 0 ? 0 : offIdx;
        int endIdx = offIdx + count - 1;
        int halfIdx = count / 2;
        int centerIdx = startIdx + halfIdx;
        if (endIdx >= freqs.Length)
            return -1;

        double max = freqs[centerIdx];
        double spanleft = max - freqs[startIdx];
        double spanright = max - freqs[endIdx];
        if (max < amp) //|| spanleft < amp || spanright < amp)
            return -1;

        for (int i = startIdx; i <= endIdx; ++i)
        {
            if (i == centerIdx)
                continue;

            if (freqs[i] >= max)
                return -1;
        }

        return centerIdx;
    }
    private double CalcThreshold()
    {
        double sum = 0;
        foreach (double val in mFreqs_Sum)
            sum += val;

        return sum / mFreqs_Sum.Length;
    }

}
