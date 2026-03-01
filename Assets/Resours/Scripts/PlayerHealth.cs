using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 5f;
    
    [Header("Здоровье")]
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Исчезновение и анимация")]
    public float disappearDuration = 0.5f;     // Как долго исчезать
    public GameObject deathEffectPrefab;        // Эффект смерти (партиклы)
    public AudioClip deathSound;                 // Звук смерти
    
    [Header("Возрождение")]
    public Transform respawnPoint;
    public float respawnDelay = 2f;
    public GameObject respawnEffectPrefab;       // Эффект возрождения
    public AudioClip respawnSound;                // ЗВУК РЕСПАУНА
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private AudioSource audioSource;
    private bool isDead = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        currentHealth = maxHealth;
        
        // Точка возрождения
        if (respawnPoint == null)
        {
            GameObject respawnObj = new GameObject("RespawnPoint");
            respawnObj.transform.position = transform.position;
            respawnPoint = respawnObj.transform;
        }
        
        Debug.Log("Player готов. Здоровье: " + currentHealth);
    }
    
    void Update()
    {
        if (isDead) return;
        
        // Движение за мышкой
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, mousePos, moveSpeed * Time.deltaTime);
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"Получен урон! Здоровье: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log("Игрок умер!");
            StartCoroutine(Die());
        }
    }
    
    IEnumerator Die()
    {
        isDead = true;
        
        Debug.Log("Начинаем процесс смерти...");
        
        // Отключаем управление и физику
        if (rb != null) 
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        if (playerCollider != null) 
        {
            playerCollider.enabled = false;
        }
        
        // Запоминаем начальную позицию и цвет
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        // АНИМАЦИЯ ИСЧЕЗНОВЕНИЯ (умирает ВЕСЬ игрок)
        float elapsedTime = 0f;
        
        while (elapsedTime < disappearDuration)
        {
            // Плавно уменьшаем прозрачность
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / disappearDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            // НЕМНОГО УВЕЛИЧИВАЕМ (эффект взрыва)
            float scale = 1 + (elapsedTime / disappearDuration) * 0.3f;
            transform.localScale = startScale * scale;
            
            // Вращаем весь объект
            transform.Rotate(0, 0, 720 * Time.deltaTime);
            
            // Немного поднимаем вверх
            transform.position = startPos + Vector3.up * (elapsedTime * 0.5f);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Полностью прозрачный и скрытый
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        transform.localScale = startScale;
        
        // ЭФФЕКТ СМЕРТИ (партиклы)
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            Debug.Log("Эффект смерти создан");
        }
        
        // ЗВУК СМЕРТИ
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log("Звук смерти проигран");
        }
        
        Debug.Log($"Возрождение через {respawnDelay} сек...");
        
        // Ждем перед возрождением
        yield return new WaitForSeconds(respawnDelay);
        
        // ВОЗРОЖДЕНИЕ
        StartCoroutine(Respawn());
    }
    
    IEnumerator Respawn()
    {
        Debug.Log("Начинаем возрождение!");
        
        // Перемещаем на точку возрождения
        transform.position = respawnPoint.position;
        
        // Сбрасываем вращение
        transform.rotation = Quaternion.identity;
        
        // Восстанавливаем размер
        transform.localScale = Vector3.one;
        
        // Восстанавливаем здоровье
        currentHealth = maxHealth;
        
        // Включаем коллайдер
        if (playerCollider != null) 
        {
            playerCollider.enabled = true;
        }
        
        // Включаем физику
        if (rb != null)
        {
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }
        
        // ЭФФЕКТ ВОЗРОЖДЕНИЯ
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            Debug.Log("Эффект возрождения создан");
        }
        
        // ЗВУК РЕСПАУНА
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
            Debug.Log("Звук респауна проигран");
        }
        
        // Анимация появления
        yield return StartCoroutine(SpawnAnimation());
    }
    
    IEnumerator SpawnAnimation()
    {
        Debug.Log("Анимация появления");
        
        // Начинаем с невидимого
        spriteRenderer.color = new Color(1, 1, 1, 0);
        
        // Быстрое появление с эффектом
        float elapsedTime = 0f;
        float spawnDuration = 0.3f;
        
        // Начальный размер (маленький)
        transform.localScale = Vector3.zero;
        
        while (elapsedTime < spawnDuration)
        {
            // Появляемся
            float t = elapsedTime / spawnDuration;
            
            // Плавно увеличиваем прозрачность
            float alpha = Mathf.Lerp(0f, 1f, t);
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            
            // Плавно увеличиваем размер
            transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, t);
            
            // Вращаемся при появлении
            transform.Rotate(0, 0, -360 * Time.deltaTime * 2);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Финальные настройки
        spriteRenderer.color = Color.white;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        
        isDead = false;
        
        Debug.Log("Игрок готов к бою!");
    }
}