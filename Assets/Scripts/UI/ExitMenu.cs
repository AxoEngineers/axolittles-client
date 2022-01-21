using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    public void ReturnToMenu()
    {
        SceneManager.Instance.ExitGame();
    }
}
