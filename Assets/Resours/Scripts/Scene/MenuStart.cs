using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuStart : MonoBehaviour
{
    public string[] levels = { "Level1", "Level2", "Level3", "Level4", "Level5", 
                                "Level6", "Level7", "Level8", "Level9", "Level10" };

    public void StartGame()
{
    int index = PlayerPrefs.GetInt("LevelIndex", 1);
    Debug.Log($"Запускаем сцену с индексом: {index}");
    SceneManager.LoadScene(index);
}
}