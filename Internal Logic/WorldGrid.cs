using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

public class WorldGrid
{
    List<GridObj>[,,] gridObjects;
    int[] worldspace;
    int[] offset;
    public List<Block> toUpdate;
    List<Block> allBlocks;
    List<GridObj> outOfBounds;

    List<Block> entBlocks;
    bool isEntangled = false;
    public Block entanglerOne;
    public Block entanglerTwo;
    public bool[] entReflects = new bool[3] { false, false, false };
    public int[] entOffsets = new int[3] { 0, 0, 0 };

    public List<int[]> buttonLocations;
    public int[] playerLocation;
    public int[] winStatePLoc;
    bool lightUpButton = false;

    bool doingGravity;
    List<Block> gravityBlocks;
    bool climbingLadder;
    Player climber = null;



    public WorldGrid(Vector3 worldsize)
    {
        worldspace = new int[3] { (int)worldsize.X, (int)worldsize.Y, (int)worldsize.Z };
        gridObjects = new List<GridObj>[worldspace[0], worldspace[1], worldspace[2]];
        offset = new int[3] { worldspace[0] / 2, worldspace[1] / 2, worldspace[2] / 2 };
        toUpdate = new List<Block>();
        entBlocks = new List<Block>();
        outOfBounds = new List<GridObj>();
        doingGravity = false;
        allBlocks = new List<Block>();
        buttonLocations = new List<int[]>();
        outOfBounds.Add(new Wall(new int[3] { 0, 0, 0 }, this, new int[3] { 0, 0, 0 }));
    }

    public void SetPlayerLocation(int[] playerLocation)
    {
        this.playerLocation = playerLocation;
    }

    public void SetAllBlocks(List<Block> blocks)
    {
        allBlocks = blocks;
    }

    public bool IsEntangled() { return isEntangled; }

    public void AddButton(Button toAdd, int[] location, bool isPlayerButton)
    {
        if (isPlayerButton) winStatePLoc = location;
        else buttonLocations.Add(location);
    }

    public void StartClimb(Player climber)
    {
        this.climber = climber;
        climbingLadder = true;
    }

    public bool IsClimbingLadder() { return climbingLadder; }

    public Player FinishClimb()
    {
        if (climber == null) return null;
        Player myReturn = climber;
        climber = null;
        return myReturn;
    }

    public bool WinCondition()
    {
        if (buttonLocations.Count == 0) return false;
        foreach (int[] loc in buttonLocations)
            if (GetObjects(loc).Count == 0) { lightUpButton = false; return false; }
        lightUpButton = true;
        if (!playerLocation.SequenceEqual(winStatePLoc)) return false;
        return true;
    }

    public bool DoLightUpButton() { return lightUpButton; }

    public void AddEntBlock(Block toAdd)
    {
        entBlocks.Add(toAdd);
    }

    public bool MoveEntBlock(bool isMainEnt, int[] dir)
    {
        int[] flippedDir = (int[])dir.Clone();
        flippedDir[0] *= -1;
        flippedDir[1] *= -1;
        flippedDir[2] *= -1;
        if (!isEntangled) return true;
        foreach (Block block in entBlocks)
        {
            if (isMainEnt)
            {
                int lastIndex = toUpdate.Count;
                if (!block.TryMove(dir, true))
                {
                    ToUpdatePartialClear(lastIndex);
                    if (dir[0] != 0)
                    {
                        if (entReflects[0])
                        {
                            if (!block.GetEntangledPair().TryMove(dir, true)) return false;
                        }
                        else { if (!block.GetEntangledPair().TryMove(flippedDir, true)) return false; }
                    } else if (dir[1] != 0)
                    {
                        if (entReflects[1])
                        {
                            if (!block.GetEntangledPair().TryMove(dir, true)) return false;
                        }
                        else { if (!block.GetEntangledPair().TryMove(flippedDir, true)) return false; }
                    } else if (dir[2] != 0)
                    {
                        if (entReflects[2])
                        {
                            if (!block.GetEntangledPair().TryMove(dir, true)) return false;
                        }
                        else { if (!block.GetEntangledPair().TryMove(flippedDir, true)) return false; }
                    }
                }
            } else
            {
                int lastIndex = toUpdate.Count;
                if (!block.GetEntangledPair().TryMove(dir, true))
                {
                    ToUpdatePartialClear(lastIndex);
                    if (dir[0] != 0)
                    {
                        if (entReflects[0])
                        {
                            if (!block.TryMove(dir, true)) return false;
                        }
                        else { if (!block.TryMove(flippedDir, true)) return false; }
                    }
                    else if (dir[1] != 0)
                    {
                        if (entReflects[1])
                        {
                            if (!block.TryMove(dir, true)) return false;
                        }
                        else { if (!block.TryMove(flippedDir, true)) return false; }
                    }
                    else if (dir[2] != 0)
                    {
                        if (entReflects[2])
                        {
                            if (!block.TryMove(dir, true)) return false;
                        }
                        else { if (!block.TryMove(flippedDir, true)) return false; }
                    }
                }
            }
        }
        return true;
    }

