using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

public class Block : GridObj
{
    Block entangledPair;
    bool isTryingToMove;
    int[] dirMove;
    int[] dim;
    protected BlockMono owner;
    bool hasHandle;
    bool hasLadder;

    public Block (int[] Location, WorldGrid Grid, int[] dimensions, BlockMono owner, bool isEnt = false, bool hasHandle=false, bool hasLadder = false) : base(Location, Grid)
    {
        isTryingToMove = false;
        dirMove = null;
        dim = dimensions;
        this.hasHandle = hasHandle;
        this.hasLadder = hasLadder;
        this.owner = owner;
        for(int i = 0; i<dimensions[0]; i++)
            for (int j = 0; j < dimensions[1]; j++)
                for (int k = 0; k < dimensions[2]; k++)
                    grid.AddObj(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k });
        if (isEnt) grid.AddEntBlock(this);
    }

    public int[] GetDirMove() { return dirMove; }
    public BlockMono GetOwner() { return owner; }
    public bool GetHasHandle() { return hasHandle; }
    public bool GetHasLadder() { return hasLadder; }

    public void SetEntangledPair(Block entangledPair)
    {
        this.entangledPair = entangledPair;
    }
    public Block GetEntangledPair()
    {
        return entangledPair;
    }
    public Block Dupe(int[] newLocOffset)
    {
        return owner.Dupe(newLocOffset);
    }
    public int[] GetDim()
    {
        return dim;
    }

    public virtual void DeleteSelf()
    {
        for (int i = 0; i < dim[0]; i++)
            for (int j = 0; j < dim[1]; j++)
                for (int k = 0; k < dim[2]; k++)
                    grid.RemoveObj(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k });
        /*if (entangledPair != null) {
            entangledPair.SetEntangledPair(null);
            entangledPair.DeleteSelf();
        }*/
        owner.DeleteSelf();
    }

    public bool Move(int[] dir) //Called by monos
    {
        return grid.TryMove(loc, dir);
    }

    public bool MoveNoGrav(int[] dir) //Called by monos
    {
        return grid.TryMove(loc, dir, false);
    }

    protected virtual bool BonusChecksPre() { return true; }
    protected virtual bool BonusChecksPost() { return true; }
    protected virtual void BonusFinishes() { }


    public override bool TryMove(int[] dir, bool skipEntCheck = false, bool skipTopCheck = false) //Only called by WorldGrid & other blocks
    {
        if (isTryingToMove)
        {
            if (!dir.SequenceEqual(dirMove)) return false;
            else return true;
        }
        isTryingToMove = true;
        dirMove = dir;
        grid.toUpdate.Add(this);

        if (!BonusChecksPre()) return false; //Extra checks for player
        if (entangledPair != null && !skipEntCheck)
        {
            int[] entDir = (int[])dir.Clone();
            if (grid.entReflects[0]) entDir[0] *= -1;
            if (grid.entReflects[1]) entDir[1] *= -1;
            if (grid.entReflects[2]) entDir[2] *= -1;
            if (!entangledPair.TryMove(entDir)) return false; //Check entangled
        }
        for (int i = 0; i < dim[0]; i++)
            for (int j = 0; j < dim[1]; j++)
                for (int k = 0; k < dim[2]; k++)
                {
                    int[] currentloc = new int[3] { loc[0] + i, loc[1] + j, loc[2] + k };
                    List<GridObj> objs = grid.GetObjects(currentloc);
                    foreach (GridObj obj in objs)
                        if (!obj.TryMove(dir)) return false; //Check overlapping

                    objs = grid.GetObjects(new int[3] { currentloc[0] + dir[0], currentloc[1] + dir[1], currentloc[2] + dir[2] });
                    foreach (GridObj obj in objs)
                        if (!obj.TryMove(dir)) return false; //Check in front of

                    objs = grid.GetObjects(new int[3] { currentloc[0] + dir[0]*2, currentloc[1] + dir[1]*2, currentloc[2] + dir[2]*2 });
                    foreach (GridObj obj in objs)
                    {
                        if (obj is Block)
                        {
                            Block castObj = (Block)obj;
                            if (castObj.GetDirMove() != null)
                            {
                                if (castObj.GetDirMove()[0] == dir[0] * -1
                                && castObj.GetDirMove()[1] == dir[1] * -1
                                && castObj.GetDirMove()[2] == dir[2] * -1) return false; //Check for collisions
                            }
                        }
                    }

                    if (j == 0 && !skipTopCheck)
                    {
                        objs = grid.GetObjects(new int[3] { currentloc[0], currentloc[1] + dim[1], currentloc[2] });
                        foreach (GridObj obj in objs)
                        {
                            int lastIndex = grid.toUpdate.Count;
                            if (!obj.TryMove(dir)) grid.ToUpdatePartialClear(lastIndex); //Check on top of
                        }
                    }
                }
        if (this == grid.entanglerOne)
        {
            if (!grid.MoveEntBlock(true, dir)) return false; //If entangler, check if can move
        }
        if (this == grid.entanglerTwo)
        {
            if (!grid.MoveEntBlock(false, dir)) return false;
        }
        if (!BonusChecksPost()) return false; //Extra checks for player
        return true;
    }

    public void CompleteMove() //Only called by WorldGrid
    {
        isTryingToMove = false;
        for (int i = 0; i < dim[0]; i++)
            for (int j = 0; j < dim[1]; j++)
                for (int k = 0; k < dim[2]; k++)
                    grid.RemoveObj(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k });
        loc = new int[3] { loc[0] + dirMove[0], loc[1] + dirMove[1], loc[2] + dirMove[2] };
        for (int i = 0; i < dim[0]; i++)
            for (int j = 0; j < dim[1]; j++)
                for (int k = 0; k < dim[2]; k++)
                    grid.AddObj(this, new int[3] { loc[0] + i, loc[1] + j, loc[2] + k });
        owner.Move(dirMove);
        dirMove = null;
        BonusFinishes();
    }

    public void CancelMove() //Only called by WorldGrid
    {
        isTryingToMove = false;
        dirMove = null;
        BonusFinishes();
    }
}
