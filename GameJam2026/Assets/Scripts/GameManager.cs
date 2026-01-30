using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] Player player;
    public Player Player => player;
    [SerializeField] private SceneController sceneController;
    public SceneController SceneController => sceneController;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            return;
        }
    }
}