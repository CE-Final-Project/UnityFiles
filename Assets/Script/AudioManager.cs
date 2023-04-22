using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Source")]
    public AudioSource musicSource;
    public AudioSource SFXSource;

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
    public AudioClip Attack0;
    public AudioClip Attack1;
    public AudioClip Attack2;
    public AudioClip Attack3;
    public AudioClip death;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartMainMenuMusic()
    {
        musicSource.clip = menuBackground;
        musicSource.Play();
    }
    
    public void StartLobbyMusic()
    {
        musicSource.clip = lobbyBackground;
        musicSource.Play();
    }
    
    public void StartGameMusic()
    {
        musicSource.clip = gameBackground1;
        musicSource.Play();
    }
    
    public void StartPostGameMusic()
    {
        musicSource.clip = postGameBackround;
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
