using ICD;
using PP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChartArea : MonoBehaviour
{
    public AudioSource mKickSrc;
    public AudioSource mTickSrc;
    public AudioSource mMusicSrc;
    public Sprite mActiveImage;
    public Sprite mEmptyImage;
    public GameObject mEditBPM;
    public GameObject mEditOFF;
    public GameObject mScrollBar;
    public GameObject mScrollRect;
    public Dropdown mDropdownBPM;
    private int mBPM = 100;
    private float mStartTime = 0.5f;
    private RectTransform graphContainer;
    private GameObject mMarker = null;
    private List<GameObject> lines = new List<GameObject>();

    private int mCurrentGraphPosX = 0;
    private float markerPosTime = 0;
    private float sampleDT = 0.01f;
    private int pixelPerSample = 2;
    private float refHeight = 400f;
    private float currentOffTime = 0;
    private float threshold = 20.0f;
    private float tapPointsHeight = 200.0f;
    private float[] originSoundData = null;
    private float[] sampledSoundData = null;
    private List<GameObject> TPs = new List<GameObject>();
    private bool isPlay = false;
    private bool isAutoCamera = false;
    private Scrollbar mScroll;
    private ScrollRect mScrollArea;
    private int mMaxPixel = 0;

    private float sampleTime = 0;
    private string mFileFullName = "";
    private bool mIsDrawingGraph = false;

    private void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        mScroll = mScrollBar.GetComponent<Scrollbar>();
        mScrollArea = mScrollRect.GetComponent<ScrollRect>();

        CreateMarker(0);
        CreateGraph();
        //TestCreateRandomTPs();
        Time.maximumDeltaTime = 3.0f;
    }
    private void Update()
    {
        if (mScrollArea.velocity.magnitude < 50)
            UpdateGraph();

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
    private IEnumerator ShowGraph(float[] values, float StartTime, float EndTime)
    {
        mIsDrawingGraph = true;

        int startIdx = (int)(StartTime / sampleDT);
        int endIdx = (int)(EndTime / sampleDT) + 1;
        endIdx = Math.Min(endIdx, values.Length);
        float graphHeight = refHeight;
        float yMaximum = 2f;
        Vector2 previousPos = new Vector2();
        int idx = 0;
        for (int i = startIdx; i < endIdx; ++i, idx++)
        {
            float xPos = i * pixelPerSample;
            float yPos = (values[i] / yMaximum) * graphHeight;
            Vector2 currentPos = new Vector2(xPos, yPos);
            //CreateCircle(currentPos);
            if (i > startIdx)
                UpdateLine(previousPos, currentPos, idx);
                //CreateLine(previousPos, currentPos);
                
            previousPos = currentPos;

            if(idx % 100 == 0)
                yield return null;
        }

        mIsDrawingGraph = false;
        yield return null;
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
    void UpdateLine(Vector2 posA, Vector2 posB, int index)
    {
        if (index >= lines.Count)
            return;

        GameObject gameObj = lines[index];
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
    }
    void ClearLines()
    {
        for (int i = 0; i < lines.Count; ++i)
            Destroy(lines[i]);
        lines.Clear();
    }
    void CreateTapPoints()
    {
        ClearTapPoints();

        float secPerPixel = sampleDT / pixelPerSample;
        float secTPs = (120f / mBPM) / 4f;
        int idx = 0;
        for (float sec = mStartTime; sec < sampleTime; sec += secTPs)
        {
            float pixel = sec / secPerPixel;
            float size = idx % 4 == 0 ? 50f : 25f;
            GameObject obj = CreateTapPoint(new Vector2(pixel, tapPointsHeight), size);
            TPs.Add(obj);
            idx++;
        }

        for (int i = 0; i < 4; ++i)
            TPs[i].GetComponent<Image>().color = Color.gray;

    }
    void ClearTapPoints()
    {
        for(int i = 0; i < TPs.Count; ++i)
            Destroy(TPs[i]);

        TPs.Clear();
    }
    void UpdateTapPoints()
    {
        mEditBPM.GetComponent<InputField>().text = mBPM.ToString();
        mEditOFF.GetComponent<InputField>().text = mStartTime.ToString();

        Bar[] bars = ExportToBars();
        CreateTapPoints();
        ImportToBars(bars);

        //float secPerPixel = sampleDT / pixelPerSample;
        //float secTPs = (120f / mBPM) / 4f;
        //float time = mStartTime + secTPs * 4f;
        //for (int i = 0; i < TPs.Count; ++i)
        //{
        //    float xPos = time / secPerPixel;
        //    RectTransform rt = TPs[i].GetComponent<RectTransform>();
        //    rt.anchoredPosition = new Vector2(xPos, tapPointsHeight);
        //
        //    time += secTPs;
        //}
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

    public void Play()
    {
        if (isPlay)
        {
            mMusicSrc.Pause();
            isPlay = false;
            isAutoCamera = false;
        }
        else
        {
            mMusicSrc.time = markerPosTime;
            mMusicSrc.Play();
            isPlay = true;
            isAutoCamera = true;
        }
    }
    public void Load()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
#endif
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

        int width = (int)(pixelPerSample * sampleTime / sampleDT) / Screen.width;
        mMaxPixel = width * Screen.width;
        Vector2 max = GetComponent<RectTransform>().anchorMax;
        GetComponent<RectTransform>().anchorMax = new Vector2(width + 1, max.y);

        CreateTapPoints();
    }

    public void Finish()
    {
        TagLib.File mp3 = TagLib.File.Create(mFileFullName);
        //Debug.Log(mp3.Tag.Title);
        //Debug.Log(mp3.Tag.AlbumArtists[0]);

        string[] splits = mFileFullName.Split(new char[2] { '\\', '/' });
        string filename = splits[splits.Length - 1];
        Song song = new Song();
        song.DBID = -1;
        song.BPM = mBPM; //128, 148, 162
        song.JumpDelay = mStartTime; //0.43f, 0.43f, 0.5f
        song.Beat = BeatType.B4B4;
        song.Type = FileType.Local;
        song.FilePath = String.Join("/", splits, 0, splits.Length - 1) + "/";
        song.FileNameNoExt = splits[splits.Length - 1].Split('.')[0];
        song.Title = song.FileNameNoExt;
        song.Artist = "artist";
        song.Grade = 0;
        song.Bars = ExportToBars();
        song.BarCount = song.Bars.Length;

        if (NetworkClient.Inst().IsConnected())
        {
            ICD.CMD_SongFile msg = new ICD.CMD_SongFile();
            msg.song = song;
            byte[] stream = File.ReadAllBytes(song.FilePath + song.FileNameNoExt + ".mp3");
            msg.stream.AddRange(stream);
            msg.FillHeader(ICD.ICDDefines.CMD_Upload);
            NetworkClient.Inst().SendMsgToServer(msg);
        }
        else
        {
            byte[] buf = Utils.Serialize(song);
            File.WriteAllBytes(Application.persistentDataPath + "/" + song.FileNameNoExt + ".bytes", buf);
        }

    }
    public void OnBtnCreateRandomTPs()
    {
        int barCount = TPs.Count / 4;
        Bar[] bars = new Bar[barCount - 1];
        for (int i = 0; i < bars.Length; ++i)
        {
            bars[i].Main = true;
            bars[i].Half = UnityEngine.Random.Range(0, 3) == 1 ? true : false;
            bars[i].PostHalf = UnityEngine.Random.Range(0, 7) == 1 ? true : false;
            bars[i].PreHalf = UnityEngine.Random.Range(0, 5) == 1 ? true : false;
        }

        ImportToBars(bars);
    }
    public void OnDropdownBPM()
    {
        int idx = mDropdownBPM.value;
        string bpm = mDropdownBPM.options[idx].text;
        mBPM = int.Parse(bpm);
        UpdateTapPoints();
    }
    public void OnBtnDetectBPM()
    {
        BMPDector dt = new BMPDector();
        int[] bpms = dt.DetectBPM(mMusicSrc.clip);
        if(bpms!=null & bpms.Length > 0)
        {
            mDropdownBPM.options.Clear();
            for(int i = 0; i<bpms.Length; ++i)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = bpms[i].ToString();
                mDropdownBPM.options.Add(option);
            }
            mBPM = bpms[0];
            mStartTime = (float)dt.GetFirstBeat();
            UpdateTapPoints();
        }
    }

    public void OnEditEnd_BPM(string value)
    {
        if(value.Length > 0)
        {
            mBPM = int.Parse(value);
            UpdateTapPoints();
        }
    }
    public void OnEditEnd_Off(string value)
    {
        if (value.Length > 0)
        {
            mStartTime = float.Parse(value);
            UpdateTapPoints();
        }
    }

    void UpdateMarkerPosition()
    {
        if (mMarker != null)
        {
            int preTpIdx = TimeToTPIndex(markerPosTime);
            if (isPlay)
                markerPosTime += Time.deltaTime;

            float delayedTime = markerPosTime - Setting.MusicDelay;
            delayedTime = delayedTime < 0 ? 0 : delayedTime;
            int cnt = (int)(delayedTime / sampleDT);
            mMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(cnt * pixelPerSample, 0);
            if (isAutoCamera && mMarker.transform.position.x >= Screen.width)
            {
                Vector3 pos = transform.position;
                pos.x -= Screen.width;
                transform.position = pos;
            }
            int postTpIdx = TimeToTPIndex(markerPosTime);
            if (postTpIdx == preTpIdx + 1 && TPs[postTpIdx].tag == "1")
            {
                mKickSrc.volume = (postTpIdx % 4 == 0) ? 1.0f : 0.5f;
                mKickSrc.Play();
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
        time -= (mStartTime);
        if (time < 0)
            return -1;

        int currentIdx = (int)(time / secTPs);
        return currentIdx >= TPs.Count ? TPs.Count - 1 : currentIdx;
    }
    bool ClickTapPoint(Vector2 point)
    {
        float currentTime = MousePointToTime(point);
        int currentIdx = TimeToTPIndex(currentTime);
        if (currentIdx < 4)
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
    public void OnClickChartArea()
    {
        Vector2 point = Input.mousePosition;
        if (ClickTapPoint(point))
            return;

        markerPosTime = MousePointToTime(point);
        GetComponent<AudioSource>().time = markerPosTime;
        isAutoCamera = false;
    }
    public void OnClickChart(Vector2 point)
    {
        if (ClickTapPoint(point))
            return;

        markerPosTime = MousePointToTime(point);
        GetComponent<AudioSource>().time = markerPosTime;
        isAutoCamera = false;
    }
    public void OnDragChart(Vector2 delta)
    {
        isAutoCamera = false;
    }
    public void OnDragChartEnd(Vector2 point)
    {
    }
    public Bar[] ExportToBars()
    {
        List<Bar> bars = new List<Bar>();
        int cnt = TPs.Count / 4;
        for(int i = 1; i < cnt; ++i)
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
        int cnt = Math.Min(bars.Length, TPs.Count / 4 - 1);
        for(int i = 0; i < cnt; ++i)
        {
            int j = i + 1;
            TPs[j * 4 + 0].tag = bars[i].Main ? "1" : "0";
            TPs[j * 4 + 0].GetComponent<Image>().sprite = bars[i].Main ? mActiveImage : mEmptyImage;
            TPs[j * 4 + 1].tag = bars[i].PreHalf ? "1" : "0";
            TPs[j * 4 + 1].GetComponent<Image>().sprite = bars[i].PreHalf ? mActiveImage : mEmptyImage;
            TPs[j * 4 + 2].tag = bars[i].Half ? "1" : "0";
            TPs[j * 4 + 2].GetComponent<Image>().sprite = bars[i].Half ? mActiveImage : mEmptyImage;
            TPs[j * 4 + 3].tag = bars[i].PostHalf ? "1" : "0";
            TPs[j * 4 + 3].GetComponent<Image>().sprite = bars[i].PostHalf ? mActiveImage : mEmptyImage;
        }
    }

    private void CreateGraph()
    {
        int cnt = (Screen.width + 400) / pixelPerSample;
        for (int i = 0; i < cnt; ++i)
            CreateLine(Vector2.zero, new Vector2(1, 0));
    }
    public void UpdateGraph()
    {
        int termPixel = 200;
        int newPosX = (int)(mMaxPixel * mScroll.value);
        if (Mathf.Abs(mCurrentGraphPosX - newPosX) > termPixel)
        {
            mCurrentGraphPosX = newPosX;
            float startTime = ((mCurrentGraphPosX - termPixel) / pixelPerSample) * sampleDT;
            float endTime = ((mCurrentGraphPosX + Screen.width + termPixel) / pixelPerSample) * sampleDT;
            startTime = Math.Max(0, startTime);
            endTime = Math.Min(sampleTime, endTime);

            //ClearLines();
            if (!mIsDrawingGraph)
                StartCoroutine(ShowGraph(sampledSoundData, startTime, endTime));
        }
    }
}
