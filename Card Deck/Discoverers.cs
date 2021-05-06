using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Discoverers : MonoBehaviour, PlayerEffect, Card
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

        List<int> calculateY = new List<int>();
        foreach (BuildInfo settlement in buildSettlements)
        {
            if (!calculateY.Contains(settlement.y)) {
                int playerId = player.GetId();
                if (resultGolds.ContainsKey(playerId))
                {
                    resultGolds[playerId]++;
                }
                else
                {
                    resultGolds.Add(playerId, 1);
                }
                // Y axis already calculate, no need calculate anymore
                calculateY.Add(settlement.y);
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

    // Update is called once per frame
    void Update()
    {

    }

}
