using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    [Header("Настройки щита")]
    public float rotationSpeed = 90f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private PlayerController playerController;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Если нет спрайта, добавляем
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            // Создаем простой круглый спрайт через код
            CreateCircleSprite();
        }
        
        // Настраиваем цвет
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0, 0.8f, 1, 0.5f); // Полупрозрачный голубой
        }
    }
    
    void Update()
    {
        // Вращение
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Пульсация
        float pulse = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
    
    public void SetOwner(PlayerController controller)
    {
        playerController = controller;
    }
    
    void CreateCircleSprite()
    {
        // Создаем текстуру с кругом
        Texture2D texture = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (dist < 14 && dist > 10)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        
        Sprite circleSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        spriteRenderer.sprite = circleSprite;
    }
}