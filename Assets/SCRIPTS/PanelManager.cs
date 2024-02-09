using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject contactPanel; 
    private List<GameObject> otherPanels; 

    void Start()
    {
        otherPanels = new List<GameObject>();
        GameObject[] panels = GameObject.FindGameObjectsWithTag("Panel");
        foreach (var panel in panels)
        {
            if (panel != contactPanel && panel.activeSelf)
            {
                otherPanels.Add(panel);
            }
        }
    }

    public void ShowContactPanel()
    {
        foreach (var panel in otherPanels)
        {
            panel.SetActive(false);
        }

        contactPanel.SetActive(true);
    }

    public void HideContactPanel()
    {
        contactPanel.SetActive(false);

        foreach (var panel in otherPanels)
        {
            panel.SetActive(true);
        }
    }
}
