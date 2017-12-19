using UnityEngine;
using System.Collections;

public class DontDestroyOnSceneChance : MonoBehaviour {
    public string gameObjectTag;

    GameObject otherOptions;
    public void Awake()
    {
        DontDestroy();
    }

    private void DontDestroy()
    {
        otherOptions = GameObject.FindGameObjectWithTag(gameObjectTag);

        if (otherOptions == this.gameObject)
            DontDestroyOnLoad(this.gameObject);
        else
            Destroy(this.gameObject);
    }
}
