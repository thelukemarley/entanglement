using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DynamicTexture : MonoBehaviour
{

    private float tileX = 1;
    private float tileZ = 1;
    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().sharedMaterial;
        tileX = 0;
        tileZ = 0;
    }

    // Update is called once per frame
    void Update()
    {
        TexStretch(0.125f);
    }

    protected void TexStretch(float scale)
    {
        if (System.Math.Round(tileX) != System.Math.Round(transform.localScale.x) || System.Math.Round(tileZ) != System.Math.Round(transform.localScale.z))
        {
            tileX = (float)(transform.localScale.x);
            tileZ = (float)(transform.localScale.z);
            if (mat != null)
            {
                mat = new Material(mat);
                mat.mainTextureScale = new Vector2(tileX * scale, tileZ * scale);
                GetComponent<Renderer>().sharedMaterial = mat;
            }

        } else
        {
            tileX = (float)(transform.localScale.x);
            tileZ = (float)(transform.localScale.z);
        }
    }
}
