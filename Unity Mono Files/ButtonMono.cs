using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMono : GridObjMono
{
    public bool isPlayerButton;
    public GameObject button;
    public Material entMat;
    public Material normalMat;
    bool lightUp = false;
    Button buttonLogic;
    int[] dim = new int[3];
    int[] loc = new int[3];
    // Start is called before the first frame update
    void Start()
    {
        Begin();
        dim[0] = 2;
        dim[1] = 1;
        dim[2] = 2;
        loc[0] = (int)System.Math.Round(0.5 - (double)dim[0] / 2 + transform.localPosition.x);
        loc[1] = (int)System.Math.Round(0.5 - (double)dim[1] / 2 + transform.localPosition.y);
        loc[2] = (int)System.Math.Round(0.5 - (double)dim[2] / 2 + transform.localPosition.z);
        buttonLogic = new Button(loc, gridRef, dim, isPlayerButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerButton && lightUp != gridRef.DoLightUpButton())
        {
            if (lightUp)
            {
                button.GetComponent<Renderer>().material = normalMat;
                lightUp = false;
            } else
            {
                button.GetComponent<Renderer>().material = entMat;
                lightUp = true;
            }
        }
    }
}
