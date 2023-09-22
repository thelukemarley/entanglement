using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class GridObjMono : MonoBehaviour
{
    public GameObject gridPrefab;
    protected WorldGrid gridRef;
    private float tileX = 1;
    private float tileZ = 1;
    Mesh mesh;
    private Material mat;
    // Start is called before the first frame update
    protected void Begin()
    {
        GameObject gridCheck = GameObject.FindWithTag("WorldGrid");
        if (gridCheck == null) gridCheck = Instantiate(gridPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        GridMono myMono = gridCheck.gameObject.gameObject.GetComponent<GridMono>();
        gridRef = myMono.GetGrid();
        mat = GetComponent<Renderer>().material;
        mesh = GetComponent<MeshFilter>().mesh;
        tileX = (float)(transform.localScale.x);
        tileZ = (float)(transform.localScale.z);
    }

    public virtual void DeleteSelf()
    {
        if (this != null && this.gameObject != null) Destroy(this.gameObject);
    }

    // Update is called once per frame
    protected void TexStretch(float scale)
    {
     mat.mainTextureScale = new Vector2(tileX * scale, tileZ * scale);
    }
}
