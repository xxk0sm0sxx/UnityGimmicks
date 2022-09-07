
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class AutoPlayManager : UdonSharpBehaviour
{
    [SerializeField] public Kinel.VideoPlayer.Scripts.KinelVideoScript videoPlayer;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist1;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist2;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist3;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist4;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist5;
    [SerializeField] public Kinel.VideoPlayer.Scripts.Playlist.KinelPlaylist playlist6;

    [SerializeField] public Text LoopText;
    [SerializeField] public Text AutoText;
    [SerializeField] public Text RestTimeText;

    [UdonSynced] bool bRepeat = false;
    [UdonSynced] int mode = 0;
    [UdonSynced] int restTime = 1;

    void Start()
    {
    }

    public override void OnDeserialization()
    {
        if (Networking.IsOwner(this.gameObject))
            return;

        videoPlayer.GetVideoPlayer().Loop = bRepeat;

        SetRepeatText();
        SetModeText();
        SetRestTimeText();
    }

    void SetRepeatText()
    {
        if (bRepeat)
            LoopText.text = "Loop: On";
        else
            LoopText.text = "Loop: Off";
    }

    void SetModeText()
    {
        if (mode == 0)
            AutoText.text = "Auto Mode: Off";
        else if (mode == 1)
            AutoText.text = "Auto Mode: Dance (Normal)";
        else if (mode == 2)
            AutoText.text = "Auto Mode: Para Para";
        else if (mode == 3)
            AutoText.text = "Auto Mode: Fit Dance";
        else if (mode == 4)
            AutoText.text = "Auto Mode: Dance Evo";
        else if (mode == 5)
            AutoText.text = "Auto Mode: Jigoku";
        else if (mode == 6)
            AutoText.text = "Auto Mode: Dance (Easy)";
    }

    void SetRestTimeText()
    {
        if (restTime == 0)
            RestTimeText.text = "Rest time\n60s";
        else if (restTime == 1)
            RestTimeText.text = "Rest time\n40s";
        else if (restTime == 2)
            RestTimeText.text = "Rest time\n10s";
    }

    public bool GetNextVideo()
    {
        if (!Networking.LocalPlayer.isMaster)
            return false;

        if (doRepeat())
        {
            return true;
        }

        if (mode == 0)
            return false;

        SpecifyRestTime();

        if (mode == 1)
            playlist1.PlayRandomVideo();

        if (mode == 2)
            playlist2.PlayRandomVideo();

        if (mode == 3)
            playlist3.PlayRandomVideo();

        if (mode == 4)
            playlist4.PlayRandomVideo();

        if (mode == 5)
            playlist5.PlayRandomVideo();

        if (mode == 6)
            playlist6.PlayRandomVideo();

        return true;
    }

    public bool doRepeat()
    {
        return bRepeat;
    }

    public void ToggleRepeat()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        bRepeat = !bRepeat;
        videoPlayer.GetVideoPlayer().Loop = bRepeat;
        RequestSerialization();
        SetRepeatText();
    }

    public void ToggleRestTime()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        restTime += 1;
        if (restTime > 2)
            restTime = 0;

        RequestSerialization();
        SetRestTimeText();
    }

    void SpecifyRestTime()
    {
        if (restTime == 0)
            videoPlayer.videoMessageTimer = 60;
        else if (restTime == 1)
            videoPlayer.videoMessageTimer = 40;
        else if (restTime == 2)
            videoPlayer.videoMessageTimer = 10;
    }

    public void SetNoPlaylist()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 0;
        RequestSerialization();
        SetModeText();
    }

    public void SetPlaylist1()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 1;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist1.PlayRandomVideo();
    }

    public void SetPlaylist2()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 2;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist2.PlayRandomVideo();
    }

    public void SetPlaylist3()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 3;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist3.PlayRandomVideo();
    }

    public void SetPlaylist4()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 4;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist4.PlayRandomVideo();
    }

    public void SetPlaylist5()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 5;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist5.PlayRandomVideo();
    }

    public void SetPlaylist6()
    {
        if (!Networking.LocalPlayer.isMaster)
            return;

        mode = 6;
        RequestSerialization();
        SetModeText();
        videoPlayer.videoMessageTimer = 10;
        playlist6.PlayRandomVideo();
    }
}
