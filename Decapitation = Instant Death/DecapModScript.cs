using UnityEngine;
using Photon;
using DrunkenWrestlers2.Gameplay;

public class DecapModScript : PunBehaviour {
    void Update()
    {
        foreach (Player player in GameplayManager.Players)
        {
            foreach (Bodypart bodypart in player.Bodyparts)
            {
                if (bodypart.ID == 0)
                {
                    if (bodypart.IsSevered)
                    {
                        player.Kill();
                    }
                }
            }
        }
    }
}