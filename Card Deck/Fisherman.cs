using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Fisherman : MonoBehaviour, PlayerEffect, Card
{
    public string tagName;

    private Dictionary<int, int> resultGolds;

    // Start is called before the first frame update
    void Start()
    {
        resultGolds = new Dictionary<int, int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Apply(Player player) {
        List<BuildInfo> buildSettlements = player.GetSettlements();
        foreach (BuildInfo settlement in buildSettlements) {
            if (settlement.adjacentBlockAreas.ContainsKey("Water") && settlement.adjacentBlockAreas["Water"]) {
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

    public string GetTagName() {
        return tagName;
    }

    public int GetGold(Player player) {
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
}
