using UnityEngine;

public class Hole : MonoBehaviour
{
    [Header("Настройки лунки")]
    public Transform respawnPoint;           // Точка возрождения в другой сцене
    public string targetSceneName;            // Имя сцены для перехода (опционально)
    public GameObject holeEffect;             // Эффект лунки
    
    [Header("Визуал")]
    public Color holeColor = Color.blue;       // Цвет лунки в редакторе
    public float pulseSpeed = 1f;              // Скорость пульсации
    public float pulseAmount = 0.1f;           // Сила пульсации

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        if (holeEffect != null)
        {
            Instantiate(holeEffect, transform.position, Quaternion.identity, transform);
        }
    }

    void Update()
    {
        // Эффект пульсации
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = holeColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (respawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, respawnPoint.position);
        }
    }
}