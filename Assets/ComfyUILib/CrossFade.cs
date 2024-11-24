using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO delete this file?

public class CrossFade : MonoBehaviour
{
    public List<Texture> textures;

    private Renderer fadeRenderer;
    private float duration = 3f;
    private int texNum = 0;
    private float lerp = 0f;

    //public AudioReact au;

    void Start()
    {
        fadeRenderer = GetComponent<Renderer>();
        fadeRenderer.material.SetFloat("_Blend", 0f);
    }

    void Update()
    {
        if (textures.Count > 1)
        {
            lerp += Time.deltaTime / duration;
            fadeRenderer.material.SetFloat("_Blend", lerp);

            /*if (au == null)
            {
                if (lerp > 1 )
                {
                    CrossFadeStart();
                }
            }

            // When using audio source for starting transitions
            else
            {
                if (lerp > 1)
                {
                    // TODO changed this for audioreaction test
                    lerp = 1;

                    if (au.avg > 0.004)
                    {
                        CrossFadeStart();
                    }
                }
            }*/
        }
    }

    public void CrossFadeStart()
    {
        lerp = 0;
        texNum++;
        if (texNum < textures.Count) {
            CrossFadeBetween(textures[texNum - 1], textures[texNum]);
        }
        else
        {
            texNum = 0;
            CrossFadeBetween(textures[textures.Count-1], textures[texNum]);

        }
    }

    private void CrossFadeBetween(Texture beforeTexture, Texture afterTexture)
    {
        fadeRenderer.material.SetTexture("_MainTex", beforeTexture);
        fadeRenderer.material.SetTexture("_TransitionTex", afterTexture);
    }
}