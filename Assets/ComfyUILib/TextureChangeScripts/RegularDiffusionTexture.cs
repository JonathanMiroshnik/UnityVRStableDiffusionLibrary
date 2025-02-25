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
        if (_diffTextures.Count > 0)
        {
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

            textureChangeDelta += Time.deltaTime;
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
