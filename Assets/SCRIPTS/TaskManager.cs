using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

public class TaskManager : MonoBehaviour
{
    public GameObject taskPrefab;
    public Transform taskListParent;
    public TMP_InputField taskInputField;
    public Toggle repetitiveTaskToggle; 
    public GameObject confirmationDialogPanel;
    public TMP_Text confirmationDialogText;

    private List<TaskData> tasks = new List<TaskData>(); 
    private GameObject taskToBeConfirmed;

    public TMP_Dropdown timeDropdown;
    public GameObject timeInputPanel;
    public TMP_InputField hourInputField;
    public TMP_InputField minuteInputField;

    public TMP_Text countdownTimerDisplay;
    public AudioSource audioSource;
    public Toggle[] dayToggles;


    private void AddTask()
    {
        string taskDescription = taskInputField.text;
        bool isRepetitive = repetitiveTaskToggle.isOn;

        if (!string.IsNullOrEmpty(taskDescription))
        {
            List<DayOfWeek> selectedDays = new List<DayOfWeek>();
            DateTime scheduledDate = DateTime.Now; 

            if (isRepetitive)
            {
                for (int i = 0; i < dayToggles.Length; i++)
                {
                    if (dayToggles[i].isOn)
                    {
                        selectedDays.Add((DayOfWeek)i);
                    }
                }
                scheduledDate = FindNextScheduledDate(selectedDays);
            }

            TimeSpan? scheduledTimeNullable = ParseTime(timeDropdown.options[timeDropdown.value].text);
            TimeSpan scheduledTime = scheduledTimeNullable ?? TimeSpan.Zero;

            TaskData newTask = new TaskData(taskDescription, isRepetitive, selectedDays, scheduledTime, scheduledDate);
            tasks.Add(newTask);

            // Determine if the task should appear under 'Today' tasks
            bool isToday = scheduledDate.Date == DateTime.Today && (!isRepetitive || scheduledTime >= DateTime.Now.TimeOfDay);

            if (isToday)
            {
                CreateTaskItem(newTask);
            }

            taskInputField.text = "";
            repetitiveTaskToggle.isOn = false;
            daySelectionPanel.SetActive(false);

            UpdateProgressBar();
            ResetDaySelection();
        }
    }
    private DateTime FindNextScheduledDate(List<DayOfWeek> selectedDays)
    {
        DateTime today = DateTime.Today.AddDays(1); // Start from tomorrow
        int daysUntilNextOccurrence = selectedDays.Select(day => ((int)day - (int)today.DayOfWeek + 7) % 7)
                                                  .Where(days => days > 0)
                                                  .Min();
        return today.AddDays(daysUntilNextOccurrence);
    }

    private DateTime NextDayOfWeek(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return daysToAdd == 0 ? start.AddDays(7) : start.AddDays(daysToAdd);
    }


    private GameObject CreateTaskItem(TaskData taskData)
    {
        GameObject taskItem = Instantiate(taskPrefab, taskListParent);

        TMP_Text taskText = taskItem.GetComponentInChildren<TMP_Text>();
        if (taskText != null)
        {
            taskText.text = taskData.Description;
        }
        else
        {
            Debug.LogError("TextMeshPro component not found on the task item prefab");
        }

        Button completeButton = taskItem.GetComponentInChildren<Button>();
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(() => ConfirmTaskCompletion(taskItem, taskData));
        }
        else
        {
            Debug.LogError("Button component not found on the task item prefab");
        }

        DateTime now = DateTime.Now;
        if (!taskData.IsComplete && taskData.ScheduledDate.Date == now.Date
            && taskData.ScheduledDate + taskData.ScheduledTime > now)
        {
            StartCoroutine(LogTaskCountdown(taskData));

            StartCoroutine(StartTaskBlinkingTimer(taskData, taskItem));
        }

