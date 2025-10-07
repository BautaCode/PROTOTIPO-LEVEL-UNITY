using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager3D : MonoBehaviour
{
    public static GameManager3D I;

    [Header("Puntaje")]
    public int score = 0;
    public int targetScore = 15;

    [Header("UI")]
    public TMP_Text scoreText;      
    public GameObject winPanel;     
    public GameObject losePanel;    

    bool ended = false;

    void Awake() { if (I == null) I = this; else Destroy(gameObject); }

    void Start()
    {
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Puntos: {score}/{targetScore}";
    }

    public void AddPoint(int n = 1)
    {
        if (ended) return;
        score += n;
        UpdateUI();
        if (score >= targetScore) Win();
    }

    public void Lose()
    {
        if (ended) return;
        ended = true;
        if (losePanel) losePanel.SetActive(true);
        Invoke(nameof(Restart), 1.2f);
    }

    void Win()
    {
        ended = true;
        if (winPanel) winPanel.SetActive(true);
        
    }

    public void Restart()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

