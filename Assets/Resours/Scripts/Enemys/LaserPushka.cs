using UnityEngine;

public class LaserPuska : MonoBehaviour
{
    [Header("Вращение")]
    public float rotationSpeed = 60f;
    public Transform pivotPoint;

    [Header("Лазер")]
    public float maxLaserDistance = 20f;
    public LayerMask wallLayer;   // СЮДА ТОЛЬКО СТЕНЫ
    public LineRenderer lineRenderer;
    public AudioClip laserSound;           // Звук лазера
    public float laserSoundVolume = 0.5f;  // Громкость звука

    [Header("Игрок")]
    public LayerMask playerLayer;  // СЮДА ТОЛЬКО ИГРОКА

    private PlayerController playerController;
    private GameObject playerObject;
    private AudioSource audioSource;
    private bool isPlayerHit = false;      // Флаг для отслеживания попадания

    void Start()
    {
        if (pivotPoint == null)
            pivotPoint = transform;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        
        // Добавляем AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && laserSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;        // Зацикливаем звук
            audioSource.volume = laserSoundVolume;
            audioSource.clip = laserSound;
        }
        
        // Находим игрока
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObject = playerObj;
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        RotateTurret();
        DrawLaser();
        HandleLaserSound();
    }

    void RotateTurret()
    {
        pivotPoint.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    void DrawLaser()
    {
        Vector2 origin = pivotPoint.position;
        Vector2 direction = pivotPoint.right;

        // Проверяем стены
        RaycastHit2D wallHit = Physics2D.Raycast(origin, direction, maxLaserDistance, wallLayer);

        // Проверяем игрока
        RaycastHit2D playerHit = Physics2D.Raycast(origin, direction, maxLaserDistance, playerLayer);

        lineRenderer.SetPosition(0, origin);

        // По умолчанию лазер на максимум
        Vector2 laserEnd = origin + direction * maxLaserDistance;

        // Если есть стена — луч до стены
        if (wallHit.collider != null)
        {
            laserEnd = wallHit.point;
        }

        // Проверяем игрока
        bool playerInLaser = false;
        
        if (playerHit.collider != null)
        {
            if (wallHit.collider == null || playerHit.distance < wallHit.distance)
            {
                laserEnd = playerHit.point;
                playerInLaser = true;
                
                // Убиваем игрока только один раз при попадании
                if (!isPlayerHit)
                {
                    KillPlayer(playerHit.collider.gameObject);
                    isPlayerHit = true;
                }
            }
        }
        
        // Если игрока нет в лазере, сбрасываем флаг
        if (!playerInLaser)
        {
            isPlayerHit = false;
        }

        lineRenderer.SetPosition(1, laserEnd);
    }

    void KillPlayer(GameObject player)
    {
        if (playerController == null)
        {
            // Пробуем найти еще раз
            FindPlayer();
            if (playerController == null) return;
        }

        // Проверяем, жив ли игрок и не в процессе возрождения
        if (playerController != null && playerController.IsAlive())
        {
            Debug.Log("Лазер убил игрока!");
            playerController.TakeDamage(999); // Большой урон для мгновенной смерти
        }
    }

    void HandleLaserSound()
    {
        if (audioSource == null || laserSound == null) return;

        // Включаем звук, если лазер активен (всегда работает)
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Визуализация для отладки
    void OnDrawGizmosSelected()
    {
        if (pivotPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pivotPoint.position, pivotPoint.right * maxLaserDistance);
        }
    }
}