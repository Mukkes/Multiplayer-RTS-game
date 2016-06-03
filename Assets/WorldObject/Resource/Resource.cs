using RTS;

public class Resource : WorldObject
{

	//Public variables
	public float capacity;

	//Variables accessible by subclass
	public float amountLeft;
	protected ResourceType resourceType;

	/*** Game Engine methods, all can be overridden by subclass ***/

	protected override void Awake()
	{
		base.Awake();
		hitPoints = 0;
	}

	protected override void Start()
	{
		base.Start();
		resourceType = ResourceType.Unknown;
		amountLeft = capacity;
	}

	/*** Public methods ***/

	// Resource kunnen niet meer uitgeput raken omdat dit erg lastig was om voorelkaar te krijgen over het netwerk.
	public void Remove(float amount)
	{
		//amountLeft -= amount;
		//if (amountLeft < 0) amountLeft = 0;
	}

	public bool isEmpty()
	{
		return amountLeft <= 0;
	}

	public ResourceType GetResourceType()
	{
		return resourceType;
	}

	protected override void CalculateCurrentHealth(float lowSplit, float highSplit)
	{
		healthPercentage = amountLeft / capacity;
		healthStyle.normal.background = ResourceManager.GetResourceHealthBar(resourceType);
	}

	protected override bool ShouldMakeDecision()
	{
		return false;
	}
	
	public override void SetParent()
	{

	}
}