using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float height = Screen.width * 0.2f;
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, height);
    }
}
