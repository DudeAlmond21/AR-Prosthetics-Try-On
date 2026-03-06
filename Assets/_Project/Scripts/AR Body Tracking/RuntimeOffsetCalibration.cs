using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime calibration panel for tuning prosthetic attachment offset on device.
///
/// ── Features ──────────────────────────────────────────────────────────────
/// • Toggle button (⚙) shows/hides the panel — hidden by default
/// • Sliders for Position (XYZ), Rotation (XYZ), Scale
/// • Values auto-saved to PlayerPrefs — persist across app launches
/// • Reset button returns to saved Inspector defaults
/// • Green text shows current values to note down if needed
///
/// ── SETUP ─────────────────────────────────────────────────────────────────
/// 1. Create Canvas in ARTryOn scene (Screen Space - Overlay)
/// 2. Right click Canvas → Create Empty → rename to "CalibrationPanel"
/// 3. Add this script to CalibrationPanel
/// 4. Assign prostheticAttacher → drag ProstheticSystem
/// 5. Right click Canvas → UI → Button → rename to "CalibrationToggle"
///    • Set button text to "⚙"
///    • Anchor to top-right corner
///    • Assign to toggleButton slot in this script
/// 6. Press Play — tap ⚙ to open/close panel
/// </summary>
public class RuntimeOffsetCalibration : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] AboveElbowProstheticAttacher prostheticAttacher;

    [Header("Toggle button (⚙ in corner)")]
    [SerializeField] Button toggleButton;

    [Header("Slider ranges")]
    [SerializeField] float positionRange = 0.5f;
    [SerializeField] float rotationRange = 180f;
    [SerializeField] float scaleMin      = 0.1f;
    [SerializeField] float scaleMax      = 3.0f;

    // PlayerPrefs keys
    const string KEY_PX = "cal_px"; const string KEY_PY = "cal_py"; const string KEY_PZ = "cal_pz";
    const string KEY_RX = "cal_rx"; const string KEY_RY = "cal_ry"; const string KEY_RZ = "cal_rz";
    const string KEY_SC = "cal_sc";

    // Current values
    Vector3 posOffset;
    Vector3 rotOffset;
    float   scaleVal;

    // UI
    GameObject panelBg;
    Slider sliderPX, sliderPY, sliderPZ;
    Slider sliderRX, sliderRY, sliderRZ;
    Slider sliderScale;
    Text   valuesDisplay;
    bool   panelVisible = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        LoadFromPrefs();
        BuildUI();
        SetPanelVisible(false);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        ApplyToAttacher();
    }

    // ── Persistence ───────────────────────────────────────────────────────────
    void LoadFromPrefs()
    {
        // Default rotation X = -90 (the value we found works for this model)
        posOffset.x = PlayerPrefs.GetFloat(KEY_PX, 0f);
        posOffset.y = PlayerPrefs.GetFloat(KEY_PY, 0f);
        posOffset.z = PlayerPrefs.GetFloat(KEY_PZ, 0f);
        rotOffset.x = PlayerPrefs.GetFloat(KEY_RX, -90f);
        rotOffset.y = PlayerPrefs.GetFloat(KEY_RY, 0f);
        rotOffset.z = PlayerPrefs.GetFloat(KEY_RZ, 0f);
        scaleVal    = PlayerPrefs.GetFloat(KEY_SC, 1f);
    }

    void SaveToPrefs()
    {
        PlayerPrefs.SetFloat(KEY_PX, posOffset.x);
        PlayerPrefs.SetFloat(KEY_PY, posOffset.y);
        PlayerPrefs.SetFloat(KEY_PZ, posOffset.z);
        PlayerPrefs.SetFloat(KEY_RX, rotOffset.x);
        PlayerPrefs.SetFloat(KEY_RY, rotOffset.y);
        PlayerPrefs.SetFloat(KEY_RZ, rotOffset.z);
        PlayerPrefs.SetFloat(KEY_SC, scaleVal);
        PlayerPrefs.Save();
        Debug.Log("[Calibration] Values saved to device.");
    }

    // ── Apply to prosthetic ───────────────────────────────────────────────────
    void ApplyToAttacher()
    {
        if (prostheticAttacher == null) return;
        prostheticAttacher.SetOffsets(posOffset, rotOffset, scaleVal);
    }

    // ── Panel toggle ──────────────────────────────────────────────────────────
    void TogglePanel() => SetPanelVisible(!panelVisible);

    void SetPanelVisible(bool visible)
    {
        panelVisible = visible;
        if (panelBg != null) panelBg.SetActive(visible);
    }

    // ── Reset ─────────────────────────────────────────────────────────────────
    void ResetToDefault()
    {
        posOffset = Vector3.zero;
        rotOffset = new Vector3(-90f, 0f, 0f);
        scaleVal  = 1f;

        // Update sliders without triggering saves mid-reset
        sliderPX.SetValueWithoutNotify(0f);
        sliderPY.SetValueWithoutNotify(0f);
        sliderPZ.SetValueWithoutNotify(0f);
        sliderRX.SetValueWithoutNotify(-90f);
        sliderRY.SetValueWithoutNotify(0f);
        sliderRZ.SetValueWithoutNotify(0f);
        sliderScale.SetValueWithoutNotify(1f);

        ApplyToAttacher();
        SaveToPrefs();
        UpdateDisplay();
    }

    // ── Display ───────────────────────────────────────────────────────────────
    void UpdateDisplay()
    {
        if (valuesDisplay == null) return;
        valuesDisplay.text =
            $"Pos: ({posOffset.x:F2}, {posOffset.y:F2}, {posOffset.z:F2})\n" +
            $"Rot: ({rotOffset.x:F1}, {rotOffset.y:F1}, {rotOffset.z:F1})\n" +
            $"Scale: {scaleVal:F2}  [AUTO-SAVED]";
    }

    // ── UI Builder ────────────────────────────────────────────────────────────
    void BuildUI()
    {
        panelBg = new GameObject("OffsetPanel");
        panelBg.transform.SetParent(transform, false);

        var bg      = panelBg.AddComponent<Image>();
        bg.color    = new Color(0f, 0f, 0f, 0.88f);
        var bgRect  = panelBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 0.5f);
        bgRect.sizeDelta = new Vector2(320f, 0f);
        bgRect.anchoredPosition = new Vector2(0f, 0f);

        var layout = panelBg.AddComponent<VerticalLayoutGroup>();
        layout.padding                = new RectOffset(10, 10, 10, 10);
        layout.spacing                = 5f;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;
        layout.childControlHeight     = true;

        var fitter = panelBg.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        AddLabel(panelBg, "⚙  Prosthetic Calibration", 15, Color.yellow);
        AddLabel(panelBg, "Position Offset (metres)", 12, Color.gray);
        sliderPX = AddSlider(panelBg, "PX", -positionRange, positionRange, posOffset.x,
            v => { posOffset.x = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });
        sliderPY = AddSlider(panelBg, "PY", -positionRange, positionRange, posOffset.y,
            v => { posOffset.y = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });
        sliderPZ = AddSlider(panelBg, "PZ", -positionRange, positionRange, posOffset.z,
            v => { posOffset.z = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });

        AddLabel(panelBg, "Rotation Offset (degrees)", 12, Color.gray);
        sliderRX = AddSlider(panelBg, "RX", -rotationRange, rotationRange, rotOffset.x,
            v => { rotOffset.x = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });
        sliderRY = AddSlider(panelBg, "RY", -rotationRange, rotationRange, rotOffset.y,
            v => { rotOffset.y = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });
        sliderRZ = AddSlider(panelBg, "RZ", -rotationRange, rotationRange, rotOffset.z,
            v => { rotOffset.z = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });

        AddLabel(panelBg, "Scale", 12, Color.gray);
        sliderScale = AddSlider(panelBg, "SC", scaleMin, scaleMax, scaleVal,
            v => { scaleVal = v; ApplyToAttacher(); SaveToPrefs(); UpdateDisplay(); });

        // Values readout
        var dgo = new GameObject("Display");
        dgo.transform.SetParent(panelBg.transform, false);
        valuesDisplay           = dgo.AddComponent<Text>();
        valuesDisplay.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        valuesDisplay.fontSize  = 11;
        valuesDisplay.color     = Color.green;
        var dr = dgo.GetComponent<RectTransform>();
        dr.sizeDelta = new Vector2(0f, 52f);

        // Buttons row
        var btnRow = new GameObject("BtnRow");
        btnRow.transform.SetParent(panelBg.transform, false);
        var btnLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
        btnLayout.spacing = 8f;
        btnLayout.childForceExpandWidth  = true;
        btnLayout.childForceExpandHeight = true;
        var btnRect = btnRow.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(0f, 40f);

        AddButton(btnRow, "Reset", ResetToDefault, new Color(0.7f, 0.2f, 0.2f));
        AddButton(btnRow, "Close", () => SetPanelVisible(false), new Color(0.2f, 0.2f, 0.7f));

        UpdateDisplay();
    }

    // ── UI Helpers ────────────────────────────────────────────────────────────
    void AddLabel(GameObject parent, string text, int size, Color color)
    {
        var go  = new GameObject("Lbl");
        go.transform.SetParent(parent.transform, false);
        var t   = go.AddComponent<Text>();
        t.text  = text; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleLeft;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size + 6f);
    }

    Slider AddSlider(GameObject parent, string lbl, float min, float max,
                     float def, UnityEngine.Events.UnityAction<float> onChange)
    {
        var row = new GameObject("Row_" + lbl);
        row.transform.SetParent(parent.transform, false);
        var hl  = row.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 5f; hl.childForceExpandHeight = true; hl.childControlWidth = false;
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

        // Label
        var lgo = new GameObject("L"); lgo.transform.SetParent(row.transform, false);
        var lt  = lgo.AddComponent<Text>();
        lt.text = lbl; lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        lt.fontSize = 12; lt.color = Color.white; lt.alignment = TextAnchor.MiddleLeft;
        lgo.GetComponent<RectTransform>().sizeDelta = new Vector2(28f, 0f);

        // Slider object
        var sgo    = new GameObject("S"); sgo.transform.SetParent(row.transform, false);
        var slider = sgo.AddComponent<Slider>();
        sgo.GetComponent<RectTransform>().sizeDelta = new Vector2(195f, 0f);

        var bggo = new GameObject("Bg"); bggo.transform.SetParent(sgo.transform, false);
        var bgI  = bggo.AddComponent<Image>(); bgI.color = new Color(0.25f, 0.25f, 0.25f);
        var bgR  = bggo.GetComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero; bgR.anchorMax = Vector2.one; bgR.sizeDelta = Vector2.zero;

        var fa  = new GameObject("FA"); fa.transform.SetParent(sgo.transform, false);
        var faR = fa.AddComponent<RectTransform>();
        faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one;
        faR.sizeDelta = new Vector2(-20f, 0f); faR.anchoredPosition = new Vector2(-5f, 0f);

        var fi  = new GameObject("Fi"); fi.transform.SetParent(fa.transform, false);
        var fiI = fi.AddComponent<Image>(); fiI.color = new Color(0.15f, 0.55f, 1f);
        var fiR = fi.GetComponent<RectTransform>();
        fiR.anchorMin = Vector2.zero; fiR.anchorMax = Vector2.one; fiR.sizeDelta = Vector2.zero;

        var ha  = new GameObject("HA"); ha.transform.SetParent(sgo.transform, false);
        var haR = ha.AddComponent<RectTransform>();
        haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one;
        haR.sizeDelta = new Vector2(-20f, 0f);

        var h   = new GameObject("H"); h.transform.SetParent(ha.transform, false);
        var hI  = h.AddComponent<Image>(); hI.color = Color.white;
        h.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 0f);

        slider.fillRect = fiR; slider.handleRect = h.GetComponent<RectTransform>();
        slider.targetGraphic = hI;
        slider.minValue = min; slider.maxValue = max; slider.value = def;
        slider.onValueChanged.AddListener(onChange);

        // Value readout
        var vgo = new GameObject("V"); vgo.transform.SetParent(row.transform, false);
        var vt  = vgo.AddComponent<Text>();
        vt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        vt.fontSize = 10; vt.color = Color.cyan; vt.alignment = TextAnchor.MiddleLeft;
        vgo.GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 0f);
        slider.onValueChanged.AddListener(v => vt.text = v.ToString("F2"));
        vt.text = def.ToString("F2");

        return slider;
    }

    void AddButton(GameObject parent, string lbl,
                   UnityEngine.Events.UnityAction onClick, Color color)
    {
        var go  = new GameObject("Btn_" + lbl); go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = color;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var tgo = new GameObject("T"); tgo.transform.SetParent(go.transform, false);
        var t   = tgo.AddComponent<Text>();
        t.text  = lbl; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 13; t.color = Color.white; t.alignment = TextAnchor.MiddleCenter;
        var tr  = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.sizeDelta = Vector2.zero;
    }
}
