using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    public ParticleSystem deathParticles;
    
    void Start()
    {
        // Создаем партиклы если не назначены
        if (deathParticles == null)
        {
            CreateDeathParticles();
        }
    }
    
    void CreateDeathParticles()
    {
        // Создаем объект с партиклами
        GameObject particlesObj = new GameObject("DeathParticles");
        particlesObj.transform.position = transform.position;
        
        // Добавляем ParticleSystem
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        
        // Основные настройки
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.5f;
        main.startColor = Color.red;
        main.maxParticles = 50;
        
        // Настройки эмиссии (сколько частиц)
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0, 20));
        
        // Настройки формы
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Настройки цвета
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.yellow, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }
        );
        colorOverLifetime.color = gradient;
        
        // Настройки размера
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, 1, 1, 0));
        
        deathParticles = ps;
    }
    
    public void PlayEffect(Vector3 position)
    {
        if (deathParticles != null)
        {
            deathParticles.transform.position = position;
            deathParticles.Play();
            
            // Уничтожаем через 2 секунды
            Destroy(deathParticles.gameObject, 2f);
        }
    }
}