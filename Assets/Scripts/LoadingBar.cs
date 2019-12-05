using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public GameObject mSlider;
    public GameObject mRotateAnim;
    public Sprite[] mImages;
    public int mIndexImage;

    private void Start()
    {
        if(mIndexImage >= 0)
            StartCoroutine(UpdateSprite());
    }
    public static LoadingBar Show(bool rotateMode = false)
    {
        GameObject parent = GameObject.Find("RootUI");
        GameObject prefab = Resources.Load< GameObject>("Prefabs/pnLoading");
        GameObject obj = Instantiate(prefab, new Vector2(0, 0), Quaternion.identity, parent.transform);
        obj.transform.SetAsLastSibling();
        LoadingBar scrpit = obj.GetComponent<LoadingBar>();
        if (rotateMode)
        {
            scrpit.mSlider.SetActive(false);
            scrpit.mRotateAnim.SetActive(true);
            scrpit.mIndexImage = 0;
        }
        else
        {
            scrpit.mSlider.SetActive(true);
            scrpit.mRotateAnim.SetActive(false);
            scrpit.mIndexImage = -1;
        }
        return scrpit;
    }
    public void Hide()
    {
        StopCoroutine(UpdateSprite());
        Destroy(gameObject);
    }
    public void SetProgress(float rate)
    {
        Slider slide = mSlider.GetComponent<Slider>();
        float value = (slide.maxValue - slide.minValue) * rate;
        slide.value = slide.minValue + value;
    }
    IEnumerator UpdateSprite()
    {
        while(mIndexImage >= 0)
        {
            mRotateAnim.GetComponent<Image>().sprite = mImages[mIndexImage];
            mIndexImage = (mIndexImage + 1) % mImages.Length;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
