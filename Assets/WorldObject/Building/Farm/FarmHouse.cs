using RTS;
using UnityEngine;

public class FarmHouse : Building
{

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Farm" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateBuilding(actionToPerform);
    }

    private void CreateBuilding(string buildingName)
    {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        int worldObjectId = PlayerManager.GetUniqueWorldObjectId();
        if (player) player.CreateBuilding(worldObjectId, buildingName, buildPoint, null, playingArea); // This "null" should be unit.
    }

    protected override bool ShouldMakeDecision()
    {
        return false;
    }
}