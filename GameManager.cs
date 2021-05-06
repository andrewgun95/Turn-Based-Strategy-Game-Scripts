using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour, IOnEventCallback
{
    /// <summary>
    /// Event condition when current player switched to the next player
    /// </summary>
    public const byte SwitchPlayerPositionEventCode = 3;

    /// <summary>
    /// Event condition when there is a player has reached to the end state
    /// </summary>
    public const byte PlayerReachEndStateEventCode = 4;

    public GameMap gameMap;

    public GameResult gameResult;

    public GameObject[] gameCards;

    public int playerBuilds;

    public int playerTurns;

    private int currentPlayer;

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Awake()
    {
        // Save player preferences
        PlayerPrefs.SetInt("playerBuilds", playerBuilds);
        PlayerPrefs.SetInt("playerTurns", playerTurns);

        // Start with player 0
        currentPlayer = 0;
        // Game is not running
        enabled = false;

        gameResult.Hide();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void LoadPlayer(Player player)
    {
        if (player != null)
        {
            int playerBuilds = PlayerPrefs.GetInt("playerBuilds");
            int playerTurns = PlayerPrefs.GetInt("playerTurns");

            Debug.Log(string.Format("Load player '{0}' [Turns = {1}, Max Builds = {2}]", player.playerName, playerTurns, playerBuilds));

            player.LoadPlayer(gameMap, playerBuilds, playerTurns);
            // Add card effect to players
            foreach (GameObject cardObject in gameCards)
            {
                PlayerEffect playerEffect = cardObject.GetComponent<PlayerEffect>();
                player.AddPlayerEffect(playerEffect);
            }
        }
        else
        {
            Debug.Log("Player can't be null");
        }
    }

    private void DisableAllPlayer()
    {
        List<GameObject> playerObjects = PlayerRegister.GetAllPlayer();
        for (int playerIndex = 0; playerIndex < playerObjects.Count; playerIndex++)
        {
            EnablePlayer(playerIndex, false);
        }
    }

    private void EnablePlayer(int playerIndex, bool status)
    {
        if (playerIndex > -1 && playerIndex < PlayerRegister.GetCount())
        {
            int playerId = GetPlayerId(playerIndex);

            GameObject playerObject = PlayerRegister.GetPlayer(playerId);
            Player player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                if (player.SetActive(status))
                {
                    Debug.Log(string.Format("{0} player \"{1}\" : ", status ? "Enable" : "Disable", player.GetId()));
                }
            }
            else
            {
                Debug.Log(string.Format("{0} is not player or has not attached to the player script", playerObject.name));
            }
        }
    }

    private int GetPlayerId(int playerIndex)
    {
        return Convert.ToInt32((playerIndex + 1) + "001");
    }

    private Player GetCurrentPlayer()
    {
        List<GameObject> playerObjects = PlayerRegister.GetAllPlayer();
        if (currentPlayer > -1 && currentPlayer < playerObjects.Count)
        {
            GameObject playerObject = playerObjects[currentPlayer];
            Player player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                return player;
            }
            else
            {
                Debug.Log(string.Format("Player object {0} is not a player", playerObject.name));
                return null;
            }
        }
        else
        {
            Debug.Log("Current player is not valid");
            return null;
        }
    }

    private bool SwitchToNextPlayer()
    {
        int playerCount = PlayerRegister.GetCount();
        if (currentPlayer < playerCount)
        {
            // Disable for the current player
            EnablePlayer(currentPlayer, false);
            int nextPlayer = (this.currentPlayer + 1) % playerCount;
            // Enable for the next player
            EnablePlayer(nextPlayer, true);
            // Set next player to the current player
            currentPlayer = nextPlayer;
            return true;
        }
        else
        {
            Debug.Log("Current player is not valid");
            return false;
        }
    }

    /// <summary>
    /// Update current player to initiate state into 'Build State'
    /// </summary>
    private void UpdatePlayer()
    {
        Player player = GetCurrentPlayer();
        // Set player next state to building
        int playerTurns = PlayerPrefs.GetInt("playerTurns", 3);
        if (player.playerBuilds < playerTurns)
        {
            // Player turns will be the leave buildings
            playerTurns = player.playerBuilds;
        }
        player.SetPlayerNextState(new BuildState(playerTurns));
    }

    /// <summary>
    /// Update all player to force state into 'End State'
    /// </summary>
    private void UpdateAllPlayers()
    {
        List<GameObject> playerObjects = PlayerRegister.GetAllPlayer();
        foreach (GameObject playerObject in playerObjects)
        {
            Player player = playerObject.GetComponent<Player>();
            if (!(player.playerState is EndState))
            {
                player.SetPlayerNextState(new EndState());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Put Game Logic in Here ...
    }

    public void StartGame()
    {
        // Start the game
        enabled = true;
        // Start from the current player
        EnablePlayer(currentPlayer, true);
    }

    public void EndGame()
    {
        // Disable all players
        DisableAllPlayer();
        // End the game
        enabled = false;
    }

    #region Photon Callbacks

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SwitchPlayerPositionEventCode)
        {
            bool status = SwitchToNextPlayer();
            if (status)
            {
                Debug.Log(string.Format("Successfully switch to the next player [{0}]", currentPlayer));

                // After switching, update for the current player
                UpdatePlayer();

                string playerInfo = string.Join(",",
                       PlayerRegister.GetStatuses()
                       .Select(element => "[" + element.Key + "," + element.Value + "]")
                   );
                Debug.Log(string.Format("Players : {0}", playerInfo));
            }
            else
            {
                Debug.Log("Failed switch to the next player");
            }
        }
        else
        if (eventCode == PlayerReachEndStateEventCode)
        {
            Debug.Log(string.Format("Reach the end state"));
            // After player reach end, update for all players
            UpdateAllPlayers();

            // Calculating the final score
            List<GameObject> playerObjects = PlayerRegister.GetAllPlayer();
            gameResult.LoadResult(gameCards, playerObjects);
            gameResult.Show();

            // End the game
            EndGame();
        }
        else
        {
            Debug.Log(string.Format("Unhandled event with code {0}", eventCode));
        }
    }

    #endregion

}
