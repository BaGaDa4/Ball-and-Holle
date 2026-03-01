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
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    
    [Header("Возрождение")]
    public Transform respawnPoint;        // Точка спавна
    public float respawnDelay = 2f;
    public GameObject respawnEffectPrefab;
    public AudioClip respawnSound;
    
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
        
        // Если точка спавна не назначена, создаем на текущей позиции
        if (respawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnObj.transform.position = transform.position;
            respawnPoint = spawnObj.transform;
        }
    }
    
    void Update()
    {
        // ЕСЛИ МЕРТВ - НИЧЕГО НЕ ДЕЛАЕМ, ВООБЩЕ НИЧЕГО
        if (isDead) return;
        
        // Движение только если жив
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            
            // Просто двигаемся к курсору
            transform.position = Vector3.MoveTowards(transform.position, mousePos, moveSpeed * Time.deltaTime);
        }
    }
    
    // Урон от пули
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"Здоровье: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Смерть от стены
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("EnemyWall") && !isDead)
        {
            Die();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyWall") && !isDead)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("Игрок умер!");
        
        // ПОЛНОСТЬЮ ОТКЛЮЧАЕМ ИГРОКА
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
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
        Debug.Log($"Спавн через {respawnDelay} сек...");
        
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log("Спавн!");
        
        // Телепортируем на точку спавна
        transform.position = respawnPoint.position;
        
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
            spriteRenderer.enabled = true;
        }
        
        // Эффект спавна
        if (respawnEffectPrefab != null)
        {
            Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Звук спавна
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        // Игрок снова жив
        isDead = false;
        
        Debug.Log("Игрок заспавнился!");
    }
}