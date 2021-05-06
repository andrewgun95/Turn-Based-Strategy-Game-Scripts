using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EndState : PlayerState
{
    public void Enter(Player player) {
        // Apply effect 'Cards' to player
        int i = 0;
        while (i < player.playerEffects.Count)
        {
            PlayerEffect playerEffect = player.playerEffects.ElementAt(i);
            if (playerEffect is Card)
            {
                playerEffect.Apply(player);
            }
            i++;
        }
    }

    public void Exit(Player player) { 
    }

    // Handle action for this state and return the next state 
    public PlayerState HandleInput(Player player) {
        return this;
    }
}
