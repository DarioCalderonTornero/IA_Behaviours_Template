using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cuadros")]
    public int totalPaintings = 5;
    private int stolenPaintings = 0;

    [Header("UI GameOver")]
    public GameObject gameOverPanel;

    [Header("UI Victoria")]
    public GameObject victoryPanel;

    [Header("UI Contador")]
    public TextMeshProUGUI paintingCounterText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        UpdateCounter();
    }

    public void PaintingStolen()
    {
        stolenPaintings++;
        UpdateCounter();

        if (stolenPaintings >= totalPaintings)
            Victory();
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
    }

    private void Victory()
    {
        Time.timeScale = 0f;
        victoryPanel.SetActive(true);
    }

    private void UpdateCounter()
    {
        if (paintingCounterText != null)
            paintingCounterText.text = $"Cuadros: {stolenPaintings} / {totalPaintings}";
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}