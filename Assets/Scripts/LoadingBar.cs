using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    static private LoadingBar mInst = null;
    static public LoadingBar GetInst() { return mInst; }
    private Slider mSlider;

    // Start is called before the first frame update
    void Start()
    {
        mInst = this;
        mSlider = GetComponentInChildren<Slider>();
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void SetProgress(float rate)
    {
        float value = (mSlider.maxValue - mSlider.minValue) * rate;
        mSlider.value = mSlider.minValue + value;
    }
}
