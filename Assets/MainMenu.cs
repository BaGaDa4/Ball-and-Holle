using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;

public class CompleteMenu : MonoBehaviour
{
    [Header("Кнопки")]
    public Button playButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;
    
    [Header("Панели")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    
    [Header("Настройки анимаций")]
    public float animationSpeed = 0.2f;
    public float hoverScale = 1.1f;
    public float clickScale = 0.9f;
    
    [Header("Настройки экрана")]
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Dropdown screenModeDropdown;
    
    [Header("Настройки звука")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle soundToggle;
    public Text volumeValueText;
    public Text masterVolumeText;
    public Text musicVolumeText;
    public Text sfxVolumeText;
    
    [Header("Настройки пост-процессинга")]
    public Volume globalVolume;
    public Toggle postProcessingToggle;
    public Toggle bloomToggle;
    public Toggle motionBlurToggle;
    public Text postProcessingStatusText;
    
    [Header("Доступные разрешения")]
    public Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1366, 768),
        new Vector2Int(1280, 720),
        new Vector2Int(1024, 768)
    };
    
    // Параметры Audio Mixer
    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";
    
    // Компоненты пост-процессинга
    private Bloom bloom;
    private MotionBlur motionBlur;
    
    private Vector3 playOriginalScale;
    private Vector3 settingsOriginalScale;
    private Vector3 exitOriginalScale;
    private Vector3 backOriginalScale;
    
    private bool settingsOpen = false;
    private bool isAnimating = false;
    private bool canPressEscape = true;
    
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool isSoundOn = true;
    
