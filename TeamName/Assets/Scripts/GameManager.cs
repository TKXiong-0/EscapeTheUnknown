using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [Header("----- UI -----")]
    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text equippedItemText;

    public Image playerHPBar;
    public GameObject player;
    public PlayerController playerScript;
    public GameObject playerSpawnPos;
    public GameObject checkpointPopup;
    public GameObject KeyPopup;
    public GameObject damagePlayerFlash;

    public bool isPaused;

    private float timeScaleOrigin;
    private int GameGoalCount;

    void Awake()
    {
        instance = this;
        timeScaleOrigin = Time.timeScale;

        player = GameObject.FindWithTag("Player");

        if (player != null)
            playerScript = player.GetComponent<PlayerController>();

        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
    }

    void Start()
    {
        UpdateEquippedItemUI("None");
    }

    void Update()
    {
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
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnPause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrigin;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (menuActive != null)
            menuActive.SetActive(false);

        menuActive = null;
    }

    public void UpdateGameGoal(int amount)
    {
        GameGoalCount += amount;

        if (gameGoalCountText != null)
            gameGoalCountText.text = GameGoalCount.ToString("F0");

        if (GameGoalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void UpdateEquippedItemUI(string itemName)
    {
        if (equippedItemText == null)
            return;

        equippedItemText.text = "Equipped: " + itemName;
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}