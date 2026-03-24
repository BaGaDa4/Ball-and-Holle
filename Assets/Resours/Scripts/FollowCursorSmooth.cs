using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DragOnHoldWithCollision : MonoBehaviour
{
    [Header("Настройки движения")]
    public float smoothSpeed = 5f;
    public float stopDistance = 0.2f;
    public float zOffset = 10f;
    public KeyCode holdButton = KeyCode.Mouse0;
    
    [Header("Границы движения")]
    public bool useBoundaries = false;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;
    
    [Header("Столкновения")]
    public LayerMask wallLayer = 1;
    public float collisionOffset = 0.1f;
    public bool usePhysicsMovement = true;

    [Header("Лунки (переход на уровень)")]
    public LayerMask holeLayer;
    public GameObject teleportEffect;

    [Header("Смерть от врагов")]
    public LayerMask enemyWallLayer;
    public float deathAnimationDuration = 0.8f;
    public float respawnDelay = 0.5f;
    public GameObject deathEffect;
    public AudioClip deathSound;
    public AudioClip respawnSound;

    [Header("Анимация смерти")]
    public AnimationCurve deathScaleCurve = AnimationCurve.EaseInOut(0, 1, 0.5f, 0);
    public float deathRotationSpeed = 360f;
    public Color deathFlashColor = Color.red;
    public float deathFlashSpeed = 5f;

    [Header("Анимация возрождения")]
    public AnimationCurve respawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);
    public float respawnRotationSpeed = 180f;
    public Color respawnFlashColor = Color.white;

    private Camera mainCamera;
    private Vector3 targetPosition;
    private bool isHolding = false;
    private bool isTeleporting = false;
    private bool isDead = false;
    private bool isRespawning = false;
    private Rigidbody2D rb;
    private Collider2D objectCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private AudioSource audioSource;
    private Vector2 currentVelocity;
    private float maxSpeed = 5f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        objectCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        startPosition = transform.position;
        maxSpeed = smoothSpeed;

        SetupPhysics();
        
        // ПРОВЕРКА ПРЕФАБА
        if (deathEffect == null)
        {
            Debug.LogWarning("Префаб эффекта смерти не назначен!");
        }
        
        Debug.Log($"Игрок готов. Текущая сцена: {SceneManager.GetActiveScene().name}, Индекс: {SceneManager.GetActiveScene().buildIndex}");
    }

    void SetupPhysics()
    {
        if (usePhysicsMovement && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.drag = 5f;
        }

        if (objectCollider == null)
        {
            objectCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        if (usePhysicsMovement && rb != null)
        {
            PhysicsMaterial2D physMat = new PhysicsMaterial2D();
            physMat.friction = 0.4f;
            physMat.bounciness = 0f;
            objectCollider.sharedMaterial = physMat;
        }
    }

    void Update()
    {
        if (isDead || isTeleporting || isRespawning) return;

        if (Input.GetKeyDown(holdButton))
        {
            isHolding = true;
        }
        
        if (Input.GetKeyUp(holdButton))
        {
            isHolding = false;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isTeleporting || isRespawning) return;

        if (isHolding)
        {
            if (usePhysicsMovement && rb != null)
            {
                MoveWithPhysicsSmooth();
            }
            else
            {
                MoveWithRaycastsSmooth();
            }
        }
        else
        {
            if (usePhysicsMovement && rb != null)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
            }
        }
    }

    void MoveWithPhysicsSmooth()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = zOffset;
        
        Vector2 targetPos = mainCamera.ScreenToWorldPoint(mousePosition);
        
        if (useBoundaries)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        
        Vector2 currentPos = rb.position;
        float distance = Vector2.Distance(currentPos, targetPos);
        
        if (distance < stopDistance)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.2f);
            
            if (rb.velocity.magnitude < 0.05f)
            {
                rb.velocity = Vector2.zero;
                rb.position = Vector2.Lerp(rb.position, targetPos, 0.1f);
            }
            return;
        }
        
        Vector2 direction = (targetPos - currentPos).normalized;
        float desiredSpeed = Mathf.Min(maxSpeed, distance * 3f);
        float speedMultiplier = Mathf.Clamp01(distance / (stopDistance * 3f));
        desiredSpeed *= speedMultiplier;
        
        Vector2 desiredVelocity = direction * desiredSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, 0.15f);
        
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void MoveWithRaycastsSmooth()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = zOffset;
        
        targetPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        
        if (useBoundaries)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }
        
        targetPosition.z = transform.position.z;
        
        Vector3 currentPos = transform.position;
        float distance = Vector3.Distance(currentPos, targetPosition);
        
        if (distance < stopDistance)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 0.2f);
            transform.position = Vector3.Lerp(currentPos, targetPosition, 0.1f);
            return;
        }
        
        Vector3 direction = (targetPosition - currentPos).normalized;
        
        RaycastHit2D hit = Physics2D.BoxCast(
            currentPos,
            objectCollider.bounds.size,
            0f,
            direction,
            distance,
            wallLayer
        );
        
        if (hit.collider != null)
        {
            float safeDistance = Mathf.Max(0, hit.distance - collisionOffset);
            Vector3 targetWallPos = currentPos + direction * safeDistance;
            transform.position = Vector3.Lerp(currentPos, targetWallPos, 0.2f);
            currentVelocity = Vector2.zero;
        }
        else
        {
            float speed = Mathf.Min(maxSpeed, distance * 3f);
            currentVelocity = Vector2.Lerp(currentVelocity, (Vector2)direction * speed, 0.15f);
            transform.position += (Vector3)currentVelocity * Time.fixedDeltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isTeleporting || isRespawning) return;

        if (((1 << other.gameObject.layer) & holeLayer) != 0)
        {
            Debug.Log($"ЛУНКА! Переход на следующий уровень");
            TeleportToNextScene();
        }
        
        if (((1 << other.gameObject.layer) & enemyWallLayer) != 0)
        {
            Debug.Log("Враг!");
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isTeleporting || isRespawning) return;

        if (((1 << collision.gameObject.layer) & enemyWallLayer) != 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead || isRespawning) return;
        
        isDead = true;
        isHolding = false;
        
        Debug.Log("Игрок умер!");
        
        // ЗВУК СМЕРТИ МГНОВЕННО
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // ЭФФЕКТ СМЕРТИ МГНОВЕННО
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
            Debug.Log("Эффект смерти создан!");
        }
        else
        {
            Debug.LogWarning("deathEffect не назначен!");
        }
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        while (elapsedTime < deathAnimationDuration)
        {
            float t = elapsedTime / deathAnimationDuration;
            float curveValue = deathScaleCurve.Evaluate(t);
            
            transform.localScale = originalScale * curveValue;
            transform.Rotate(0, 0, deathRotationSpeed * Time.deltaTime);
            
            if (spriteRenderer != null)
            {
                float flash = Mathf.PingPong(Time.time * deathFlashSpeed, 1);
                spriteRenderer.color = Color.Lerp(originalColor, deathFlashColor, flash);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        yield return new WaitForSeconds(respawnDelay);
        
        StartCoroutine(RespawnAnimation(originalScale, originalColor));
    }

    IEnumerator RespawnAnimation(Vector3 originalScale, Color originalColor)
    {
        isRespawning = true;
        
        Debug.Log("Возрождение...");
        
        // ЗВУК ВОЗРОЖДЕНИЯ МГНОВЕННО
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        transform.position = startPosition;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }
        
        transform.localScale = Vector3.zero;
        
        if (rb != null)
        {
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
        
        float elapsedTime = 0f;
        float respawnDuration = deathAnimationDuration * 0.7f;
        
        while (elapsedTime < respawnDuration)
        {
            float t = elapsedTime / respawnDuration;
            float curveValue = respawnScaleCurve.Evaluate(t);
            
            transform.localScale = originalScale * curveValue;
            transform.Rotate(0, 0, -respawnRotationSpeed * Time.deltaTime);
            
            if (spriteRenderer != null)
            {
                float flash = Mathf.PingPong(Time.time * deathFlashSpeed * 2, 1);
                spriteRenderer.color = Color.Lerp(originalColor, respawnFlashColor, flash);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        transform.localScale = originalScale;
        
        isRespawning = false;
        isDead = false;
        
        Debug.Log("Игрок возродился!");
    }

    void TeleportToNextScene()
    {
        if (isTeleporting) return;
        
        isTeleporting = true;
        
        Debug.Log("Касание лунки! Переход на следующий уровень...");
        
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        
        
    PlayerPrefs.SetInt("LevelIndex", nextSceneIndex);
    PlayerPrefs.Save();
    Debug.Log($"Сохранён индекс: {nextSceneIndex}");
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Загружаем уровень {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Это последний уровень! Загружаем первый...");
            SceneManager.LoadScene(0);
        }
    }
}