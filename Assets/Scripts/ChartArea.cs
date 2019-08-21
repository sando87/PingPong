using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartArea : MonoBehaviour
{
    [SerializeField] private Sprite mCircleSprite;
    [SerializeField] private AudioClip mAudioClip;
    private RectTransform graphContainer;
    private GameObject mMarker = null;
    private List<GameObject> lines = new List<GameObject>();

    private float accTime = 0;
    private float sampleDT = 0.01f;
    private float sampleTime = 180f;
    private int pixelPerSample = 2;
    private float refHeight = 400f;
    private float currentOffTime = 0;
    private float[] originSoundData = null;
    private float[] sampledSoundData = null;

    private void Awake()
    {
        graphContainer = GetComponent<RectTransform>();
        originSoundData = new float[mAudioClip.samples * mAudioClip.channels];
        mAudioClip.GetData(originSoundData, 0);
        sampledSoundData = Sample(originSoundData, sampleDT);
        ShowGraph(sampledSoundData, currentOffTime);
        int width = (int)(pixelPerSample * sampleTime / sampleDT) / Screen.width + 1;
        Vector2 max = GetComponent<RectTransform>().anchorMax;
        GetComponent<RectTransform>().anchorMax = new Vector2(width, max.y);
    }
    private void Update()
    {
        if(mMarker != null)
        {
            accTime += Time.deltaTime;
            int cnt = (int)(accTime / sampleDT);
            mMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(cnt * pixelPerSample, 0);
        }
    }
    private float[] Sample(float[] origin, float dt)
    {
        int jumpIdx = (int)(dt * mAudioClip.frequency);
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
        gameObj.GetComponent<Image>().sprite = mCircleSprite;
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
        CreateMarker(0);
        GetComponent<AudioSource>().Play();
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
}
