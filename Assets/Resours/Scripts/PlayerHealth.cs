using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 5f;
    
    [Header("Здоровье")]
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Смерть")]
    public GameObject deathEffectPrefab;     // Эффект смерти (партиклы)
    public AudioClip deathSound;              // Звук смерти
    public float deathAnimationDuration = 0.5f; // Длительность анимации смерти
    
    [Header("Возрождение")]
    public Transform respawnPoint;
    public float respawnDelay = 2f;
    public GameObject respawnEffectPrefab;    // Эффект возрождения
    public AudioClip respawnSound;             // ЗВУК ВОЗРОЖДЕНИЯ
    
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
        
        if (respawnPoint == null)
        {
            GameObject respawnObj = new GameObject("RespawnPoint");
            respawnObj.transform.position = transform.position;
            respawnPoint = respawnObj.transform;
        }
    }
    
    void Update()
    {
        if (isDead) return;
        
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = Vector3.MoveTowards(transform.position, mousePos, moveSpeed * Time.deltaTime);
        }
    }
    
    // ВЫЗЫВАЕТСЯ ПРИ ПОПАДАНИИ ПУЛИ
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"Здоровье: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            StartCoroutine(Die()); // Запускаем анимацию смерти
        }
    }
    
    // ВЫЗЫВАЕТСЯ ПРИ КАСАНИИ СТЕНЫ-ВРАГА
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyWall") && !isDead)
        {
            Debug.Log("Смерть от стены!");
            StartCoroutine(Die()); // Запускаем анимацию смерти
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyWall") && !isDead)
        {
            Debug.Log("Смерть от стены!");
            StartCoroutine(Die()); // Запускаем анимацию смерти
        }
    }
    
    IEnumerator Die()
    {
        if (isDead) yield break;
        
        isDead = true;
        
        Debug.Log("Игрок умирает...");
        
        // Отключаем физику и коллайдер
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        // АНИМАЦИЯ СМЕРТИ (для пули и стены)
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < deathAnimationDuration)
        {
            // Плавно уменьшаем прозрачность
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / deathAnimationDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            // Увеличиваем размер (эффект взрыва)
            float scale = 1 + (elapsedTime / deathAnimationDuration) * 0.5f;
            transform.localScale = startScale * scale;
            
            // Вращаем
            transform.Rotate(0, 0, 720 * Time.deltaTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Полностью скрываем
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        transform.localScale = startScale;
        
        // ЭФФЕКТ СМЕРТИ (партиклы)
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // ЗВУК СМЕРТИ
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Запускаем возрождение
        StartCoroutine(Respawn());
    }
    
    IEnumerator Respawn()
    {
        Debug.Log($"Возрождение через {respawnDelay} сек...");
        
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log("Возрождение!");
        
        // Перемещаем на точку респауна
        transform.position = respawnPoint.position;
        
        // Сбрасываем вращение и размер
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
        // Восстанавливаем здоровье
        currentHealth = maxHealth;
        
        // Включаем всё обратно
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = true;
        }
        
        // ЭФФЕКТ ВОЗРОЖДЕНИЯ
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // ЗВУК ВОЗРОЖДЕНИЯ
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        // Маленькая анимация появления
        StartCoroutine(SpawnAnimation());
    }
    
    IEnumerator SpawnAnimation()
    {
        float elapsedTime = 0f;
        float spawnDuration = 0.3f;
        
        // Начинаем с невидимого
        spriteRenderer.color = new Color(1, 1, 1, 0);
        transform.localScale = Vector3.zero;
        
        while (elapsedTime < spawnDuration)
        {
            float t = elapsedTime / spawnDuration;
            
            // Плавно появляемся
            float alpha = Mathf.Lerp(0f, 1f, t);
            spriteRenderer.color = new Color(1, 1, 1, alpha);
            
            // Увеличиваемся
            transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, t);
            
            // Вращаемся при появлении
            transform.Rotate(0, 0, -360 * Time.deltaTime * 2);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Финальные настройки
        spriteRenderer.color = Color.white;
        transform.localScale = Vector3.one;
        
        isDead = false;
        
        Debug.Log("Игрок снова жив!");
    }
}