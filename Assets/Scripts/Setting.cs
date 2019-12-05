using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    private const string ConfigFilename = "config.ini";
    [Serializable]
    public class SettingInfo
    {
        public float TimePerBar = 0;
        public float JumpHeightHalf = 0;
        public float JumpHeight = 10f;
        public float SpeedMoveX = 10f;
        public float SpeedRotate = 210f;
        public float RatePassFail = 0.125f;
        public float RateAccuracy = 0.0625f;
        public float MusicDelay = 0;
        public int PlayMode = 0;
        public string username = "";
        public bool Initialized = false;
    }

    public Text TextSliderValue;
    public Slider SliderDelay;
    public Button BtnNormalMode;
    public Button BtnRandomMode;

    private static Setting mInst = null;
    public static Setting Inst() { return mInst; }

    private SettingInfo mInfo = new SettingInfo();

    public float TimePerBar { get { return mInfo.TimePerBar; } set { mInfo.TimePerBar = value; } }
    public float JumpHeightHalf { get { return mInfo.JumpHeightHalf; } set { mInfo.JumpHeightHalf = value; } }
    public float JumpHeight { get { return mInfo.JumpHeight; } }
    public float SpeedMoveX { get { return mInfo.SpeedMoveX; } }
    public float SpeedRotate { get { return mInfo.SpeedRotate; } }
    public float RatePassFail { get { return mInfo.RatePassFail; } }
    public float RateAccuracy { get { return mInfo.RateAccuracy; } }
    public float MusicDelay { get { return mInfo.MusicDelay; } }
    public int PlayMode { get { return mInfo.PlayMode; } }
    public string UserName { get { return mInfo.username; } }
    public SettingInfo Info { get { return mInfo; } }

    private void Awake()
    {
        mInst = this;
        string fullname = Application.persistentDataPath + "/" + ConfigFilename;
        if (File.Exists(fullname))
        {
            byte[] buf = File.ReadAllBytes(fullname);
            mInfo = Utils.Deserialize_CS<SettingInfo>(buf);
            ValueToUI();
        }
        else
        {
            ValueToFile();
        }
    }
    private void OnDisable()
    {
        string fullname = Application.persistentDataPath + "/" + ConfigFilename;
        if (File.Exists(fullname))
            ValueToFile();
    }


    public bool IsFirst()
    {
        return !mInfo.Initialized;
    }
    public void SetInitalized()
    {
        mInfo.Initialized = true;
        ValueToFile();
    }
    public void SetMusicDelay(float value)
    {
        mInfo.MusicDelay = value; // -0.5f ~ 0.5f
        ValueToUI();
        ValueToFile();
    }
    public void SetUserName(string name)
    {
        mInfo.username = name;
        ValueToFile();
    }


    public void OnSlideValue(float value)
    {
        mInfo.MusicDelay = value; // -0.5f ~ 0.5f
        TextSliderValue.text = value.ToString("F4");
    }
    public void OnBtnNormalMode()
    {
        mInfo.PlayMode = 0;
        ValueToUI();
    }
    public void OnBtnRandomMode()
    {
        mInfo.PlayMode = 1;
        ValueToUI();
    }


    private void ValueToUI()
    {
        TextSliderValue.text = mInfo.MusicDelay.ToString("F4");
        SliderDelay.value = mInfo.MusicDelay;

        BtnNormalMode.GetComponent<Image>().color = mInfo.PlayMode == 0 ? Color.green : Color.white;
        BtnRandomMode.GetComponent<Image>().color = mInfo.PlayMode == 1 ? Color.green : Color.white;
    }
    private void ValueToFile()
    {
        string fullname = Application.persistentDataPath + "/" + ConfigFilename;
        byte[] data = Utils.Serialize_CS(mInfo);
        File.WriteAllBytes(fullname, data);
    }
    

}
