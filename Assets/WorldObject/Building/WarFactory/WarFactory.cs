public class WarFactory : Building
{
	protected override void Awake()
	{
		base.Awake();
		objectName = "WarFactory";
		maxHitPoints = 100;
		cost = 250;
		sellValue = 125;
	}

	protected override void Start ()
	{
		base.Start();
		actions = new string[] { "Tank", "ConvoyTruck", "Panzer I", "Panzer II", "Panzer III", "Panzer IV" };
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
