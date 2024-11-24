using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;

public class RegularDiffusionTexture : DiffusionTextureChanger
{
    public float changeTextureEvery = 1;
    public bool changeTextureToChildren = false;    

    private float textureChangeDelta = 0;

    // Update is called once per frame
    protected void Update()
    {
        if (diff_Textures.Count > 0)
        {
            if (diff_Textures.Count == 1)
            {
                if (changeTextureToChildren)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        base.changeTextureOn(child, diff_Textures[0]);
                    }
                }
                else
                {
                    base.changeTextureOn(gameObject, diff_Textures[0]);
                }

                return;
            }

            textureChangeDelta += Time.deltaTime;
            if (textureChangeDelta > changeTextureEvery)
            {
                if (changeTextureToChildren)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        base.changeTextureOn(child, diff_Textures[curTextureIndex]);
                    }
                }
                else
                {
                    base.changeTextureOn(gameObject, diff_Textures[curTextureIndex]);
                }

                curTextureIndex++;
                curTextureIndex %= diff_Textures.Count;

                textureChangeDelta = 0;
            }
        }
    }
}
