using UnityEngine;

public class AddMaterialToObject : MonoBehaviour
{
    // Drag and drop the material you want to add in the Inspector
    public Material additionalMaterial;

    // Adds the material to the object without overriding the original materials
    public void AddMaterial()
    {
        // Get the Renderer component of the object
        Renderer objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // Get the current materials of the object
            Material[] currentMaterials = objectRenderer.materials;

            // Check if the material is already added to prevent duplicates
            foreach (Material mat in currentMaterials)
            {
                if (mat.name == additionalMaterial.name + " (Instance)")
                {
                    Debug.LogWarning("Material already added to the object.");
                    return;
                }
            }

            // Create a new array with an additional slot for the new material
            Material[] newMaterials = new Material[currentMaterials.Length + 1];

            // Copy the existing materials to the new array
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                newMaterials[i] = currentMaterials[i];
            }

            // Add the new material to the last slot of the new array
            newMaterials[newMaterials.Length - 1] = additionalMaterial;

            // Assign the new materials array back to the object's renderer
            objectRenderer.materials = newMaterials;
        }
        else
        {
            Debug.LogWarning("Renderer not found on the object. Make sure this script is attached to an object with a Renderer component.");
        }
    }

    // Removes the material from the object if it exists
    public void RemoveMaterial()
    {
        // Get the Renderer component of the object
        Renderer objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // Get the current materials of the object
            Material[] currentMaterials = objectRenderer.materials;

            // Find the index of the material to be removed
            int materialIndex = -1;
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                if (currentMaterials[i].name == additionalMaterial.name + " (Instance)")
                {
                    materialIndex = i;
                    break;
                }
            }

            if (materialIndex != -1)
            {
                // Create a new array with one less slot
                Material[] newMaterials = new Material[currentMaterials.Length - 1];

                // Copy the existing materials to the new array, excluding the material to be removed
                int index = 0;
                for (int i = 0; i < currentMaterials.Length; i++)
                {
                    if (i != materialIndex)
                    {
                        newMaterials[index] = currentMaterials[i];
                        index++;
                    }
                }

                // Assign the new materials array back to the object's renderer
                objectRenderer.materials = newMaterials;

                Debug.Log("Material removed.");
            }
            else
            {
                Debug.LogWarning("Material not found on the object.");
            }
        }
        else
        {
            Debug.LogWarning("Renderer not found on the object. Make sure this script is attached to an object with a Renderer component.");
        }
    }
}
