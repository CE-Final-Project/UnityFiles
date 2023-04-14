using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clip")]
    public AudioClip menuBackground;
    public AudioClip lobbyBackground;
    public AudioClip gameBackground1;
    public AudioClip gameBackground2;
    public AudioClip gameBackground3;
    public AudioClip postGameBackround;
    public AudioClip click;
    public AudioClip select;
    public AudioClip ready;
    public AudioClip death;

    private void Start()
    {
        // Get scene name to slelct BGM to play
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "MainMenu")
        {
            musicSource.clip = menuBackground;
        }
        else if (currentScene.name == "Lobby")
        {
            musicSource.clip = lobbyBackground;
        }
        else if (currentScene.name == "inGame")
        {
            musicSource.clip = gameBackground1;
        }

        musicSource.Play();
    }

    //------ Play SFX from anywhere ------
    // 1. Declare a AudioManager variable.
    // 2. Declare Awake() Method and call "*variable_name = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();"
    // 3. Call *variable_name.PlaySFX(*variable_name.*effect_name); when ever needs.
    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
