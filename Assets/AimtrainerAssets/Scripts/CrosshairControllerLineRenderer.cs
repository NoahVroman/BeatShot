using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Components")]
    [SerializeField] private Image topLine;
    [SerializeField] private Image bottomLine;
    [SerializeField] private Image leftLine;
    [SerializeField] private Image rightLine;
    [SerializeField] private Image centerDot;

    private Outline topOutlineComponent;
    private Outline bottomOutlineComponent;
    private Outline leftOutlineComponent;
    private Outline rightOutlineComponent;
    private Outline centerOutlineComponent;

    [Header("Customizable Settings")]
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float lineLength = 50f;
    [SerializeField] private float lineThickness = 5f;
    [SerializeField] private float gap = 15f;
    [SerializeField] private bool useCenterDot = true;
    [SerializeField] private float centerDotSize = 10f;
    [SerializeField] private float opacity = 1f;

    [Header("Outline Settings")]
    [SerializeField] private bool useOutline = true;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineOpacity = 1f;

    [Header("UI Controls")]
    [SerializeField] private Toggle centerDotToggle;
    [SerializeField] private Toggle outlineToggle;
    [SerializeField] private Slider lineLengthSlider;
    [SerializeField] private Slider lineThicknessSlider;
    [SerializeField] private Slider gapSlider;
    [SerializeField] private TMP_Dropdown crosshairColorDropdown;
    [SerializeField] private Slider opacitySlider;

    [SerializeField] private TMP_Text lineLengthText;
    [SerializeField] private TMP_Text lineThicknessText;
    [SerializeField] private TMP_Text gapText;
    [SerializeField] private TMP_Text opacityText;

    private void Start()
    {
        // Load settings from PlayerPrefs
        LoadSettings();

        // Set up outline components
        topOutlineComponent = topLine.GetComponent<Outline>();
        bottomOutlineComponent = bottomLine.GetComponent<Outline>();
        leftOutlineComponent = leftLine.GetComponent<Outline>();
        rightOutlineComponent = rightLine.GetComponent<Outline>();
        centerOutlineComponent = centerDot.GetComponent<Outline>();

        // Initialize UI sliders and toggles
        if (lineLengthSlider != null)
        {
            lineLengthSlider.maxValue = 100f;
            lineLengthSlider.value = lineLength;
            lineLengthText.text = lineLength.ToString("0.0");
        }
        if (lineThicknessSlider != null)
        {
            lineThicknessSlider.maxValue = 10f;
            lineThicknessSlider.value = lineThickness;
            lineThicknessText.text = lineThickness.ToString("0.0");
        }
        if (gapSlider != null)
        {
            gapSlider.maxValue = 50f;
            gapSlider.value = gap;
            gapText.text = gap.ToString("0.0");
        }
        if (opacitySlider != null)
        {
            opacitySlider.maxValue = 1f;
            opacitySlider.value = opacity;
            opacityText.text = opacity.ToString("0.0");
        }

        if (centerDotToggle != null) centerDotToggle.onValueChanged.AddListener(UpdateCenterDotVisibility);
        if (outlineToggle != null) outlineToggle.onValueChanged.AddListener(UpdateOutlineVisibility);
        if (lineLengthSlider != null) lineLengthSlider.onValueChanged.AddListener(UpdateLineLength);
        if (lineThicknessSlider != null) lineThicknessSlider.onValueChanged.AddListener(UpdateLineThickness);
        if (gapSlider != null) gapSlider.onValueChanged.AddListener(UpdateGap);
        if (crosshairColorDropdown != null) crosshairColorDropdown.onValueChanged.AddListener(UpdateCrosshairColor);
        if (opacitySlider != null) opacitySlider.onValueChanged.AddListener(UpdateOpacity);

        ApplyCrosshairSettings();
    }

    private void Update()
    {
        lineLengthText.text = lineLength.ToString("0.0");
        lineThicknessText.text = lineThickness.ToString("0.0");
        gapText.text = gap.ToString("0.0");
        opacityText.text = opacity.ToString("0.0");

        ApplyCrosshairSettings();
    }

    public void ApplyCrosshairSettings()
    {
        UpdateLine(topLine, Vector2.up * (gap + lineLength / 2), lineThickness, lineLength);
        UpdateLine(bottomLine, Vector2.down * (gap + lineLength / 2), lineThickness, lineLength);
        UpdateLine(leftLine, Vector2.left * (gap + lineLength / 2), lineLength, lineThickness);
        UpdateLine(rightLine, Vector2.right * (gap + lineLength / 2), lineLength, lineThickness);

        centerDot.gameObject.SetActive(useCenterDot);
        if (useCenterDot)
        {
            UpdateLine(centerDot, Vector2.zero, centerDotSize, centerDotSize);
        }

        UpdateColor(crosshairColor, opacity);

        if (useOutline)
        {
            SetOutlineActive(true);
            UpdateOutlineColor(outlineColor, outlineOpacity);
        }
        else
        {
            SetOutlineActive(false);
        }

        // Save settings to PlayerPrefs
        SaveSettings();
    }

    private void UpdateLine(Image line, Vector2 position, float width, float height)
    {
        if (line == null) return;

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, height);
    }

    private void UpdateColor(Color color, float alpha)
    {
        color.a = alpha;
        Image[] lines = new[] { topLine, bottomLine, leftLine, rightLine, centerDot };

        foreach (Image line in lines)
        {
            if (line != null)
                line.color = color;
        }
    }

    private void UpdateOutlineColor(Color color, float alpha)
    {
        color.a = alpha;
        Outline[] outlines = new[] { topOutlineComponent, bottomOutlineComponent, leftOutlineComponent, rightOutlineComponent, centerOutlineComponent };

        foreach (Outline outline in outlines)
        {
            if (outline != null)
                outline.effectColor = color;
        }
    }

    private void SetOutlineActive(bool active)
    {
        topOutlineComponent.enabled = active;
        bottomOutlineComponent.enabled = active;
        leftOutlineComponent.enabled = active;
        rightOutlineComponent.enabled = active;
        centerOutlineComponent.enabled = active;
    }

    private void UpdateLineLength(float value)
    {
        lineLength = value;
        lineLengthText.text = value.ToString("0.0");
        ApplyCrosshairSettings();
    }

    private void UpdateLineThickness(float value)
    {
        lineThickness = value;
        lineThicknessText.text = value.ToString("0.0");
        ApplyCrosshairSettings();
    }

    private void UpdateGap(float value)
    {
        gap = value;
        gapText.text = value.ToString("0.0");
        ApplyCrosshairSettings();
    }

    private void UpdateOpacity(float value)
    {
        opacity = value;
        opacityText.text = value.ToString("0.0");
        ApplyCrosshairSettings();
    }

    private void UpdateCenterDotVisibility(bool value)
    {
        useCenterDot = value;
        ApplyCrosshairSettings();
    }

    private void UpdateOutlineVisibility(bool value)
    {
        useOutline = value;
        ApplyCrosshairSettings();
    }

    private void UpdateCrosshairColor(int value)
    {
        Color[] presetColors = new Color[] { Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        crosshairColor = presetColors[value];
        ApplyCrosshairSettings();
    }

    // Save settings to PlayerPrefs
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("LineLength", lineLength);
        PlayerPrefs.SetFloat("LineThickness", lineThickness);
        PlayerPrefs.SetFloat("Gap", gap);
        PlayerPrefs.SetFloat("Opacity", opacity);
        PlayerPrefs.SetInt("UseCenterDot", useCenterDot ? 1 : 0);
        PlayerPrefs.SetInt("UseOutline", useOutline ? 1 : 0);
        PlayerPrefs.SetInt("CrosshairColor", crosshairColorDropdown.value);
        PlayerPrefs.Save();
    }

    // Load settings from PlayerPrefs
    private void LoadSettings()
    {
        lineLength = PlayerPrefs.GetFloat("LineLength", 50f);
        lineThickness = PlayerPrefs.GetFloat("LineThickness", 5f);
        gap = PlayerPrefs.GetFloat("Gap", 15f);
        opacity = PlayerPrefs.GetFloat("Opacity", 1f);
        useCenterDot = PlayerPrefs.GetInt("UseCenterDot", 1) == 1;
        useOutline = PlayerPrefs.GetInt("UseOutline", 1) == 1;
        int crosshairColorIndex = PlayerPrefs.GetInt("CrosshairColor", 0);

        Color[] presetColors = new Color[] { Color.white, Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };
        crosshairColor = presetColors[Mathf.Clamp(crosshairColorIndex, 0, presetColors.Length - 1)];
    }
}
