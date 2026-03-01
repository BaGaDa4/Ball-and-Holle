using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    
    void Start()
    {
        Destroy(gameObject, 3f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            // Уничтожаем пулю
            Destroy(gameObject);
        }
        
        // Попадание в стену (любую)
        if (other.CompareTag("Wall") || other.CompareTag("EnemyWall"))
        {
            Destroy(gameObject);
        }
    }
}