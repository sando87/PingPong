using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCategory : MonoBehaviour
{
    private static GameObject PreviousView;
    public GameObject SelectView;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rectTF = transform.parent.GetComponentInParent<RectTransform>();
        HorizontalLayoutGroup hlg = transform.parent.GetComponentInParent<HorizontalLayoutGroup>();
        float width = rectTF.rect.height - hlg.padding.top - hlg.padding.bottom;
        Vector2 newSize = GetComponent<RectTransform>().sizeDelta;
        newSize.x = width;
        GetComponent<RectTransform>().sizeDelta = newSize;

        PreviousView = GameObject.Find("pnMusicList");
    }

    public void OnClickButton()
    {
        PreviousView.SetActive(false);
        SelectView.SetActive(true);
        PreviousView = SelectView;
    }
}
