using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class ScoreManager : MonoBehaviour
{
    GameStats gameStats;
    public GameManager gameManager;
    public Text Player1ScoreText;
    public Text Player2ScoreText;
    public Text Makaf;

    void Awake()
    {
        gameStats = GameObject.FindGameObjectWithTag("GameStats").GetComponent<GameStats>();
        gameManager.CowWon += CowWon;
        gameManager.GameEnded += GameEnded;
        UpdateScores();
    }

    private void UpdateScores()
    {
        Player1ScoreText.text = gameStats.player1Score.ToString();
        Player2ScoreText.text = gameStats.player2Score.ToString();
        Makaf.text = "-";
    }

    private void CowWon(GameObject winningCow)
    {
        Players player = (Players)winningCow.GetComponent<CowStats>().playerNumber;

        if (player == Players.Player1)
            gameStats.player1Score++;
        else
            gameStats.player2Score++;

        Player1ScoreText.text = string.Empty;
        Player2ScoreText.text = string.Empty;
        Makaf.text = string.Empty;
    }

    private void GameEnded(bool viaEsc)
    {
        if (viaEsc) 
            gameStats.Reset();
    }


}
