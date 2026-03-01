using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    [Header("Настройки движения")]
    public float amplitude = 1f;        // Амплитуда (высота) движения
    public float frequency = 1f;        // Частота (скорость) движения

    private float startY;                // Начальная позиция Y
    private float timeOffset;             // Смещение по времени (чтобы объекты не двигались синхронно)

    void Start()
    {
        // Запоминаем начальную позицию по Y
        startY = transform.localPosition.y;
        // Случайное смещение, чтобы объекты не двигались все одновременно
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Вычисляем новую позицию Y с помощью синуса
        // Mathf.Sin дает значение от -1 до 1
        // Умножаем на amplitude, чтобы получить нужный размах
        // Умножаем время на frequency, чтобы контролировать скорость
        float newY = startY + Mathf.Sin((Time.time + timeOffset) * frequency) * amplitude;

        // Применяем новую позицию к объекту, оставляя X и Z без изменений
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}