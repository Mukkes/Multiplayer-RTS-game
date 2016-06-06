using UnityEngine;
using System.Collections;

public class Barracks : Building
{
    protected override void Awake()
    {
        base.Awake();
        objectName = "Barracks";
        maxHitPoints = 100;
        cost = 250;
        sellValue = 125;
    }

    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Sniper", "ATU", "Troup" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
    }

    protected override bool ShouldMakeDecision()
    {
        return false;
    }
}
