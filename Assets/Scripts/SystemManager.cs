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

    public CubePlayer Player;
    public AudioSource audioSource;

    private Song CurrentSong;
    private float accTime = -1f;
    private int IndexNextTP = 0;
    private List<TabInfo> tabPoints = new List<TabInfo>();

    private SystemState State = SystemState.None;
    // Start is called before the first frame update
    void Awake()
    {
        mInst = this;
    }

    // Update is called once per frame
    void Update()
    {
        switch(State)
        {
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

    IEnumerator ShowProgressBar()
    {
        LoadingBar loadingbar = LoadingBar.GetInst();
        loadingbar.Show();
        
        while(!NetworkClient.Inst().IsRecvData())
            yield return new WaitForEndOfFrame();

        float rate = 0;
        while (rate < 1f)
        {
            rate = NetworkClient.Inst().GetProgressState();
            loadingbar.SetProgress(rate);
            yield return new WaitForEndOfFrame();
        }
        loadingbar.Hide();
    }
    public void SelectSong(Song song)
    {
        if(song.BarCount == 0)
        {
            ICD.CMD_SongFile msg = new ICD.CMD_SongFile();
            msg.song = song;
            msg.FillHeader(ICD.ICDDefines.CMD_Download);
            NetworkClient.Inst().SendMsgToServer(msg);
            StartCoroutine(ShowProgressBar());
        }
        else if(song.DBID == -1)
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
            CurrentSong = song;
            UpdateCurve();
            CreateTapPointScripts();

            AudioClip clip = Resources.Load<AudioClip>(PathInfo.AudioClip + CurrentSong.FileNameNoExt + ".mp3");
            if (clip != null)
            {
                audioSource.clip = clip;
            }
            else
            {
                audioSource.clip = LoadAudioClip(CurrentSong.FilePath + CurrentSong.FileNameNoExt + ".mp3");
            }


            State = SystemState.Standby;
            pnRootUI.SetActive(false);
            Player.ResetCube();
        }
    }
    public void OnClickItem(BaseEventData _data)
    {
        PointerEventData data = _data as PointerEventData;
        ItemDisplay item = data.pointerEnter.GetComponentInParent<ItemDisplay>();
        SelectSong(item.SongInfo);
    }
    public void OnRecvSong(ICD.stHeader _msg, string _info)
    {
        if (_msg.GetType() != typeof(CMD_SongFile))
            return;

        ICD.CMD_SongFile msg = (ICD.CMD_SongFile)_msg;
        if(msg.head.cmd == ICD.ICDDefines.CMD_Download)
        {
            msg.song.FilePath = Application.persistentDataPath + "/";
            File.WriteAllBytes(Application.persistentDataPath + "/" + msg.song.FileNameNoExt + ".mp3", msg.stream.ToArray());
        }
        byte[] buf = Utils.Serialize(msg.song);
        File.WriteAllBytes(Application.persistentDataPath + "/" + msg.song.FileNameNoExt + ".bytes", buf);
    }
    public AudioClip LoadAudioClip(string filename)
    {
        string url = "file://" + filename;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            www.SendWebRequest();
            while (!www.isDone) { }
            if (www.isNetworkError)
                return null;

            return DownloadHandlerAudioClip.GetContent(www);
        }
    }
    private void PlaySong()
    {
        State = SystemState.WaitJump;
        float delay = CurrentSong.JumpDelay + Setting.MusicDelay;
        if (delay < 0)
            delay = 0;

        Invoke("StartJump", delay);
        audioSource.Play();
        accTime = -delay;
        IndexNextTP = 0;
    }
    private void StartJump()
    {
        State = SystemState.Playing;
        Player.StartJump();
    }
    public void StopJump()
    {
        tabPoints.Clear();
        audioSource.Stop();
        pnRootUI.SetActive(true);
    }

    private void UpdateCurve()
    {
        Setting.TimePerBar = 120.0f / CurrentSong.BPM;
        Curve.UpdateLinear(Setting.SpeedMoveX, 1);
        Curve.UpdateCurve(new Vector2(Setting.TimePerBar * 0.5f, Setting.JumpHeight), new Vector2(0, 0));
        Setting.JumpHeightHalf = Curve.GetCurveY(Setting.TimePerBar * 0.25f);
    }

    private void CreateTapPointScripts()
    {
        tabPoints.Clear();
        float accTime = 0;
        TabInfo[] times = ToDeltas(CurrentSong.Bars);
        Vector2 current = new Vector2(0, 0);
        float dir = 1.0f;
        for (int i = 0; i < times.Length; ++i)
        {
            accTime += times[i].time;
            float t = times[i].time;
            current.x += (dir * Setting.SpeedMoveX * t);
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
        float stepDelta = Setting.TimePerBar * 0.25f;
        float delayTime = Setting.TimePerBar;
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
