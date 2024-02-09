using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro text

public class ContactUI : MonoBehaviour
{
    public TMP_Text nameLabel;
    public Button callButton;

    private string contactNumber;
    private ContactManager contactManager;

    public void Setup(Contact contact, ContactManager manager)
    {
        nameLabel.text = contact.name;
        contactNumber = contact.number;
        contactManager = manager;

        callButton.onClick.AddListener(() => contactManager.CallContact(contactNumber));
    }
}
