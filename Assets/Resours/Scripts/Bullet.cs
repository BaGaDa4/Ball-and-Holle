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
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Wall") || other.CompareTag("EnemyWall"))
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}