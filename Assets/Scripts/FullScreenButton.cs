using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FullScreenButton : MonoBehaviour
{
    [SerializeField] Sprite cropOut;
    [SerializeField] Sprite cropIn;
    Image buttonImage;

    public void Start()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Destroy(this.gameObject);
        }

        buttonImage = this.GetComponent<Image>();
    }


    private void Update()
    {
        if (Screen.fullScreen)
        {
            buttonImage.sprite = cropIn;
        }
        else
        {
            buttonImage.sprite = cropOut;
        }
    }

    public void ChangeScreenSize()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

}
