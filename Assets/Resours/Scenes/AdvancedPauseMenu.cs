using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseWithAnimations : MonoBehaviour
{
    [Header("Панель паузы")]
    public RectTransform pausePanel;
    
    [Header("Кнопки")]
    public Button continueButton;
    public Button menuButton;
    public Button exitButton;
    
    [Header("Настройки")]
    public string mainMenuSceneName = "MainMenu";
    public float animationDuration = 0.3f;
    
    [Header("Настройки анимаций")]
    public float panelStartScale = 0.3f;
    public float panelEndScale = 1f;
    public float buttonHoverScale = 1.1f;
    public float buttonClickScale = 0.9f;
    
    private bool isPaused = false;
    private bool isAnimating = false;
    
    // Для хранения оригинальных размеров
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
        
        // Настраиваем кнопки
        SaveOriginalScales();
        SetupButtons();
        AddButtonEffects();
    }
    
    void Update()
    {
        // КАЖДЫЙ КАДР включаем курсор
        ForceCursorVisible();
        
        // ESC открывает/закрывает паузу
        if (Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            if (isPaused)
                StartCoroutine(ClosePauseMenu());
            else
                StartCoroutine(OpenPauseMenu());
        }
    }
    
    void ForceCursorVisible()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    void SaveOriginalScales()
    {
        if (continueButton != null)
            continueOriginalScale = continueButton.transform.localScale;
            
        if (menuButton != null)
            menuOriginalScale = menuButton.transform.localScale;
            
        if (exitButton != null)
            exitOriginalScale = exitButton.transform.localScale;
    }
    
    void SetupButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => StartCoroutine(OnContinueClick()));
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => StartCoroutine(OnMenuClick()));
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => StartCoroutine(OnExitClick()));
        }
    }
    
    void AddButtonEffects()
    {
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
            if (!isAnimating && pausePanel.gameObject.activeSelf) 
                StartCoroutine(AnimateButtonScale(button, originalScale * buttonHoverScale, originalScale)); 
        });
        trigger.triggers.Add(enterEntry);
        
        // PointerExit (уход)
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { 
            if (!isAnimating && pausePanel.gameObject.activeSelf) 
                StartCoroutine(AnimateButtonScale(button, originalScale, originalScale)); 
        });
        trigger.triggers.Add(exitEntry);
        
        // PointerDown (нажатие)
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) => { 
            if (!isAnimating && pausePanel.gameObject.activeSelf) 
                StartCoroutine(AnimateButtonScale(button, originalScale * buttonClickScale, originalScale)); 
        });
        trigger.triggers.Add(downEntry);
        
        // PointerUp (отпускание)
        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener((data) => { 
            if (!isAnimating && pausePanel.gameObject.activeSelf) 
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
    
    IEnumerator OpenPauseMenu()
    {
        isAnimating = true;
        
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