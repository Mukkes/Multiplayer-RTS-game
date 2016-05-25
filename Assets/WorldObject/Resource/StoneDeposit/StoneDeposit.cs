using UnityEngine;
using RTS;

public class StoneDeposit : Resource
{

    private int numBlocks;

    protected override void Start()
    {
        base.Start();
        numBlocks = GetComponentsInChildren<Stone>().Length;
        resourceType = ResourceType.Stone;
    }

    protected override void Update()
    {
        base.Update();
        float percentLeft = (float)amountLeft / (float)capacity;
        if (percentLeft < 0) percentLeft = 0;
        int numBlocksToShow = (int)(percentLeft * numBlocks);
        Stone[] blocks = GetComponentsInChildren<Stone>();
        if (numBlocksToShow >= 0 && numBlocksToShow < blocks.Length)
        {
            Stone[] sortedBlocks = new Stone[blocks.Length];
            //sort the list from highest to lowest
            foreach (Stone ore in blocks)
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