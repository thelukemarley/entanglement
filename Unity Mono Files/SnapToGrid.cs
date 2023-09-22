using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    public bool splitsGrid = false;
    public bool snapOnY = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Snap();
    }

    private void Snap()
    {
        float[] snapValue = new float[3];
        if (System.Math.Round(transform.localScale.x) % 4 == 0) snapValue[0] = -0.5f;
        else if (System.Math.Round(transform.localScale.x) % 2 == 0) snapValue[0] = 0.5f; 
        else snapValue[0] = 0;
        if (System.Math.Round(transform.localScale.y) % 4 == 0) snapValue[1] = -0.5f;
        else if (System.Math.Round(transform.localScale.y) % 2 == 0) snapValue[1] = 0.5f; 
        else snapValue[1] = 0;
        if (System.Math.Round(transform.localScale.z) % 4 == 0) snapValue[2] = -0.5f;
        else if (System.Math.Round(transform.localScale.z) % 2 == 0) snapValue[2] = 0.5f; 
        else snapValue[2] = 0;
        int gs;
        if (splitsGrid) gs = 1;
        else gs = 2;
        var position = new Vector3(
            Mathf.RoundToInt((this.transform.position.x - snapValue[0]) / gs) * gs + snapValue[0],
            Mathf.RoundToInt((this.transform.position.y - snapValue[1]) / gs) * gs + snapValue[1],
            Mathf.RoundToInt((this.transform.position.z - snapValue[2]) / gs) * gs + snapValue[2]
        );
        if (!snapOnY) position[1] = this.transform.position.y;
        this.transform.position = position;
    }
}
