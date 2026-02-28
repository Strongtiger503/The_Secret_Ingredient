using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Total real-world minutes the match lasts.")]
    public float totalRealTimeMinutes = 15f;
    [Tooltip("How many real-world minutes equal 1 in-game hour?")]
    public float realMinutesPerInGameHour = 2f;
    [Tooltip("Starting hour in 24-hour format (e.g., 17 = 5:00 PM)")]
    public float startHour = 17f;

    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public GameObject gameWonPanel;

    private float realTimeElapsed = 0f;
    private bool isGameWon = false;
    private playerMovement2 playerMovement;

    void Start()
    {
        // Safely locate the player using the Tag system to disable controls upon winning
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerMovement = playerObj.GetComponent<playerMovement2>();
        }

        if (gameWonPanel != null)
        {
            gameWonPanel.SetActive(false);
        }

        Debug.Log($"System: GameTimer started. Match length: {totalRealTimeMinutes} mins. Starting at {startHour}:00.");
    }

    void Update()
    {
        if (isGameWon) return;

        realTimeElapsed += Time.deltaTime;

        UpdateClockUI();

        // Check if the 15 real-time minutes are up (converted to seconds)
        if (realTimeElapsed >= totalRealTimeMinutes * 60f)
        {
            WinGame();
        }
    }

    void UpdateClockUI()
    {
        if (timeText == null) return;

        // Calculate how many in-game hours have passed based on the conversion rate
        float inGameHoursElapsed = (realTimeElapsed / 60f) / realMinutesPerInGameHour;
        float currentInGameTime = startHour + inGameHoursElapsed;

        // Break the float down into standard hours and minutes
        int displayHour = Mathf.FloorToInt(currentInGameTime) % 24;
        int displayMinute = Mathf.FloorToInt((currentInGameTime % 1) * 60f);

        // Determine AM or PM
        string amPm = displayHour >= 12 ? "PM" : "AM";

        // Convert 24-hour time to 12-hour time for the display
        int hour12 = displayHour % 12;
        if (hour12 == 0) hour12 = 12;

        // Update the UI text (e.g., "05:00 PM")
        timeText.text = $"{hour12:00}:{displayMinute:00} {amPm}";
    }

    void WinGame()
    {
        isGameWon = true;
        Debug.Log("Game Over: Player survived the night! GAME WON.");

        // 1. Show the Game Won UI
        if (gameWonPanel != null)
        {
            gameWonPanel.SetActive(true);
        }

        // 2. Pause the game world
        Time.timeScale = 0f;

        // 3. Disable player controls
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    // You can hook this up to a "Play Again" button on your Game Won Panel
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("System: Scene Restarted after win.");
    }
}