using com.thekingdom.game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActionController : MonoBehaviour
{
    public GameLauncher gameLauncher;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void backMainMenu()
    {
        // Destroy local player network
        gameLauncher.DestroyLocalPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
