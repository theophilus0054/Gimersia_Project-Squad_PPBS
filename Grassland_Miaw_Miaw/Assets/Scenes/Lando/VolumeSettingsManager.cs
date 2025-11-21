using UnityEngine;
using UnityEngine.Audio; // Required
using UnityEngine.UI;    // Required

public class VolumeSettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("UI Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    // PlayerPrefs keys for saving
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Start()
    {
        // Set sliders to saved values (or 1 if no save exists)
        // This will also trigger the 'OnValueChanged' listeners to update the mixer
        masterSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        bgmSlider.value = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        // Add listeners
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // Public functions to be called by sliders
    public void SetMasterVolume(float sliderValue)
    {
        // Use the exact string name from the Audio Mixer
        SetMixerVolume("MasterVolume", sliderValue); 
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, sliderValue);
    }

    public void SetBGMVolume(float sliderValue)
    {
        SetMixerVolume("BGMVolume", sliderValue);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        SetMixerVolume("SFXVolume", sliderValue);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sliderValue);
    }

    // Helper function to convert linear (0-1) to log (dB)
    private void SetMixerVolume(string parameterName, float sliderValue)
    {
        // Ensure sliderValue is not 0 to avoid Log(0) error
        float volumeDb = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, volumeDb);
    }
}