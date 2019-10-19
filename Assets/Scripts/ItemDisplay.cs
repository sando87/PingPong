using ICD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    private Image TitleImage;
    private Text TitleName;
    private Text SingerName;
    private Image GradeImage;

    public Song SongInfo { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        //InitEventTrigger();

        FindChildUI();

        Song info = SongInfo;
        TitleImage.sprite = Resources.Load<Sprite>(PP.PathInfo.Images + info.FileNameNoExt + "png");
        GradeImage.sprite = Resources.Load<Sprite>(PP.PathInfo.Images + "star");
        TitleName.text = info.Title;
        SingerName.text = info.Artist;

        float height = Screen.width * 0.2f;
        int fontSize = (int)(Screen.width * 0.0625);
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, height);
        TitleName.fontSize = fontSize;
        SingerName.fontSize = (int)(fontSize * 0.7);
    }
    void FindChildUI()
    {
        Transform[] childs = GetComponentsInChildren<Transform>();
        foreach(Transform child in childs)
        {
            string name = child.gameObject.name;
            switch(name)
            {
                case "SongImage": TitleImage = child.GetComponent<Image>(); break;
                case "Title": TitleName = child.GetComponent<Text>(); break;
                case "Singer": SingerName = child.GetComponent<Text>(); break;
                case "Grade": GradeImage = child.GetComponent<Image>(); break;
                default: break;
            }
        }
    }
    void InitEventTrigger()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener(SystemManager.Inst().OnClickItem);
        trigger.triggers.Add(entry);
    }
}
