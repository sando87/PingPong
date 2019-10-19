using ICD;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickEventSong : MonoBehaviour
{
    [Serializable]
    public class UnityEventClick : UnityEvent<Song> { }
    public UnityEventClick mMouseClick = null;

    public Canvas canvas;
    PointerEventData ped;
    GraphicRaycaster gr;

    private void Start()
    {
        gr = canvas.GetComponent<GraphicRaycaster>();
        ped = new PointerEventData(null);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ped.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            gr.Raycast(ped, results);
            foreach (var result in results)
            {
                ItemDisplay item = result.gameObject.GetComponent<ItemDisplay>();
                if (item != null)
                {
                    mMouseClick.Invoke(item.SongInfo);
                    break;
                }
            }
        }
    }


}
