using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : GridObj
{
    public Button(int[] Location, WorldGrid Grid, int[] dimensions, bool isPlayerButton) : base(Location, Grid)
    {
        if (isPlayerButton) grid.AddButton(this, new int[3] { loc[0], loc[1], loc[2]}, true);
        else
        {
            for (int i = 0; i < dimensions[0]; i++)
                for (int j = 0; j < dimensions[1]; j++)
                    for (int k = 0; k < dimensions[2]; k++)
                        grid.AddButton(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k }, false);
        }
    }
    public override bool TryMove(int[] dir, bool skipEntCheck, bool skipTopCheck)
    {
        return true;
    }
}
