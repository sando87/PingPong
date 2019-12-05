using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using PP;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using ICD;

/*
 * <SystemManager 클래스가 하는 일>
 * 1. 첫 화면 구성(파일에서 음악 리스트를 로드해서 화면 List를 구성한다.
 * 2. 사용자가 음악 선택시 해당 정보로 tapPoint들을 구성한다.(아직 게임Object 생성x)
 * 3. Play도중 tapPoint GameObject를 시간에따라 순차적으로 생성한다.
 */

public class SystemManager : MonoBehaviour
{
    static private SystemManager mInst = null;
    static public SystemManager Inst() { return mInst; }

    public GameObject pnContents;
    public GameObject prefabListItem;
    public GameObject pnRootUI;
    public GameObject PrefabTapPoint;
    public GameObject CountDownObject;
    public Sprite[] Numbers = new Sprite[3];

    public CubePlayer Player;
    public AudioSource audioSource;

    private Song CurrentSong;
    private float accTime = -1f;
    private int IndexNextTP = 0;
    private List<TabInfo> tabPoints = new List<TabInfo>();

    CacheSystem<AudioClip> mCacheMusic = new CacheSystem<AudioClip>();

    private SystemState State = SystemState.None;
    // Start is called before the first frame update
    void Awake()
    {
        mInst = this;
    }
    
    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case SystemState.UIMode:
                break;
            case SystemState.Standby:
                if (Input.GetMouseButtonDown(0))
                    PlaySong();
                break;
            case SystemState.WaitJump:
            case SystemState.Playing:
                accTime += Time.deltaTime;
                InstantiateTapPoint();
                break;
        }
    }

    public void SelectSong(Song song)
    {
        CurrentSong = song;
        UpdateCurve();
        CreateTapPointScripts();

        if(CurrentSong.UserID == Defs.ADMIN_USERNAME)
        {
            audioSource.clip = Resources.Load<AudioClip>(PathInfo.DefaultMusics + "/" + CurrentSong.FileNameNoExt);
        }
        else
        {
            string fullname = CurrentSong.FilePath + CurrentSong.FileNameNoExt + ".mp3";
            audioSource.clip = mCacheMusic.CacheOrLoad(fullname, (name) => { return Utils.LoadMusic(name); });
        }

        State = SystemState.Standby;
        pnRootUI.SetActive(false);
        Player.ResetCube();
    }
    private void PlaySong()
    {
        State = SystemState.WaitJump;
        float gapBars = 2f * 60f / CurrentSong.BPM;
        float jumpDelay = gapBars * 3f + Setting.Inst().MusicDelay;
        float playMusicDelay = gapBars * 4f - CurrentSong.StartTime;
        float playOff = playMusicDelay > 0 ? 0 : playMusicDelay * -1;

        StartCoroutine(CountDown());
        StartCoroutine(PlayMusic(playOff, Mathf.Max(0, playMusicDelay)));
        Invoke("StartJump", jumpDelay);
        accTime = -jumpDelay;
        IndexNextTP = 0;
    }
    private void StartJump()
    {
        State = SystemState.Playing;
        Player.StartJump();
    }
    IEnumerator PlayMusic(float off, float delay)
    {
        if(delay > 0)
        {
            off = 0;
            yield return new WaitForSeconds(delay);
        }

        if(State != SystemState.UIMode)
        {
            audioSource.time = off;
            audioSource.Play();
        }
    }
    IEnumerator CountDown()
    {
        float waitTime = 2f * 60f / CurrentSong.BPM;
        SpriteRenderer mesh = CountDownObject.GetComponent<SpriteRenderer>();
        CountDownObject.SetActive(true);
        mesh.sprite = Numbers[2];
        yield return new WaitForSeconds(waitTime);
        mesh.sprite = Numbers[1];
        yield return new WaitForSeconds(waitTime);
        mesh.sprite = Numbers[0];
        yield return new WaitForSeconds(waitTime);
        CountDownObject.SetActive(false);
    }
    public void StopJump()
    {
        State = SystemState.UIMode;
        tabPoints.Clear();
        audioSource.Stop();
        pnRootUI.SetActive(true);
    }

    private void UpdateCurve()
    {
        Setting.Inst().TimePerBar = 120.0f / CurrentSong.BPM;
        Curve.UpdateLinear(Setting.Inst().SpeedMoveX, 1);
        Curve.UpdateCurve(new Vector2(Setting.Inst().TimePerBar * 0.5f, Setting.Inst().JumpHeight), new Vector2(0, 0));
        Setting.Inst().JumpHeightHalf = Curve.GetCurveY(Setting.Inst().TimePerBar * 0.25f);
    }

    private void CreateTapPointScripts()
    {
        tabPoints.Clear();
        float accTime = 0;
        Bar[] bars = null;

        if (Setting.Inst().PlayMode == 1) //Random Mode
            bars = Utils.CreateRandomBars(CurrentSong.BarCount);
        else
            bars = CurrentSong.Bars;

        TabInfo[] times = ToDeltas(bars);
        Vector2 current = new Vector2(0, 0);
        float dir = 1.0f;
        for (int i = 0; i < times.Length; ++i)
        {
            accTime += times[i].time;
            float t = times[i].time;
            current.x += (dir * Setting.Inst().SpeedMoveX * t);
            current.y += Curve.GetCurveY(t);
            dir *= -1;

            times[i].idxStepToNext = i < times.Length - 1 ? times[i + 1].idxStep - times[i].idxStep : -1; //한마디에 4개의 step을 의미(한박,반박,반의반x2)
            times[i].worldPos = current;
            times[i].time = accTime;
            tabPoints.Add(times[i]);
        }
    }
    TabInfo[] ToDeltas(Bar[] bars)
    {
        int stepIndex = 0;
        float stepDelta = Setting.Inst().TimePerBar * 0.25f;
        float delayTime = Setting.Inst().TimePerBar;
        List<TabInfo> times = new List<TabInfo>();
        for (int i = 0; i < bars.Length; ++i)
        {
            if (bars[i].Main) { times.Add(new TabInfo(delayTime, TabType.Main, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].PreHalf) { times.Add(new TabInfo(delayTime, TabType.PreHalf, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].Half) { times.Add(new TabInfo(delayTime, TabType.Half, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;

            if (bars[i].PostHalf) { times.Add(new TabInfo(delayTime, TabType.PostHalf, stepIndex)); delayTime = stepDelta; }
            else delayTime += stepDelta;
            stepIndex++;
        }
        return times.ToArray();
    }
    private bool InstantiateTapPoint()
    {
        if (IndexNextTP >= tabPoints.Count)
            return false;

        TabInfo info = tabPoints[IndexNextTP];
        if (accTime >= tabPoints[IndexNextTP].time - TabPoint.Delay)
        {
            Vector3 pos = info.worldPos;
            GameObject tab = Instantiate(PrefabTapPoint, pos, Quaternion.identity);
            TabPoint tp = tab.GetComponent<TabPoint>();
            info.script = tp;
            tp.TapInfo = info;
            tabPoints[IndexNextTP] = info;
            IndexNextTP++;
            return true;
        }
        return false;
    }
    public TabInfo GetTapInfo(int index)
    {
        return tabPoints[index];
    }
}
