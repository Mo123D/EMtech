using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplaySettingsManager : MonoBehaviour
{
    public Slider iconSizeSlider;
    public RectTransform[] allIcons;

    private float[] initialTextSizes;
    private Vector3[] initialIconScales;
    public Slider colorChangeSlider; 
    public SpriteRenderer targetSpriteRenderer; 
    private Color initialColor = new Color(62f / 255f, 119f / 255f, 185f / 255f, 1f); 


    void Start()
    {
        Color.RGBToHSV(initialColor, out float hue, out _, out _);

        colorChangeSlider.value = hue;

        targetSpriteRenderer.color = initialColor;

        colorChangeSlider.onValueChanged.AddListener(delegate { AdjustSpriteColor(); });

        initialIconScales = new Vector3[allIcons.Length];

        
        for (int i = 0; i < allIcons.Length; i++)
        {
            initialIconScales[i] = allIcons[i].localScale;
        }

        iconSizeSlider.value = PlayerPrefs.GetFloat("IconSize", iconSizeSlider.value);

        iconSizeSlider.onValueChanged.AddListener(delegate { AdjustIconSize(); });

        AdjustIconSize();
    }

    
    public void AdjustIconSize()
    {
        for (int i = 0; i < allIcons.Length; i++)
        {
            allIcons[i].localScale = initialIconScales[i] * iconSizeSlider.value;
        }
        PlayerPrefs.SetFloat("IconSize", iconSizeSlider.value);
    }
    public void AdjustSpriteColor()
    {
        Color newColor = Color.HSVToRGB(colorChangeSlider.value, 1f, 1f);
        targetSpriteRenderer.color = newColor;

        PlayerPrefs.SetFloat("SpriteColorHue", colorChangeSlider.value);
    }

}
