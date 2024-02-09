using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject contactPanel;

    public void ToggleContactPanel()
    {
        contactPanel.SetActive(!contactPanel.activeSelf);
    }
}
