using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        volumeSlider.value = backgroundMusic.volume;
        UpdateVolumeText();
        volumeSlider.onValueChanged.AddListener(SetVolumeFromSlider);
    }

    private void SetVolumeFromSlider(float value)
    {
        backgroundMusic.volume = value;
        UpdateVolumeText();
    }

    private void UpdateVolumeText()
    {
        volumeText.text = (backgroundMusic.volume * 100).ToString("F0");
    }
}
