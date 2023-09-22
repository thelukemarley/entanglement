using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class EntanglerMono : BlockMono
{
    public bool isMainEnt;
    public bool reflectX;
    public bool reflectY;
    public bool reflectZ;
    public Material entMat;
    public Material normalMat;
    public GameObject machine1;
    public GameObject machine2;
    public GameObject machine;
    bool updateEnt;
    // Start is called before the first frame update
    void Start()
    {
        Begin2();
        if (isMainEnt) gridRef.entanglerOne = blockLogic;
        else gridRef.entanglerTwo = blockLogic;
        if (reflectX) {gridRef.entReflects[0] = !gridRef.entReflects[0]; machine.transform.localScale = new Vector3(machine.transform.localScale.x*-1, machine.transform.localScale.y, machine.transform.localScale.z); }
        if (reflectY) {gridRef.entReflects[1] = !gridRef.entReflects[1]; machine.transform.localScale = new Vector3(machine.transform.localScale.x, machine.transform.localScale.y*-1, machine.transform.localScale.z); }
        if (reflectZ) {gridRef.entReflects[2] = !gridRef.entReflects[2]; machine.transform.localScale = new Vector3(machine.transform.localScale.x, machine.transform.localScale.y, machine.transform.localScale.z*-1); }
        updateEnt = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (gridRef == null) gridRef = GameObject.FindWithTag("WorldGrid").gameObject.gameObject.GetComponent<GridMono>().GetGrid(); ;
        if (updateEnt != gridRef.IsEntangled())
        {
            if (!gridRef.IsEntangled() && !isMainEnt)
            {
                machine1.GetComponent<Renderer>().material = normalMat;
                machine2.GetComponent<Renderer>().material = normalMat;
            }
            else
            {
                machine1.GetComponent<Renderer>().material = entMat;
                machine2.GetComponent<Renderer>().material = entMat;
            }
            updateEnt = gridRef.IsEntangled();
        }
    }
}
