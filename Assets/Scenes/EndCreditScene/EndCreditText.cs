using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndCreditText : MonoBehaviour
{
    public List<string> Credits;
    public TextMeshProUGUI creditTextPanel;
    int curCredit = 0;    

    // Start is called before the first frame update
    void Start()
    {
        if (Credits.Count <= 0 || creditTextPanel == null)
        {
            Debug.LogError("Add Credits or TextPanel for End Credit Scene Text script");
        }

        InvokeRepeating("ShowNextCredit", 3f, 3f);
    }

    void ShowNextCredit()
    {
        if (curCredit == Credits.Count) return;
        creditTextPanel.text = Credits[curCredit];
        curCredit++;
    }
}
