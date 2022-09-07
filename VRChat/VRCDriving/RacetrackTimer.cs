
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.UI;

public class RacetrackTimer : UdonSharpBehaviour
{
    public Checkpoint[] checkpoints;
    TimeSpan start;
    int currCheckpoint;
    int totalCheckpoint;
    public bool isStarted;
    TimeSpan bestTime;

    public Text timerText;

    void Start()
    {
        currCheckpoint = -1;
        totalCheckpoint = checkpoints.Length;
        for (int i = 0; i < checkpoints.Length; ++i)
        {
            checkpoints[i].checkpointNum = i;
            checkpoints[i].timer = this;
        }
        isStarted = false;
        start = new TimeSpan(0);
    }

    public void PassCheckpoint(int curr)
    {
        if (curr == currCheckpoint)
            return;

        if (curr == 0)
        {
            currCheckpoint = 0;
            start = TimeSpan.Zero;
            isStarted = true;
            return;
        }

        if ((currCheckpoint + 1) == curr)
        {
            currCheckpoint = curr;
        }

        if (currCheckpoint == totalCheckpoint-1)
        {
            isStarted = false;
            if (bestTime == TimeSpan.Zero || start < bestTime)
                bestTime = start;
            timerText.text = "Lap time: " + start.ToString(@"mm\:ss\:fff") + "\nBest time: " + bestTime.ToString(@"mm\:ss\:fff");
            currCheckpoint = -1;
        }
    }

    private void Update()
    {
        if (isStarted)
        {
            start = start.Add(new TimeSpan(0, 0, 0, 0, (int)(Time.deltaTime * 1000)));
            timerText.text = "Lap time: " + start.ToString(@"mm\:ss\:fff") + "\nBest time: " + bestTime.ToString(@"mm\:ss\:fff");
        }
    }
}
