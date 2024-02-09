using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneCaller : MonoBehaviour
{
    public TMP_InputField numberInputField;

    public void Call()
    {
        Debug.Log("Call method triggered");
        string number = numberInputField.text;
        CallNumber(number);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void CallNumber(string number)
    {
        try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.DIAL");
                intent.Call<AndroidJavaObject>("setData", new AndroidJavaObject("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "tel:" + number));
                currentActivity.Call("startActivity", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error calling number: " + e.Message);
        }
    }
#else
    // For testing in Editor; you can add mock functionality here if needed.
    private void CallNumber(string number)
    {
        Debug.Log("Dialing number (Editor Mode): " + number);
        // Add editor-specific behavior here if necessary.
    }
#endif
}
