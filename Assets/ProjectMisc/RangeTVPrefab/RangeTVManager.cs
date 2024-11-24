using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Video;

public class RangeTVManager : MonoBehaviour
{
    public VideoPlayer player;
    public GameObject XROrigin;

    public float radius = 1;

    private bool TVPlaying = false;
    private void Start()
    {
        if (XROrigin == null) Debug.LogError("Add XROrigin to RangeTV " + name);
        if (player == null) Debug.LogError("Add VideoPlayer to RangeTV " + name);
    }
    private void Update()
    {
        if (Vector3.Distance(XROrigin.transform.position, transform.position) < radius)
        {
            if (TVPlaying) return;
            player.Play();
            TVPlaying = true;
        }
        else
        {
            player.Stop();
            TVPlaying = false;
        }
    }


    // TODO remove?
    /*private void OnTriggerEnter(Collider other)
    {
        if (GameManager.getInstance() == null || player == null) return;
        // TODO check only for player enterance/exit
        player.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (GameManager.getInstance() == null || player == null) return;
        // TODO check only for player enterance/exit
        player.Stop();
    }*/
}
