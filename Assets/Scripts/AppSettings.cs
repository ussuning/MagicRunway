using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppSettings : MonoBehaviour {

    public Slider DownwardLightSlider;
    public Text DownwardLightSliderValText;
    public Slider LeftLightSlider;
    public Text LeftLightSliderValText;
    public Slider RightLightSlider;
    public Text RightLightSliderValText;

    public Light DownwardLight;
    public Light LeftLight;
    public Light RightLight;

    public float maxLightIntensity = 6f;

    private float downwardLightIntensity = 0.8f;
    private float leftLightIntensity = 0.4f;
    private float rightLightIntensity = 0.4f;

    private float defaultDownwardLightIntensity = 0.8f;
    private float defaultLeftLightIntensity = 0.4f;
    private float defaultRightLightIntensity = 0.4f;

    private void Awake()
    {
        downwardLightIntensity = PlayerPrefs.GetFloat("DownLight", defaultDownwardLightIntensity);
        leftLightIntensity = PlayerPrefs.GetFloat("LeftLight", defaultLeftLightIntensity);
        rightLightIntensity = PlayerPrefs.GetFloat("RightLight", defaultRightLightIntensity);

        DownwardLightSlider.maxValue = LeftLightSlider.maxValue = RightLightSlider.maxValue = maxLightIntensity;

        DownwardLight.intensity = DownwardLightSlider.value = downwardLightIntensity;
        LeftLight.intensity = LeftLightSlider.value = leftLightIntensity;
        RightLight.intensity = RightLightSlider.value = rightLightIntensity;

        DownwardLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(downwardLightIntensity / maxLightIntensity * 100f));
        LeftLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(leftLightIntensity / maxLightIntensity * 100f));
        RightLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(rightLightIntensity / maxLightIntensity * 100f));
    }

    public void OnDownwardLightSliderValueChanged()
    {
        DownwardLight.intensity = downwardLightIntensity = DownwardLightSlider.value;
        DownwardLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(downwardLightIntensity/maxLightIntensity * 100f));
        PlayerPrefs.SetFloat("DownLight", downwardLightIntensity);
    }

    public void OnLeftLightSliderValueChanged()
    {
        LeftLight.intensity = leftLightIntensity = LeftLightSlider.value;
        LeftLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(leftLightIntensity/maxLightIntensity * 100f));
        PlayerPrefs.SetFloat("LeftLight", leftLightIntensity);
    }

    public void OnLRightLightSliderValueChanged()
    {
        RightLight.intensity = rightLightIntensity = RightLightSlider.value;
        RightLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(rightLightIntensity/maxLightIntensity * 100f));
        PlayerPrefs.SetFloat("RightLight", rightLightIntensity);
    }

    public void OnRestoreSettings()
    {
        DownwardLight.intensity = DownwardLightSlider.value = downwardLightIntensity = defaultDownwardLightIntensity;
        LeftLight.intensity = LeftLightSlider.value = leftLightIntensity = defaultLeftLightIntensity;
        RightLight.intensity = RightLightSlider.value = rightLightIntensity = defaultRightLightIntensity;

        DownwardLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(downwardLightIntensity/maxLightIntensity * 100f));
        LeftLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(leftLightIntensity/maxLightIntensity * 100f));
        RightLightSliderValText.text = string.Format("{0}%", Mathf.RoundToInt(rightLightIntensity/maxLightIntensity * 100f));

        PlayerPrefs.SetFloat("DownLight", downwardLightIntensity);
        PlayerPrefs.SetFloat("LeftLight", leftLightIntensity);
        PlayerPrefs.SetFloat("RightLight", rightLightIntensity);
    }
}
