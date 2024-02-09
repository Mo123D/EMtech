using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ContactManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public TMP_InputField numberInputField;
    public GameObject contactPrefab; 
    public Transform contactsParent; 

    private List<Contact> contacts = new List<Contact>();

    public void AddContact()
    {
        string name = nameInputField.text;
        string number = numberInputField.text;

        Contact newContact = new Contact() { name = name, number = number };
        contacts.Add(newContact);

        DisplayContact(newContact);

        nameInputField.text = "";
        numberInputField.text = "";
    }

    private void DisplayContact(Contact contact)
    {
        GameObject contactObj = Instantiate(contactPrefab, contactsParent);
        contactObj.GetComponent<ContactUI>().Setup(contact, this);
    }

    public void CallContact(string number)
    {
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
        // For testing in Editor
        private void CallNumber(string number)
        {
            Debug.Log("Dialing number (Editor Mode): " + number);
        }
#endif
    public void SOS()
    {
        CallNumber("116" +
            "");
    }
}

