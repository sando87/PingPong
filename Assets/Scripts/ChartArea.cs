using PP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChartArea : MonoBehaviour
{
    public AudioSource mKickSrc;
    public AudioSource mTickSrc;
    public AudioSource mMusicSrc;
    public Sprite mActiveImage;
    public Sprite mEmptyImage;
    public int mBPM;
    public float mStartTime;
    private RectTransform graphContainer;
    private GameObject mMarker = null;
    private List<GameObject> lines = new List<GameObject>();

    public float tapSyncOffTime = 0.15f;
    private float markerPosTime = 0;
    private float sampleDT = 0.01f;
    private int pixelPerSample = 2;
    private float refHeight = 400f;
    private float currentOffTime = 0;
    private float threshold = 20.0f;
    private float[] originSoundData = null;
    private float[] sampledSoundData = null;
    private List<GameObject> TPs = new List<GameObject>();
    private bool isPlay = false;

    private float sampleTime = 0;
    private string mFileFullName = "";

    private void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        CreateMarker(0);
        //TestCreateRandomTPs();
    }
    private void Update()
    {
        UpdateMarkerPosition();
    }
    private float[] Sample(float[] origin, float dt)
    {
        int jumpIdx = (int)(dt * mMusicSrc.clip.frequency);
        List<float> rets = new List<float>();
        int cnt = origin.Length / 2; //채널 개수가 2라서
        for(int idx = 0; idx < cnt; idx += jumpIdx)
        {
            rets.Add((origin[idx * 2] + origin[idx * 2 + 1]) / 2f);
        }
        return rets.ToArray();
    }
    private void ShowGraph(float[] values, float OffTime)
    {
        int startIdx = (int)(OffTime / sampleDT);
        int endIdx = startIdx + (int)(sampleTime / sampleDT);
        endIdx = Math.Min(endIdx, values.Length);
        float graphHeight = refHeight;
        float yMaximum = 2f;
        float xSize = pixelPerSample;
        Vector2 previousPos = new Vector2();
        for (int i = startIdx; i < endIdx; ++i)
        {
            float xPos = (i - startIdx) * xSize;
            float yPos = (values[i] / yMaximum) * graphHeight;
            Vector2 currentPos = new Vector2(xPos, yPos);
            //CreateCircle(currentPos);
            if (i > startIdx)
                CreateLine(previousPos, currentPos);

            previousPos = currentPos;
        }
    }
    void CreateMarker(float xPos)
    {
        float graphHeight = refHeight;
        mMarker = new GameObject("marker", typeof(Image));
        mMarker.transform.SetParent(graphContainer, false);
        RectTransform rt = mMarker.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(3, graphHeight);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);

    }
    void CreateCircle(Vector2 anchoredPos)
    {
        GameObject gameObj = new GameObject("circle", typeof(Image));
        gameObj.transform.SetParent(graphContainer, false);
        //gameObj.GetComponent<Image>().sprite = mCircleSprite;
        RectTransform rt = gameObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(50, 50);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
    }
    void CreateLine(Vector2 posA, Vector2 posB)
    {
        GameObject gameObj = new GameObject("line", typeof(Image));
        gameObj.transform.SetParent(graphContainer, false);
        float dist = (posA - posB).magnitude;
        Vector3 dir = (posB - posA).normalized;
        float degree = Mathf.Atan(dir.y / dir.x) * Mathf.Rad2Deg;
        RectTransform rt = gameObj.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = posA;
        rt.sizeDelta = new Vector2(dist, 1f);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.localEulerAngles = new Vector3(0, 0, degree);
        lines.Add(gameObj);
    }
    void ClearLines()
    {
        for (int i = 0; i < lines.Count; ++i)
            Destroy(lines[i]);
        lines.Clear();
    }
    void CreateTapPoints(float height)
    {
        ClearTapPoints();

        float secPerPixel = sampleDT / pixelPerSample;
        float secTPs = (120f / mBPM) / 4f;
        int idx = 0;
        for (float sec = mStartTime; sec < sampleTime; sec += secTPs)
        {
            float pixel = sec / secPerPixel;
            float size = idx % 4 == 0 ? 50f : 25f;
            GameObject obj = CreateTapPoint(new Vector2(pixel, height), size);
            TPs.Add(obj);
            idx++;
        }
    }
    void ClearTapPoints()
    {
        for(int i = 0; i < TPs.Count; ++i)
            Destroy(TPs[i]);

        TPs.Clear();
    }
    GameObject CreateTapPoint(Vector2 anchoredPos, float size)
    {
        GameObject gameObj = new GameObject("TP", typeof(Image));
        gameObj.transform.SetParent(transform, false);
        gameObj.GetComponent<Image>().sprite = mEmptyImage;
        gameObj.tag = "0";
        RectTransform rt = gameObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        return gameObj;
    }
    void UpdateGraph(int xOff)
    {
        int sampleIdx = xOff / pixelPerSample;
        currentOffTime += sampleIdx * sampleDT;
        if (currentOffTime < 0)
            currentOffTime = 0;

        ClearLines();
        ShowGraph(sampledSoundData, currentOffTime);
    }
    public void Play()
    {
        if (isPlay)
        {
            mMusicSrc.Pause();
            isPlay = false;
        }
        else
        {
            mMusicSrc.time = markerPosTime;
            mMusicSrc.Play();
            isPlay = true;
        }
    }
    public void Load()
    {
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnLoadSuccess, null);
    }
    public void OnLoadSuccess(string fullname)
    {
        mFileFullName = fullname;
        StartCoroutine(GetAudioClip(fullname));
    }
    IEnumerator GetAudioClip(string filename)
    {
        string url = "file://" + filename;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                UpdateAudioInfo(audioClip);
            }
        }
    }
    public void UpdateAudioInfo(AudioClip clip)
    {
        sampleTime = clip.length;
        mMusicSrc.clip = clip;
        mMusicSrc.volume = 0.5f;

        originSoundData = new float[clip.samples * clip.channels];
        clip.GetData(originSoundData, 0);
        sampledSoundData = Sample(originSoundData, sampleDT);

        //ShowGraph(sampledSoundData, currentOffTime);

        int width = (int)(pixelPerSample * sampleTime / sampleDT) / Screen.width + 1;
        Vector2 max = GetComponent<RectTransform>().anchorMax;
        GetComponent<RectTransform>().anchorMax = new Vector2(width, max.y);

        CreateTapPoints(200.0f);
    }
    public void Finish()
    {
        Song song = new Song();
        song.BPM = mBPM; //128, 148, 162
        song.JumpDelay = mStartTime; //0.43f, 0.43f, 0.5f
        song.Beat = BeatType.B4B4;
        song.FullName = mFileFullName;
        song.SongFileName = ""; //GoAway, Hurt, YouAndI
        song.TitleImageName = "";
        song.SongName = "lsjTest";
        song.SingerName = "";
        song.Grade = 0;

        song.Bars = ExportToBars();
        byte[] buf = Utils.Serialize(song);
        File.WriteAllBytes(PathInfo.BasicSongs + song.SongName + ".bytes", buf);
    }
    void UpdateMarkerPosition()
    {
        if (mMarker != null)
        {
            int preTpIdx = TimeToTPIndex(markerPosTime + tapSyncOffTime);
            if(isPlay)
                markerPosTime += Time.deltaTime;

            int cnt = (int)(markerPosTime / sampleDT);
            mMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(cnt * pixelPerSample, 0);
            if(mMarker.transform.position.x >= Screen.width)
            {
                Vector3 pos = transform.position;
                pos.x -= Screen.width;
                transform.position = pos;
            }
            int postTpIdx = TimeToTPIndex(markerPosTime + tapSyncOffTime);
            if(postTpIdx == preTpIdx + 1 && TPs[postTpIdx].tag == "1")
            {
                if (postTpIdx % 4 == 0)
                    mKickSrc.Play();
                else
                    mTickSrc.Play();
            }
        }
    }

    float MousePointToTime(Vector2 point)
    {
        float secPerPixel = sampleDT / pixelPerSample;
        Vector2 pos = transform.position;
        Vector2 relPos = point - pos;
        float currentTime = relPos.x * secPerPixel;
        return currentTime;
    }
    int TimeToTPIndex(float time)
    {
        float secTPs = (120f / mBPM) / 4f;
        time -= mStartTime;
        if (time < 0)
            return -1;

        int currentIdx = (int)(time / secTPs);
        return currentIdx >= TPs.Count ? TPs.Count - 1 : currentIdx;
    }
    bool ClickTapPoint(Vector2 point)
    {
        float currentTime = MousePointToTime(point);
        int currentIdx = TimeToTPIndex(currentTime);
        if (currentIdx < 0)
            return false;

        Vector2 obj1 = TPs[currentIdx].transform.position;
        Vector2 obj2 = TPs[currentIdx + 1].transform.position;
        if ((obj1 - point).magnitude < threshold)
        {
            string isActive = TPs[currentIdx].tag;
            TPs[currentIdx].GetComponent<Image>().sprite = isActive == "0" ? mActiveImage : mEmptyImage;
            TPs[currentIdx].tag = isActive == "0" ? "1" : "0";
            return true;
        }
        else if ((obj2 - point).magnitude < threshold)
        {
            string isActive = TPs[currentIdx + 1].tag;
            TPs[currentIdx + 1].GetComponent<Image>().sprite = isActive == "0" ? mActiveImage : mEmptyImage;
            TPs[currentIdx + 1].tag = isActive == "0" ? "1" : "0";
            return true;
        }
        return false;
    }
    public void OnClickChart(Vector2 point)
    {
        if (ClickTapPoint(point))
            return;

        markerPosTime = MousePointToTime(point);
        GetComponent<AudioSource>().time = markerPosTime;
    }
    public void OnDragChart(Vector2 delta)
    {
        Vector3 pos = transform.position;
        pos.x += delta.x;
        transform.position = pos;
    }
    public void OnDragChartEnd(Vector2 point)
    {
        //Vector3 pos = transform.position;
        //int offX = (int)pos.x * -1;
        //UpdateGraph(offX);
        //pos.x = 0;
        //transform.position = pos;
    }
    public Bar[] ExportToBars()
    {
        List<Bar> bars = new List<Bar>();
        int cnt = TPs.Count / 4;
        for(int i = 0; i < cnt; ++i)
        {
            Bar bar = new Bar();
            bar.Main        = TPs[i * 4 + 0].tag == "0" ? false : true;
            bar.PreHalf     = TPs[i * 4 + 1].tag == "0" ? false : true;
            bar.Half         = TPs[i * 4 + 2].tag == "0" ? false : true;
            bar.PostHalf    = TPs[i * 4 + 3].tag == "0" ? false : true;
            bars.Add(bar);
        }
        return bars.ToArray();
    }
    public void ImportToBars(Bar[] bars)
    {
        int cnt = Math.Min(bars.Length, TPs.Count / 4);
        for(int i = 0; i < cnt; ++i)
        {
            TPs[i * 4 + 0].tag = bars[i].Main ? "1" : "0";
            TPs[i * 4 + 0].GetComponent<Image>().sprite = bars[i].Main ? mActiveImage : mEmptyImage;

            TPs[i * 4 + 1].tag = bars[i].PreHalf ? "1" : "0";
            TPs[i * 4 + 1].GetComponent<Image>().sprite = bars[i].PreHalf ? mActiveImage : mEmptyImage;

            TPs[i * 4 + 2].tag = bars[i].Half ? "1" : "0";
            TPs[i * 4 + 2].GetComponent<Image>().sprite = bars[i].Half ? mActiveImage : mEmptyImage;

            TPs[i * 4 + 3].tag = bars[i].PostHalf ? "1" : "0";
            TPs[i * 4 + 3].GetComponent<Image>().sprite = bars[i].PostHalf ? mActiveImage : mEmptyImage;
        }
    }
    public void TestCreateRandomTPs()
    {
        Bar[] bars = new Bar[128];
        for (int i = 0; i < bars.Length; ++i)
        {
            bars[i].Main = true;
            bars[i].Half = UnityEngine.Random.Range(0, 3) == 1 ? true : false;
            bars[i].PostHalf = UnityEngine.Random.Range(0, 7) == 1 ? true : false;
            bars[i].PreHalf = UnityEngine.Random.Range(0, 5) == 1 ? true : false;
        }

        ImportToBars(bars);
    }
}
