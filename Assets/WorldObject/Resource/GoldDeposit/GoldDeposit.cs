using UnityEngine;
using RTS;

public class GoldDeposit : Resource
{

    private int numBlocks;

    protected override void Start()
    {
        base.Start();
        numBlocks = GetComponentsInChildren<Gold>().Length;
        resourceType = ResourceType.Gold;
    }

    protected override void Update()
    {
        base.Update();
        float percentLeft = (float)amountLeft / (float)capacity;
        if (percentLeft < 0) percentLeft = 0;
        int numBlocksToShow = (int)(percentLeft * numBlocks);
        Gold[] blocks = GetComponentsInChildren<Gold>();
        if (numBlocksToShow >= 0 && numBlocksToShow < blocks.Length)
        {
            Gold[] sortedBlocks = new Gold[blocks.Length];
            //sort the list from highest to lowest
            foreach (Gold ore in blocks)
            {
                sortedBlocks[blocks.Length - int.Parse(ore.name)] = ore;
            }
            for (int i = numBlocksToShow; i < sortedBlocks.Length; i++)
            {
                sortedBlocks[i].GetComponent<Renderer>().enabled = false;
            }
            CalculateBounds();
        }
    }
}