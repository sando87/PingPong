using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;

public class CubePlayer : MonoBehaviour
{
    //track1 : 0.938f , 1161.4f
    // 1 : 1,224.0f
    ParticleSystem ps;
    AudioSource audioSource;
    Camera cam;
    public GameObject prefabTapPoint;
    private List<TabPoints> tapPointlist = new List<TabPoints>();

    struct Bar
    {
        public bool Main;
        public bool Half;
        public bool PreHalf;
        public bool PostHalf;
    }

    Bar[] BarArray = null;
    public float musicSpeed = 0.938f;
    public float cubeJumpHeight = 10.0f;
    private float cubeJumpHeight_half = 7.5f;
    private float coff = 0;
    private float coffA = 0;
    private float coffB = 0;
    private float offX = 0;
    private float offY = 0;

    public float offTab = 0.1f;
    public float offStart = 2.0f;
    public float specFactor = 8.0f;
    private float specTime = 0;

    int dir = 1;
    public float speed = 10f;
    public float rotSpeed = 120f;
    private bool isStarted = false;

    private float speeddynamic = 0; //user의 tab오차를 보정해주기 위한 x충 동적 스피드 조정
    private int tapCount = 0;
    private Vector3 camTarget;
    float camSlope = 0;
    float camDelta = 0;
    Vector3 camDir = new Vector3();
    // Start is called before the first frame update
    void CalcCoff(Vector2 basePt, Vector2 pt)
    {
        coffA = basePt.x;
        coffB = basePt.y;
        coff = (pt.y - basePt.y) / ((pt.x - basePt.x) * (pt.x - basePt.x));
    }
    void Start()
    {
        specTime = musicSpeed / specFactor;
        GameObject obj = GameObject.Find("Particle System");
        ps = obj.GetComponent<ParticleSystem>();
        audioSource = GameObject.Find("AudioPlayer").GetComponent<AudioSource>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        camTarget = cam.transform.position;
        CalcCoff(new Vector2(musicSpeed*0.5f, cubeJumpHeight), new Vector2(0, 0));
        cubeJumpHeight_half = NextHeight(musicSpeed * 0.25f);
        speeddynamic = speed;

        BarArray = new Bar[16];
        for (int i = 0; i < BarArray.Length; ++i)
        {
            BarArray[i].Main = true;
            BarArray[i].Half =     UnityEngine.Random.Range(0, 3) == 1 ? true : false;
            BarArray[i].PostHalf =  UnityEngine.Random.Range(0, 5) == 1 ? true : false;
            BarArray[i].PreHalf =  UnityEngine.Random.Range(0, 5) == 1 ? true : false;
        }

        CreateTapPoints(BarArray);

        //float halfSec = musicSpeed * 0.5f;
        //float camX = NextX(dir * speeddynamic, halfSec);
        //float camY = NextHeight(halfSec);
        //UpdateCameraFactor(camX, camY);

    }

    public void OnBtnStart()
    {
        isStarted = true;
        foreach(var item in tapPointlist)
            item.isStarted = isStarted;

        accTime = -offStart;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //EmitEveryBar();
        if (!StandByForSec())
            return;

        Vector3 pos = transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            TabPoints tp = tapPointlist[tapCount];
            TabPoints tpNext = tapPointlist[tapCount + 1];
            float offsetX = tp.transform.position.x - transform.position.x;
            offsetX = dir > 0 ? offsetX : -offsetX;
            float offT = (1 / speeddynamic) * offsetX;
            if (Mathf.Abs(offT) > specTime)
            {
                audioSource.Stop();
                Destroy(gameObject);
                return;
            }

            float nextTime = tpNext.lifetime - tp.lifetime;
            float time = nextTime + offT;
            float dist = nextTime * speed - offsetX;
            speeddynamic = dist / time;


            float offsetY = transform.position.y - tp.transform.position.y;
            //float offsetT = HeightToTime(transform.position.y - offY);
            //coff = CalcCoff(new Vector2(0.5f , cubeJumpHeight - offsetY), new Vector2(0, 0)); half

            
            float baseT = 0;
            if (nextTime > musicSpeed * 0.9f)
            {
                float fixedH = cubeJumpHeight - offsetY;
                float fixedT = nextTime + offT;
                float fixedY = -offsetY;
                baseT = CalcCenterTime2(fixedH, fixedT, fixedY, false);
            }
            else if (nextTime > musicSpeed * 0.7f)
            {
                float fixedH = cubeJumpHeight - offsetY;
                float fixedT = nextTime + offT;
                float fixedY = cubeJumpHeight_half - offsetY;
                baseT = CalcCenterTime2(fixedH, fixedT, fixedY, false);
            }
            else if(nextTime > musicSpeed * 0.4f)
            {
                baseT = (nextTime + offT);
            }
            else if (nextTime > musicSpeed * 0.2f)
            {
                float fixedH = cubeJumpHeight - offsetY;
                float fixedT = nextTime + offT;
                float fixedY = cubeJumpHeight_half - offsetY;
                baseT = CalcCenterTime2(fixedH, fixedT, fixedY, true);
            }

            float baseY = cubeJumpHeight - offsetY;
            CalcCoff(new Vector2(baseT, baseY), new Vector2(0, 0));

            offX = Time.deltaTime;
            offY = pos.y;
            float retf = NextHeight(offX);
            pos.y = offY + retf;

            tapCount++;
            dir *= -1;

            float halfSec = musicSpeed * 0.5f;
            float camX = pos.x + NextX(dir * speeddynamic, halfSec);
            float camY = offY + NextHeight(halfSec);
            UpdateCameraFactor(camX, camY);

            Destroy(tp.gameObject);

            ps.transform.position = transform.position;
            ps.Play();
        }
        else
        {
            offX += Time.deltaTime;
            pos.y = offY + NextHeight(offX);
        }

