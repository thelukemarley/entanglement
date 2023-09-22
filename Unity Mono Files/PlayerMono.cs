using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMono : BlockMono
{
    public GameObject body;
    public GameObject entBody;
    public GameObject entPlayerPrefab;
    public float moveDelay;
    float count = 0;
    float gravityCount = 0;
    bool movedHorizLast;
    int twoStep = 0;
    float twoStepCount = 0;
    bool lockRot = false;
    bool allowOneRot = false;
    bool isBackstepping = false;
    // Start is called before the first frame update
    void Start()
    {
        Begin2(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Restart")) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (Input.GetButtonDown("Cancel")) SceneManager.LoadScene(0);
        if (gridRef.IsDoingGravity() || gridRef.IsClimbingLadder())
        {
            gravityCount+=Time.deltaTime;
            if (gravityCount >= moveDelay)
            {
                gravityCount = 0;
                if (gridRef.IsClimbingLadder() && !gridRef.Ladder()) { //If you're at the top of the ladder, move forward
                    Player climber = gridRef.FinishClimb();
                    int[] moveDir = climber.GetClimbDir();
                    if (climber != blockLogic) {
                        if (gridRef.entReflects[0]) moveDir[0] *= -1;
                        if (gridRef.entReflects[1]) moveDir[1] *= -1;
                        if (gridRef.entReflects[2]) moveDir[2] *= -1;
                    }
                    if (moveDir != null && moveDir[0] != 0)
                    { if (!PlayerMove(true, moveDir[0]))
                        {
                            twoStep = 0;
                            gridRef.CallGravity();
                        }
                    }
                    else
                    { if (!PlayerMove(false, moveDir[2]))
                        {
                            twoStep = 0;
                            gridRef.CallGravity();
                        }
                    }
                    climber.SetIsClimbing(false);
                } 
                else gridRef.CallGravity();
            }
            return;
        }
        if (twoStep == 0)
        {
            if (Input.GetButtonDown("Jump"))
            {
                gridRef.Entangle();
            }
            if (Input.GetAxisRaw("Vertical") != 0 && Input.GetAxisRaw("Horizontal") != 0)
            {
                if (count >= moveDelay)
                {
                    count = 0;
                    if (movedHorizLast)
                    {
                        if (!PlayerMove(false, Input.GetAxis("Vertical"))) twoStep = 0;
                    }
                    else
                    {
                        if (!PlayerMove(true, Input.GetAxis("Horizontal"))) twoStep = 0;
                    }
                }
                count += Time.deltaTime;
            }
            else if (Input.GetAxisRaw("Vertical") != 0)
            {
                movedHorizLast = false;
                if (count >= moveDelay) {
                    count = 0;
                    if (!PlayerMove(false, Input.GetAxis("Vertical"))) twoStep = 0; 
                }
                count += Time.deltaTime;
            }
            else if (Input.GetAxisRaw("Horizontal") != 0)
            {
                movedHorizLast = true;
                if (count >= moveDelay) {
                    count = 0;
                    if (!PlayerMove(true, Input.GetAxis("Horizontal"))) twoStep = 0; 
                }
                count += Time.deltaTime;
            }
            else count = moveDelay;
        } else
        {
            twoStepCount += Time.deltaTime;
            if (twoStepCount >= moveDelay)
            {
                twoStepCount = 0;
                if (twoStep == -1 || twoStep == -3)
                {
                    if (!PlayerMove(true, twoStep + 2))
                    {
                        if (isBackstepping)
                        {
                            PlayerMove(false, twoStep + 2);
                            if (twoStep == -1) twoStep -= 2;
                            else twoStep += 2;
                            bool test = PlayerMove(true, twoStep + 2);
                            isBackstepping = false;
                            twoStep = 0;
                        }
                        else
                        {
                            isBackstepping = true;
                            if (twoStep == -1) twoStep -= 2;
                            else twoStep += 2;
                        }
                    } else
                    {
                        isBackstepping = false;
                        twoStep = 0;
                    }
                } else
                {
                    if (!PlayerMove(false, twoStep + 3))
                    {
                        if (isBackstepping)
                        {
                            PlayerMove(true, twoStep + 3);
                            if (twoStep == -1) twoStep -= 2;
                            else twoStep += 2;
                            bool test = PlayerMove(false, twoStep + 3);
                            isBackstepping = false;
                            twoStep = 0;
                        }
                        else
                        {
                            isBackstepping = true;
                            if (twoStep == -2) twoStep -= 2;
                            else twoStep += 2;
                        }
                    }
                    else
                    {
                        isBackstepping = false;
                        twoStep = 0;
                    }
                }
            }
        }
    }

    bool PlayerMove(bool isHoriz, float amount)
    {
        bool success;
        int dir;
        if (amount > 0) dir = 1; else dir = -1;
        if (isHoriz)
        {
            if (twoStep == 0) success = blockLogic.MoveNoGrav(new int[3] { dir, 0, 0 }); //Fix later maybe for falling when in between grid
            else success = blockLogic.Move(new int[3] { dir, 0, 0 });
            twoStep = dir - 2;
        } else
        {
            if (twoStep == 0) success = blockLogic.MoveNoGrav(new int[3] { 0, 0, dir });
            else success = blockLogic.Move(new int[3] { 0, 0, dir });
            twoStep = dir - 3;
        }
        gridRef.SetPlayerLocation(blockLogic.GetLoc());

        if ((!lockRot || allowOneRot) && !isBackstepping) { 
            if (isHoriz && amount < 0) SetPlayerRotation(0.0f);
            if (isHoriz && amount > 0) SetPlayerRotation(180.0f);
            if (!isHoriz && amount < 0) SetPlayerRotation(-90.0f);
            if (!isHoriz && amount > 0) SetPlayerRotation(90.0f);
            allowOneRot = false;
        }
        return success;
    }

    public override void DeleteSelf()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public override Block Dupe(int[] newLocOffset)
    {
        GameObject newP = Instantiate(entPlayerPrefab, new Vector3(
            (float)(loc[0] + newLocOffset[0] + (double)dim[0] / 2 - 0.5),
            (float)(loc[1] + newLocOffset[1] + (double)dim[1] / 2 - 0.5),
            (float)(loc[2] + newLocOffset[2] + (double)dim[2] / 2 - 0.5)), Quaternion.identity);
        BlockMono newB = newP.gameObject.gameObject.GetComponent<BlockMono>();
        entBody = newP.gameObject.gameObject.GetComponentInChildren<PlayerBodyMono>().gameObject;
        SetPlayerRotation(body.transform.eulerAngles.y);
        if (gridRef.entReflects[0]) entBody.transform.localScale = new Vector3(entBody.transform.localScale.x * -1, entBody.transform.localScale.y, entBody.transform.localScale.z);
        if (gridRef.entReflects[1]) entBody.transform.localScale = new Vector3(entBody.transform.localScale.x, entBody.transform.localScale.y * -1, entBody.transform.localScale.z);
        if (gridRef.entReflects[2]) entBody.transform.localScale = new Vector3(entBody.transform.localScale.x, entBody.transform.localScale.y, entBody.transform.localScale.z * -1);
        newB.isEnt = false;
        newB.Begin2(true);
        if (lockRot)
        {
            ArmsDummyMono[] arms = FindObjectsOfType<ArmsDummyMono>();
            foreach (ArmsDummyMono arm in arms) arm.gameObject.GetComponent<Renderer>().enabled = true;
        }
        return newB.getBlockLogic();
    }

    void SetPlayerRotation(float degrees)
    {
        body.transform.eulerAngles = new Vector3(body.transform.eulerAngles.x, degrees, body.transform.eulerAngles.z);
        if (entBody != null)
        {
            if (gridRef.entReflects[0] ^ gridRef.entReflects[2])
                entBody.transform.eulerAngles = new Vector3(entBody.transform.eulerAngles.x, -degrees, entBody.transform.eulerAngles.z);
            else entBody.transform.eulerAngles = new Vector3(entBody.transform.eulerAngles.x, degrees, entBody.transform.eulerAngles.z);
        }
    }

    public void GrabHandle()
    {
        lockRot = true;
        ArmsDummyMono[] arms = FindObjectsOfType<ArmsDummyMono>();
        foreach (ArmsDummyMono arm in arms) arm.gameObject.GetComponent<Renderer>().enabled = true;
        allowOneRot = true;
    }
    public void UngrabHandle()
    {
        lockRot = false;
        ArmsDummyMono[] arms = FindObjectsOfType<ArmsDummyMono>();
        foreach (ArmsDummyMono arm in arms) arm.gameObject.GetComponent<Renderer>().enabled = false;
    }
}
