using ICD;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomTP : MonoBehaviour
{
    public Sprite mEmptyImage;
    public Sprite mActiveImage;
    public GameObject mPrefabPoint;

    private AudioSource mTickSound;
    private GameObject mMarker = null;
    private GameObject mBaseLine = null;
    private List<GameObject> TPs = new List<GameObject>();
    private List<GameObject> Bars = new List<GameObject>();

    private float mTPHeight = 100;
    private float mPlayTime = 0;
    private float mTimePerTP = 0;
    private float mPixelPerTP = 0;
    private int mBPM = 0;
    private float mStart = 0;
    private float mEnd = 0;
    private int mCurrentTPIndex = 0;

    public bool AutoMode { get; set; }
    public bool IsPlay { get; set; }
    public int BPM { get { return mBPM; } }
    public float StartTime { get { return mStart; } }
    public float EndTime { get { return mEnd; } }
    public float PlayTime { get { return mPlayTime; } }

    // Start is called before the first frame update
    void Start()
    {
        mTPHeight = Screen.width * 0.1f;
        mPixelPerTP = 64.0f;// Screen.width / 20.0f;
        mTickSound = GetComponent<AudioSource>();
        CreateMarker(0);

        //Initialize(128, 0.4f, 180);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransform rt = GetComponent<RectTransform>();
            Vector2 posOff = Input.mousePosition - transform.position;
            if (rt.rect.Contains(posOff))
                AutoMode = false;
        }

        if(IsPlay)
            mPlayTime += Time.deltaTime;

        PlayTickSound();
        UpdateMarkerPosition();
        UpdatePosition();
    }

    public void Initialize(int bpm, float start, float end)
    {
        ReleaseAll();

        mBPM = bpm;
        mStart = start;
        mEnd = end;

        mTimePerTP = (60.0f * 2.0f) / (mBPM * 4.0f);

        float pixel = TimeToPixel(end);
        int rate = (int)(pixel / Screen.width);
        Vector2 max = GetComponent<RectTransform>().anchorMax;
        GetComponent<RectTransform>().anchorMax = new Vector2(rate + 1, max.y);

        CreateTapPoints();
        CreateBaseLine();
        gameObject.SetActive(true);
    }
    public void ReleaseAll()
    {
        mBPM = 0;
        mStart = 0;
        mEnd = 0;
        mTimePerTP = 0;

        Vector2 max = GetComponent<RectTransform>().anchorMax;
        GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, max.y);
        gameObject.SetActive(false);
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
    GameObject CreateTapPoint2(Vector2 anchoredPos, float size)
    {
        GameObject gameObj = Instantiate(mPrefabPoint, Vector2.zero, Quaternion.Euler(0,0,45f), transform);
        gameObj.tag = "0";
        RectTransform rt = gameObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        return gameObj;
    }
    void ClearTapPoints()
    {
        for (int i = 0; i < TPs.Count; ++i)
            Destroy(TPs[i]);

        TPs.Clear();
    }
    void CreateTapPoints()
    {
        ClearTapPoints();

        float startPixel = TimeToPixel(mStart);
        float endPixel = TimeToPixel(mEnd);
        float size = mTPHeight * 0.35f;
        for (float pixel = startPixel; pixel < endPixel; pixel += mPixelPerTP)
        {
            GameObject obj = CreateTapPoint2(new Vector2(pixel, mTPHeight), size);
            TPs.Add(obj);
        }

        CreateBars();
    }

    float TimeToPixel(float time)
    {
        float tpIdx = time / mTimePerTP;
        return tpIdx * mPixelPerTP;
    }
    float PixelToTime(float pixel)
    {
        float timePerPixel = mTimePerTP / mPixelPerTP;
        return pixel * timePerPixel;
        
    }

    void PlayTickSound()
    {
        float timeOff = mPlayTime - mStart;
        int idx = (int)(timeOff / mTimePerTP);
        if (timeOff < 0 || idx >= TPs.Count)
        {
            mCurrentTPIndex = -1;
            return;
        }

        if (mCurrentTPIndex + 1 == idx && TPs[idx].tag == "1")
            mTickSound.Play();

        mCurrentTPIndex = idx;
    }
    void UpdateMarkerPosition()
    {
        float delayedTime = mPlayTime - Setting.Inst().MusicDelay;
        delayedTime = delayedTime < 0 ? 0 : delayedTime;
        float pixel = TimeToPixel(delayedTime);
        mMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(pixel, 0);
    }
    private void UpdatePosition()
    {
        if (!AutoMode)
            return;

        float markerPos = mMarker.transform.position.x;
        if (markerPos < 0 || Screen.width < markerPos)
        {
            Vector3 pos = transform.position;
            pos.x -= markerPos;
            transform.position = pos;
        }
    }

    void CreateMarker(float xPos)
    {
        float height = GetComponent<RectTransform>().rect.height;
        mMarker = new GameObject("marker", typeof(Image));
        mMarker.transform.SetParent(transform, false);
        RectTransform rt = mMarker.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(xPos, 0);
        rt.sizeDelta = new Vector2(3, height);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
    }
    void CreateBaseLine()
    {
        Transform transBaseLine = transform.Find("baseline");
        if (transBaseLine != null)
            Destroy(transBaseLine.gameObject);

        float pixel = TimeToPixel(mEnd);
        mBaseLine = new GameObject("baseline", typeof(Image));
        mBaseLine.transform.SetParent(transform, false);
        RectTransform rt = mBaseLine.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, mTPHeight);
        rt.sizeDelta = new Vector2(pixel, 3);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0.5f);
    }
    void CreateBars()
    {
        for (int i = 0; i < Bars.Count; ++i)
            Destroy(Bars[i]);

        Bars.Clear();

        for (int idx = 1; idx < TPs.Count; idx++)
        {
            if (idx % 4 != 0)
                continue;

            Vector2 tpA = TPs[idx - 1].GetComponent<RectTransform>().anchoredPosition;
            Vector2 tpB = TPs[idx].GetComponent<RectTransform>().anchoredPosition;
            Vector2 barPos = (tpA + tpB) / 2;

            GameObject obj = new GameObject("bar", typeof(Image));
            obj.transform.SetParent(transform, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = barPos;
            rt.sizeDelta = new Vector2(4, 10);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);

            Bars.Add(obj);
        }
    }

    //클릭한 위치의 노래 재생시간 반환
    public float ClickGraph()
    {
        //PointerEventData data = _data as PointerEventData;
        Vector3 point = Input.mousePosition;
        RectTransform rt = GetComponent<RectTransform>();
        float pressX = point.x - rt.anchoredPosition.x;
        float startPixel = TimeToPixel(mStart);
        float x = pressX- startPixel + mPixelPerTP / 2;
        int tpIdx = (int)(x / mPixelPerTP);
        tpIdx = Mathf.Max(tpIdx, 0);
        tpIdx = Mathf.Min(tpIdx, TPs.Count - 1);

        GameObject tapPoint = TPs[tpIdx];
        if((point - tapPoint.transform.position).magnitude < mPixelPerTP)
        {
            string isActive = tapPoint.tag;
            SetActiveTapPoint(tapPoint, isActive == "0" ? true : false);
            return -1;
        }

        float playOffpixel = Mathf.Max(0, pressX);
        float time = PixelToTime(playOffpixel);
        mPlayTime = time;
        return time;
    }
    public Bar[] ExportToBars()
    {
        List<Bar> bars = new List<Bar>();
        int cnt = TPs.Count / 4;
        for (int i = 0; i < cnt; ++i)
        {
            Bar bar = new Bar();
            bar.Main = TPs[i * 4 + 0].tag == "0" ? false : true;
            bar.PreHalf = TPs[i * 4 + 1].tag == "0" ? false : true;
            bar.Half = TPs[i * 4 + 2].tag == "0" ? false : true;
            bar.PostHalf = TPs[i * 4 + 3].tag == "0" ? false : true;
            bars.Add(bar);
        }
        return bars.ToArray();
    }
    public void ImportToBars(Bar[] bars)
    {
        int cnt = Math.Min(bars.Length, TPs.Count / 4);
        for (int i = 0; i < cnt; ++i)
        {
            SetActiveTapPoint(TPs[i * 4 + 0], bars[i].Main);
            SetActiveTapPoint(TPs[i * 4 + 1], bars[i].PreHalf);
            SetActiveTapPoint(TPs[i * 4 + 2], bars[i].Half);
            SetActiveTapPoint(TPs[i * 4 + 3], bars[i].PostHalf);

            //TPs[i * 4 + 0].tag = bars[i].Main ? "1" : "0";
            //TPs[i * 4 + 0].GetComponent<Image>().sprite = bars[i].Main ? mActiveImage : mEmptyImage;
            //TPs[i * 4 + 1].tag = bars[i].PreHalf ? "1" : "0";
            //TPs[i * 4 + 1].GetComponent<Image>().sprite = bars[i].PreHalf ? mActiveImage : mEmptyImage;
            //TPs[i * 4 + 2].tag = bars[i].Half ? "1" : "0";
            //TPs[i * 4 + 2].GetComponent<Image>().sprite = bars[i].Half ? mActiveImage : mEmptyImage;
            //TPs[i * 4 + 3].tag = bars[i].PostHalf ? "1" : "0";
            //TPs[i * 4 + 3].GetComponent<Image>().sprite = bars[i].PostHalf ? mActiveImage : mEmptyImage;
        }
    }
    public void CreateRandomTPs(bool random = true)
    {
        int barCount = TPs.Count / 4;
        Bar[] bars = Utils.CreateRandomBars(barCount);

        ImportToBars(bars);
    }
    private void SetActiveTapPoint(GameObject tp, bool isActive)
    {
        tp.tag = isActive ? "1" : "0";
        Image[] imgs = tp.GetComponentsInChildren<Image>();
        imgs[0].color = new Color(0, 0, 0, 0);
        imgs[1].color = isActive ? Color.green : Color.white;
        imgs[2].color = isActive ? Color.green : Color.white;
    }
}
