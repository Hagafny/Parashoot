using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    private AudioSource audioSrc;
    public AudioClip mooSound;
    public int CountdownSeconds = 3;
    public Text m_MessageText; // Reference to the our Huge Text in the middle of the screen.
    public Text Player1NameText;
    public Text Player2NameText;
    public GameObject m_CowPrefab; // This is the prefab to spawn.
    public CowManager[] m_Cows; // Array of CowManagers. Each Cow has a CowManager of her own.
    public Action<GameObject> CowWon;
    public Action<GameObject, GameObject, GameObject> ACowHasShotAPowerUp;
    public Action<GameObject, GameObject, GameObject> ACowHasShotABalloon;
    public Action<bool> GameEnded;

    private CowManager winningCow;
    private bool gameWon = false;
    private GameOptions gameOptions;

    /// <summary>
    /// This is called at the first frame.
    /// </summary>
    void Start()
    {
        GameObject gameOptionsObject = GameObject.Find("GameOptions");
        if (gameOptionsObject == null)
            gameOptions = CreateMockGameOptions(CowType.Human);
        else
            gameOptions = gameOptionsObject.GetComponent<GameOptions>();

        audioSrc = GetComponent<AudioSource>();


        SpawnAllCows(); // Spawn the cows at the corresponding spawn points.

        if (gameOptions.cowOptions[1].type != CowType.Human)
            SetupAI();

        Player1NameText.text = m_Cows[0].m_CowName.ToUpper();
        Player2NameText.text = m_Cows[1].m_CowName.ToUpper();

        StartCoroutine(GameLoop()); // Start the GameLoop.
    }

    private GameOptions CreateMockGameOptions(CowType cowType)
    {
        GameObject gameOptionsObject = new GameObject("GameOptions");
        gameOptionsObject.tag = "GameOptions";
        GameOptions go = gameOptionsObject.AddComponent<GameOptions>();
        CowOptions[] cowOptions = new CowOptions[2];
        for (int i = 0; i < m_Cows.Length; i++)
            cowOptions[i] = new CowOptions("Player " + (i + 1), m_Cows[i].m_PlayerColor, CowType.Human);


        cowOptions[1].type = cowType;

        go.cowOptions = cowOptions;

        return go;

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameEnded != null)
                GameEnded(true);

            SceneManager.LoadScene(1);
        }
    }

    /// <summary>
    /// This method is in charge of actually instantiating the cows (A.K.A the m_CowPrefab), populating the CowManager's playerNumber property and calling Setup. 
    /// </summary>
    private void SpawnAllCows()
    {
        //Iterate through the managers.
        int cowsLength = m_Cows.Length;
        for (int i = 0; i < cowsLength; i++)
        {
            // Create an instance of a cow at the position and rotation of the spawn point.
            m_Cows[i].m_Instance =
                Instantiate(m_CowPrefab, m_Cows[i].m_StartingPoint.position, m_Cows[i].m_StartingPoint.rotation) as GameObject;

            // Populate the playerNumber property, this would later be importend to the cow's Shoot and Move scripts.
            m_Cows[i].m_PlayerNumber = i + 1;

            // Call Setup on each cow to populate scripts and other basic functionalities.
            m_Cows[i].Setup(gameOptions.cowOptions[i]);

            m_Cows[i].m_Shooting.ShooterHitPowerUp += ShooterHitPowerUp;
            m_Cows[i].m_Shooting.ShooterHitBalloon += ShooterHitBalloon;
            m_Cows[i].m_Health.CowDead += CowDead;
        }
    }

    /// <summary>
    ///  This is the main loop. It's a simple state machine that is in charge of moving the game from one state to another.
    ///  We only move to GamePlaying after we are finished with GameStarting. We only move to GameEnding after we finish with GamePlaying, etc
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(GameStarting());
        yield return StartCoroutine(GamePlaying());
        yield return StartCoroutine(GameEnding());

        if (GameEnded != null)
            GameEnded(false);
        // When we finally get here, the game has gone through all level and is finished. For now, we'll restart it.

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Represents what happens when the game starts
    /// </summary>
    private IEnumerator GameStarting()
    {
        //Disable movement and shooting.
        MoveCowsToStartingPosition();
        DisableCowControl();
        StartCoroutine(countdown());


        //Wait for the amount of m_StartWait and move to the GamePlaying state.
        yield return new WaitForSeconds(CountdownSeconds);
    }
    private void MoveCowsToStartingPosition()
    {
        for (int i = 0; i < m_Cows.Length; i++)
        {
            CowManager cowManager = m_Cows[i];
            iTween.MoveTo(cowManager.m_Instance, cowManager.m_EndPoint.transform.position, CountdownSeconds);


        }
    }
    private IEnumerator countdown()
    {
        int countdown = CountdownSeconds;
        while (countdown > 0)
        {
            m_MessageText.text = countdown.ToString();
            countdown--;
            yield return new WaitForSeconds(1);
        }
    }


    /// <summary>
    /// Represents what happens when the game is played.
    /// </summary>
    private IEnumerator GamePlaying()
    {
        //Enable movement and shooting.
        audioSrc.PlayOneShot(mooSound);
        EnableCowControl();

        //This makes the huge text message go away. No one wants a huge text message when they play.
        m_MessageText.text = string.Empty;

        //TODO : Write documentation here
        while (!gameWon)
            yield return null;
    }

    /// <summary>
    /// Represents what happens when the game is ending.
    /// </summary>
    private IEnumerator GameEnding()
    {
        //Disable the controls again. No need to move the cow now.
        DisableCowControl();

        string message = GetEndMessage(winningCow);

        //Display the message
        m_MessageText.text = message;
        m_MessageText.alignment = TextAnchor.UpperCenter;



        if (CowWon != null)
            CowWon(winningCow.m_Instance);

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;
    }


    /// <summary>
    /// Iterate through the cows and call "EneableControl" on each.
    /// </summary>
    private void EnableCowControl()
    {
        for (int i = 0; i < m_Cows.Length; i++)
        {
            m_Cows[i].EnableControl();
        }
    }

    /// <summary>
    /// Iterate through the cows and call "DisableCowControl" on each.
    /// </summary>
    private void DisableCowControl()
    {
        for (int i = 0; i < m_Cows.Length; i++)
        {
            m_Cows[i].DisableControl();
        }
    }

    private void ShooterHitPowerUp(GameObject ThePowerUp, GameObject CowThatHitThePowerUp)
    {
        if (ACowHasShotAPowerUp != null)
        {
            GameObject enemyCow = null;

            for (int i = 0; i < m_Cows.Length; i++)
            {
                if (m_Cows[i].m_Instance.transform.name != CowThatHitThePowerUp.transform.name)
                {
                    enemyCow = m_Cows[i].m_Instance;
                    continue;
                }
            }
            if (enemyCow != null)
                ACowHasShotAPowerUp(ThePowerUp, CowThatHitThePowerUp, enemyCow);
        }
    }

    private void ShooterHitBalloon(GameObject TheBalloon, GameObject TheBullet, GameObject CowThatHitTheBalloon)
    {
        if (ACowHasShotAPowerUp != null)
        {
            ACowHasShotABalloon(TheBalloon, TheBullet, CowThatHitTheBalloon);
        }
    }



    private string GetEndMessage(CowManager WinningCow)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(WinningCow.m_PlayerColor);

        StringBuilder endMessage = new StringBuilder();

        endMessage.Append(ColorizeSentence(colorHex, winningCow.m_CowName));
        endMessage.Append("\n");
        endMessage.Append(changeFontSize(200, "wins the"));
        endMessage.Append("\n");
        endMessage.Append(changeFontSize(205, "game!"));
        endMessage.Append("\n");
        endMessage.Append(ColorizeSentence(colorHex, "esc") + " to menu");
        endMessage.Append("\n");
        endMessage.Append(ColorizeSentence(colorHex, "space") + " to restart");

        return endMessage.ToString().ToUpper();

    }

    private string ColorizeSentence(string colorHex, string sentence)
    {
        return string.Format("<color=#{0}>{1}</color>", colorHex, sentence);
    }

    private string changeFontSize(int fontSize, string sentence)
    {
        return string.Format("<size={0}>{1}</size>", fontSize, sentence);
    }

    private void CowDead(GameObject deadCow)
    {
        FindWinningCow(deadCow);
        gameWon = true;

    }

    private void FindWinningCow(GameObject deadCow)
    {
        for (int i = 0; i < m_Cows.Length; i++)
        {
            if (m_Cows[i].m_Instance.name != deadCow.transform.name)
            {
                winningCow = m_Cows[i];
                return;
            }

        }
    }



    #region AI

    private void SetupAI()
    {
        GameObject playerCow = m_Cows[0].m_Instance;
        GameObject aiCow = m_Cows[1].m_Instance;

        //Set target of rotation to the player cow.
        aiCow.GetComponent<AIRotation>().targetTransform = playerCow.transform;

        //create a path from 2 points. The top y position and the bottom y position.
        //----------------------------------------------
        AIMovement aiMovement = aiCow.GetComponent<AIMovement>();

        Transform[] aiMovingPoints = new Transform[2];
        float xPoint = m_Cows[1].m_EndPoint.position.x;

        Transform maxYPoint = createAIBoundryTransform("AI_max_y_point", xPoint, m_Cows[1].stats.yMaxBounadry);
        Transform minYPoint = createAIBoundryTransform("AI_min_y_point", xPoint, m_Cows[1].stats.yMinBounadry);

        //the first aiMovingPoint will be the direction the cow goes in the beggininng. I don't want the cow to always go up or always go down
        //because that will make it predicatble. So I throw a coin and the cow either starts going up or down... 50 50. 
        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
        {
            aiMovingPoints[0] = maxYPoint;
            aiMovingPoints[1] = minYPoint;
        }
        else
        {
            aiMovingPoints[0] = minYPoint;
            aiMovingPoints[1] = maxYPoint;
        }
        aiMovement.Points = aiMovingPoints;

        SetAiLevel();
        //----------------------------------------------
    }
    private void SetAiLevel()
    {
        GameObject aiCow = m_Cows[1].m_Instance;
        CowType aiLevel = gameOptions.cowOptions[1].type;
        CowStats stats = aiCow.GetComponent<CowStats>();

        switch (aiLevel)
        {
            case CowType.AIEasy:
                stats.movementSpeed -= 7;
                stats.rotationSpeed -= 7;
                stats.fireDelay *= 2f;

                break;
            case CowType.AINormal:
                stats.movementSpeed -= 2;
                stats.rotationSpeed -= 2;
                stats.fireDelay *= 1.5f;
                break;
            case CowType.AIHard:
                stats.movementSpeed += 2;
                stats.rotationSpeed += 2;
                stats.fireDelay *= 0.75f;
                break;
            case CowType.AIInsane:
                stats.movementSpeed += 5;
                stats.rotationSpeed += 5;
                stats.fireDelay *= 0.5f;
                break;
            case CowType.Human:
            default:
                break;
        }
    }

    private Transform createAIBoundryTransform(string gameObjectName, float xBoundry, float yBoundry)
    {
        GameObject aiYPoint = new GameObject(gameObjectName);
        aiYPoint.transform.position = new Vector3(xBoundry, yBoundry, 0f);
        return aiYPoint.transform;
    }


    #endregion



}
