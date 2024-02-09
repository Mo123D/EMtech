using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContrastManager : MonoBehaviour
{

    public Material textMaterial;
    private float currentDilateValue = 0f; 
    private int highContrastClickCount = 0; 
    private int lowContrastClickCount = 0;

    private const int MaxClickCount = 4; // Maximum number of times the user can click
    private const float DilateIncrement = 0.05f; // The increment value for each click

    public void SetHighContrast()
    {
        if (highContrastClickCount < MaxClickCount)
        {
            currentDilateValue += DilateIncrement;
            textMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, currentDilateValue);
            highContrastClickCount++;
        }
    }

    public void SetLowContrast()
    {
        if (lowContrastClickCount < MaxClickCount)
        {
            currentDilateValue -= DilateIncrement;
            textMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, currentDilateValue);
            lowContrastClickCount++;
        }
    }

    public void ResetContrast()
    {
        highContrastClickCount = 0;
        lowContrastClickCount = 0;
        currentDilateValue = 0f; 
        textMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, currentDilateValue);
    }



}