    public void Entangle()
    {
        if (!isEntangled)
        {
            if (entanglerOne != null && entanglerTwo != null)
            {
                if (entReflects[0]) entOffsets[0] = entanglerTwo.GetLoc()[0] + (entanglerOne.GetLoc()[0] - entanglerTwo.GetLoc()[0]) / 2 + 1;
                else entOffsets[0] = entanglerTwo.GetLoc()[0] - entanglerOne.GetLoc()[0];
                if (entReflects[1]) entOffsets[1] = entanglerTwo.GetLoc()[1] + (entanglerOne.GetLoc()[1] - entanglerTwo.GetLoc()[1]) / 2 + 1;
                else entOffsets[1] = entanglerTwo.GetLoc()[1] - entanglerOne.GetLoc()[1];
                if (entReflects[2]) entOffsets[2] = entanglerTwo.GetLoc()[2] + (entanglerOne.GetLoc()[2] - entanglerTwo.GetLoc()[2]) / 2 + 1;
                else entOffsets[2] = entanglerTwo.GetLoc()[2] - entanglerOne.GetLoc()[2];
                foreach (Block block in entBlocks)
                {
                    int[] offset = new int[3] { 0, 0, 0 };
                    if (entReflects[0]) offset[0] = -2 * (block.GetLoc()[0] - entOffsets[0]) - (block.GetDim()[0] - 1) - 1;
                    else offset[0] = entOffsets[0];
                    if (entReflects[1]) offset[1] = -2 * (block.GetLoc()[1] - entOffsets[1]) - (block.GetDim()[1] - 1) - 1;
                    else offset[1] = entOffsets[1];
                    if (entReflects[2]) offset[2] = -2 * (block.GetLoc()[2] - entOffsets[2]) - (block.GetDim()[2] - 1) - 1;
                    else offset[2] = entOffsets[2];
                    Block newEnt = block.Dupe(offset);
                    newEnt.SetEntangledPair(block);
                    block.SetEntangledPair(newEnt);
                }
                isEntangled = true;
            }
        }
        else
        {
            foreach (Block block in entBlocks)
            {
                block.GetEntangledPair().DeleteSelf();
                block.SetEntangledPair(null);
            }
            List<Block> gravCheck = allBlocks;
            if (Gravity(gravCheck))
            {
                doingGravity = true;
            }
            else toUpdate = new List<Block>();
            isEntangled = false;
        }
    }

    public List<GridObj> GetObjects(int[] loc)
    {
        if (loc[0] + offset[0] >= worldspace[0] || loc[0] + offset[0] < 0
            || loc[1] + offset[1] >= worldspace[1] || loc[1] + offset[1] < 0
            || loc[2] + offset[2] >= worldspace[2] || loc[2] + offset[2] < 0) //If out of bounds
        {
            return outOfBounds;
        }

        List<GridObj> myList = gridObjects[loc[0] + offset[0], loc[1] + offset[1], loc[2] + offset[2]];
        if (myList == null)
        {
            gridObjects[loc[0] + offset[0], loc[1] + offset[1], loc[2] + offset[2]] = new List<GridObj>();
            myList = gridObjects[loc[0] + offset[0], loc[1] + offset[1], loc[2] + offset[2]];
        }
        return myList;
    }

    public void AddObj(GridObj toAdd, int[] loc)
    {
        //if (toAdd is Block) allBlocks.Add((Block)toAdd);
        GetObjects(loc).Add(toAdd);
    }

    public void RemoveObj(GridObj obj, int[] loc)
    {
        GetObjects(loc).Remove(obj);
    }

    public bool IsDoingGravity()
    {
        return doingGravity;
    }

    public void CallGravity()
    {
        if (!Gravity(gravityBlocks))
        {
            doingGravity = false;
        }
        else doingGravity = true;
    }

