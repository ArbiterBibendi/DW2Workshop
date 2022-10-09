using UnityEngine;
using DrunkenWrestlers2.Gameplay;
using DrunkenWrestlers2.Networking;

public class HeavyHitsScript : PhysicsRules {

    public override HitData OnPlayerHit(HitData hit)
    {
        hit.force *= 5;
        return hit;
    }
    public override HitData OnObjectHit(HitData hit)
    {
        hit.damage *= 2;
        return hit;
    }

    private void Start()
    {
        if (Network.IsMasterClient)
        {
            RoomSettings.SetDismemberment(true);
        }
    }
}