using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildState : PlayerState
{
    private int buildMax;
    private string buildArea;
    private List<Vector3Int> buildCommit;
    private int buildCount;

    private bool adjacent;

    public BuildState(int buildMax) : this(buildMax, "All", new List<Vector3Int>())
    {
    }

    public BuildState(int buildMax, string buildArea, List<Vector3Int> buildCommit) {
        this.buildMax = buildMax;
        this.buildArea = buildArea;
        this.buildCommit = new List<Vector3Int>(buildCommit);

        buildCount = 0;
        adjacent = false;
    }

    public void Enter(Player player) {
        Debug.Log(string.Format("Entering \"{0}\" state", this.GetType().Name));
        
        if (!string.IsNullOrEmpty(buildArea)) {
            Debug.Log(string.Format(""));
            player.playerMap.SetAllowedAreas(buildArea);
        }
    }

    public void Exit(Player player) {
        Debug.Log(string.Format("Exiting \"{0}\" state", this.GetType().Name));
    }

    public PlayerState HandleInput(Player player) { // Visitor pattern
        if (Input.GetMouseButton(0))
        {
            // Set tile player
            BuildInfo info = player.playerMap.Build(player.GetId(), Input.mousePosition, adjacent);
            if (info != null)
            {
                buildCount++;
                buildCommit.Add(new Vector3Int(info.x, info.y, 0));

                // Begin start the building
                StartBuild(info, player);

                // Next building  will adjacent
                if (!adjacent) {
                    adjacent = true;
                }
                
                if (buildCount == buildMax) {
                    // End of building
                    return EndBuild(info, player);
                }
                Debug.Log(string.Format("Set a tile at area {0} in ({1}, {2})", info.area, info.x, info.y));
            }
        }
        return this;
    }

    private void StartBuild(BuildInfo info, Player player) {
        if (info.tileLocationsAround != null)
        {
            // Add effects to player after building
            foreach (Tilemap tileLocation in info.tileLocationsAround)
            {
                PlayerEffect playerEffect = tileLocation.GetComponent<PlayerEffect>();
                player.AddPlayerEffect(playerEffect);
            }
        }
        else {
            Debug.Log(string.Format("Player \"{0}\" build has no location tiles around", player.GetId()));
        }
    }

    private PlayerState EndBuild(BuildInfo info, Player player) {
        int i = 0;
        while (i < player.playerEffects.Count) {
            PlayerEffect playerEffect = player.playerEffects.ElementAt(i);
            if (playerEffect is GainExtraTurn) {
                GainExtraTurn gainExtraTurn = (GainExtraTurn) playerEffect;
                // Apply effect 'Gain Extra Turn' to player
                gainExtraTurn.Apply(player);
                player.playerEffects.RemoveAt(i);
                // Have another chance to builds
                return new BuildState(gainExtraTurn.amountTurn, gainExtraTurn.targetArea, buildCommit);
            }
            i++;
        }

        // No other effect applies
        return new ConfirmState(buildCommit);
    }

}
