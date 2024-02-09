using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefab; // Assign your task item prefab in the Inspector
    public Transform taskListParent; // Assign the parent object in the Scroll View content area
    public InputField taskInputField;



    public GameObject confirmationDialogPanel;
    public TMP_Text confirmationDialogText;
    private GameObject taskToBeConfirmed;

    private List<GameObject> taskItems = new List<GameObject>();

    // Method to call when the Add Task button is clicked
    public void AddTask()
    {
        string taskDescription = taskInputField.text;
        if (!string.IsNullOrEmpty(taskDescription))
        {
            GameObject newTask = CreateTaskItem(taskDescription);
            taskItems.Add(newTask);
            taskInputField.text = ""; // Clear the input field
        }
    }

    // Method to create a task item UI element
    private GameObject CreateTaskItem(string description)
    {
        GameObject taskItem = Instantiate(taskPrefab, taskListParent);
        taskItem.GetComponentInChildren<Text>().text = description;

        // Find the CompleteTaskButton and add a click listener to it
        Button completeButton = taskItem.GetComponentInChildren<Button>();
        completeButton.onClick.AddListener(() => CompleteTask(taskItem));

        return taskItem;
    }

    // Method to handle task completion
    private void CompleteTask(GameObject taskItem)
    {
        taskItems.Remove(taskItem);
        Destroy(taskItem);
    }



    public void ConfirmTaskCompletion(GameObject taskItem)
    {
        taskToBeConfirmed = taskItem;
        string taskDescription = taskItem.GetComponentInChildren<TMP_Text>().text;
        confirmationDialogText.text = "Are you sure you have done '" + taskDescription + "'?";
        confirmationDialogPanel.SetActive(true);
    }

    public void CompleteTaskConfirmed()
    {
        if (taskToBeConfirmed != null)
        {
            CompleteTask(taskToBeConfirmed);
        }
        confirmationDialogPanel.SetActive(false);
    }
    public void CancelTaskCompletion()
    {
        taskToBeConfirmed = null;
        confirmationDialogPanel.SetActive(false);
    }
}
