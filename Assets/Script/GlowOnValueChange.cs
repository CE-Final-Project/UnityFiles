using UnityEngine;
using UnityEngine.UI;

public class GlowOnValueChange : MonoBehaviour
{
    private Slider slider;
    private float previousValue;
    private bool isGlowing;
    private float delayTimer;
    private float glowDelay = 1f;

    public Color glowColor = Color.white;
    public float glowIntensity = 1f;

    private Color newCol;

    void Start()
    {
        slider = GetComponent<Slider>();
        previousValue = slider.value;
    }

    void Update()
    {
        if (slider.value != previousValue)
        {
            previousValue = slider.value;
            delayTimer = glowDelay;
            isGlowing = false;
        }

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
        }
        else
        {
            isGlowing = true;
        }


        if (isGlowing)
        {
            // Glow effect code goes here
            // You can use any method to create a glow effect,
            // such as using a particle system, a shader, or
            // modifying the slider's image color and/or alpha
            // For example:
            Image image = slider.fillRect.GetComponent<Image>();
            image.color = glowColor * Mathf.LinearToGammaSpace(glowIntensity);
        }
        else
        {
            // Reset the slider's image color and/or alpha to normal
            Image image = slider.fillRect.GetComponent<Image>();
            ColorUtility.TryParseHtmlString("#B02613", out newCol);
            image.color = newCol;
        }
    }
}