    public bool TryMove(int[] loc, int[] dir, bool doGravity = true)
    {
        if (doingGravity) return false; //Precaution
        List<GridObj> myObjs = GetObjects(loc);
        if (myObjs.Count == 0) return true;
        toUpdate = new List<Block>();
        canceledBlocks = new List<Block>();

        bool success = myObjs[0].TryMove(dir);
        foreach (Block block in toUpdate)
        {
            if (success) block.CompleteMove();
            else block.CancelMove();
        }
        toUpdate.AddRange(canceledBlocks);
        if (success && doGravity && Gravity(toUpdate)) doingGravity = true;
        canceledBlocks = null;
        toUpdate = new List<Block>();
        return success;
    }

    List<Block> canceledBlocks;
    public void ToUpdatePartialClear(int index)
    {
        for (int i = index; i < toUpdate.Count; i++)
        {
            toUpdate[i].CancelMove();
        }
        if (canceledBlocks != null) canceledBlocks.AddRange(toUpdate.GetRange(index, toUpdate.Count - index));
        toUpdate.RemoveRange(index, toUpdate.Count - index);
    }

    public bool Gravity(List<Block> checkedBlocks)
    {
        if (isEntangled && entReflects[1]) return FlippedGravity(allBlocks);
        toUpdate = new List<Block>();
        foreach (Block block in checkedBlocks)
        {
            int lastIndex = toUpdate.Count;
            if (!block.TryMove(new int[3] { 0, -1, 0 })) ToUpdatePartialClear(lastIndex);
            if (block.GetLoc()[1] == -offset[1]) //Delete if at bottom of world
            {
                ToUpdatePartialClear(lastIndex);
                if (block.GetEntangledPair() != null) block.GetEntangledPair().DeleteSelf();
                block.DeleteSelf();
            }
        }
        gravityBlocks = toUpdate;
        foreach (Block block in toUpdate) 
            block.CompleteMove();
        toUpdate = new List<Block>();
        return (gravityBlocks.Count != 0);
    }

    public bool Ladder()
    {
        if (climber == null) return false;
        int[] ladderCheckLoc = new int[3] {climber.GetClimbLoc()[0] + climber.GetLoc()[0],
                                           climber.GetClimbLoc()[1] + climber.GetLoc()[1],
                                           climber.GetClimbLoc()[2] + climber.GetLoc()[2]};
        List<GridObj> myObjs = GetObjects(ladderCheckLoc);
        bool ladderRunOut = true;
        if (!(myObjs.Count == 0))
            foreach (GridObj myObj in myObjs)
                if (myObj is Block && ((Block)myObj).GetHasLadder())
                {
                    ladderRunOut = false;
                    break;
                }
        if (ladderRunOut) {
            if (gravityBlocks == null) gravityBlocks = new List<Block>();
            gravityBlocks.Add(climber);
            climbingLadder = false; 
            return false; 
        }

        bool success = climber.TryMove(new int[3] { 0, 1, 0 });
        if (gravityBlocks == null) gravityBlocks = toUpdate;
        else gravityBlocks.AddRange(toUpdate);

        if (success)
        {
            foreach (Block block in toUpdate) block.CompleteMove();
            toUpdate = new List<Block>();
            return true;
        }
        else
        {
            foreach (Block block in toUpdate) block.CancelMove();
            toUpdate = new List<Block>();
            return false;
        }
    }


    public bool FlippedGravity(List<Block> checkedBlocks)
    {
        /*toUpdate = new List<Block>();
        foreach (Block block in checkedBlocks)
        {
            int lastIndex = toUpdate.Count;
            if (!block.TryMove(new int[3] { 0, -1, 0 }, true)) ToUpdatePartialClear(lastIndex);
        }
        foreach (Block block in checkedBlocks) block.CancelMove();
        List<Block> allPossibleFallingBlocks = toUpdate;

        toUpdate = new List<Block>();
        foreach (Block block in checkedBlocks)
        {
            int lastIndex = toUpdate.Count;
            if (!block.TryMove(new int[3] { 0, -1, 0 }, true, true)) ToUpdatePartialClear(lastIndex);
        }
        foreach (Block block in checkedBlocks) block.CancelMove();
        List<Block> alwaysFallingBlocks = toUpdate;

        foreach (Block block in entBlocks)
        {
            toUpdate = new List<Block>();
            bool success = block.TryMove(new int[3] { 0, 1, 0 }, true);
            List<Block> 
        }
        //block.CompleteMove();*/
        return false;
    }
}
