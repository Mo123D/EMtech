using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkEffect : MonoBehaviour
{
    public Button buttonToBlink; // Assign the button you want to blink
    public float blinkDuration = 3.0f; // Total duration of the blink effect
    public float blinkInterval = 0.5f; // Interval for blinking (in seconds)

    private void Start()
    {
        if (buttonToBlink != null)
        {
            StartCoroutine(BlinkButton());
        }
    }

    private IEnumerator BlinkButton()
    {
        float endTime = Time.time + blinkDuration;
        while (Time.time < endTime)
        {
            // Toggle the visibility of the button
            buttonToBlink.gameObject.SetActive(!buttonToBlink.gameObject.activeSelf);

            // Wait for the specified interval
            yield return new WaitForSeconds(blinkInterval);
        }

        // Ensure the button is visible after blinking
        buttonToBlink.gameObject.SetActive(true);
    }
}
