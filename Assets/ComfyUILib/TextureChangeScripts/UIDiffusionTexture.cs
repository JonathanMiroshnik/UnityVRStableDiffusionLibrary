using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// DiffusionTextureChanger used by the game UI, and the Gadget UI, to pop up images or add them to the gadget inventory.
/// </summary>
public class UIDiffusionTexture : DiffusionTextureChanger
{
    // Display on which the textures that are added are shown as a "Pop up"
    public GameObject PopupDisplay;

    // Gadget UI display, or "inventory" of images to which images are added to
    public GameObject imagesDisplayPrefab;    

    // TODO: docs below
    [SerializeField] public GameObject AIDisplayPrefab;    
    public UnityEvent unityEvent;

    private GameObject _curDisplayPrefab;
    private bool _displayTextures = false;
    [Min(0.01f)]
    private float _changeRate = 3.0f;
    private float _curChangeDelta = 0f;

    //private static float IMAGES_REDUCE_SIZE_FACTOR = 512;

    public PlaySounds playSounds;

    private void Start()
    {
        if (PopupDisplay == null || imagesDisplayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return;
        }

        if (playSounds == null)
        {
            Debug.LogError("Add all UIDiffusionTexture inputs");
        }
    }

    // Adding the Image in the Gadget panel as well
    // TODO: should this part be in a separate place? should these textures have a global variable for global access? shouldn't the gadget deal with it? is UI and gadget sepearate?
    // TODO: change with this and make a PopupDiffusionTexture instead?
    public void CreateImagesInside(List<Texture2D> textures, GameObject toBeParent, bool destroyPreviousChildren)
    {
        if (toBeParent == null) return;
        if (destroyPreviousChildren)
        {
            var children = new List<GameObject>();
            foreach (Transform child in toBeParent.transform) children.Add(child.gameObject);
            children.ForEach(child => DestroyImmediate(child));
        }

        if (textures == null || textures.Count == 0) return;

        foreach (Texture2D tex in textures)
        {
            GameObject childGameObject = new GameObject("Image");

            // Set the new GameObject as a child of the parentGameObject
            childGameObject.transform.SetParent(toBeParent.transform, false);

            // Add a RectTransform component to the child GameObject if not already present            
            RectTransform rectTransform = childGameObject.AddComponent<RectTransform>();            
            // rectTransform.localScale = new Vector2(IMAGES_REDUCE_SIZE_FACTOR / ((float)tex.width), IMAGES_REDUCE_SIZE_FACTOR / ((float)tex.height));
            //rectTransform.sizeDelta = new Vector2(tex.width / IMAGES_REDUCE_SIZE_FACTOR, tex.height / IMAGES_REDUCE_SIZE_FACTOR); // Adjust the size as needed
            
            
            rectTransform.sizeDelta = new Vector2(tex.width / 100, tex.height / 100);

            // Add an Image component to the child GameObject
            Image curImage = childGameObject.AddComponent<Image>();
        }

        for (int i = 0; i < textures.Count; i++)
        {
            GameObject go = toBeParent.transform.GetChild(i).gameObject;
            if (go != null)
            {
                changeTextureOn(go, textures[i]);
            }
        }
    }

    private void CreatePopupTemplate(List<Texture2D> textures, GameObject givenDispayPrefab)
    {
        if (textures.Count == 0)
        {
            Debug.Log("Tried creating popup with no textures");
            return;
        }
        
        if (PopupDisplay == null || imagesDisplayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return;
        }
        
        _curChangeDelta = 0f;

        if (_curDisplayPrefab != null)
        {
            Destroy(_curDisplayPrefab);
            _curDisplayPrefab = null;
        }
        
        
        _curDisplayPrefab = Instantiate(givenDispayPrefab, PopupDisplay.transform, false);
        Debug.Log("Instantiated popup!");

        CreateImagesInside(textures, _curDisplayPrefab, true);
        
        Debug.Log("Created images inside popup!");

        _displayTextures = true;
        playSounds.PlaySound("ShowUIElement");
    }

    public void CreatePopup(List<Texture2D> textures)
    {
        Debug.Log("textures: " + textures.Count);
        CreatePopupTemplate(textures, imagesDisplayPrefab);
    }
    
    public void CreateAIPopup(List<Texture2D> textures)
    {
        CreatePopupTemplate(textures, AIDisplayPrefab);
    }

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (PopupDisplay == null || imagesDisplayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return false;
        }
        if (base.AddTexture(diffusionRequest))
        {
            CreatePopup(_diffTextures);
            
            // TODO: fix, disassociate uiDiffusionTexture and the gadget TextureQuueue, delete the textureQueue in the Gadget in general, it should not have this responsibility
            //GameManager.getInstance().gadget.AddTexturesToQueue(diff_Textures);

            // Sending broadcast to Game timeline script
            unityEvent?.Invoke();

            return true;
        }

        return false;
    }

    private void Update()
    {
        if (_displayTextures && _curDisplayPrefab != null)
        {
            _curChangeDelta += Time.deltaTime;

            // Notice changeRate > 0
            float curChange = 2 * _curChangeDelta / _changeRate;
            float curTotalChangeDelta = Mathf.Min(curChange, 2 - curChange);

            // Assume all children of UIDisplay are Images
            foreach (Transform child in _curDisplayPrefab.transform)
            {
                Image curImage = child.GetComponent<Image>();
                // 1 - (curChangeDelta / changeRate)
                curImage.color = new Color(curImage.color.r, curImage.color.g, curImage.color.b, curTotalChangeDelta);
            }

            Image displayImage = _curDisplayPrefab.GetComponent<Image>();
            // 1 - (curChangeDelta / changeRate)
            displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, curTotalChangeDelta);

            if (_curChangeDelta > _changeRate)
            {
                _displayTextures = false;

                DestroyImmediate(_curDisplayPrefab);
                _curDisplayPrefab = null;
            }
        }
        else
        {
            _curChangeDelta = 0f;
        }        
    }

    public override void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null)
        {
            Debug.LogError("Tried to change texture while the texture or target GameObject doesn't exist");
            return;
        }

        Sprite curSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        Image curImage = curGameObject.GetComponent<Image>();
        curImage.sprite = curSprite;
    }
}
