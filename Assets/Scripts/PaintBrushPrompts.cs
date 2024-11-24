using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintBrushPrompts : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private List<string> promptsList = new List<string>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addPrompt(string prompt)
    {
        if (promptsList.Count < 2 && !promptsList.Contains(prompt))
        {
            promptsList.Add(prompt);
        }
    }

    public void wipePromptList()
    {
        promptsList = new List<string>();
    }

    public string passAndWipePromptList() 
    {
        if (promptsList.Count != 2)
        {
            return null;
        }
        
        List<string> copy = new List<string>(promptsList);
        promptsList = new List<string>();
        copy.Sort();
        return copy[0] + ", " + copy[1];
    }

}
