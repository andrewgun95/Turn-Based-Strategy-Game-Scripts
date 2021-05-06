using System.Linq;
using Photon.Realtime;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Giving a proper namespace to your script prevents clashes with other assets and developers
namespace com.thekingdom.game
{
    public class GameLauncher : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        /// <summary>
        /// Event condition when game start to playing
        /// </summary>
        public const byte GameStartEventCode = 1;
        /// <summary>
        /// Event condition when one of the user is exiting the game
        /// </summary>
        public const byte GameExitEventCode = 2;

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 2;

        public GameObject playerPrefab;
        public Tile[] playerTiles;

        public GameManager gameManager;

        [HideInInspector]
        public GameObject localPlayerObject;

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>          
        string gameVersion = "1";

        private bool gameStart;

        void Awake()
        {
            // The loaded scene is the same for every connected player
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        // Start is called before the first frame update
        void Start()
        {
            Connect();
            gameStart = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (gameStart) {
                return;
            }

            int playerCount = PlayerRegister.GetCount();
            if (playerCount > (maxPlayersPerRoom - 1))
            {
                // Only raise event if host player
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("Match all players. Continue to starting the game ...");
                    // Send event to begin start game
                    PhotonNetwork.RaiseEvent(
                        GameStartEventCode,
                        null,
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable
                    );

                    gameStart = true;
                }
            }
            else
            {
                // TODO: Display status message waiting for another player
            }
        }

        #region Photon

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, connect this application instance to Photon Server
        /// </summary>
        public void Connect() // Menghubungkan ke Photon Server
        {
            if (PhotonNetwork.IsConnected)
            {
                // #Critical: Attempt joining a Random Room.
                // 1. Success - notified to OnJoinedRoom()
                // 2. Failed  - notified to OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical: Attempt connecting to Photon Server
                // 1. Success - notified to OnConnectedToMaster()
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Will override -> virtual method of 'OnConnectedToMaster'
        /// Different virtual method and abstract method ?
        /// // 1. Virtual method (late binding) - can have default implementations - it's okay if not implement in Derived Class
        /// // Late binding ? Used to implement run time polymorphism
        /// // 2. Abstract method - no implementations - implementations must declared in Derived Class
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connecting to the Server ...");
            PhotonNetwork.JoinRandomRoom();
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("Network has disconnected cause of '{0}'", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message) // Ketika tidak berhasil, bakal membuat Room baru
        {
            Debug.Log("No room was available, Creating a new room ...");

            // #Critical: Failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            // 1. Success - notified OnCreatedRoom() and OnJoinedRoom()
            // 2. Failed  - notified OnCreateRoomFailed()

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.CreateRoom(null, options, TypedLobby.Default);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Found a room. Try to joining the room ...");

            // Get player data
            string playerName  = PlayerPrefs.GetString("playerName");
            string playerColor = PlayerPrefs.GetString("playerColor");

            Debug.Log(string.Format("Load player prefabs : [playerName : {0}, playerColor : {1}]", playerName, playerColor));

            object[] playerInfo = new object[] {  
                playerName,
                playerColor
            };
            // Create our player object when joined to the room
            localPlayerObject = PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0, playerInfo);
        }

        public void DestroyLocalPlayer() {
            if (localPlayerObject != null)
            {
                PhotonView photonView = localPlayerObject.GetComponent<PhotonView>();

                int playerId = photonView.ViewID;

                Debug.Log(string.Format("Destroying local player with id : {0}", playerId));

                PhotonNetwork.Destroy(photonView);
            }
            else
            {
                Debug.Log(string.Format("Can't destroy local player. Local player never been instantiated"));
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            Debug.Log(string.Format("Received an event with code {0}", Convert.ToInt64(eventCode)));
            if (eventCode == GameStartEventCode)
            {
                Debug.Log(string.Format("Starting a game ... with {0} players : {1}", PlayerRegister.GetCount(), PlayerRegister.GetPlayerInfo()));

                // Start the game
                gameManager.StartGame();
            }
            if (eventCode == GameExitEventCode) {
                // Destroying all available players network
                PhotonNetwork.DestroyAll(false);

                // Unregistered all players
                PlayerRegister.DeleteAll();

                // Game is running, force to change into Main Menu
                SceneManager.LoadScene("EasyMainMenu/Scenes/MainMenu");

                // Disconnect server connection
                PhotonNetwork.Disconnect();
            }
        }

        #endregion
    }
}
