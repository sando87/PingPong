using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserTap : MonoBehaviour
{
    public int mBPM;
    public float mStartTime;
    public Sprite mActiveImage;
    public Sprite mEmptyImage;

    private float sampleDT = 0.01f;
    private float sampleTime = 180f;
    private int pixelPerSample = 2;
    private float threshold = 50.0f;

    private List<GameObject> TPs = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        float secPerPixel = sampleDT / pixelPerSample;
        float height = 200f;
        float secTPs = (120f / mBPM);
        for(float sec = mStartTime; sec < sampleTime; sec += secTPs)
        {
            float pixel = sec / secPerPixel;
            GameObject obj = CreateTapPoint(new Vector2(pixel, height));
            TPs.Add(obj);
        }
    }

    GameObject CreateTapPoint(Vector2 anchoredPos)
    {
        GameObject gameObj = new GameObject("TP", typeof(Image));
        gameObj.transform.SetParent(transform, false);
        gameObj.GetComponent<Image>().sprite = mEmptyImage;
        gameObj.tag = "0";
        RectTransform rt = gameObj.GetComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(50, 50);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        return gameObj;
    }

    public void OnClickChart(Vector2 point)
    {
        float secPerPixel = sampleDT / pixelPerSample;
        float secTPs = (120f / mBPM);
        Vector2 pos = transform.position;
        Vector2 relPos = point - pos;
        float currentTime = relPos.x * secPerPixel;
        currentTime -= mStartTime;
        int currentIdx = (int)(currentTime / secTPs);
        if (currentIdx < 0)
            currentIdx = 0;
        if (currentIdx >= TPs.Count)
            currentIdx = TPs.Count - 2;

        Vector2 obj1 = TPs[currentIdx].transform.position;
        Vector2 obj2 = TPs[currentIdx + 1].transform.position;
        if ((obj1 - point).magnitude < threshold)
        {
            string isActive = TPs[currentIdx].tag;
            TPs[currentIdx].GetComponent<Image>().sprite = isActive == "0" ? mActiveImage : mEmptyImage;
            TPs[currentIdx].tag = isActive == "0" ? "1" : "0";
        }
        else if ((obj2 - point).magnitude < threshold)
        {
            string isActive = TPs[currentIdx + 1].tag;
            TPs[currentIdx + 1].GetComponent<Image>().sprite = isActive == "0" ? mActiveImage : mEmptyImage;
            TPs[currentIdx + 1].tag = isActive == "0" ? "1" : "0";
        }
    }
}
