using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [Header("Настройки музыки")]
    public AudioClip backgroundMusic;        // Музыка для игры
    public float volume = 0.5f;               // Громкость
    
    private static MusicManager instance;      // Синглтон для сохранения между сценами
    private AudioSource audioSource;
    
    void Awake()
    {
        // СИНГЛТОН - чтобы не создавать копии при перезагрузке сцены
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // НЕ УНИЧТОЖАТЬ ПРИ ЗАГРУЗКЕ НОВОЙ СЦЕНЫ
        }
        else
        {
            Destroy(gameObject); // Уничтожаем копию
            return;
        }
        
        // Настройка AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Настройки аудио
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = true; // Зацикливаем
        audioSource.playOnAwake = true;
        
        // Запускаем музыку
        audioSource.Play();
    }
    
    // Метод для смены музыки (если нужно)
    public void ChangeMusic(AudioClip newMusic, float newVolume = -1)
    {
        if (newMusic != null)
        {
            audioSource.clip = newMusic;
            audioSource.Play();
        }
        
        if (newVolume >= 0)
        {
            audioSource.volume = newVolume;
        }
    }
    
    // Метод для остановки музыки
    public void StopMusic()
    {
        audioSource.Stop();
    }
    
    // Метод для паузы
    public void PauseMusic()
    {
        audioSource.Pause();
    }
    
    // Метод для продолжения
    public void ResumeMusic()
    {
        audioSource.UnPause();
    }
}