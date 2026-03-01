using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public GameObject hitEffect;
    
    void Start()
    {
        Destroy(gameObject, 3f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Пуля попала в: {other.gameObject.name}, Tag: {other.tag}");
        
        // Попадание в игрока
        if (other.CompareTag("Player"))
        {
            Debug.Log("Попадание в игрока!");
            
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Наносим урон {damage} игроку");
            }
            else
            {
                Debug.LogError("На игроке нет компонента PlayerController!");
            }
        }
        
        // Эффект попадания
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Уничтожаем пулю
        Destroy(gameObject);
    }
}