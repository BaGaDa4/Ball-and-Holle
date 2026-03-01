using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DragOnHoldWithCollision : MonoBehaviour
{
    [Header("Настройки движения")]
    public float smoothSpeed = 5f;           // Максимальная скорость
    public float stopDistance = 0.2f;         // Дистанция остановки перед курсором
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
    public string nextSceneName = "Level2";
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
    private Rigidbody2D rb;
    private Collider2D objectCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private AudioSource audioSource;
    
    // Для плавной остановки
    private Vector2 currentVelocity;
    private float maxSpeed = 5f;
    private float acceleration = 15f;
    private float deceleration = 20f;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        // Получаем компоненты
        rb = GetComponent<Rigidbody2D>();
        objectCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Запоминаем стартовую позицию
        startPosition = transform.position;
        maxSpeed = smoothSpeed;

        // Настраиваем физику
        SetupPhysics();
    }

    void SetupPhysics()
    {
        if (usePhysicsMovement && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.drag = 5f; // Добавляем сопротивление для плавной остановки
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
        if (isDead || isTeleporting) return;

        // Проверяем зажатие кнопки
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
        if (isDead || isTeleporting) return;

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
            // Плавная остановка когда не держим кнопку
            if (usePhysicsMovement && rb != null)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.1f);
            }
        }
    }

    // Новая плавная физика движения
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
        
        // Если уже достаточно близко к курсору - тормозим
        if (distance < stopDistance)
        {
            // Плавно тормозим
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, 0.2f);
            
            // Если почти остановились - просто ставим на место
            if (rb.velocity.magnitude < 0.05f)
            {
                rb.velocity = Vector2.zero;
                rb.position = Vector2.Lerp(rb.position, targetPos, 0.1f);
            }
            return;
        }
        
        // Вычисляем желаемое направление
        Vector2 direction = (targetPos - currentPos).normalized;
        
        // Желаемая скорость зависит от расстояния
        float desiredSpeed = Mathf.Min(maxSpeed, distance * 3f); // Чем дальше, тем быстрее
        
        // Чем ближе к цели, тем сильнее тормозим
        float speedMultiplier = Mathf.Clamp01(distance / (stopDistance * 3f));
        desiredSpeed *= speedMultiplier;
        
        Vector2 desiredVelocity = direction * desiredSpeed;
        
        // Плавно меняем скорость
        rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, 0.15f);
        
        // Ограничиваем максимальную скорость
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    // Новая плавная версия для Raycast движения
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
        
        // Если достаточно близко - останавливаемся
        if (distance < stopDistance)
        {
            // Плавно останавливаемся
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, 0.2f);
            transform.position = Vector3.Lerp(currentPos, targetPosition, 0.1f);
            return;
        }
        
        // Вычисляем направление
        Vector3 direction = (targetPosition - currentPos).normalized;
        
        // Проверка на стены
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
            // Есть стена - останавливаемся перед ней
            float safeDistance = Mathf.Max(0, hit.distance - collisionOffset);
            Vector3 targetWallPos = currentPos + direction * safeDistance;
            
            // Плавно двигаемся к стене
            transform.position = Vector3.Lerp(currentPos, targetWallPos, 0.2f);
            currentVelocity = Vector2.zero;
        }
        else
        {
            // Нет стен - плавно двигаемся к курсору
            float speed = Mathf.Min(maxSpeed, distance * 3f);
            
            // Плавно меняем скорость
            currentVelocity = Vector2.Lerp(
                currentVelocity, 
                (Vector2)direction * speed, 
                0.15f
            );
            
            // Применяем движение
            transform.position += (Vector3)currentVelocity * Time.fixedDeltaTime;
        }
    }

    // Остальные методы без изменений...
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isTeleporting) return;

        if (((1 << other.gameObject.layer) & holeLayer) != 0)
        {
            TeleportToNextScene();
        }
        
        if (((1 << other.gameObject.layer) & enemyWallLayer) != 0)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isTeleporting) return;

        if (((1 << collision.gameObject.layer) & enemyWallLayer) != 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isHolding = false;
        
        Debug.Log("Игрок умер! Касание стены-врага");
        
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
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
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
        Debug.Log("Возрождение...");
        
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
        
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
        
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
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
        
        isDead = false;
        
        Debug.Log("Игрок возродился!");
    }

    void TeleportToNextScene()
    {
        if (isTeleporting) return;
        
        isTeleporting = true;
        
        Debug.Log($"Касание лунки! Переход на сцену: {nextSceneName}");
        
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Имя следующей сцены не указано!");
        }
    }
}