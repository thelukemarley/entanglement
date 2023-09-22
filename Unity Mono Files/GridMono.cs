using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridMono : MonoBehaviour
{
    WorldGrid grid;
    public GameObject firework;
    GameObject myFirework;
    float count = -1;
    public System.Numerics.Vector3 worldSize = new System.Numerics.Vector3(50, 50, 50);
    // Start is called before the first frame update
    void Start()
    {
        BlockMono[] blockMonos = FindObjectsOfType<BlockMono>();
        List<Block> blocks = new List<Block>();
        foreach (BlockMono blk in blockMonos) blocks.Add(blk.getBlockLogic());
        grid.SetAllBlocks(blocks);
    }

    private void OnEnable()
    {
        grid = new WorldGrid(worldSize);
    }

    ParticleSystem mySys = null;
    // Update is called once per frame
    void Update()
    {
       if (grid.WinCondition() && count==-1)
        {
            PlayerMono player = FindObjectOfType<PlayerMono>();
            myFirework = Instantiate(firework, player.gameObject.transform.position, Quaternion.identity);
            mySys = myFirework.gameObject.GetComponentInChildren<ParticleSystem>();
            mySys.time = 1.0f;
            count = 0;
        }
        if (count >= 0) count+= Time.deltaTime;
        if (count > 2 && myFirework != null) Destroy(myFirework);
        if (count > 3) {
            if (SceneManager.GetActiveScene().buildIndex + 1 ==SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(0);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
        }
    }

    public WorldGrid GetGrid()
    {
        return grid;
    }
}
