using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SetupBloom : MonoBehaviour
{
    void Start()
    {
        // Включаем HDR на камере (нужно для Bloom)
        Camera cam = GetComponent<Camera>();
        cam.allowHDR = true;
        
        // Добавляем Volume для пост-эффектов
        Volume volume = gameObject.AddComponent<Volume>();
        
        // Создаем профиль с Bloom
        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;
        
        // Добавляем Bloom эффект
        Bloom bloom = profile.Add<Bloom>(true);
        bloom.threshold.value = 1f; // Что считать ярким (1 = обычная яркость)
        bloom.intensity.value = 2f;  // Сила свечения
        bloom.scatter.value = 0.5f;  // Радиус размытия
    }
}