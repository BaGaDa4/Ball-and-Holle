using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager создан и защищён от уничтожения");
        }
        else
        {
            Debug.Log("GameManager уже существует, уничтожаем дубликат");
            Destroy(gameObject);
        }
    }

    public void SaveLevel(int levelNumber)
    {
        int saved = PlayerPrefs.GetInt("MaxLevel", 1);
        Debug.Log($"SaveLevel вызван: новый={levelNumber}, сохранённый={saved}");
        if (levelNumber > saved)
        {
            PlayerPrefs.SetInt("MaxLevel", levelNumber);
            PlayerPrefs.Save();
            Debug.Log($"Сохранено! MaxLevel = {levelNumber}");
        }
    }

    public int GetMaxLevel()
    {
        int level = PlayerPrefs.GetInt("MaxLevel", 1);
        Debug.Log($"GetMaxLevel вернул: {level}");
        return level;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) PlayerPrefs.Save();
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused) PlayerPrefs.Save();
    }
}