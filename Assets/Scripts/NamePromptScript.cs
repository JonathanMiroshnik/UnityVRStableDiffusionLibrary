using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamePromptScript : MonoBehaviour
{
    [SerializeField] private string _prompt;
    [SerializeField] private PaintBrushPrompts _paintBrushPrompts;

    public void addPrompt()
    {
        _paintBrushPrompts.addPrompt(_prompt);
    }
}
