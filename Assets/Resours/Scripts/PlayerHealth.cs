using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 5f;
    
    [Header("Здоровье")]
    public int maxHealth = 3;
    private int currentHealth;
    
    [Header("Стены")]
    public LayerMask wallLayer;              // Обычные стены (упор)
    public LayerMask enemyWallLayer;         // Стены-враги (смерть)
    
    [Header("Смерть")]
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    public float deathAnimationDuration = 0.5f;
    
    [Header("Возрождение")]
    public Transform respawnPoint;
    public float respawnDelay = 2f;
    public GameObject respawnEffectPrefab;
    public AudioClip respawnSound;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private AudioSource audioSource;
    private bool isDead = false;
    private bool isRespawning = false;
    
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
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.position = transform.position;
            respawnPoint = spawnObj.transform;
        }
    }
    
    void Update()
    {
        if (isDead || isRespawning) return;
        
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            
            // ДВИЖЕНИЕ С ПРОВЕРКОЙ СТЕН
            MoveWithWallCheck(mousePos);
        }
    }
    
    // Движение с проверкой стен
    void MoveWithWallCheck(Vector3 targetPos)
    {
        Vector3 currentPos = transform.position;
        Vector3 direction = (targetPos - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, targetPos);
        
        // Сколько можем пройти в этом кадре
        float moveDistance = Mathf.Min(moveSpeed * Time.deltaTime, distance);
        
        // Проверяем, есть ли стена на пути
        RaycastHit2D hit = Physics2D.BoxCast(
            currentPos,
            playerCollider.bounds.size,
            0f,
            direction,
            moveDistance,
            wallLayer
        );
        
        if (hit.collider != null)
        {
            // Если стена на пути - упираемся
            // Можем двигаться только до стены
            float safeDistance = Mathf.Max(0, hit.distance - 0.05f);
            
            if (safeDistance > 0)
            {
                transform.position = currentPos + direction * safeDistance;
            }
            // Если safeDistance <= 0, не двигаемся вообще
        }
        else
        {
            // Нет стены - двигаемся свободно
            transform.position = Vector3.MoveTowards(currentPos, targetPos, moveDistance);
        }
        
        // Дополнительно проверяем стены-враги
        CheckForEnemyWalls();
    }
    
    // Проверка стен-врагов
    void CheckForEnemyWalls()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.3f, enemyWallLayer);
        
        foreach (Collider2D hit in hits)
        {
            Debug.Log("Стена-враг обнаружена!");
            StartCoroutine(Die());
            return;
        }
    }
    
    // Для турели
    public bool IsAlive()
    {
        return !isDead && !isRespawning;
    }
    
    // Урон от пули
    public void TakeDamage(int damage)
    {
        if (isDead || isRespawning) return;
        
        currentHealth -= damage;
        Debug.Log($"Здоровье: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }
    
    // Стена-враг (физика)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyWallLayer) != 0 && !isDead && !isRespawning)
        {
            Debug.Log("Смерть от стены!");
            StartCoroutine(Die());
        }
    }
    
    // Стена-враг (триггер)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyWallLayer) != 0 && !isDead && !isRespawning)
        {
            Debug.Log("Смерть от стены!");
            StartCoroutine(Die());
        }
    }
    
    // Если застрял в стене-враге
    void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyWallLayer) != 0 && !isDead && !isRespawning)
        {
            Debug.Log("Игрок в стене-враге!");
            StartCoroutine(Die());
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyWallLayer) != 0 && !isDead && !isRespawning)
        {
            Debug.Log("Игрок в стене-враге!");
            StartCoroutine(Die());
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
        
        // Анимация смерти
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < deathAnimationDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / deathAnimationDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            float scale = 1 + (elapsedTime / deathAnimationDuration) * 0.5f;
            transform.localScale = startScale * scale;
            
            transform.Rotate(0, 0, 720 * Time.deltaTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        transform.localScale = startScale;
        
        // Эффект смерти
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Звук смерти
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Запускаем возрождение
        StartCoroutine(Respawn());
    }
    
    IEnumerator Respawn()
    {
        isRespawning = true;
        
        Debug.Log($"Возрождение через {respawnDelay} сек...");
        
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log("Возрождение!");
        
        // Перемещаем на точку спавна
        transform.position = respawnPoint.position;
        
        // Восстанавливаем здоровье
        currentHealth = maxHealth;
        
        // Сбрасываем вращение и размер
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        
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
        
        // Эффект возрождения
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Звук возрождения
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        yield return new WaitForSeconds(0.1f);
        
        isRespawning = false;
        isDead = false;
        
        Debug.Log("Игрок снова жив!");
    }
    
    // Визуализация
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}