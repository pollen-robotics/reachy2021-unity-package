using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrumsButton : MonoBehaviour
{
    public Transform drums;
    public Button drumsButton;

    public void Start()
    {
        drumsButton.onClick.AddListener(DisplayDrums);
    }
    
    public void DisplayDrums()
    {
        Debug.Log("Display drums");
        drums.gameObject.SetActive(true);
    }

    public void HideDrums()
    {
        Debug.Log("Hide drums");
        drums.gameObject.SetActive(false);
    }
}
