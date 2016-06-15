public class WarFactory : Building
{
	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start ()
	{
		base.Start();
		actions = new string[] { "Panzer I", "Panzer II", "Panzer III", "Panzer IV" };
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
