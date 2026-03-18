using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class Reklama : MonoBehaviour
{
    void Start()
{ 
    YG2.GameReadyAPI();
    Debug.Log("АПИ работает");
}
    public void GamePLay()
    {
        YG2.GameplayStart();
        Debug.Log("Игра пошла");
    }
    public void GameStop()
    {
    YG2.GameplayStop();
    Debug.Log("Игра остановлена");
    }
    
}
