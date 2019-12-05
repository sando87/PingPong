using ICD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    private GameObject mPanelDisplay;
    private GameObject mPanelFuncItem;
    private Image TitleImage;
    private Text TitleName;
    private Text SingerName;
    private Image GradeImage;
    private bool mIsExpanded;
    private Button BtnPlay;
    private Button BtnDownload;
    private Button BtnUpload;
    private Button BtnEdit;
    private Button BtnSetting;
    private Button BtnDelete;

    private Song mSong;
    public Song SongInfo { get { return mSong; } set { mSong = value; UpdateButtonState(); } }
    // Start is called before the first frame update
    void Start()
    {
        FindChildUI();

        Song info = SongInfo;
        TitleImage.sprite = Resources.Load<Sprite>(PP.PathInfo.Images + info.FileNameNoExt + "png");
        GradeImage.sprite = Resources.Load<Sprite>(PP.PathInfo.Images + "star");
        TitleName.text = info.Title;
        SingerName.text = info.Artist;

        Expand(false);
        UpdateButtonState();
    }


    public void OnBtnClickItem()
    {
        Expand(!mIsExpanded);
    }
    public void OnBtnStartGame()
    {
        if (!SongInfo.Playable())
            return;

        SystemManager.Inst().SelectSong(SongInfo);
    }
    public void OnBtnDownload()
    {
        if (!NetworkClient.Inst().IsConnected())
        {
            MessageBox.Show("Network Error:", "Network Disconnected.", null);
            return;
        }

        ICD.CMD_SongFile msg = new ICD.CMD_SongFile();
        msg.song = SongInfo;
        msg.FillHeader(ICD.ICDDefines.CMD_Download);
        NetworkClient.Inst().SendMsgToServer(msg);
        StartCoroutine(ShowProgressBar());
        NetworkClient.Inst().mOnRecv.AddListener(OnRecvDownload);
    }
    public void OnBtnUpload()
    {
        if (!NetworkClient.Inst().IsConnected())
        {
            MessageBox.Show("Network Error:", "Network Disconnected.", null);
            return;
        }

        ICD.CMD_SongFile msg = new ICD.CMD_SongFile();
        msg.song = SongInfo;
        byte[] stream = File.ReadAllBytes(SongInfo.FilePath + SongInfo.FileNameNoExt + ".mp3");
        msg.stream.AddRange(stream);
        msg.FillHeader(ICD.ICDDefines.CMD_Upload);
        NetworkClient.Inst().SendMsgToServer(msg);
        NetworkClient.Inst().mOnRecv.AddListener(OnRecvUpload);
    }
    public void OnBtnEdit()
    {
        if (!SongInfo.Editalbe())
            return;

        MainCategory mainCate = GameObject.Find("btnMaker").GetComponent<MainCategory>();
        mainCate.OnClickButton();

        EditView ev = mainCate.SelectView.GetComponentInChildren<EditView>();
        ev.LoadSong(SongInfo, gameObject);

    }
    public void OnBtnSetting()
    {
    }
    public void OnBtnDelete()
    {
        string filename = Application.persistentDataPath + "/" + SongInfo.FileNameNoExt;
        File.Delete(filename + ".mp3");
        File.Delete(filename + ".bytes");
        Destroy(gameObject);
    }



    IEnumerator ShowProgressBar()
    {
        LoadingBar loadingbar = LoadingBar.Show();

        while (!NetworkClient.Inst().IsRecvData())
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
    private void OnRecvDownload(ICD.stHeader _msg, string _info)
    {
        if (_msg.GetType() != typeof(CMD_SongFile))
            return;
        if (_msg.head.cmd != ICD.ICDDefines.CMD_Download)
            return;

        ICD.CMD_SongFile msg = (ICD.CMD_SongFile)_msg;
        msg.song.FilePath = Application.persistentDataPath + "/";
        File.WriteAllBytes(Application.persistentDataPath + "/" + msg.song.FileNameNoExt + ".mp3", msg.stream.ToArray());
        byte[] buf = Utils.Serialize(msg.song);
        File.WriteAllBytes(Application.persistentDataPath + "/" + msg.song.FileNameNoExt + ".bytes", buf);
        SongInfo = msg.song;

        NetworkClient.Inst().mOnRecv.RemoveListener(OnRecvDownload);
    }
    private void OnRecvUpload(ICD.stHeader _msg, string _info)
    {
        if (_msg.GetType() != typeof(CMD_SongFile))
            return;
        if (_msg.head.cmd != ICD.ICDDefines.CMD_Upload)
            return;

        ICD.CMD_SongFile msg = (ICD.CMD_SongFile)_msg;
        try
        {
            SongInfo = msg.song;
            byte[] buf = Utils.Serialize(msg.song);
            File.WriteAllBytes(Application.persistentDataPath + "/" + msg.song.FileNameNoExt + ".bytes", buf);
        }
        catch(Exception ex)
        {
            MessageBox.Show(Application.persistentDataPath, msg.song.FileNameNoExt, null);
            MessageBox.Show("DBID: " + msg.song.DBID, ex.ToString(), null);
        }

        NetworkClient.Inst().mOnRecv.RemoveListener(OnRecvUpload);
    }
    private void Expand(bool expand)
    {
        mIsExpanded = expand;
        if(mIsExpanded)
        {
            float currentHeight = mPanelDisplay.GetComponent<RectTransform>().rect.height;
            float rate = 0.7f;
            float height = currentHeight / rate;
            int fontSize = (int)(Screen.width * 0.0625);
            GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, height);
            TitleName.fontSize = fontSize;
            SingerName.fontSize = (int)(fontSize * 0.7);

            mPanelDisplay.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1 - rate);
            mPanelFuncItem.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1 - rate);
        }
        else
        {
            float height = Screen.width * 0.18f;
            int fontSize = (int)(Screen.width * 0.0625);
            GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, height);
            TitleName.fontSize = fontSize;
            SingerName.fontSize = (int)(fontSize * 0.7);

            mPanelDisplay.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            mPanelFuncItem.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
        }
    }
    private void FindChildUI()
    {
        mPanelDisplay = transform.Find("pnDisplay").gameObject;
        mPanelFuncItem = transform.Find("pnFuncItem").gameObject;
        Transform[] childs = GetComponentsInChildren<Transform>();
        foreach (Transform child in childs)
        {
            string name = child.gameObject.name;
            switch (name)
            {
                case "SongImage": TitleImage = child.GetComponent<Image>(); break;
                case "Title": TitleName = child.GetComponent<Text>(); break;
                case "Singer": SingerName = child.GetComponent<Text>(); break;
                case "Grade": GradeImage = child.GetComponent<Image>(); break;
                case "pnDisplay": mPanelDisplay = child.gameObject; break;
                case "pnFuncItem": mPanelFuncItem = child.gameObject; break;
                case "btnPlay": BtnPlay = child.GetComponent<Button>(); break;
                case "btnDownload": BtnDownload = child.GetComponent<Button>(); break;
                case "btnUpload": BtnUpload = child.GetComponent<Button>(); break;
                case "btnEdit": BtnEdit = child.GetComponent<Button>(); break;
                case "btnSetting": BtnSetting = child.GetComponent<Button>(); break;
                case "btnDelete": BtnDelete = child.GetComponent<Button>(); break;
                default: break;
            }
        }
    }
    private void SetButtonEnable(Button btn, bool enable)
    {
        if (btn == null)
            return;

        btn.GetComponent<Image>().color = enable ? Color.white : Color.gray;
        btn.enabled = enable;
    }
    private void UpdateButtonState()
    {
        SetButtonEnable(BtnPlay, SongInfo.Playable());
        SetButtonEnable(BtnDownload, SongInfo.Downloadable());
        SetButtonEnable(BtnUpload, SongInfo.NeedUpdate());
        SetButtonEnable(BtnEdit, SongInfo.Editalbe());
        SetButtonEnable(BtnDelete, SongInfo.Deletable());
    }

    void AddEventHanlerDynamically()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        //entry.callback.AddListener(...);
        trigger.triggers.Add(entry);
    }
}
