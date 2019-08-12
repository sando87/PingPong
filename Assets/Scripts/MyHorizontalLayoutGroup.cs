using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyHorizontalLayoutGroup : HorizontalLayoutGroup
{
    // Start is called before the first frame update
    override protected void Start()
    {
        RectTransform rectTrans = (RectTransform)transform;
        float height = rectTrans.rect.height - padding.top - padding.bottom;
        RectTransform[] childs = GetComponentsInChildren<RectTransform>();
        for(int i = 1; i < childs.Length; ++i)
        {
            RectTransform trans = childs[i];
            trans.sizeDelta = new Vector2(height, height);
        }

        
    }
}
