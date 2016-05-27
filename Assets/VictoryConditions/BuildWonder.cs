public class BuildWonder : VictoryCondition
{

	public override string GetDescription()
	{
		return "Building Wonder";
	}

	public override bool PlayerMeetsConditions(Player player)
	{
		Building wonder = player.GetComponentInChildren<Wonder>();
		return ((player) && 
				(!player.IsDead()) && 
				(wonder) && 
				(!wonder.UnderConstruction()) && 
				(!wonder.isTempBuilding));
	}
}