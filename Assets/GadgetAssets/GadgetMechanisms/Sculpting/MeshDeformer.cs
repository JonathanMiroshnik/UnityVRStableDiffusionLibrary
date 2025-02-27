using UnityEngine;

public class MeshDeformer : MonoBehaviour
{
    public void DeformMesh(Pose firstPosition, Pose secondPosition, float influenceRadius, float deformationStrength)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] originalVertices = mesh.vertices;
        Vector3[] modifiedVertices = mesh.vertices;

        Vector3 movementDirection = (secondPosition.position - firstPosition.position).normalized;
        float distanceMoved = Vector3.Distance(firstPosition.position, secondPosition.position);

        // Loop through vertices and modify them if they are within influenceRadius
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 worldVertexPosition = transform.TransformPoint(originalVertices[i]);
            float distanceToFirstPosition = Vector3.Distance(worldVertexPosition, firstPosition.position);

            if (distanceToFirstPosition < influenceRadius)
            {
                // Move vertex in the direction of movement
                Vector3 deformation = movementDirection * distanceMoved * deformationStrength;
                modifiedVertices[i] += deformation;
            }
        }

        // Apply the modified vertices back to the mesh
        mesh.vertices = modifiedVertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
