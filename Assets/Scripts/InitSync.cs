using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class InitSync : MonoBehaviour
{
    public GameObject panelMainCategory;
    public GameObject panelMusicList;
    public GameObject panelEditView;
    public GameObject panelSetting;

    private AudioSource sound;
    private Slider slider;
    private GameObject bar;
    private GameObject syncRef;

    private Setting mSetting;
    private bool mIsPlaying = false;
    private float mAccTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        panelMainCategory.SetActive(false);
        panelMusicList.SetActive(false);
        panelEditView.SetActive(false);
        panelSetting.SetActive(false);
        float height = panelMainCategory.GetComponent<RectTransform>().rect.height;
        panelMusicList.transform.position = new Vector3(0, height, 0);
        panelEditView.transform.position = new Vector3(0, height, 0);
        panelSetting.transform.position = new Vector3(0, height, 0);
        mSetting = panelSetting.GetComponentInChildren<Setting>();

        if (mSetting.IsFirst())
        {
            mSetting.SetInitalized();

            sound = GetComponent<AudioSource>();
            slider = GetComponentInChildren<Slider>();
            bar = transform.Find("imgBar").gameObject;
            syncRef = transform.Find("imgSync").gameObject;

            if (NetworkClient.Inst().IsConnected())
            {
                ICD.CMD_UserInfo info = new ICD.CMD_UserInfo();
                info.body.devicename = SystemInfo.deviceUniqueIdentifier;
                info.FillHeader(ICD.ICDDefines.CMD_NewUser);
                NetworkClient.Inst().mOnRecv.AddListener(OnRecvNewUser);
                NetworkClient.Inst().SendMsgToServer(info);
            }
        }
        else
        {
            ICD.CMD_UserInfo info = new ICD.CMD_UserInfo();
            info.body.username = mSetting.UserName;
            info.body.reserve = 1; //LOGIN
            info.FillHeader(ICD.ICDDefines.CMD_LoggingUser);
            NetworkClient.Inst().SendMsgToServer(info);

            MainCategory.PreviousView = panelMusicList;
            panelMusicList.SetActive(true);
            panelMainCategory.SetActive(true);
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mIsPlaying)
        {
            float preTime = mAccTime;
            mAccTime += Time.deltaTime;
            if (preTime < 0 && 0 <= mAccTime)
            {
                sound.Play();
                UpdateBarPos();
            }
            else if(mAccTime > 1.0f)
            {
                mIsPlaying = false;
                sound.Stop();
                sound.time = 0;
                mAccTime = 0;
                bar.transform.position = syncRef.transform.position;
            }
            else
                UpdateBarPos();
        }
    }

    private void UpdateBarPos()
    {
        float pixelPerSec = 100.0f;
        float currentPixelOff = (mAccTime - slider.value) * pixelPerSec;
        Vector2 pos = syncRef.transform.position;
        pos.x += currentPixelOff;
        bar.transform.position = pos;
    }
    private void OnRecvNewUser(ICD.stHeader _msg, string _info)
    {
        if (_msg.head.cmd != ICD.ICDDefines.CMD_NewUser)
            return;

        ICD.CMD_UserInfo msg = (ICD.CMD_UserInfo)_msg;
        mSetting.SetUserName(msg.body.username);
    }

    public void OnBtnPlay()
    {
        mIsPlaying = true;
        mAccTime = -1.5f;
    }

    public void OnBtnCompelete()
    {
        mSetting.SetMusicDelay(slider.value);
        MainCategory.PreviousView = panelMusicList;
        panelMusicList.SetActive(true);
        panelMainCategory.SetActive(true);
        Destroy(gameObject);
    }
}
