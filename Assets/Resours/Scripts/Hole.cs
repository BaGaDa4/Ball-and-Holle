using UnityEngine;
using UnityEngine.SceneManagement;

public class Hole : MonoBehaviour
{
    [Header("Настройки лунки")]
    public Transform respawnPoint;           // Точка возрождения в другой сцене
    public string targetSceneName;            // Имя сцены для перехода
    public GameObject holeEffect;             // Эффект лунки
    
    [Header("Визуал")]
    public Color holeColor = Color.blue;
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isActivated = false;         // Флаг, чтобы сработало только 1 раз
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Добавляем коллайдер-триггер если его нет
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        
        if (holeEffect != null)
        {
            Instantiate(holeEffect, transform.position, Quaternion.identity, transform);
        }
        
        // Проверяем, указана ли сцена
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"Лунка {gameObject.name}: не указано имя целевой сцены!");
        }
        else
        {
            Debug.Log($"Лунка {gameObject.name} будет вести на сцену: {targetSceneName}");
        }
    }

    void Update()
    {
        // Эффект пульсации
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Если уже активирована - игнорируем
        if (isActivated) return;
        
        // Проверяем, что коснулся именно игрок
        if (other.CompareTag("Player"))
        {
            isActivated = true; // Ставим флаг, чтобы не сработало второй раз
            
            Debug.Log($"ИГРОК КОСНУЛСЯ ЛУНКИ! Переход на сцену: {targetSceneName}");
            
            // Эффект при касании
            if (holeEffect != null)
            {
                Instantiate(holeEffect, transform.position, Quaternion.identity);
            }
            
            // Проверяем, указано ли имя сцены
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError("Имя целевой сцены не указано!");
                return;
            }
            
            // Пробуем загрузить сцену
            try
            {
                SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка загрузки сцены {targetSceneName}: {e.Message}");
                Debug.Log("Убедись, что сцена добавлена в Build Settings!");
            }
        }
    }
    
    // Визуализация
    void OnDrawGizmos()
    {
        Gizmos.color = holeColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Gizmos.color = Color.white;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, $"-> {targetSceneName}");
            #endif
        }
    }
}