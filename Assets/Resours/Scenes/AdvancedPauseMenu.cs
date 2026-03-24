using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AdvancedPauseMenu : MonoBehaviour
{
    [Header("Панель паузы")]
    public RectTransform pausePanel;
    
    [Header("Кнопки")]
    public Button pauseButton;      // Кнопка открытия паузы (в игровом UI)
    public Button continueButton;   // Кнопка "Продолжить" в панели паузы
    public Button menuButton;       // Кнопка "В меню" в панели паузы
    public Button exitButton;       // Кнопка "Выход" в панели паузы
    
    [Header("Настройки")]
    public string mainMenuSceneName = "Menu";
    public float animationDuration = 0.3f;
    
    [Header("Настройки анимаций")]
    public float panelStartScale = 0.3f;
    public float panelEndScale = 1f;
    public float buttonHoverScale = 1.1f;
    public float buttonClickScale = 0.9f;
    
    private bool isPaused = false;
    private bool isAnimating = false;
    
    // Для хранения оригинальных размеров
    private Vector3 pauseButtonOriginalScale;
    private Vector3 continueOriginalScale;
    private Vector3 menuOriginalScale;
    private Vector3 exitOriginalScale;
    
    void Awake()
    {
        // КУРСОР ВСЕГДА ВИДИМ
        ForceCursorVisible();
    }
    
    void Start()
    {
        ForceCursorVisible();
        
        // Проверяем панель
        if (pausePanel == null)
        {
            Debug.LogError("Pause Panel не назначена!");
            return;
        }
        
        // Скрываем панель при старте
        pausePanel.gameObject.SetActive(false);
        pausePanel.localScale = Vector3.one * panelStartScale;
        
        // Добавляем CanvasGroup для плавности
        CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = pausePanel.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        
        // Настраиваем все кнопки
        SaveOriginalScales();
        SetupButtons();
        AddButtonEffects();
    }
    
    
    void ForceCursorVisible()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    void SaveOriginalScales()
    {
        if (pauseButton != null)
            pauseButtonOriginalScale = pauseButton.transform.localScale;
            
        if (continueButton != null)
            continueOriginalScale = continueButton.transform.localScale;
            
        if (menuButton != null)
            menuOriginalScale = menuButton.transform.localScale;
            
        if (exitButton != null)
            exitOriginalScale = exitButton.transform.localScale;
    }
    
    void SetupButtons()
    {
        // Кнопка открытия паузы (в игровом UI)
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(() => {
                if (!isPaused && !isAnimating)
                    StartCoroutine(OpenPauseMenu());
            });
        }
        
        // Кнопка продолжения
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => StartCoroutine(OnContinueClick()));
        }
        
        // Кнопка меню
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => StartCoroutine(OnMenuClick()));
        }
        
        // Кнопка выхода
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => StartCoroutine(OnExitClick()));
        }
    }
    
    void AddButtonEffects()
    {
        if (pauseButton != null)
            AddButtonEvents(pauseButton, pauseButtonOriginalScale);
            
        if (continueButton != null)
            AddButtonEvents(continueButton, continueOriginalScale);
            
        if (menuButton != null)
            AddButtonEvents(menuButton, menuOriginalScale);
            
        if (exitButton != null)
            AddButtonEvents(exitButton, exitOriginalScale);
    }
    
    void AddButtonEvents(Button button, Vector3 originalScale)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();
        
        trigger.triggers.Clear();
        
        // PointerEnter (наведение)
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { 
            if (!isAnimating) 
                StartCoroutine(AnimateButtonScale(button, originalScale * buttonHoverScale, originalScale)); 
        });
        trigger.triggers.Add(enterEntry);
        
        // PointerExit (уход)
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { 
            if (!isAnimating) 
                StartCoroutine(AnimateButtonScale(button, originalScale, originalScale)); 
        });
        trigger.triggers.Add(exitEntry);
        
        // PointerDown (нажатие)
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) => { 
            if (!isAnimating) 
                StartCoroutine(AnimateButtonScale(button, originalScale * buttonClickScale, originalScale)); 
        });
        trigger.triggers.Add(downEntry);
        
        // PointerUp (отпускание)
        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener((data) => { 
            if (!isAnimating) 
                StartCoroutine(AnimateButtonScale(button, originalScale, originalScale)); 
        });
        trigger.triggers.Add(upEntry);
    }
    
    IEnumerator AnimateButtonScale(Button button, Vector3 targetScale, Vector3 originalScale)
    {
        float time = 0;
        Vector3 startScale = button.transform.localScale;
        
        while (time < animationDuration / 2)
        {
            time += Time.deltaTime;
            float t = time / (animationDuration / 2);
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        button.transform.localScale = targetScale;
    }
    
    // Публичный метод для открытия паузы (можно вызвать из другой кнопки)
    public void OpenPause()
    {
        if (!isPaused && !isAnimating)
            StartCoroutine(OpenPauseMenu());
    }
    
    // Публичный метод для закрытия паузы
    public void ClosePause()
    {
        if (isPaused && !isAnimating)
            StartCoroutine(ClosePauseMenu());
    }
    
    IEnumerator OpenPauseMenu()
    {
        isAnimating = true;
        
        // Анимация кнопки открытия паузы (если она была нажата)
        if (pauseButton != null && !Input.GetKeyDown(KeyCode.Escape))
        {
            pauseButton.transform.localScale = pauseButtonOriginalScale * buttonClickScale;
            yield return new WaitForSecondsRealtime(0.05f);
            pauseButton.transform.localScale = pauseButtonOriginalScale;
        }
        
        // Показываем панель
        pausePanel.gameObject.SetActive(true);
        
        CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
        Vector3 startScale = Vector3.one * panelStartScale;
        Vector3 endScale = Vector3.one * panelEndScale;
        
        float time = 0;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            
            // Плавная кривая (ease out)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // Масштаб
            pausePanel.localScale = Vector3.Lerp(startScale, endScale, smoothT);
            
            // Прозрачность
            if (canvasGroup != null)
                canvasGroup.alpha = t;
            
            yield return null;
        }
        
        // Финальные значения
        pausePanel.localScale = endScale;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        
        // Останавливаем время
        Time.timeScale = 0f;
        
        isPaused = true;
        isAnimating = false;
    }
    
    IEnumerator ClosePauseMenu()
    {
        isAnimating = true;
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
        Vector3 startScale = pausePanel.localScale;
        Vector3 endScale = Vector3.one * panelStartScale;
        
        float time = 0;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;
            
            // Плавная кривая (ease in)
            float smoothT = t * t;
            
            // Масштаб
            pausePanel.localScale = Vector3.Lerp(startScale, endScale, smoothT);
            
            // Прозрачность
            if (canvasGroup != null)
                canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        // Скрываем панель
        pausePanel.gameObject.SetActive(false);
        
        isPaused = false;
        isAnimating = false;
    }
    
    IEnumerator OnContinueClick()
    {
        // Анимация кнопки
        if (continueButton != null)
        {
            continueButton.transform.localScale = continueOriginalScale * buttonClickScale;
            yield return new WaitForSecondsRealtime(0.05f);
            continueButton.transform.localScale = continueOriginalScale;
        }
        
        yield return StartCoroutine(ClosePauseMenu());
    }
    
    IEnumerator OnMenuClick()
    {
        // Анимация кнопки
        if (menuButton != null)
        {
            menuButton.transform.localScale = menuOriginalScale * buttonClickScale;
            yield return new WaitForSecondsRealtime(0.05f);
            menuButton.transform.localScale = menuOriginalScale;
        }
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        // Небольшая задержка для анимации
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Загружаем меню
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    IEnumerator OnExitClick()
    {
        // Анимация кнопки
        if (exitButton != null)
        {
            exitButton.transform.localScale = exitOriginalScale * buttonClickScale;
            yield return new WaitForSecondsRealtime(0.05f);
            exitButton.transform.localScale = exitOriginalScale;
        }
        
        // Возвращаем время
        Time.timeScale = 1f;
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}