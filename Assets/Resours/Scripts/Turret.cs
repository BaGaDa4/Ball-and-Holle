using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Цель")]
    public Transform player;
    
    [Header("Стрельба")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 10f;
    
    [Header("Поворот")]
    public Transform pivotPoint;
    
    [Header("Дальность")]
    public float shootingRange = 10f;
    
    private float nextFireTime;
    private PlayerController playerController;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }
        else
        {
            playerController = player.GetComponent<PlayerController>();
        }
        
        if (pivotPoint == null)
            pivotPoint = transform;
        
        if (firePoint == null)
            firePoint = transform;
    }
    
    void Update()
    {
        if (player == null || playerController == null) return;
        
        // Используем IsAlive() для проверки
        if (!playerController.IsAlive())
            return;
        
        float distance = Vector2.Distance(pivotPoint.position, player.position);
        
        if (distance <= shootingRange)
        {
            Vector2 direction = player.position - pivotPoint.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            pivotPoint.rotation = Quaternion.Euler(0, 0, angle);
            
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = firePoint.right * bulletSpeed;
        
        Destroy(bullet, 3f);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pivotPoint != null ? pivotPoint.position : transform.position, shootingRange);
    }
}