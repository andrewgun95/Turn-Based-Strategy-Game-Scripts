using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;
using com.thekingdom.game;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Player : MonoBehaviour, IPunInstantiateMagicCallback, IOnPhotonViewPreNetDestroy
{
    public PlayerState playerState { get; private set; }
    public GameMap playerMap { get; private set; }
    public List<PlayerEffect> playerEffects { get; private set; }
    public int playerBuilds { get; private set; }

    public Color playerColor;
    public string playerName;
    public Tile playerTile;
    [HideInInspector]
    public PhotonView photonView;
    
    private Text playerInfo;

    void Awake()
    {
        // Getting player info
        GameObject infoObject = transform.Find("Info").gameObject;
        if (infoObject != null)
        {
            playerInfo = infoObject.GetComponent<Text>();
        }

        // Getting photon view from player
        photonView = transform.GetComponent<PhotonView>();
        photonView.AddCallbackTarget(this);

        playerEffects = new List<PlayerEffect>();
    }

    void OnDestroy()
    {
        photonView.RemoveCallbackTarget(this);    
    }

    // Start is called before the first frame update
    void Start() 
    {
        RefreshPlayerStatus();
    }

    public void RefreshPlayerStatus()
    {
        RefreshPlayerFlag();
        RefreshPlayerName();
    }

    public void RefreshPlayerFlag()
    {
        GameObject flagObject = transform.Find("Flag").gameObject;
        if (flagObject != null)
        {
            Image imageField = flagObject.GetComponent<Image>();
            if (imageField != null && playerTile != null)
            {
                imageField.color = playerColor;
                Debug.Log(string.Format("Set player flag to \"{0}\"", playerColor.ToString()));
            }
        }
    }

    public void RefreshPlayerName()
    {
        GameObject nameObject = transform.Find("Name").gameObject;
        if (nameObject != null)
        {
            Text textField = nameObject.GetComponent<Text>();
            if (textField != null)
            {
                textField.text = playerName;
                Debug.Log(string.Format("Set player name to \"{0}\"", playerName.ToString()));
            }
        }
    }

    public void RefreshPlayerInfo()
    {
        if (playerInfo != null)
        {
            playerInfo.text = string.Format("Build Limits x {0}", playerBuilds);
        }
        else
        {
            Debug.Log(string.Format("Can't get player info"));
        }
    }

    public void LoadPlayer(GameMap playerMap, int playerBuilds, int playerTurns)
    {
        this.playerMap = playerMap;
        this.playerBuilds = playerBuilds;
        // Initate player state
        SetPlayerNextState(new BuildState(playerTurns));

        // Refresh player info
        RefreshPlayerInfo();
    }

    [PunRPC]
    public void UpdatePlayer(int playerBuilds, PhotonMessageInfo info) {
        Debug.LogFormat("Message Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);

        this.playerBuilds = playerBuilds;

        // Refresh player info
        RefreshPlayerInfo();
    }

    public void SetPlayerNextState(PlayerState nextState)
    {
        if (playerState != null)
        {
            playerState.Exit(this);
        }
        playerState = nextState;
        playerState.Enter(this);
    }

    public void AddPlayerEffect(PlayerEffect locationEffect)
    {
        this.playerEffects.Add(locationEffect);
    }

    public List<BuildInfo> GetSettlements()
    {
        return playerMap.GetSettlements(playerColor);
    }

    public void DecreasePlayerBuilds(int amount)
    {
        if ((playerBuilds - amount) > 0)
        {
            playerBuilds -= amount;
        }
        else
        {
            playerBuilds = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only update for this local player
        if (photonView.IsMine) {
            PlayerState nextState = playerState.HandleInput(this);
            if (nextState != null && nextState != playerState)
            {
                // Update player state
                SetPlayerNextState(nextState);

                Debug.Log(string.Format("Update player \"{0}\" from state '{1}' to state '{2}'", GetId(), playerState, nextState));
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        GameObject playerHUD = GameObject.Find("PlayerCanvas/HUD/PlayerHUD").gameObject;

        GameObject playerPanelObject = playerHUD.transform.Find("PlayerBorder").gameObject;
        transform.SetParent(playerPanelObject.transform, false);

        // Refresh player panel layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerPanelObject.GetComponent<RectTransform>());

        // Load player status
        object[] playerInfo = info.photonView.InstantiationData;
        playerName = (string) playerInfo[0];
        ColorUtility.TryParseHtmlString((string)playerInfo[1], out playerColor);

        // Refresh player status
        RefreshPlayerStatus();

        // Disable player
        SetActive(false);

        // Register as a new player
        PlayerRegister.Insert(gameObject);

        // Refresh player counts
        GameObject playerSizeObject = playerHUD.transform.Find("PlayerSize").gameObject;
        playerSizeObject.GetComponent<Text>().text = string.Format("Player Count : {0}", PlayerRegister.GetCount());
    }

    public void OnPreNetDestroy(PhotonView photonView)
    {
        // Send event to force exiting the game
        PhotonNetwork.RaiseEvent(
                    GameLauncher.GameExitEventCode,
                    null,
                    new RaiseEventOptions { Receivers = ReceiverGroup.All },
                    SendOptions.SendReliable
                );
    }

    public bool SetActive(bool status)
    {
        enabled = status;
        GameObject panelObject = transform.GetChild(0).gameObject;
        if (panelObject.name.Equals("Panel"))
        {
            // Enable / Disable outline for active player
            panelObject.SetActive(status);
            return true;
        }
        else
        {
            Debug.LogError("Can't get panel object from 0 element");
            return false;
        }
    }

    public int GetId()
    {
        return photonView.ViewID;
    }

}
