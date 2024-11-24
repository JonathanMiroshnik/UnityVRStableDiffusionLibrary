using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffusionActivationTest : MonoBehaviour
{
    public ComfyOrganizer comfyOrg;
    public GameObject testGameObj;

    public DiffusionRequest diffReq;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("genTest", 1, 4);
        //StartCoroutine(genTestEnum());
    }

    private void genTest()
    {
        comfyOrg.SendDiffusionRequest(diffReq);
    }

    private IEnumerator genTestEnum()
    {
        comfyOrg.SendDiffusionRequest(diffReq);
        yield return null;
    }
}
