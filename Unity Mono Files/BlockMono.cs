using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMono : GridObjMono
{
    public bool isEnt = false;
    public bool hasLadder = false;
    public bool hasHandle = false;
    protected Block blockLogic;
    protected int[] dim = new int[3];
    protected int[] loc = new int[3];
    bool ranOnce = false;
    //int count = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (!ranOnce)
            Begin2();
    }
    public Block getBlockLogic() { return blockLogic; }
    public void Begin2(bool isPlayer = false)
    {
        Begin();
        dim[0] = (int)System.Math.Round(transform.localScale.x);
        dim[1] = (int)System.Math.Round(transform.localScale.y);
        dim[2] = (int)System.Math.Round(transform.localScale.z);
        loc[0] = (int)System.Math.Round(0.5 - (double)dim[0] / 2 + transform.localPosition.x);
        loc[1] = (int)System.Math.Round(0.5 - (double)dim[1] / 2 + transform.localPosition.y);
        loc[2] = (int)System.Math.Round(0.5 - (double)dim[2] / 2 + transform.localPosition.z);
        if (isPlayer) blockLogic = new Player(loc, gridRef, dim, this, isEnt);
        else blockLogic = new Block(loc, gridRef, dim, this, isEnt, hasHandle, hasLadder);
        transform.position = new Vector3(
            (float)(loc[0] + (double)dim[0] / 2 - 0.5),
            (float)(loc[1] + (double)dim[1] / 2 - 0.5),
            (float)(loc[2] + (double)dim[2] / 2 - 0.5));
        ranOnce = true;
    }

    public virtual Block Dupe(int[] newLocOffset)
    {
        BlockMono newB = Instantiate(this, new Vector3(
            (float)(loc[0] + newLocOffset[0] + (double)dim[0] / 2 - 0.5),
            (float)(loc[1] + newLocOffset[1] + (double)dim[1] / 2 - 0.5),
            (float)(loc[2] + newLocOffset[2] + (double)dim[2] / 2 - 0.5)), Quaternion.identity);
        newB.isEnt = false;
        newB.Begin2();
        if (gridRef.entReflects[0]) newB.gameObject.transform.localScale = 
                new Vector3(newB.gameObject.transform.localScale.x * -1, newB.gameObject.transform.localScale.y, newB.gameObject.transform.localScale.z);
        if (gridRef.entReflects[1]) newB.gameObject.transform.localScale = 
                new Vector3(newB.gameObject.transform.localScale.x, newB.gameObject.transform.localScale.y * -1, newB.gameObject.transform.localScale.z);
        if (gridRef.entReflects[2]) newB.gameObject.transform.localScale = 
                new Vector3(newB.gameObject.transform.localScale.x, newB.gameObject.transform.localScale.y, newB.gameObject.transform.localScale.z * -1);
        return newB.blockLogic;
    }

    public void Move(int[] dir)
    {
        transform.Translate(dir[0], dir[1], dir[2]);
        loc[0] += dir[0];
        loc[1] += dir[1];
        loc[2] += dir[2];
    }

    // Update is called once per frame
    void Update()
    {
        //count++;
        //if (count % 50 == 0) blockLogic.Move(new int[3] {0,0,1});
    }
}
