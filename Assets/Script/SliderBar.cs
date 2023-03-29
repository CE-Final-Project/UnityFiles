using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;

    public void SetMaxValue(float Health)
    {
        slider.maxValue = Health;
        slider.value = Health;
    }
    public void SetValue(float Health)
    {
        slider.value = Health;
    }
}
