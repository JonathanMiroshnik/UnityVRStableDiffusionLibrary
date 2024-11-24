using UnityEngine;

public class XRScenePersistence : MonoBehaviour
{
    public XRScenePersistence instance;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
