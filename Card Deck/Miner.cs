using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class Miner : MonoBehaviour, PlayerEffect, Card
{
    public string tagName;

    private Dictionary<int, int> resultGolds;

    // Start is called before the first frame update
    void Start()
    {
        resultGolds = new Dictionary<int, int>();
    }

    public void Apply(Player player)
    {
        List<BuildInfo> buildSettlements = player.GetSettlements();
        foreach (BuildInfo settlement in buildSettlements)
        {
            List<Tilemap> tileLocations = settlement.tileLocationsAround;
            if (HasMountain(tileLocations))
            {
                int playerId = player.GetId();
                if (resultGolds.ContainsKey(playerId))
                {
                    resultGolds[playerId]++;
                }
                else
                {
                    resultGolds.Add(playerId, 1);
                }
            }
        }

        Debug.Log(string.Format("Applied card effect \"{0}\" for player \"{1}\" and got gold \"{2}\"", this.GetType().Name, player.GetId(), GetGold(player)));
    }

    private bool HasMountain(List<Tilemap> tileLocations)
    {
        foreach (Tilemap tilemap in tileLocations)
        {
            if (tilemap.name.Equals("Mountain")) return true;
        }
        // Has no mountain in tile locations
        return false;
    }

    public string GetTagName()
    {
        return tagName;
    }

    public int GetGold(Player player)
    {
        int playerId = player.GetId();
        if (resultGolds.ContainsKey(playerId))
        {
            return resultGolds[playerId];
        }
        else
        {
            return 0;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
