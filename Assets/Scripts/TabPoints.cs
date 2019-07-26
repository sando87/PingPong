using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PP;

public class TabPoints : MonoBehaviour
{
    public int index = -1;
    public TabType type = TabType.None;
    public float lifetime = 0;
    public float musicspeed = 0;
    public float startOff = 0;
    private float accTime = 0;
    public bool isStarted = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStarted)
            return;

        accTime += Time.deltaTime;
        float reftime = lifetime + startOff;
        float period = musicspeed * 1.5f;
        if (reftime - period <= accTime && accTime <= reftime)
        {
            float from = reftime - period;
            float rate = (accTime - from) / period;
            float angle = -rate * 360.0f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
