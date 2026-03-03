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

    [Header("Игрок")]
    public LayerMask playerLayer;  // СЮДА ТОЛЬКО ИГРОКА
    public float damage = 100f;    // Количество урона или "смерть"

    void Start()
    {
        if (pivotPoint == null)
            pivotPoint = transform;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        RotateTurret();
        DrawLaser();
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

        // Если игрок попал в луч и находится ближе, чем стена — убиваем
        if (playerHit.collider != null)
        {
            if (wallHit.collider == null || playerHit.distance < wallHit.distance)
            {
                laserEnd = playerHit.point;
                KillPlayer(playerHit.collider.gameObject);
            }
        }

        lineRenderer.SetPosition(1, laserEnd);
    }

    void KillPlayer(GameObject player)
    {
        // Простейший вариант — просто уничтожаем объект
        Destroy(player);

        // Если есть скрипт здоровья:
        // player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
    }
}