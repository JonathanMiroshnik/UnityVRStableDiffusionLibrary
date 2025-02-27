using System.Collections.Generic;
using UnityEngine;

// TODO: comments

/// <summary>
/// Image Gadget Mechanism for sculpting a polygonal model
/// </summary>
public class SculptingMechanism : GadgetMechanism
{
    public List<MeshDeformer> meshDeformerList;
    public int radius = 5;

    public override string mechanismText => "Sculpting";

    public override void GripProperty(GameObject GO, Pose curPose)
    {
        if (meshDeformerList == null) return;
        if (meshDeformerList.Count <= 0) return;        

        foreach(var meshDeformer in meshDeformerList) {
            meshDeformer.DeformMesh(curPose, new Pose(GO.transform.position, GO.transform.rotation), radius, 1f);
        }

        // Debug.Log(curTransform.position + " " + GO.transform.position);

        return;
    }
}