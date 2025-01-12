using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Movement : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    [SerializeField] private float sensitivity = 30f;
    [SerializeField] private int mouseDPI = 800;
    [SerializeField] private float sensitivityMultiplier = 1f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text sensitivityText;
    [SerializeField] private UnityEngine.UI.Slider sensitivityMultiplierSlider;

    private float xRotation = 0f;
    private Vector2 mouseDelta;

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    private void Start()
    {
        // Load saved sensitivity settings from PlayerPrefs
        LoadSensitivitySettings();

        // Initialize UI
        sensitivityMultiplierSlider.maxValue = 5f;
        sensitivityMultiplierSlider.value = sensitivityMultiplier;
        UpdateSensitivityText();

        // Add listener for slider changes
        sensitivityMultiplierSlider.onValueChanged.AddListener(SetSensitivityMultiplierFromSlider);
    }

    private void Update()
    {
        float scaledSensitivity = sensitivity * (mouseDPI / 800f) * sensitivityMultiplier;
        xRotation -= mouseDelta.y * scaledSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up, mouseDelta.x * scaledSensitivity * Time.deltaTime);
    }

    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = Mathf.Max(0.1f, newSensitivity);
        UpdateSensitivityText();
        SaveSensitivitySettings();
    }

    private void SetSensitivityMultiplierFromSlider(float value)
    {
        sensitivityMultiplier = value;
        UpdateSensitivityText();
        SaveSensitivitySettings();
    }

    private void UpdateSensitivityText()
    {
        sensitivityText.text = $"{sensitivityMultiplier:F1}";
    }

    // Save sensitivity settings to PlayerPrefs
    private void SaveSensitivitySettings()
    {
        PlayerPrefs.SetFloat("SensitivityMultiplier", sensitivityMultiplier);
        PlayerPrefs.Save();
    }

    // Load sensitivity settings from PlayerPrefs
    private void LoadSensitivitySettings()
    {
        sensitivityMultiplier = PlayerPrefs.GetFloat("SensitivityMultiplier", 1f); // Default is 1
    }
}