        return taskItem;
    }



    private IEnumerator LogTaskCountdown(TaskData taskData)
    {
        while (!taskData.IsComplete && taskData.ScheduledDate + taskData.ScheduledTime > DateTime.Now)
        {
            TimeSpan timeRemaining = (taskData.ScheduledDate.Date + taskData.ScheduledTime) - DateTime.Now;
            Debug.Log("Task: " + taskData.Description + " - Time Remaining: " + timeRemaining);
            yield return new WaitForSeconds(1);
        }
    }



    private IEnumerator StartTaskBlinkingTimer(TaskData taskData, GameObject taskItem)
    {
        TMP_Text taskText = taskItem.GetComponentInChildren<TMP_Text>();
        DateTime taskDateTime = taskData.ScheduledDate.Date + taskData.ScheduledTime;
        TimeSpan timeUntilTask = taskDateTime - DateTime.Now;

        if (timeUntilTask.TotalSeconds > 0) // Start only if the task time is in the future
        {
            yield return new WaitForSeconds((float)timeUntilTask.TotalSeconds);


             PlayTaskAudio(taskData.Description);

            // Start blinking for 30 seconds
            yield return StartCoroutine(BlinkText(taskText, 30, 0.5f));
        }
    }


    private IEnumerator BlinkText(TMP_Text text, int duration, float blinkInterval)
    {
        float endTime = Time.time + duration;
        bool isRed = false;

        while (Time.time < endTime)
        {
            isRed = !isRed;
            text.color = isRed ? Color.red : Color.white;
            yield return new WaitForSeconds(blinkInterval);
        }

        text.color = Color.white; // Reset to default color
    }




    private void ConfirmTaskCompletion(GameObject taskItem, TaskData taskData)
    {
        taskToBeConfirmed = taskItem;
        confirmationDialogText.text = "Is '" + taskData.Description + "' done?";
        confirmationDialogPanel.SetActive(true);
    }

    public void CompleteTaskConfirmed()
    {
        if (taskToBeConfirmed != null)
        {
            CompleteTask(taskToBeConfirmed);
            UpdateProgressBar();
        }
        confirmationDialogPanel.SetActive(false);
    }

    private void CompleteTask(GameObject taskItem)
    {
        TaskData taskData = tasks.Find(t => t.Description == taskItem.GetComponentInChildren<TMP_Text>().text);

        if (taskData != null)
        {
            taskData.IsComplete = true;

            if (taskData.timerCoroutine != null)
            {
                StopCoroutine(taskData.timerCoroutine);
                taskItem.GetComponentInChildren<TMP_Text>().color = Color.white; // Reset color
            }

            RefreshTaskList();
            UpdateProgressBar();
        }
    }
    private void ResetTaskCompletionDaily()
    {
        DateTime today = DateTime.Today;

        foreach (var task in tasks)
        {
            if (task.IsRepetitive && !task.ScheduledDays.Contains(today.DayOfWeek))
            {
                task.IsComplete = false; // Reset completion status for tasks not due today
            }
            // For repetitive tasks that are due today, set them to incomplete at the start of the day
            if (task.IsRepetitive && task.ScheduledDays.Contains(today.DayOfWeek))
            {
                task.IsComplete = false;
                task.ScheduledDate = today + task.ScheduledTime;
            }
        }
    }


    public void CancelTaskCompletion()
    {
        taskToBeConfirmed = null;
        confirmationDialogPanel.SetActive(false);
    }

    private void UpdateProgressBar()
    {
        int totalTasks = tasks.Count;
        int completedTasks = tasks.FindAll(t => t.IsComplete).Count;

        CircularProgress progressBar = FindObjectOfType<CircularProgress>();
        if (progressBar != null)
        {
            progressBar.SetProgress(completedTasks, totalTasks);
        }
    }











    private TimeSpan? ParseTime(string timeString)
    {
        string[] parts = timeString.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int minutes))
        {
            return new TimeSpan(hours, minutes, 0);
        }
        else
        {
            // Not a valid time format or doesn't require time management
            return null;
        }
    }

    public GameObject daySelectionPanel;

    public void OnRepetitiveToggleChanged(bool isOn)
    {
        daySelectionPanel.SetActive(isOn);
        if (!isOn)
        {
            ResetDaySelection(); // Implement this method to reset the toggles if needed
        }
    }
    private void ResetDaySelection()
    {
        foreach (var toggle in dayToggles)
        {
            toggle.isOn = false; // Set each toggle to the off state
        }
    }

    public void FinalizeTaskCreation()
    {
        string taskDescription = taskInputField.text;
        bool isRepetitive = repetitiveTaskToggle.isOn;
        List<DayOfWeek> selectedDays = new List<DayOfWeek>();

        if (isRepetitive)
        {
            for (int i = 0; i < dayToggles.Length; i++)
            {
                if (dayToggles[i].isOn)
                {
                    selectedDays.Add((DayOfWeek)i);
                }
            }
        }

        TimeSpan? scheduledTimeNullable = ParseTime(timeDropdown.options[timeDropdown.value].text);
        TimeSpan scheduledTime = scheduledTimeNullable ?? TimeSpan.Zero;

        DateTime scheduledDate = DateTime.Now; 

        if (!string.IsNullOrEmpty(taskDescription))
        {
            TaskData newTask = new TaskData(taskDescription, isRepetitive, selectedDays, scheduledTime, scheduledDate);
            tasks.Add(newTask);
            CreateTaskItem(newTask);

            taskInputField.text = "";
            repetitiveTaskToggle.isOn = false;
            daySelectionPanel.SetActive(false);

            UpdateProgressBar();
        }
        else
        {
            Debug.LogError("Task description is empty.");
        }
    }

    public void HideDaySelectionPanel()
    {
        daySelectionPanel.SetActive(false);
    }


    private TaskFilter currentFilter = TaskFilter.Today; 


    private void RefreshTaskList()
    {
        foreach (Transform child in taskListParent)
        {
            Destroy(child.gameObject);
        }

        DateTime now = DateTime.Now;
        IEnumerable<TaskData> filteredTasks = tasks.Where(task => {
            if (currentFilter == TaskFilter.Past)
            {
                return task.IsComplete && (!task.IsRepetitive || !task.ScheduledDays.Contains(now.DayOfWeek));
            }
            else if (currentFilter == TaskFilter.Future)
            {
              
                return !task.IsComplete &&
                       ((!task.IsRepetitive && task.ScheduledDate > now) ||
                        (task.IsRepetitive && !task.ScheduledDays.Contains(now.DayOfWeek)));
            }
            else if (currentFilter == TaskFilter.Today)
            {
              
                return !task.IsComplete &&
                       ((!task.IsRepetitive && task.ScheduledDate.Date == now.Date) ||
                        (task.IsRepetitive && task.ScheduledDays.Contains(now.DayOfWeek)));
            }
            return false;
        });

        foreach (var task in filteredTasks)
        {
            CreateTaskItem(task);
        }
    }

    public enum TaskFilter
    {
        Past,
        Future,
        Today
    }



    private IEnumerator CheckForDateChange()
    {
        DateTime lastChecked = DateTime.Today;

        while (true)
        {
            if (DateTime.Today > lastChecked)
            {
                ResetTaskCompletionDaily();
                RefreshTaskList();
                lastChecked = DateTime.Today;
            }
            yield return new WaitForSeconds(3600);
        }
    }



    void TimeDropdownChanged(int index)
    {
        if (timeDropdown.options[index].text == "Add Specific Time")
        {
            timeInputPanel.SetActive(true); 
        }
    }

    private void Start()
    {
        StartCoroutine(CheckForDateChange());
        ResetTaskCompletionDaily();
        RefreshTaskList();
        UpdateProgressBar();
        timeDropdown.onValueChanged.AddListener(delegate { TimeDropdownChanged(timeDropdown.value); });
        timeInputPanel.SetActive(false);
        

    }


    public void ConfirmTimeEntry()
    {
        if (int.TryParse(hourInputField.text, out int hours) && int.TryParse(minuteInputField.text, out int minutes))
        {
            TimeSpan customTime = new TimeSpan(hours, minutes, 0);
            string customTimeString = customTime.ToString("hh':'mm");

            timeDropdown.options.Insert(2, new TMP_Dropdown.OptionData(customTimeString));

            timeDropdown.value = 2;

            timeDropdown.RefreshShownValue();

            timeInputPanel.SetActive(false); 
        }
        else
        {
            Debug.LogError("Invalid time input");
        }
    }



    public void ShowPastTasks()
    {
        currentFilter = TaskFilter.Past;
        RefreshTaskList();
    }

    public void ShowFutureTasks()
    {
        currentFilter = TaskFilter.Future;
        RefreshTaskList();
    }

    public void ShowTodayTasks()
    {
        currentFilter = TaskFilter.Today;
        RefreshTaskList();
    }

    private void FilterAndDisplayTasks(Func<TaskData, bool> filter)
    {
        foreach (Transform child in taskListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var task in tasks.Where(filter))
        {
            CreateTaskItem(task);
        }
    }
    public void OnSetTimerClicked()
    {
        timerSettingPanel.SetActive(true);
    }

    public GameObject timerSettingPanel;
    public TMP_InputField timerInputField;

    private IEnumerator StartTaskTimer(int durationInSeconds)
    {
        float timeRemaining = durationInSeconds;
        countdownTimerDisplay.gameObject.SetActive(true); 

        while (timeRemaining > 0)
        {
            countdownTimerDisplay.text = FormatTime(timeRemaining);
            yield return new WaitForSeconds(1);
            timeRemaining--;
        }

        yield return StartCoroutine(BlinkText(30, 0.5f));

        countdownTimerDisplay.gameObject.SetActive(false); 
    }
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private IEnumerator BlinkText(int duration, float blinkInterval)
    {
        float endTime = Time.time + duration;
        bool isRed = false; 

        while (Time.time < endTime)
        {
            isRed = !isRed; 
            countdownTimerDisplay.color = isRed ? Color.red : Color.white;
            countdownTimerDisplay.gameObject.SetActive(true);
            yield return new WaitForSeconds(blinkInterval);

            countdownTimerDisplay.gameObject.SetActive(false);
            yield return new WaitForSeconds(blinkInterval);
        }

        countdownTimerDisplay.gameObject.SetActive(false); 
        countdownTimerDisplay.color = Color.white; 
    }


    public void OnStartTimerClicked()
    {
        if (int.TryParse(timerInputField.text, out int duration))
        {
            StartCoroutine(StartTaskTimer(duration * 60)); // Convert minutes to seconds
            timerSettingPanel.SetActive(false); // Hide the timer setting panel
        }
        else
        {
            Debug.LogError("Invalid timer input");
        }
    }



    
    
   
    public void PlayTaskAudio(string taskDescription)
    {
        StartCoroutine(DownloadAndPlayAudio(taskDescription));
        
    }

    IEnumerator DownloadAndPlayAudio(string textToSpeak)
    {
        string url = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&q=" + WWW.EscapeURL(textToSpeak) + "&tl=en";
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error in downloading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                audioSource.clip = clip;
                if (clip != null)
                {
                    // Debugging Information
                    Debug.Log("Attempting to play audio clip");
                    Debug.Log("Audio Clip Name: " + clip.name);
                    Debug.Log("Audio Source Volume: " + audioSource.volume);
                    Debug.Log("Audio Source Mute: " + audioSource.mute);
                    Debug.Log("Audio Source Play On Awake: " + audioSource.playOnAwake);
                    Debug.Log("Audio Source Loop: " + audioSource.loop);
                    Debug.Log("Audio Source is Active and Enabled: " + audioSource.isActiveAndEnabled);

                    audioSource.Play();
                   

                    Debug.Log("Audio Source is Playing: " + audioSource.isPlaying);
                }
                else
                {
                    Debug.LogError("Downloaded clip is null");
                }
            }
        }
    }








}




