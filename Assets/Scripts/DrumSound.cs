using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumSound : MonoBehaviour
{
    public AudioClip sound;

    void Start()
    {
        GetComponent<AudioSource>().playOnAwake = false;
        GetComponent<AudioSource>().clip = sound;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Left Wrist Roll" || collision.gameObject.name == "Right Wrist Roll")
        {
            Debug.Log("Play sound");
            GetComponent<AudioSource>().PlayOneShot(sound, 0.7F);;
        }
    }
}
