using UnityEngine;
using System.Collections;

public class GameStats : MonoBehaviour
{  
    [HideInInspector]
    public int player1Score = 0;
    [HideInInspector]
    public int player2Score = 0;

    public void PlayerWins(Players player)
    {
        switch (player)
        {   
            case Players.Player1:
                player1Score++;
                break;
            case Players.Player2:
                player2Score++;
                break;
            default:
                break;
        }
    }
    public void Reset()
    {
        player1Score = 0;
        player2Score = 0;
    }
}
