using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Цель")]
    public Transform player;
    
    [Header("Стрельба")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 20f;        // УВЕЛИЧИЛ СКОРОСТЬ (было 5)
    public float bulletDamage = 1f;
    
    [Header("Поворот")]
    public Transform pivotPoint;
    
    [Header("Эффекты выстрела")]
    public GameObject muzzleFlashPrefab;    // Вспышка
    public AudioClip shootSound;             // Звук
    public float shootForce = 500f;          // СИЛА ВЫСТРЕЛА (для физики)
    
    private float nextFireTime;
    private AudioSource audioSource;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Update()
    {
        if (player == null || pivotPoint == null) return;
        
        // Поворот на игрока
        Vector2 direction = player.position - pivotPoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pivotPoint.rotation = Quaternion.Euler(0, 0, angle);
        
        // Стрельба
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
    
    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        // СОЗДАЕМ ПУЛЮ
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // ДАЕМ ИМПУЛЬС (мощный толчок)
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // СПОСОБ 1: Прямая скорость (проще)
            rb.velocity = firePoint.right * bulletSpeed;
            
            // СПОСОБ 2: Сила (для физики)
            // rb.AddForce(firePoint.right * shootForce);
            
            // СПОСОБ 3: Импульс (мгновенная сила)
            // rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);
        }
        
        // ЭФФЕКТ ВСПЫШКИ
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }
        
        // ЗВУК ВЫСТРЕЛА
        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Уничтожаем пулю через 3 секунды
        Destroy(bullet, 3f);
        
        Debug.Log("БАХ! Мощный выстрел!");
    }
    
    void OnDrawGizmos()
    {
        if (pivotPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pivotPoint.position, 0.2f);
        }
        
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);
            
            // Рисуем направление выстрела
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(firePoint.position, firePoint.right * 2f);
        }
    }
}