using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static event Action<int> OnLivesChanged = delegate { };
    public static event Action OnGameLost = delegate { };

    public static GameManager instance = null;

    //Riferimento al player
    GameObject player;

    //Vite a disposizione del giocatore prima di perdere la partita
    int _totalLives = 3;

    int TotalLives
    {
        get => _totalLives;

        set
        {
            _totalLives = value;
            OnLivesChanged?.Invoke(_totalLives);
        }
    }

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }

    private void Start()
    {
        //Inizializzo lo stato di gioco quando comincia il gioco
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }

    private void OnApplicationQuit()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == 1)
        {
            player = null;
            PlayerHealthSystem.OnPlayerDead -= HandlePlayerDeath;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    { 
        switch(scene.buildIndex)
        {
            case 1:
                TotalLives = 3;
                player = GameObject.FindGameObjectWithTag("Player");
                PlayerHealthSystem.OnPlayerDead += HandlePlayerDeath;
                break;
        }
    }    

    void HandlePlayerDeath(GameObject playerObject)
    {
        playerObject.SetActive(false);
        TotalLives--;
        
        if (TotalLives > 0)
        {
            //Respawna
            RespawnManager.Instance.RespawnPlayer(playerObject);
        }
        else
        {
            //Gioco perso
            GameStateManager.Instance.SetState(GameState.Lost);

            OnGameLost?.Invoke();
        }
    }

    public void SetPlayer(bool canActivePlayer)
    {
        player.GetComponent<PlayerHealthSystem>().enabled = canActivePlayer;
    }
}