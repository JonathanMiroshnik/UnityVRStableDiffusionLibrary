
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureParticleSystemManage : MonoBehaviour
{
    List<GameObject> particleSystems = new List<GameObject>();
    public GameObject particleSystemBase;

    private void Start()
    {
        if (particleSystemBase == null)
        {
            Debug.LogError("Add a Particle System to the TextureParticleSystemsManager");

        }
        if (particleSystemBase.GetComponent<ParticleSystem>() == null) 
        {
            Debug.LogError("Add a Particle System component to the base object");
        }
    }

    public void AddTextureParticleSystem(Texture2D texture)
    {
        if (texture == null) return;

        GameObject newPS = Instantiate(particleSystemBase, transform, true);
        ParticleSystemRenderer newPSRenderer = newPS.GetComponent<ParticleSystemRenderer>();
        if (newPSRenderer == null)
        {
            Destroy(newPS);
            return;
        }

        newPSRenderer.material.mainTexture = texture;
        newPS.gameObject.SetActive(true);

        particleSystems.Add(newPS);
    }

    public void AddRandomParticles(int NumOfTextures)
    {
        if (NumOfTextures <= 0) return;
        if (GameManager.getInstance() == null) return;

        foreach (Texture2D texture in GeneralGameLibraries.GetRandomElements(GameManager.getInstance().comfyOrganizer.allTextures, NumOfTextures))
        {
            AddTextureParticleSystem(texture);
        }
    }
}