        pos.x += NextX(dir * speeddynamic, Time.deltaTime);
        transform.position = pos;
        transform.Rotate(new Vector3(0, 0, -1), dir * rotSpeed * Time.deltaTime);
        MoveCamera();
    }

    void UpdateCameraFactor(float x, float y)
    {
        Vector3 targetPos = new Vector3(x, y + (cubeJumpHeight * 0.5f), -10);
        float len = (cam.transform.position - targetPos).magnitude;
        if (len < 0.5f)
            return;

        camTarget = targetPos;
        float t = musicSpeed * 0.5f;
        camSlope = len / (t * t);
        camDelta = 0;
        camDir = cam.transform.position - camTarget;
        camDir.Normalize();
    }
    void MoveCamera()
    {
        Vector3 newPos = transform.position;
        newPos.z = cam.transform.position.z;
        cam.transform.position = newPos;
        return;

        float t = musicSpeed * 0.5f;
        if (camDelta > t)
            camDelta = t;
        float y = camSlope * (camDelta - t) * (camDelta - t);
        Vector3 newPt = camTarget + camDir * y;
        cam.transform.position = newPt;
        camDelta += Time.deltaTime;
    }

    private void CreateTapPoints(Bar[] barArray)
    {
        float acctime = 0;
        TabInfo[] times = ToDeltas(barArray);
        Vector2 current = new Vector2(0, 0);
        float dir = 1.0f;
        for (int i = 0; i < times.Length; ++i)
        {
            acctime += times[i].time;
            float t = times[i].time + offTab;
            current.x += NextX(dir * speed, t);
            current.y += NextHeight(t);
            dir *= -1;

            GameObject tab = Instantiate(prefabTapPoint, current, Quaternion.identity);
            TabPoints script = tab.GetComponent<TabPoints>();
            script.index = i;
            script.type = times[i].type;
            script.lifetime = acctime;
            script.musicspeed = musicSpeed;
            script.startOff = offStart;
            tapPointlist.Add(script);
        }
    }

    float CalcCenterTime(double offY, double time)
    {
        if (Math.Abs(offY) < 0.0001f)
            return musicSpeed * 0.5f;

        double s = time;
        double y1 = cubeJumpHeight - offY;
        double y2 = offY * -1f;
        double a = y2;
        double b = -4f * y1 * s;
        double c = 4f * y1 * s * s;
        double d = Math.Sqrt(b * b - 4 * a * c);
        //double t1 = (-b + d) / (2 * a);
        double t2 = (-b - d) / (2 * a);
        return (float)t2 * 0.5f;
    }
    float CalcCenterTime2(double height, double time, double y, bool whichOne)
    {
        double s = time;
        double y1 = height;
        double y2 = y;
        double a = y2;
        double b = -4f * y1 * s;
        double c = 4f * y1 * s * s;
        double d = Math.Sqrt(b * b - 4 * a * c);
        double t1 = (-b + d) / (2 * a);
        double t2 = (-b - d) / (2 * a);
        float t = whichOne ? (float)t1 : (float)t2;
        return t * 0.5f;
    }

    float accTime = 0;
    void EmitEveryBar()
    {
        accTime += Time.deltaTime;
        if(accTime > musicSpeed)
        {
            accTime -= musicSpeed;
            ps.transform.position = transform.position;
            ps.Play();
        }
    }

    float waitTime = 0;
    bool StandByForSec()
    {
        if (!isStarted)
            return false;

        waitTime += Time.deltaTime;
        return (waitTime > offStart) ? true : false;
    }
    float NextHeight(float t)
    {
        return coff * (t - coffA) * (t - coffA) + coffB;
        //return coff * t * (t - musicSpeed);
    }
    float HeightToTime(float h)
    {
        float a = coff;
        float b = -coff * musicSpeed;
        float c = -h;
        float d = Mathf.Sqrt(b * b - 4 * a * c);
        return (-b - d) / (2 * a);
    }
    float NextX(float speed, float t)
    {
        return speed * t;
    }
    TabInfo[] ToDeltas(Bar[] bars)
    {
        float stepDelta = musicSpeed * 0.25f;
        float delayTime = musicSpeed;
        List<TabInfo> times = new List<TabInfo>();
        for (int i = 0; i < bars.Length; ++i)
        {
            if (bars[i].Main) { times.Add(new TabInfo(delayTime, TabType.Main)); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].PreHalf) { times.Add(new TabInfo(delayTime, TabType.PreHalf)); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].Half) { times.Add(new TabInfo(delayTime, TabType.Half)); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].PostHalf) { times.Add(new TabInfo(delayTime, TabType.PostHalf)); delayTime = stepDelta; }
            else delayTime += stepDelta;
        }
        return times.ToArray();
    }

}
