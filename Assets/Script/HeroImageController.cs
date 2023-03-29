
using UnityEngine;
using UnityEngine.UI;

public class HeroImageController : MonoBehaviour
{
    public Image heroImg;
    public Sprite elfSprite;
    public Sprite knightSprite;
    public Sprite lizardSprite;
    public Sprite wizzardSprite;

    public void changeToElf()
    {
        heroImg.sprite = elfSprite;
    }
    public void changeToKnight()
    {
        heroImg.sprite = knightSprite;
    }
    public void changeToLizard()
    {
        heroImg.sprite = lizardSprite;
    }
    public void changeToWizzard()
    {
        heroImg.sprite = wizzardSprite;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            changeToElf();
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            changeToKnight();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            changeToLizard();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            changeToWizzard();
        }
    }
}
