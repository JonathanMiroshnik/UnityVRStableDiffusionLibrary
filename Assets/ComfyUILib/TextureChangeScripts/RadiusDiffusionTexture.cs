using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Represents a ring(technically a circle) of effected GameObjects that is effected by the Diffusion request that begins at its center
/// </summary>
public class DiffusionRing
{    
    // Max Radius of the circle
    public float maxRadius = 1;
    public float curRadius = 1;

    // Max Time must be greater than 0
    public float changeMaxTime = 5f;
    public float curChangeTime = 0;

    // If False, stops the enlargening of the Ring
    public bool changeTextures = false;

    // GameObjects effected by the Diffusion texture change
    public List<GameObject> gameObjects;

    // Textures of the Diffusion circle
    public List<Texture2D> diffusionTextureList;

    // Circle center position
    public Vector3 centerPosition;
}

/// <summary>
/// Diffusion Texture Changer that creates a Diffusion texture change in circles
/// </summary>
public class RadiusDiffusionTexture : DiffusionTextureChanger
{    
    public List<DiffusionRing> radiusDiffusionRings;

    // Max Radius of the circles that are made at the current time
    public float CurrentMaxRadius = 1;

    // Counts the number of Radius generations that went through
    [NonSerialized]
    public int totalGenerationCounter = 0;

    // Event that is invoked after many(>=20) throws have been done, and the EXPLOSION needs to begin
    public UnityEvent ManyThrowsEvent;

    // Similar to enabled, but keeps the mechanism technically working
    public bool stopAnymoreGenerations = false;

    protected override void Awake()
    {
        base.Awake();
        radiusDiffusionRings = new List<DiffusionRing>();        
    }

    // Update is called once per frame
    protected void Update()
    {
        if (radiusDiffusionRings.Count <= 0) return;

        foreach (DiffusionRing dr in radiusDiffusionRings)
        {
            if (!dr.changeTextures) continue;

            dr.curChangeTime += Time.deltaTime;
            dr.curRadius = (dr.curChangeTime / dr.changeMaxTime) * dr.maxRadius;

            addRadiusGameObjects(dr);

            if (dr.curChangeTime > dr.changeMaxTime)
            {
                dr.changeTextures = false;
            }
        }
    }


    // TODO: bad designed function
    /// <summary>
    /// Helper function used entirely to change the CurrentMaxRadius in accordance 
    /// to the number of generations in this texture changer for the final explosion scene 
    /// </summary>
    private void CurMaxRad()
    {
        if (GameManager.getInstance() == null) return;

        if (totalGenerationCounter > 4)
        {            
            CurrentMaxRadius = 2;
        }
        if (totalGenerationCounter > 5)
        {
            CurrentMaxRadius = 3;
        }
        if (totalGenerationCounter > 10)
        {
            CurrentMaxRadius = 4;
        }
        if (totalGenerationCounter > 11)
        {
            CurrentMaxRadius = 6;
        }
        if (totalGenerationCounter > 15)
        {
            CurrentMaxRadius = 10;

            // Start of the explosion
            ManyThrowsEvent?.Invoke();
            /*GameManager.getInstance().gadget.GadgetMechanisms[0].enabled = false; // TODO: non general gadget
            GameManager.getInstance().gadget.MechanismText.text = "?????????????????";*/
            stopAnymoreGenerations = true;
        }
    }

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (GameManager.getInstance() == null) return false;
        if (stopAnymoreGenerations) return false;
        // TODO: think if this line is even useful in this script
        //base.AddTexture(diffusionRequest);

        // TODO: do I want this to only work if it is grabbed AND the textures get there?
        // TODO: if you keep this line, notice at the line below, can be removed there
        if (!diffusionRequest.diffusableObject.grabbed) return false;

        // For the Throwing mechanism, after the textures for a certain grabbing have finished generating and downloading,
        // this activates the particles that indicate the end of the generation
        if (diffusionRequest.diffusableObject.grabbed)
        {
            if (diffusionRequest.diffusableObject.transform.childCount > 0)
            {                
                diffusionRequest.diffusableObject.transform.GetChild(0).gameObject.SetActive(true);
            }       
        }

        AddedTextureUnityEvent?.Invoke();

        // TODO: these two lines, bad design
        totalGenerationCounter++;
        CurMaxRad();

        DiffusionRing newDiffusionRing = new DiffusionRing();
        newDiffusionRing.gameObjects = new List<GameObject>();
        newDiffusionRing.diffusionTextureList = new List<Texture2D>();
        newDiffusionRing.maxRadius = CurrentMaxRadius;

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            newDiffusionRing.diffusionTextureList.Add(texture);
        }
        radiusDiffusionRings.Add(newDiffusionRing);

        return true;
    }

    /// <summary>
    /// Adds all GameObjects in the DiffusionRing Radius to the DiffusionRing, along with adding the textures to them
    /// </summary>
    public void addRadiusGameObjects(DiffusionRing diffusionRing)
    {
        if (GameManager.getInstance() == null) return;
        if (diffusionRing == null) return;

        // Finding all relevant GameObjects inside the DiffusionRing current radius that are not yet in the DiffusionRing gameObjects
        List<GameObject> curRadiusGameObjects = gameObjectsInRadius(diffusionRing.curRadius, diffusionRing.centerPosition);
        List<GameObject> newRadiusGameObjects = new List<GameObject>();
        foreach(GameObject GO in curRadiusGameObjects)
        {
            if (GO == null) continue;
            if (!diffusionRing.gameObjects.Contains(GO)) {
                newRadiusGameObjects.Add(GO);
            }
        }

        // Adding the textures to the new ring GameObjects
        foreach (GameObject GO in newRadiusGameObjects)
        {
            if (GO.TryGetComponent<TextureTransition>(out TextureTransition TT))
            {
                TT.AddTexture(diffusionRing.diffusionTextureList, true);
                diffusionRing.gameObjects.Add(GO);
            }
        }
    }

    /// <summary>
    /// Finds all the GameObjects in given radius from a given center position
    /// </summary>
    /// <param name="curRadius">Radius of the ball in-which the GameObjects are found</param>
    /// <param name="position">Center position</param>
    /// <returns></returns>
    private List<GameObject> gameObjectsInRadius(float curRadius, Vector3 position)
    {
        if (GameManager.getInstance() == null) return null;

        List<GameObject> radiusGameObjects = new List<GameObject>();
        foreach (GameObject go in GameManager.getInstance().DiffusionList)
        {
            if (go == null) continue;
            if (Vector3.Distance(go.transform.position, position) <= curRadius)
            {
                radiusGameObjects.Add(go);
            }
        }
        return radiusGameObjects;
    }

    /// <summary>
    /// Called when a relevant DiffusableObject has collided and should thus create a Diffusion Texture Transition effect
    /// </summary>
    /// <param name="collision">Collision of the Diffusable Object with another GameObject</param>
    public void DiffusableObjectCollided(Collision collision)
    {
        if (GameManager.getInstance() == null) return;

        if (radiusDiffusionRings.Count <= 0) return;
        DiffusionRing dr = radiusDiffusionRings[radiusDiffusionRings.Count - 1];

        if (dr.gameObjects.Count > 0) return;        

        dr.centerPosition = collision.transform.position;
        // TODO: delete, and delete collision in diffusionrequest??
        addRadiusGameObjects(dr);

        dr.changeTextures = true;
    }
       
}
