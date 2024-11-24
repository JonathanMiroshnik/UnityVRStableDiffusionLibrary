using System.Collections.Generic;
using UnityEngine;
using static GeneralGameLibraries;

// TODO documentation
public class AIGadgetAssistant : DiffusionTextureChanger
{
    public static List<Texture2D> AITextures;

    public string AIAudioClipFolder = "Sounds/Voiceover";
    private AudioClipsLibrary AudioClipsLibrary;
    
    public AudioSource audioSource;

    public UIDiffusionTexture uiDiffusionTexture;

    // Default prompts for the AI character image creation
    private static string DEFAULT_POSITIVE_PROMPT = "masterpiece,high quality,highres,solo,pslain,x hair ornament,brown eyes,dress" +
                                                    ",hoop,black dress,strings,floating circles,blue orbs,turning around,detached sleeves," +
                                                    "black background, short hair,luminous hair,blonde hair,smile";
    private static string DEFAULT_NEGATIVE_PROMPT = "EasyNegativeV2,negative_hand-neg,(low quality, worst quality:1.2)";

    protected override void Awake()
    {
        base.Awake();

        if (uiDiffusionTexture == null) Debug.LogError("Add UIDiffusionTexture to the AIGadgetAssistant");

        if (AITextures == null)
        {
            AITextures = new List<Texture2D>();
        }

        AudioClipsLibrary = new AudioClipsLibrary(AIAudioClipFolder);       
    }


    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            AITextures.Add(texture);
        }

        return base.AddTexture(diffusionRequest);
    }

    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        foreach (Texture2D texture in newDiffTextures)
        {
            AITextures.Add(texture);
        }

        return base.AddTexture(newDiffTextures, addToTextureTotal);
    }

    /// <summary>
    /// Sends an Image generation request for the AI representation, according to the input keywords
    /// </summary>
    /// <param name="keywords">To be added to the prompts of the Image Generation request</param>
    public void CreateAITexture(string keywords = "")
    {
        if (GameManager.getInstance() == null) return;

        DiffusionRequest diffusionRequest = new DiffusionRequest();
        diffusionRequest.diffusionModel = diffusionModels.ghostmix;

        // TODO need to ADD keywords to an existing prompt?
        diffusionRequest.positivePrompt = DEFAULT_POSITIVE_PROMPT + ", " + keywords;
        diffusionRequest.negativePrompt += DEFAULT_NEGATIVE_PROMPT;

        diffusionRequest.targets.Add(this);
        diffusionRequest.addToTextureTotal = true;
        diffusionRequest.diffusionJsonType = diffusionWorkflows.AIAssistant;

        // TODO do I need so many at every time?
        diffusionRequest.numOfVariations = 5;

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    /// <summary>
    /// Plays an AI assistant Audio Clip and creates a popup to go along with it
    /// </summary>
    /// <param name="audioClipName">AI Assistant Audio Clip to be played</param>
    public void AITalk(string audioClipName = "")
    {
        if (audioSource == null)
        {
            Debug.Log("Add a Audio Source to AI Assistant");
            return;
        }

        audioSource.PlayOneShot(AudioClipsLibrary.AudioClips[audioClipName]);
        if (AITextures.Count == 0) return;

        Texture2D currentTexture = AITextures[curTextureIndex];

        if (AITextures.Count-1 > curTextureIndex) {
            curTextureIndex++;
        }   
        else
        {
            curTextureIndex = 0;
        }

        if (uiDiffusionTexture != null) uiDiffusionTexture.CreateAIPopup(new List<Texture2D>() { currentTexture });       
    }

    
}
