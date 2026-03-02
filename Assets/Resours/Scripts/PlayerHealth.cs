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
    public LayerMask wallLayer;
    public LayerMask enemyWallLayer;
    
    [Header("Анимация смерти")]
    public float deathAnimationDuration = 0.8f;
    public AnimationCurve deathScaleCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 1.5f);
    public AnimationCurve deathRotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 720);
    public Color deathStartColor = Color.red;
    public Color deathEndColor = Color.clear;
    public float deathShakeIntensity = 0.2f;
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;
    
    [Header("Анимация возрождения")]
    public float respawnAnimationDuration = 0.6f;
    public AnimationCurve respawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1.2f);
    public AnimationCurve respawnAlphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Color respawnStartColor = Color.cyan;
    public Color respawnEndColor = Color.white;
    public float respawnRotationSpeed = 180f;
    public GameObject respawnEffectPrefab;
    public AudioClip respawnSound;
    
    [Header("Возрождение")]
    public Transform respawnPoint;
    public float respawnDelay = 1.5f;
    
    // ================ ЩИТ (НЕУЯЗВИМОСТЬ) ================
    [Header("Щит (неуязвимость)")]
    public GameObject shieldObject;
    public float invincibilityDuration = 2f;
    public float invincibilityFlashSpeed = 0.1f;
    // ===================================================
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private AudioSource audioSource;
    private bool isDead = false;
    private bool isRespawning = false;
    private bool isInvincible = false;
    private bool isDying = false;
    
    private Color originalColor;
    private bool isFlashing = false;
    
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
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
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
        if (isDead || isRespawning || isDying) return;
        
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            MoveWithWallCheck(mousePos);
        }
    }
    
    void MoveWithWallCheck(Vector3 targetPos)
    {
        Vector3 currentPos = transform.position;
        Vector3 direction = (targetPos - currentPos).normalized;
        float distance = Vector3.Distance(currentPos, targetPos);
        float moveDistance = Mathf.Min(moveSpeed * Time.deltaTime, distance);
        
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
            float safeDistance = Mathf.Max(0, hit.distance - 0.05f);
            if (safeDistance > 0)
            {
                transform.position = currentPos + direction * safeDistance;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(currentPos, targetPos, moveDistance);
        }
    }
    
    public bool IsAlive()
    {
        return !isDead && !isRespawning && !isDying;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isRespawning || isInvincible || isDying) return;
        
        currentHealth -= damage;
        Debug.Log($"Здоровье: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }
    
    IEnumerator FlashRed()
    {
        if (isFlashing) yield break;
        isFlashing = true;
        
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        
        isFlashing = false;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isRespawning || isInvincible || isDying) return;
        
        if (((1 << other.gameObject.layer) & enemyWallLayer) != 0)
        {
            Debug.Log("Смерть от стены!");
            StartCoroutine(Die());
        }
    }
    
    IEnumerator Die()
    {
        if (isDead || isDying) yield break;
        
        isDying = true;
        isDead = true;
        
        Debug.Log("Игрок умирает...");
        
        // ЗВУК СМЕРТИ МГНОВЕННО
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Выключаем щит
        if (shieldObject != null)
            shieldObject.SetActive(false);
        
        // Отключаем физику
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }
        
        // Запоминаем начальные значения
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < deathAnimationDuration)
        {
            float t = elapsedTime / deathAnimationDuration;
            
            float scale = deathScaleCurve.Evaluate(t);
            transform.localScale = startScale * scale;
            
            float rotation = deathRotationCurve.Evaluate(t);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            
            Color currentColor = Color.Lerp(deathStartColor, deathEndColor, t);
            spriteRenderer.color = currentColor;
            
            float shakeX = Random.Range(-deathShakeIntensity, deathShakeIntensity) * (1 - t);
            float shakeY = Random.Range(-deathShakeIntensity, deathShakeIntensity) * (1 - t);
            transform.position = startPos + new Vector3(shakeX, shakeY, 0);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = deathEndColor;
        transform.localScale = startScale;
        transform.position = startPos;
        
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        yield return new WaitForSeconds(respawnDelay);
        
        StartCoroutine(Respawn());
    }
    
    IEnumerator Respawn()
    {
        isRespawning = true;
        
        Debug.Log("Возрождение...");
        
        // ЗВУК ВОЗРОЖДЕНИЯ МГНОВЕННО
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        transform.position = respawnPoint.position;
        transform.rotation = Quaternion.identity;
        
        spriteRenderer.color = originalColor;
        currentHealth = maxHealth;
        
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < respawnAnimationDuration)
        {
            float t = elapsedTime / respawnAnimationDuration;
            
            float scale = respawnScaleCurve.Evaluate(t);
            transform.localScale = Vector3.one * scale;
            
            float alpha = respawnAlphaCurve.Evaluate(t);
            Color spawnColor = spriteRenderer.color;
            spawnColor.a = alpha;
            spriteRenderer.color = spawnColor;
            
            transform.Rotate(0, 0, -respawnRotationSpeed * Time.deltaTime);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = originalColor;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        
        if (respawnEffectPrefab != null)
        {
            GameObject effect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // ВКЛЮЧАЕМ ЩИТ
        StartCoroutine(InvincibilityWithShield());
        
        // Сбрасываем флаги
        isRespawning = false;
        isDead = false;
        isDying = false;
        
        Debug.Log("Игрок готов!");
    }
    
    IEnumerator InvincibilityWithShield()
    {
        isInvincible = true;
        
        if (shieldObject != null)
        {
            shieldObject.SetActive(true);
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            
            if (shieldObject != null)
            {
                shieldObject.SetActive(true);
                shieldObject.transform.Rotate(0, 0, 360 * Time.deltaTime);
            }
            
            yield return new WaitForSeconds(invincibilityFlashSpeed);
            elapsedTime += invincibilityFlashSpeed;
        }
        
        if (shieldObject != null)
        {
            shieldObject.SetActive(false);
        }
        
        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
        
        isInvincible = false;
    }
}