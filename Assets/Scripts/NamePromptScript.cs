using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePromptScript : MonoBehaviour
{
    [SerializeField] private string prompt;

    [SerializeField] private PaintBrushPrompts _paintBrushPrompts;
    // Stalizrt is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addPrompt()
    {
        _paintBrushPrompts.addPrompt(prompt);
    }
}
