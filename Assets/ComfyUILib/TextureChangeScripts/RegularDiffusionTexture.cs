using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// DiffusionTextureChanger that changes the texture of a GameObject on a regular interval.
/// Could also be used to change the texture of a GameObject to a single texture.
/// Or to change the textures of the children of the GameObject.
/// </summary>
public class RegularDiffusionTexture : DiffusionTextureChanger
{
    // Time between texture changes
    public float changeTextureEvery = 1;
    // Whether to change the texture of the children of the GameObject
    public bool changeTextureToChildren = false;    
    // Time since the last texture change
    private float textureChangeDelta = 0;

    // Update is called once per frame
    protected void Update()
    {
        // If there are textures to change
        if (_diffTextures.Count > 0)
        {
            // If there is only one texture, change the texture of the GameObject or its children
            if (_diffTextures.Count == 1)
            {
                if (changeTextureToChildren)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        base.changeTextureOn(child, _diffTextures[0]);
                    }
                }
                else
                {
                    base.changeTextureOn(gameObject, _diffTextures[0]);
                }

                return;
            }

            // Increment the time since the last texture change
            textureChangeDelta += Time.deltaTime;
            // If the time since the last texture change is greater than the time between texture changes
            if (textureChangeDelta > changeTextureEvery)
            {
                if (changeTextureToChildren)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        base.changeTextureOn(child, _diffTextures[_curTextureIndex]);
                    }
                }
                else
                {
                    base.changeTextureOn(gameObject, _diffTextures[_curTextureIndex]);
                }

                _curTextureIndex++;
                _curTextureIndex %= _diffTextures.Count;

                textureChangeDelta = 0;
            }
        }
    }
}
