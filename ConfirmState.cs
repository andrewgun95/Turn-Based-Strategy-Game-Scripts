using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmState : PlayerState
{

    private List<Vector3Int> buildCommit;

    public ConfirmState(List<Vector3Int> buildCommit) {
        this.buildCommit = buildCommit;
    }

    public void Enter(Player player)
    {
        GenericDialog dialog = GenericDialog.Instance();
        dialog.SetTitle(string.Format("Good job !, {0}", player.playerName));
        dialog.SetMessage("You must confirm your turn");
       
        dialog.SetOnAccept("Yes", () => {
            int resultBuilds = DecreasePlayerBuilds(player.playerBuilds, buildCommit.Count);
            
            // RPC call to Update Player
            PhotonView target = player.photonView;
            target.RPC("UpdatePlayer", RpcTarget.All, resultBuilds);

            if (resultBuilds == 0)
            {
                Debug.Log(string.Format("Player \"{0}\" finish the game", player.GetId()));

                // Set into "End State" to End Game
                player.SetPlayerNextState(new EndState());
                // Publish event to End Game
                PhotonNetwork.RaiseEvent(
                  GameManager.PlayerReachEndStateEventCode,
                  null,
                  new RaiseEventOptions { Receivers = ReceiverGroup.All },
                  SendOptions.SendReliable
              );
            }
            else
            {
                Debug.Log(string.Format("Player \"{0}\" turn is over ...", player.GetId()));

                // Get player info
                object[] playerInfo = new object[] { player.GetId(), player.playerName, player.playerBuilds };
                // Publish event to Switch Player
                PhotonNetwork.RaiseEvent(
                    GameManager.SwitchPlayerPositionEventCode,
                    playerInfo,
                    new RaiseEventOptions { Receivers = ReceiverGroup.All },
                    SendOptions.SendReliable
                );
            }

            dialog.Hide();
        });

        dialog.SetOnDecline("No", () => {
            // Flat positions into x's and y's of build commits
            FlatPositions(buildCommit, out int[] x, out int[] y);

            // RPC call to Remove Settlements
            PhotonView target = player.playerMap.photonView;
            target.RPC("RemoveSettlements", RpcTarget.All, x, y);

            // Return back to Build State
            int playerTurns = PlayerPrefs.GetInt("playerTurns");
            player.SetPlayerNextState(new BuildState(playerTurns));

            dialog.Hide();
        });

        dialog.Show();
    }

    private void FlatPositions(List<Vector3Int> positions, out int[] x, out int[] y) {
        x = new int[positions.Count];
        y = new int[positions.Count];
        for (int i = 0; i < positions.Count; i++) {
            x[i] = positions[i].x;
            y[i] = positions[i].y;
        }
    }

    public int DecreasePlayerBuilds(int playerBuilds, int amount)
    {
        if ((playerBuilds - amount) > 0)
        {
            return playerBuilds - amount;
        }
        else
        {
            return 0;
        }
    }

    public void Exit(Player player)
    {
        
    }

    public PlayerState HandleInput(Player player)
    {
        return this;
    }


}
