using UnityEngine;

public class Refinery : Building
{
	protected override void Awake()
	{
		base.Awake();
		objectName = "Refinery";
		maxHitPoints = 50;
		cost = 500;
		sellValue = 250;
	}

	protected override void Start()
	{
		base.Start();
		actions = new string[] { "Harvester" };
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