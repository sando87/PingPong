using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlayer : MonoBehaviour
{
    //track1 : 0.938f , 1161.4f
    // 1 : 1,224.0f
    Rigidbody rb;
    ParticleSystem ps;
    public GameObject prefabTapPoint;
    private List<GameObject> TapPoints = new List<GameObject>();

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
    private float coff = 0;
    private float coffA = 0;
    private float coffB = 0;
    private float offX = 0;
    private float offY = 0;

    int dir = 1;
    public int speed = 10;
    public int rotSpeed = 120;

    private float speeddynamic = 0; //user의 tab오차를 보정해주기 위한 x충 동적 스피드 조정
    private int tapCount = 0;
    // Start is called before the first frame update
    float CalcCoff(Vector2 basePt, Vector2 pt)
    {
        coffA = basePt.x;
        coffB = basePt.y;
        return (pt.y - basePt.y) / ((pt.x - basePt.x) * (pt.x - basePt.x));
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject obj = GameObject.Find("Particle System");
        ps = obj.GetComponent<ParticleSystem>();
        coff = CalcCoff(new Vector2(musicSpeed*0.5f, cubeJumpHeight), new Vector2(0, 0));

        BarArray = new Bar[4];
        for (int i = 0; i < BarArray.Length; ++i)
        {
            BarArray[i].Main = true;
            BarArray[i].Half = false;
            BarArray[i].PostHalf = false;
            BarArray[i].PreHalf = false;
        }
        BarArray[1].Main = true;
        BarArray[1].Half = true;
        BarArray[1].PostHalf = false;
        BarArray[1].PreHalf = false;

        Vector2[] pts = ToPoints(BarArray);
        for(int idx = 0; idx < pts.Length; ++idx)
        {
            GameObject tap = Instantiate(prefabTapPoint, pts[idx], Quaternion.identity);
            TapPoints.Add(tap);
        }

        speeddynamic = speed;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            float offsetX = TapPoints[tapCount].transform.position.x - transform.position.x;
            speeddynamic = dir > 0 ? speed - offsetX : speed + offsetX;
            float offsetY = transform.position.y - TapPoints[tapCount].transform.position.y;
            float offsetT = HeightToTime(transform.position.y);
            //coff = CalcCoff(new Vector2(0.5f , cubeJumpHeight - offsetY), new Vector2(0, 0)); half
            coff = CalcCoff(new Vector2(offsetT - musicSpeed * 0.5f, cubeJumpHeight - offsetY), new Vector2(0, 0));
            float deg = NextHeight(1.0f);
            Debug.Log(deg+"...."+ offsetY);

            tapCount++;

            offX = Time.deltaTime;
            offY = pos.y;
            pos.y = offY + NextHeight(offX);

            dir *= -1;

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
    Vector2[] ToPoints(Bar[] bars)
    {
        Vector2 startPos = new Vector2(0,0);
        List<Vector2> list = new List<Vector2>();
        float[] times = ToDeltas(bars);
        Vector2 current = startPos;
        float dir = 1.0f;
        for (int i = 0; i< times.Length; ++i)
        {
            float t = times[i];
            current.x += NextX(dir * speed, t);
            current.y += NextHeight(t);
            list.Add(current);
            dir *= -1;
        }

        return list.ToArray();
    }
    float[] ToDeltas(Bar[] bars)
    {
        float stepDelta = musicSpeed * 0.25f;
        float delayTime = musicSpeed;
        List<float> times = new List<float>();
        for (int i = 0; i < bars.Length; ++i)
        {
            if (bars[i].Main) { times.Add(delayTime); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].PreHalf) { times.Add(delayTime); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].Half) { times.Add(delayTime); delayTime = stepDelta; }
            else delayTime += stepDelta;

            if (bars[i].PostHalf) { times.Add(delayTime); delayTime = stepDelta; }
            else delayTime += stepDelta;
        }
        return times.ToArray();
    }

}
