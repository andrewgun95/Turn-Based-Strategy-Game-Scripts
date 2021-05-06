using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Locate for all player registration
/// </summary>
public class PlayerRegister
{

    private static Dictionary<int, GameObject> playerDictionary = new Dictionary<int, GameObject>();

    public static void Insert(GameObject playerObject)
    {
        Player player = playerObject.GetComponent<Player>();
        if (player != null)
        {
            GameObject gameObject = GameObject.Find("GameManager").gameObject;
            GameManager gameManager = gameObject.GetComponent<GameManager>();
            if (gameManager != null)
            {
                // Load player in Game Manager
                gameManager.LoadPlayer(player);
            }
            else
            {
                Debug.LogError("Can't found any game manager");
            }

            playerDictionary.Add(player.GetId(), playerObject);
        }
        else
        {
            Debug.LogError(string.Format("Game object {0} is not a player object. Must add component Player to the object.", playerObject.name));
        }
    }

    public static void Delete(int playerId) {
        if (!playerDictionary.Remove(playerId))
        {
            Debug.LogError(string.Format("Can't found any player with registered id {0}", playerId));
        }
    }

    public static void DeleteAll() {
        playerDictionary.Clear();
    }

    public static List<GameObject> GetAllPlayer()
    {
        return playerDictionary.Values.ToList();
    }

    public static int GetCount()
    {
        return playerDictionary.Count;
    }

    public static Dictionary<int, bool> GetStatuses() {
        Dictionary<int, bool> statusDictionary = new Dictionary<int, bool>();
        foreach (KeyValuePair<int, GameObject> item in playerDictionary)
        {
            GameObject playerObject = item.Value;
            Player player = playerObject.GetComponent<Player>();
            if (player != null)
            {
                statusDictionary.Add(item.Key, player.enabled);
            }
        }
        return statusDictionary;    
    }

    public static string GetPlayerInfo() {
        string playerInfo = string.Join(",",
                        PlayerRegister.GetStatuses()
                        .Select(element => "[" + element.Key + "," + element.Value + "]")
                    );
        return playerInfo;
    }

    public static GameObject GetPlayer(int playerId)
    {
        if (playerDictionary.ContainsKey(playerId))
        {
            return playerDictionary[playerId];
        }
        else
        {
            return null;
        }
    }

}
