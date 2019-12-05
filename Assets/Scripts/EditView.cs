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

public class EditView : MonoBehaviour
{
    public GameObject mInputFilename;
    public GameObject mInputTitle;
    public GameObject mInputArtist;

    public GameObject mPanelBpmDelay;
    public GameObject mPanelTPs;
    public GameObject mPanelFunctions;

    private const string OBJECT_NAME_BPM = "bpm";
    private const string OBJECT_NAME_START = "start";
    private const string OBJECT_NAME_END = "end";
    private const string OBJECT_NAME_PLAYER = "player";

    CustomTP mCustomTP;
    AudioSource mAudioSrc;
    GameObject mEditTarget;

    // Start is called before the first frame update
    void Start()
    {
        mCustomTP = mPanelTPs.GetComponentInChildren<CustomTP>();
        mAudioSrc = GetComponent<AudioSource>();

        mPanelBpmDelay.SetActive(false);
        mPanelTPs.SetActive(false);
        mPanelFunctions.SetActive(false);
    }
    

    public void OnBtnLoadMusic()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
#endif
        SimpleFileBrowser.FileBrowser.ShowLoadDialog(OnLoadSuccess, null);

    }
    public void OnEditApply()
    {
        string bpm = mPanelBpmDelay.transform.Find(OBJECT_NAME_BPM).GetComponent<InputField>().text;
        string start = mPanelBpmDelay.transform.Find(OBJECT_NAME_START).GetComponent<InputField>().text;
        string end = mPanelBpmDelay.transform.Find(OBJECT_NAME_END).GetComponent<InputField>().text;
        Bar[] backupBars = mCustomTP.ExportToBars();
        mCustomTP.Initialize(int.Parse(bpm), float.Parse(start), float.Parse(end));
        mCustomTP.ImportToBars(backupBars);
    }
    public void OnClickTPs()
    {
        float currentPlayTime = mCustomTP.ClickGraph();
        if(currentPlayTime > 0)
        {
            mAudioSrc.time = currentPlayTime;
        }
    }
    public void OnBtnPlayMusic()
    {
        Text comp = mPanelFunctions.transform.Find(OBJECT_NAME_PLAYER).GetComponentInChildren<Text>();
        if(comp.text == "Play")
        {
            //mAudioSrc.time = mCustomTP.PlayTime;
            mAudioSrc.Play();
            comp.text = "Stop";
            mCustomTP.IsPlay = true;
            mCustomTP.AutoMode = true;
        }
        else if(comp.text == "Stop")
        {
            mAudioSrc.Pause();
            comp.text = "Play";
            mCustomTP.IsPlay = false;
        }
    }
    public void OnBtnRandomTPs()
    {
        mCustomTP.CreateRandomTPs();
    }
    public void OnBtnFinish()
    {
        string fullname = mInputFilename.GetComponent<InputField>().text;
        string[] splits = fullname.Split(new char[2] { '\\', '/' });
        string filename = splits[splits.Length - 1];
        Song song = new Song();
        song.DBID = -1;
        song.BPM = mCustomTP.BPM;
        song.StartTime = mCustomTP.StartTime;
        song.EndTime = mCustomTP.EndTime;
        song.FilePath = String.Join("/", splits, 0, splits.Length - 1) + "/";
        song.FileNameNoExt = splits[splits.Length - 1].Split('.')[0];
        song.Title = mInputTitle.GetComponent<InputField>().text;
        song.Artist = mInputArtist.GetComponent<InputField>().text;
        song.StarCount = 0;
        song.UserID = Setting.Inst().UserName;
        song.Bars = mCustomTP.ExportToBars();
        song.BarCount = song.Bars.Length;

        byte[] buf = Utils.Serialize(song);
        File.WriteAllBytes(Application.persistentDataPath + "/" + song.FileNameNoExt + ".bytes", buf);

        if(mEditTarget != null)
        {
            mEditTarget.GetComponent<ItemDisplay>().SongInfo = song;
        }
        else
        {
            GameObject musicListObj = GameObject.Find("btnList").GetComponent<MainCategory>().SelectView;
            MusicLoader loader = musicListObj.GetComponentInChildren<MusicLoader>();
            loader.AddNewSong(song);
        }
        MainCategory mainCate = GameObject.Find("btnList").GetComponent<MainCategory>();
        mainCate.OnClickButton();

        ResetAll();
        mCustomTP.ReleaseAll();
    }
    public void LoadSong(Song song, GameObject target)
    {
        ResetAll();
        mCustomTP.ReleaseAll();

        //mFullName = Application.persistentDataPath + "/" + song.FileNameNoExt + ".mp3";
        mEditTarget = target;
        string fullName = song.FilePath + song.FileNameNoExt + ".mp3";

        mInputFilename.GetComponent<InputField>().text = fullName;
        mInputTitle.GetComponent<InputField>().text = song.Title;
        mInputArtist.GetComponent<InputField>().text = song.Artist;
        
        AudioClip audioClip = Utils.LoadMusic(fullName);
        mAudioSrc.clip = audioClip;
        mAudioSrc.volume = 0.5f;

        int bpm = song.BPM;
        float start = song.StartTime;
        float end = song.EndTime;
        mPanelBpmDelay.transform.Find(OBJECT_NAME_BPM).GetComponent<InputField>().text = bpm.ToString();
        mPanelBpmDelay.transform.Find(OBJECT_NAME_START).GetComponent<InputField>().text = start.ToString();
        mPanelBpmDelay.transform.Find(OBJECT_NAME_END).GetComponent<InputField>().text = end.ToString();
        mCustomTP.Initialize(bpm, start, end);
        mCustomTP.ImportToBars(song.Bars);

        mPanelBpmDelay.SetActive(true);
        mPanelTPs.SetActive(true);
        mPanelFunctions.SetActive(true);
    }


    private void OnLoadSuccess(string fullname)
    {
        ResetAll();
        mCustomTP.ReleaseAll();

        TagLib.File mp3 = TagLib.File.Create(fullname);
        mInputFilename.GetComponent<InputField>().text = fullname;
        mInputTitle.GetComponent<InputField>().text = mp3.Tag.Title;
        mInputArtist.GetComponent<InputField>().text = mp3.Tag.AlbumArtists.Length == 0 ? " " : mp3.Tag.AlbumArtists[0];

        mEditTarget = null;
        StartCoroutine(LoadingProcess(fullname));
    }
    IEnumerator LoadingProcess(string filename)
    {
        LoadingBar loading = LoadingBar.Show(true);
        //loading.SetProgress(0);
        string url = "file://" + filename;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            //loading.SetProgress(0.3f);

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                mAudioSrc.clip = audioClip;
                mAudioSrc.volume = 0.5f;

                //loading.SetProgress(0.5f);
                yield return null;
                // DetectBPM(audioClip);
                IEnumDetectBPM detector = new IEnumDetectBPM(audioClip);
                yield return detector;
                int bpm = detector.BPM;
                //loading.SetProgress(0.7f);

                float start = 0.5f;
                float end = audioClip.length;
                mPanelBpmDelay.transform.Find(OBJECT_NAME_BPM).GetComponent<InputField>().text = bpm.ToString();
                mPanelBpmDelay.transform.Find(OBJECT_NAME_START).GetComponent<InputField>().text = start.ToString();
                mPanelBpmDelay.transform.Find(OBJECT_NAME_END).GetComponent<InputField>().text = end.ToString();
                mCustomTP.Initialize(bpm, start, end);
                mCustomTP.CreateRandomTPs(false);

                mPanelBpmDelay.SetActive(true);
                mPanelTPs.SetActive(true);
                mPanelFunctions.SetActive(true);

                yield return null;
                //loading.SetProgress(1.0f);
            }
        }
        loading.Hide();
    }
    private int DetectBPM(AudioClip clip)
    {
        //BMPDector dt = new BMPDector();
        //int[] bpms = dt.DetectBPM(clip);
        //if (bpms != null & bpms.Length > 0)
        //{
        //    return bpms[0];
        //}
        return 0;
    }
    private void ResetAll()
    {
        mEditTarget = null;
        mInputFilename.GetComponent<InputField>().text = "";
        mInputTitle.GetComponent<InputField>().text = "";
        mInputArtist.GetComponent<InputField>().text = "";
        mPanelFunctions.transform.Find(OBJECT_NAME_PLAYER).GetComponentInChildren<Text>().text = "Play";
        mPanelBpmDelay.transform.Find(OBJECT_NAME_BPM).GetComponent<InputField>().text = "0";
        mPanelBpmDelay.transform.Find(OBJECT_NAME_START).GetComponent<InputField>().text = "0";
        mPanelBpmDelay.transform.Find(OBJECT_NAME_END).GetComponent<InputField>().text = "0";

        mPanelBpmDelay.SetActive(false);
        mPanelTPs.SetActive(false);
        mPanelFunctions.SetActive(false);
    }
}
