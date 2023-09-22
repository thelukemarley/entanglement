using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Player : Block
{
    bool isGrabbing = false;
    bool isClimbing = false;
    int[] grabDir;
    int[] grabLoc;
    int[] climbDir;
    int[] climbLoc;
    Block grabbedBlock;
    public bool entPlayerCantPush = false;

    public Player(int[] Location, WorldGrid Grid, int[] dimensions, BlockMono owner, bool isEnt = false) : base(Location, Grid, dimensions, owner, isEnt)
    { }

    public bool GetIsGrabbing() { return isGrabbing; }

    public void SetIsGrabbing(bool isGrabbing) { this.isGrabbing = isGrabbing; }

    public void SetIsClimbing(bool isClimbing) { this.isClimbing = isClimbing; }

    public int[] GetGrabDir() { return grabDir; }

    public int[] GetClimbDir() { return climbDir; }
    public int[] GetClimbLoc() { return climbLoc; }

    protected override bool BonusChecksPre()
    {
        if (GetDirMove()[1] != 0)
        {
            if (isClimbing && GetDirMove()[1] == -1) return false; //Don't make player fall when climbing
            return true; //Skip ground & handle checks for vertical movement
        }
        int[] inFrontLoc = (int[])GetDirMove().Clone();
        if (inFrontLoc[0] == 1) { inFrontLoc[0] = 2; inFrontLoc[2] = 1; }
        else if (inFrontLoc[2] == 1) { inFrontLoc[2] = 2; }
        else if (inFrontLoc[2] == -1) { inFrontLoc[0] = 1; }

        int[] checkLoc = new int[3] { inFrontLoc[0] + loc[0], inFrontLoc[1] + loc[1], inFrontLoc[2] + loc[2] };

        //Check for if you, in holding position but not holding something, move into something that can be held
        if (GetEntangledPair() != null && ((Player)GetEntangledPair()).GetIsGrabbing() && !isGrabbing)
        {
            int[] entGrabDir = (int[])((Player)GetEntangledPair()).GetGrabDir().Clone();
            if (grid.entReflects[0]) entGrabDir[0] *= -1;
            if (grid.entReflects[1]) entGrabDir[1] *= -1;
            if (grid.entReflects[2]) entGrabDir[2] *= -1;
            int[] entGrabLoc = (int[])entGrabDir.Clone();
            if (entGrabLoc[0] == 1) entGrabLoc[0] = 2;
            if (entGrabLoc[2] == 1) entGrabLoc[2] = 2;
            int[] myGrabLoc = new int[3] { entGrabLoc[0] + loc[0], entGrabLoc[1] + loc[1], entGrabLoc[2] + loc[2] };
            List<GridObj> holdCheckObjs = grid.GetObjects(myGrabLoc);
            foreach (GridObj gobj in holdCheckObjs)
                if (gobj is Block && ((Block)gobj).GetHasHandle())
                {
                    isGrabbing = true;
                    grabLoc = entGrabLoc;
                    grabDir = entGrabDir;
                    grabbedBlock = (Block)gobj;
                }
        }

        //Check for grabbing a handle
        List<GridObj> interactObjs = grid.GetObjects(checkLoc);
        foreach (GridObj gobj in interactObjs)
        {
            if (!isGrabbing && gobj is Block && ((Block)gobj).GetHasHandle())
            {
                isGrabbing = true;
                grabDir = (int[])GetDirMove().Clone();
                grabLoc = inFrontLoc;
                grabbedBlock = (Block)gobj;
                if (owner is PlayerMono) ((PlayerMono)owner).GrabHandle();
                else if (GetEntangledPair() != null && GetEntangledPair().GetOwner() is PlayerMono)
                    ((PlayerMono)GetEntangledPair().GetOwner()).GrabHandle(); //Ent pair grabbing

                //Case for if both ent and normal are grabbing different handles at the same time
                if (GetEntangledPair() != null && GetEntangledPair().GetDirMove() == null)
                {
                    int[] entDir = (int[])GetDirMove().Clone();
                    if (grid.entReflects[0]) entDir[0] *= -1;
                    if (grid.entReflects[1]) entDir[1] *= -1;
                    if (grid.entReflects[2]) entDir[2] *= -1;
                    if (!GetEntangledPair().TryMove(entDir)) return false;
                }

                //Case for letting go of one handle so ent can turn around and grab a different one
                if (GetEntangledPair() != null && ((Player)GetEntangledPair()).GetIsGrabbing())
                {
                    int[] entGrabDir = (int[])grabDir.Clone();
                    if (grid.entReflects[0]) entGrabDir[0] *= -1;
                    if (grid.entReflects[1]) entGrabDir[1] *= -1;
                    if (grid.entReflects[2]) entGrabDir[2] *= -1;
                    if (!((Player)GetEntangledPair()).GetGrabDir().SequenceEqual(entGrabDir)) ((Player)GetEntangledPair()).SetIsGrabbing(false);
                }

                return false;
            }
        }


        //Check for if handle has fallen/been displaced
        if (isGrabbing && !grid.GetObjects(new int[3] { grabLoc[0] + loc[0], grabLoc[1] + loc[1], grabLoc[2] + loc[2] }).Contains(grabbedBlock))
        {
            if (GetEntangledPair() == null || (GetEntangledPair() is Player && !((Player)GetEntangledPair()).GetIsGrabbing()))
            {
                if (owner is PlayerMono) ((PlayerMono)owner).UngrabHandle(); //Let go of handle
                else if (GetEntangledPair() != null && GetEntangledPair().GetOwner() is PlayerMono)
                    ((PlayerMono)GetEntangledPair().GetOwner()).UngrabHandle(); //Ent pair let go
            }
            isGrabbing = false;
        }

        //Check for climbing a ladder
        foreach (GridObj gobj in interactObjs)
        {
            if (gobj is Block && ((Block)gobj).GetHasLadder())
            {
                isClimbing = true;
                climbDir = (int[])GetDirMove().Clone();
                climbLoc = inFrontLoc;
                grid.StartClimb(this);
                return false;
            }
        }

        //Check for if you're currently holding a handle
        if (isGrabbing)
        {
            if (GetDirMove()[0] == -1 * grabDir[0] && GetDirMove()[2] == -1 * grabDir[2])
            {
                if (!grabbedBlock.TryMove(GetDirMove())) return false; //Pull a block
            }
            else if (!(GetDirMove()[0] == grabDir[0] && GetDirMove()[2] == grabDir[2]))
            {
                if (owner is PlayerMono) ((PlayerMono)owner).UngrabHandle(); //Let go of handle
                else if (GetEntangledPair() != null && GetEntangledPair().GetOwner() is PlayerMono)
                    ((PlayerMono)GetEntangledPair().GetOwner()).UngrabHandle(); //Ent pair let go
                isGrabbing = false;
                if (GetEntangledPair() != null && GetEntangledPair() is Player) ((Player)GetEntangledPair()).SetIsGrabbing(false);
                return false;
            }
        }
        return true;
    }


    protected override bool BonusChecksPost()
    {
        if (GetDirMove()[1] == -1 && isGrabbing) //Ungrab when falling
        {
            if (GetEntangledPair() == null || (GetEntangledPair() is Player && !((Player)GetEntangledPair()).GetIsGrabbing()))
            {
                if (owner is PlayerMono) ((PlayerMono)owner).UngrabHandle(); //Let go of handle
                else if (GetEntangledPair() != null && GetEntangledPair().GetOwner() is PlayerMono)
                    ((PlayerMono)GetEntangledPair().GetOwner()).UngrabHandle(); //Ent pair let go
            }
            isGrabbing = false;
        }
        if (GetDirMove()[1] != 0) return true; //Skip standing checks for vertical movement

        //Check what you're standing on
        bool standingGood = false;
        for (int i = 0; i < GetDim()[0]; i++)
            for (int j = 0; j < GetDim()[2]; j++)
                if (canWalk(new int[3] { loc[0] + i, loc[1] - 1, loc[2] + j })) standingGood = true;
        if (!standingGood)
        {
            //If one ent player can walk and one can't, let them walk
            if (GetEntangledPair() == null) return false;
            if (entPlayerCantPush == true) return false;
            ((Player)GetEntangledPair()).entPlayerCantPush = true;
        }

        return true;
    }

    protected override void BonusFinishes()
    {
        entPlayerCantPush = false;
    }

    bool canWalk(int[] standingOnLoc)
    {
        List<GridObj> standingOnObjs = grid.GetObjects(standingOnLoc);
        bool isStandingOnSomething = false;
        foreach (GridObj gobj in standingOnObjs)
        {
            //Check if you're on the block you're trying to push
            if (gobj is Block && ((Block)gobj).GetDirMove() != null && GetDirMove().SequenceEqual(((Block)gobj).GetDirMove()))
            {
                return false;
            }
            if (gobj is Wall || gobj is Block) isStandingOnSomething = true;
        }
        //Check if you're standing on anything
        if (!isStandingOnSomething && GetEntangledPair() != null && !isClimbing)
        {
            return false;
        }
        return true;
    }

    public override void DeleteSelf()
    {
        if (!(owner is PlayerMono) && GetEntangledPair() != null && GetEntangledPair() is Player && GetEntangledPair().GetOwner() is PlayerMono && //All just checks to make sure it doesnt crash
            isGrabbing && !((Player)GetEntangledPair()).GetIsGrabbing()) //If ent is grabbing and non ent isnt, on unentangle
            ((PlayerMono)GetEntangledPair().GetOwner()).UngrabHandle(); //og person let go
        base.DeleteSelf();
    }
}
