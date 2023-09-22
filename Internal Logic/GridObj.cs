using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public abstract class GridObj
{
    protected int[] loc;
    protected WorldGrid grid;

    public GridObj (int[] Location, WorldGrid Grid)
    {
        loc = Location;
        grid = Grid;
    }

    public int[] GetLoc()
    {
        return loc;
    }
    public abstract bool TryMove(int[] dir, bool skipEntCheck, bool skipTopCheck);

    public bool TryMove(int[] dir)
    {
        return TryMove(dir, false, false);
    }

    public bool TryMove(int[] dir, bool skipEntCheck)
    {
        return TryMove(dir, skipEntCheck, false);
    }
}
