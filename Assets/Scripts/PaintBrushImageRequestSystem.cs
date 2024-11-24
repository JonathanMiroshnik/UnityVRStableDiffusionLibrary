using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PaintBrushImageRequestSystem : MonoBehaviour
{

    [SerializeField] private List<Texture2D> monalisastarrynight;
    [SerializeField] private List<Texture2D> monalisarubberduck;
    [SerializeField] private List<Texture2D> brickwallmonalisa;
    [SerializeField] private List<Texture2D> rubberduckstarrynight;
    [SerializeField] private List<Texture2D> brickwallstarrynight;
    [SerializeField] private List<Texture2D> brickwallrubberduck;

    [SerializeField] private PaintBrushPrompts _paintBrushPrompts;
    
    /*
            [mona lisa, starry night]
            [mona lisa, rubber duck]
            [brick wall, mona lisa]
            [rubber duck, starry night]
            [brick wall, starry night]
            [brick wall, rubber duck]
         */
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void foo()
    {
        
    }
    
    public Texture2D RequestImage()
    {
        string prompts = _paintBrushPrompts.passAndWipePromptList();
        if (prompts == null)
        {
            Debug.Log("Error! Prompt list is not of length 2");
            return null;
        }
        return textureFactory(prompts);
    }
    
    private Texture2D textureFactory(string mergedPrompts)
    {
        switch (mergedPrompts)
        {
            case "mona lisa, starry night":
                return monalisastarrynight[Random.Range(0, monalisastarrynight.Count)];
            case "mona lisa, rubber duck":
                return monalisarubberduck[Random.Range(0, monalisarubberduck.Count)];
            case "brick wall, mona lisa":
                return brickwallmonalisa[Random.Range(0, brickwallmonalisa.Count)];
            case "rubber duck, starry night":
                return rubberduckstarrynight[Random.Range(0, rubberduckstarrynight.Count)];
            case "brick wall, starry night":
                return brickwallstarrynight[Random.Range(0, brickwallstarrynight.Count)];
            case "brick wall, rubber duck":
                return brickwallrubberduck[Random.Range(0, brickwallrubberduck.Count)];
            default:
                Debug.Log("Invalid Merged Prompt!");
                break;
        }

        return null;
    }

    
    
    
}
