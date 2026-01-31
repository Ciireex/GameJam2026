using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event EventHandler OnTimeIsUp;

    [Header("References (auto)")]
    [SerializeField] private Player player; // opcional: se auto-rellena si est� vac�o
    public Player Player => player;

    // No guardes referencia serializada: usa el singleton real
    public SceneController SceneController => SceneController.Instance;

    [Header("Level Timer")]
    [SerializeField] private float levelTime = 120f;
    private float timeLeft;
    private bool timerRunning;

    private void Awake()
    {
        // Singleton correcto + persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Nos enteramos cuando cambian escenas para pillar el nuevo Player
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void Start()
    {
        FindAndBindPlayer();
    }
    private void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            timerRunning = false;
            OnTimeOut();
        }
    }

    private void OnDestroy()
    {
        // Limpieza
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindPlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que se carga una escena, buscamos el Player nuevo
        FindAndBindPlayer();
    }

    private void FindAndBindPlayer()
    {
        // Desuscribe del anterior, si lo hab�a
        UnbindPlayer();

        // Busca el Player de la escena actual (asumiendo 1 por escena)
        player = FindFirstObjectByType<Player>();

        if (player != null)
        {
            player.OnPlayerDeath += Player_OnPlayerDeath;

            // APLICAR DRAIN DEL NIVEL (NUEVO)
            LevelSettings settings = FindFirstObjectByType<LevelSettings>();
            if (settings != null)
            {
                player.SetDrainMultiplier(settings.drainMultiplier);
                levelTime = settings.levelTime;
            }

            StartTimer();
        }
        else
        {
            Debug.LogWarning("[GameManager] No se encontr� Player en la escena actual.");
        }
    }

    private void UnbindPlayer()
    {
        if (player != null)
        {
            player.OnPlayerDeath -= Player_OnPlayerDeath;
        }
    }

    private void Player_OnPlayerDeath(object sender, EventArgs e)
    {
        //GameOver();
    }

    private void GameOver()
    {
        Time.timeScale = 0f;
    }

    public float GetTimeLeftNomralized()
    {
        if (!timerRunning || levelTime <= 0f)
            return 0f;
        return Mathf.Clamp01(timeLeft / levelTime);
    }

    public void StartTimer()
    {
        timeLeft = levelTime;
        timerRunning = true;
    }

    private void OnTimeOut()
    {
        Debug.Log("Time's up!");
        OnTimeIsUp?.Invoke(this, EventArgs.Empty);
    }


}