    void Start()
    {
        // Курсор всегда видим
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Сохраняем оригинальные размеры кнопок
        SaveOriginalScales();
        
        // Настраиваем кнопки
        SetupButtons();
        
        // Настраиваем выпадающие списки
        SetupResolutionDropdown();
        SetupScreenModeDropdown();
        
        // Настраиваем звук
        SetupAudioSettings();
        
        // Настраиваем пост-процессинг
        SetupPostProcessing();
        
        // Загружаем сохраненные настройки
        LoadSettings();
        
        // Применяем настройки
        ApplyAudioSettings();
        ApplyPostProcessingSettings();
        
        // Начальное состояние
        if (mainPanel != null)
            mainPanel.SetActive(true);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    void SetupPostProcessing()
    {
        // Находим Volume если не назначен
        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();
            
        if (globalVolume != null && globalVolume.profile != null)
        {
            // Получаем эффекты из профиля
            globalVolume.profile.TryGet(out bloom);
            globalVolume.profile.TryGet(out motionBlur);
            
            Debug.Log($"Post Processing: Bloom={(bloom != null)}, MotionBlur={(motionBlur != null)}");
        }
        else
        {
            Debug.LogWarning("Global Volume или его профиль не найдены!");
        }
        
        // Настраиваем UI
        if (postProcessingToggle != null)
            postProcessingToggle.onValueChanged.AddListener(SetPostProcessing);
            
        if (bloomToggle != null)
            bloomToggle.onValueChanged.AddListener(SetBloom);
            
        if (motionBlurToggle != null)
            motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
    }
    
    void ApplyPostProcessingSettings()
    {
        if (globalVolume != null)
        {
            bool ppEnabled = PlayerPrefs.GetInt("PostProcessingEnabled", 1) == 1;
            bool bloomEnabled = PlayerPrefs.GetInt("BloomEnabled", 1) == 1;
            bool motionBlurEnabled = PlayerPrefs.GetInt("MotionBlurEnabled", 1) == 1;
            
            // Применяем к UI
            if (postProcessingToggle != null) postProcessingToggle.isOn = ppEnabled;
            if (bloomToggle != null) bloomToggle.isOn = bloomEnabled;
            if (motionBlurToggle != null) motionBlurToggle.isOn = motionBlurEnabled;
            
            // Включаем/выключаем Volume
            globalVolume.enabled = ppEnabled;
            
            // Включаем/выключаем эффекты
            if (bloom != null) bloom.active = bloomEnabled && ppEnabled;
            if (motionBlur != null) motionBlur.active = motionBlurEnabled && ppEnabled;
            
            UpdatePostProcessingStatus();
        }
    }
    
    public void SetPostProcessing(bool enabled)
    {
        if (globalVolume != null)
        {
            globalVolume.enabled = enabled;
            PlayerPrefs.SetInt("PostProcessingEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            // Обновляем статус эффектов
            if (bloom != null && bloomToggle != null)
                bloom.active = enabled && bloomToggle.isOn;
                
            if (motionBlur != null && motionBlurToggle != null)
                motionBlur.active = enabled && motionBlurToggle.isOn;
                
            UpdatePostProcessingStatus();
            Debug.Log($"Пост-процессинг {(enabled ? "включен" : "выключен")}");
        }
    }
    
    public void SetBloom(bool enabled)
    {
        if (bloom != null)
        {
            // Включаем только если включен общий пост-процессинг
            bool finalEnabled = enabled && (globalVolume != null && globalVolume.enabled);
            bloom.active = finalEnabled;
            
            PlayerPrefs.SetInt("BloomEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"Bloom {(enabled ? "включен" : "выключен")}" + 
                     (finalEnabled != enabled ? " (но пост-процессинг выключен)" : ""));
        }
    }
    
    public void SetMotionBlur(bool enabled)
    {
        if (motionBlur != null)
        {
            // Включаем только если включен общий пост-процессинг
            bool finalEnabled = enabled && (globalVolume != null && globalVolume.enabled);
            motionBlur.active = finalEnabled;
            
            PlayerPrefs.SetInt("MotionBlurEnabled", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            Debug.Log($"Motion Blur {(enabled ? "включен" : "выключен")}" + 
                     (finalEnabled != enabled ? " (но пост-процессинг выключен)" : ""));
        }
    }
    
    void UpdatePostProcessingStatus()
    {
        if (postProcessingStatusText != null)
        {
            string status = globalVolume != null && globalVolume.enabled ? "Вкл" : "Выкл";
            postProcessingStatusText.text = $"Пост-процессинг: {status}";
        }
    }
    
    void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].x + " x " + resolutions[i].y;
            options.Add(option);
            
            if (Screen.width == resolutions[i].x && Screen.height == resolutions[i].y)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
    }
    
    void SetupScreenModeDropdown()
    {
        if (screenModeDropdown == null) return;
        
        screenModeDropdown.ClearOptions();
        
        List<string> options = new List<string>
        {
            "Оконный режим",
            "Полноэкранный режим",
            "Полноэкранный (оконный)"
        };
        
        screenModeDropdown.AddOptions(options);
        screenModeDropdown.value = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? 1 : 
                                   Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? 2 : 0;
        screenModeDropdown.RefreshShownValue();
        
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }
    
    void SetupAudioSettings()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0.0001f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0.0001f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0.0001f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }
    }
    
    void LoadSettings()
    {
        // Загрузка настроек громкости
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        
        // Применяем настройки к UI
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (soundToggle != null) soundToggle.isOn = isSoundOn;
        
        UpdateVolumeTexts();
    }
    
    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    void ApplyAudioSettings()
    {
        if (audioMixer == null)
        {
            float finalVolume = isSoundOn ? masterVolume : 0f;
            AudioListener.volume = finalVolume;
        }
        else
        {
            SetMixerVolume(MASTER_VOLUME_PARAM, masterVolume);
            SetMixerVolume(MUSIC_VOLUME_PARAM, musicVolume);
            SetMixerVolume(SFX_VOLUME_PARAM, sfxVolume);
            
            if (!isSoundOn)
            {
                SetMixerVolume(MASTER_VOLUME_PARAM, 0.0001f);
            }
        }
        
        UpdateVolumeTexts();
    }
    
    void SetMixerVolume(string param, float value)
    {
        if (audioMixer == null) return;
        
        float dbValue;
        
        if (value <= 0.0001f)
        {
            dbValue = -80f;
        }
        else
        {
            dbValue = Mathf.Log10(value) * 20;
            dbValue = Mathf.Clamp(dbValue, -80f, 0f);
        }
        
        audioMixer.SetFloat(param, dbValue);
    }
    
    void UpdateVolumeTexts()
    {
        float displayMaster = isSoundOn ? masterVolume * 100f : 0f;
        float displayMusic = isSoundOn ? musicVolume * 100f : 0f;
        float displaySFX = isSoundOn ? sfxVolume * 100f : 0f;
        
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(displayMaster) + "%";
            
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(displayMaster) + "%";
            
        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(displayMusic) + "%";
            
        if (sfxVolumeText != null)
            sfxVolumeText.text = Mathf.RoundToInt(displaySFX) + "%";
    }
    
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        ApplyAudioSettings();
        SaveSettings();
    }
    
    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        ApplyAudioSettings();
        SaveSettings();
    }
    
    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        ApplyAudioSettings();
        SaveSettings();
    }
    
    public void ToggleSound(bool isOn)
    {
        isSoundOn = isOn;
        ApplyAudioSettings();
        SaveSettings();
        Debug.Log($"Звук {(isOn ? "включен" : "выключен")}");
    }
    
    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        
        Vector2Int res = resolutions[index];
        Screen.SetResolution(res.x, res.y, Screen.fullScreen);
        Debug.Log($"Разрешение: {res.x} x {res.y}");
    }
    
    public void SetScreenMode(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    // Остальные методы (анимации, кнопки и т.д.)
    void SaveOriginalScales()
    {
        if (playButton != null) playOriginalScale = playButton.transform.localScale;
        if (settingsButton != null) settingsOriginalScale = settingsButton.transform.localScale;
        if (exitButton != null) exitOriginalScale = exitButton.transform.localScale;
        if (backButton != null) backOriginalScale = backButton.transform.localScale;
    }
    
    void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
            AddButtonEvents(playButton, "Play");
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OpenSettings);
            AddButtonEvents(settingsButton, "Settings");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitGame);
            AddButtonEvents(exitButton, "Exit");
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseSettings);
            AddButtonEvents(backButton, "Back");
        }
    }
    
    void AddButtonEvents(Button button, string buttonName)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        trigger.triggers.Clear();
        
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnButtonHover(buttonName, true); });
        trigger.triggers.Add(enterEntry);
        
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnButtonHover(buttonName, false); });
        trigger.triggers.Add(exitEntry);
        
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) => { OnButtonDown(buttonName); });
        trigger.triggers.Add(downEntry);
        
        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener((data) => { OnButtonUp(buttonName); });
        trigger.triggers.Add(upEntry);
    }
    
    void OnButtonHover(string buttonName, bool isHovering)
    {
        if (isAnimating) return;
        
        Button button = GetButtonByName(buttonName);
        Vector3 originalScale = GetOriginalScale(buttonName);
        
        if (button != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButtonScale(button, isHovering ? originalScale * hoverScale : originalScale));
        }
    }
    
    void OnButtonDown(string buttonName)
    {
        if (isAnimating) return;
        
        Button button = GetButtonByName(buttonName);
        Vector3 originalScale = GetOriginalScale(buttonName);
        
        if (button != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButtonScale(button, originalScale * clickScale));
        }
    }
    
    void OnButtonUp(string buttonName)
    {
        if (isAnimating) return;
        
        Button button = GetButtonByName(buttonName);
        Vector3 originalScale = GetOriginalScale(buttonName);
        
        if (button != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateButtonScale(button, originalScale));
        }
    }
    
    IEnumerator AnimateButtonScale(Button button, Vector3 targetScale)
    {
        float time = 0;
        Vector3 startScale = button.transform.localScale;
        
        while (time < animationSpeed / 2)
        {
            time += Time.deltaTime;
            float t = time / (animationSpeed / 2);
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        button.transform.localScale = targetScale;
    }
    
    Button GetButtonByName(string buttonName)
    {
        switch (buttonName)
        {
            case "Play": return playButton;
            case "Settings": return settingsButton;
            case "Exit": return exitButton;
            case "Back": return backButton;
            default: return null;
        }
    }
    
    Vector3 GetOriginalScale(string buttonName)
    {
        switch (buttonName)
        {
            case "Play": return playOriginalScale;
            case "Settings": return settingsOriginalScale;
            case "Exit": return exitOriginalScale;
            case "Back": return backOriginalScale;
            default: return Vector3.one;
        }
    }
    
    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (Input.GetKeyDown(KeyCode.Escape) && settingsOpen && !isAnimating && canPressEscape)
        {
            canPressEscape = false;
            CloseSettings();
            StartCoroutine(ResetEscapeFlag());
        }
    }
    
    IEnumerator ResetEscapeFlag()
    {
        yield return new WaitForSeconds(0.5f);
        canPressEscape = true;
    }
    
    void OpenSettings()
    {
        if (isAnimating || settingsOpen) return;
        StartCoroutine(OpenSettingsAnimation());
    }
    
    void CloseSettings()
    {
        if (isAnimating || !settingsOpen) return;
        StartCoroutine(CloseSettingsAnimation());
    }
    
    IEnumerator OpenSettingsAnimation()
    {
        isAnimating = true;
        
        if (settingsButton != null)
        {
            settingsButton.transform.localScale = settingsOriginalScale * clickScale;
            yield return new WaitForSeconds(0.05f);
            settingsButton.transform.localScale = settingsOriginalScale;
        }
        
        if (mainPanel != null)
        {
            CanvasGroup mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
                mainCanvasGroup = mainPanel.AddComponent<CanvasGroup>();
            
            float time = 0;
            while (time < animationSpeed)
            {
                time += Time.deltaTime;
                mainCanvasGroup.alpha = 1 - (time / animationSpeed);
                yield return null;
            }
            
            mainCanvasGroup.alpha = 0;
            mainPanel.SetActive(false);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            CanvasGroup settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsCanvasGroup == null)
                settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            
            settingsCanvasGroup.alpha = 0;
            settingsPanel.transform.localScale = Vector3.one * 0.8f;
            
            float time = 0;
            while (time < animationSpeed)
            {
                time += Time.deltaTime;
                float t = time / animationSpeed;
                settingsCanvasGroup.alpha = t;
                
                float scaleT = Mathf.Sin(t * Mathf.PI * 0.5f);
                settingsPanel.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, scaleT);
                
                yield return null;
            }
            
            settingsCanvasGroup.alpha = 1;
            settingsPanel.transform.localScale = Vector3.one;
        }
        
        settingsOpen = true;
        isAnimating = false;
    }
    
    IEnumerator CloseSettingsAnimation()
    {
        isAnimating = true;
        
        if (backButton != null && !canPressEscape)
        {
            backButton.transform.localScale = backOriginalScale * clickScale;
            yield return new WaitForSeconds(0.05f);
            backButton.transform.localScale = backOriginalScale;
        }
        
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CanvasGroup settingsCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsCanvasGroup == null)
                settingsCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            
            float time = 0;
            while (time < animationSpeed)
            {
                time += Time.deltaTime;
                settingsCanvasGroup.alpha = 1 - (time / animationSpeed);
                settingsPanel.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, time / animationSpeed);
                yield return null;
            }
            
            settingsCanvasGroup.alpha = 0;
            settingsPanel.SetActive(false);
        }
        
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            
            CanvasGroup mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
                mainCanvasGroup = mainPanel.AddComponent<CanvasGroup>();
            
            mainCanvasGroup.alpha = 0;
            
            float time = 0;
            while (time < animationSpeed)
            {
                time += Time.deltaTime;
                mainCanvasGroup.alpha = time / animationSpeed;
                yield return null;
            }
            
            mainCanvasGroup.alpha = 1;
        }
        
        settingsOpen = false;
        isAnimating = false;
    }
    
    void PlayGame()
    {
        if (isAnimating) return;
        StartCoroutine(PlayGameAnimation());
    }
    
    IEnumerator PlayGameAnimation()
    {
        isAnimating = true;
        
        if (playButton != null)
        {
            playButton.transform.localScale = playOriginalScale * clickScale;
            yield return new WaitForSeconds(0.05f);
            playButton.transform.localScale = playOriginalScale;
        }
        
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene("Level1");
    }
    
    void ExitGame()
    {
        if (isAnimating) return;
        StartCoroutine(ExitGameAnimation());
    }
    
    IEnumerator ExitGameAnimation()
    {
        isAnimating = true;
        
        if (exitButton != null)
        {
            exitButton.transform.localScale = exitOriginalScale * clickScale;
            yield return new WaitForSeconds(0.05f);
            exitButton.transform.localScale = exitOriginalScale;
        }
        
        yield return new WaitForSeconds(0.2f);
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}