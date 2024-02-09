using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

public class ButtonController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay; 
    public Canvas mainCanvas; 

    private List<GameObject> activeObjects;

    private void Start()
    {
        activeObjects = new List<GameObject>();

        videoPlayer.loopPointReached += EndReached;
    }

    public void OnButtonClick()
    {
        if (videoPlayer.isPlaying)
        {
            EndReached(videoPlayer);
        }
        else
        {
            PlayVideo();
        }
    }

    private void PlayVideo()
    {
       

        // Deactivate all active objects except the video display and this button
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            if (go.activeSelf && go != videoDisplay.gameObject && go != gameObject)
            {
                activeObjects.Add(go);
                go.SetActive(false);
            }
        }
        videoDisplay.gameObject.SetActive(true);
        videoPlayer.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(true);

        videoPlayer.Play();
    }

    private void EndReached(VideoPlayer vp)
    {
        // Reactivate all previously active objects
        foreach (var go in activeObjects)
        {
            go.SetActive(true);
        }
        activeObjects.Clear();

        // Deactivate video display and stop the video
        videoDisplay.gameObject.SetActive(false);
        videoPlayer.Stop();
    }
}
