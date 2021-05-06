using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerState
{
    void Enter(Player player);

    void Exit(Player player);

    // Handle action for this state and return the next state 
    PlayerState HandleInput(Player player);

}
