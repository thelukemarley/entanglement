using System.Numerics;

public class Wall : GridObj
{
    public Wall(int[] Location, WorldGrid Grid, int[] dimensions) : base(Location, Grid) 
    {
        for (int i = 0; i < dimensions[0]; i++)
            for (int j = 0; j < dimensions[1]; j++)
                for (int k = 0; k < dimensions[2]; k++)
                    grid.AddObj(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k });
    }
    public override bool TryMove(int[] dir, bool skipEntCheck, bool skipTopCheck)
    {
        return false;
    }
}