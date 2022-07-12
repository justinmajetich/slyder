using System;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public static event Action<Constants.Scene> OnStartGame;

    public void OnStartClicked()
    {
        OnStartGame?.Invoke(Constants.Scene.Bedroom);
    }
}
