using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class WallMono : GridObjMono
{
    Wall wallLogic;
    int[] dim = new int[3];
    int[] loc = new int[3];
    // Start is called before the first frame update
    void Start()
    {
        Begin();
        dim[0] = (int)System.Math.Round(transform.localScale.x);
        dim[1] = (int)System.Math.Round(transform.localScale.y);
        dim[2] = (int)System.Math.Round(transform.localScale.z);
        loc[0] = (int)System.Math.Round(0.5 - (double)dim[0] / 2 + transform.localPosition.x);
        loc[1] = (int)System.Math.Round(0.5 - (double)dim[1] / 2 + transform.localPosition.y);
        loc[2] = (int)System.Math.Round(0.5 - (double)dim[2] / 2 + transform.localPosition.z);
        wallLogic = new Wall(loc, gridRef, dim);
    }

    // Update is called once per frame
    void Update()
    {
        //TexStretch(0.125f);
    }
}
