using System.Collections;
using UnityEngine;
using YG;

public class Reklama : MonoBehaviour
{
    void OnEnable()
    {
        YG2.onGetSDKData += OnSDKReady;
    }

    void OnDisable()
    {
        YG2.onGetSDKData -= OnSDKReady;
    }

    void OnSDKReady()
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

    public void Rek()
    {
        YG2.InterstitialAdvShow();
    }
}