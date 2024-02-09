using UnityEngine;

public class CircularProgress : MonoBehaviour
{

    //progress bar
    public void SetProgress(int completedTasks, int totalTasks)
    {
        float progress = (totalTasks > 0) ? (float)completedTasks / totalTasks : 0f;
        gameObject.GetComponent<Renderer>().material.SetFloat("_Progress", progress);
    }
}