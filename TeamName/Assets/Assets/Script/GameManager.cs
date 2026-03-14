using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("--------------- Menus ---------------")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text gameGoalCountText;

    [Header("--------------- Player Data ---------------")]
    public Image playerHPBar;
    public GameObject player;
    public PlayerControler2 playerScript; // Fixed name mismatch (added the '2')
    public bool isPaused;

    private float timeScaleOrigin;
    private int GameGoalCount;

    void Awake()
    {
        // Using Awake instead of Start so it's ready before the EnemyAI
        instance = this;
        timeScaleOrigin = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerControler2>();
        }
        else
        {
            Debug.LogError("GameManager: Could not find an object tagged 'Player'!");
        }
    }

    void Update()
    {
        // Toggle Pause with 'Cancel' button (Esc)
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnPause();
            }
        }
    }

    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0; // Freeze the game world
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnPause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrigin; // Resume the game world
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
    }

    public void UpdateGameGoal(int amount)
    {
        GameGoalCount += amount;

        // Update the UI text
        if (gameGoalCountText != null)
        {
            gameGoalCountText.text = GameGoalCount.ToString();
        }

        // Win Condition
        if (GameGoalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
