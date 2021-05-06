using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Card
{
    string GetTagName();
    int GetGold(Player player);
}
