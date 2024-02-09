using UnityEngine;
using UnityEngine.UI;

public class InternalButtonController : MonoBehaviour
{
    public Button externalVideoButton; // Assign the external button here

    // This method will be called when the internal button is clicked
    public void TriggerExternalButtonClick()
    {
        if (externalVideoButton != null)
        {
            // Simulate a click on the external button
            externalVideoButton.onClick.Invoke();
        }
    }
}
