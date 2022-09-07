
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Note: This script adjusts all players' voice based on room to reduce lag from voice processing.
//Because the event world runs on Quest with 40 players, voice should be kept nearer if player count is high.
public class VoiceAdjustment : UdonSharpBehaviour
{
    public BoxCollider[] voiceCollider;
    int playerIter;
    int idLimit;
    int playerCount;
    int activePlayers;

    bool checkInCollider;

    [SerializeField] public Kinel.VideoPlayer.Scripts.KinelVideoScript videoPlayer;

    private void Start()
    {
        playerIter = 0;
        activePlayers = 0;
        checkInCollider = true;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.playerId > idLimit)
        {
            idLimit = player.playerId;
        }

        playerCount = VRCPlayerApi.GetPlayerCount();
    }

    private void FixedUpdate()
    {
		//Instead of running a for loop here, only one player is processed every loop to reduce lag
        if (playerIter > idLimit)
        {
            playerIter = 0;
            checkInCollider = !checkInCollider;

            if (checkInCollider == true)
            {
                activePlayers = 0;
            }
        }
        
        VRCPlayerApi currPlayer = VRCPlayerApi.GetPlayerById(playerIter);
        if (currPlayer == null)
        {
            playerIter++;
            return;
        }
        
        if (videoPlayer.videoStartedPlaying == false || playerCount < 20)
        {
            currPlayer.SetVoiceDistanceNear(0);
            currPlayer.SetVoiceDistanceFar(60);
        }
        else
        {
            if (currPlayer.isMaster)
            {
                currPlayer.SetVoiceDistanceNear(3);
                currPlayer.SetVoiceDistanceFar(60);
                playerIter++;
                return;
            }

            if (currPlayer.isLocal)
            {
                currPlayer.SetVoiceDistanceNear(0);
                currPlayer.SetVoiceDistanceFar(60);
                playerIter++;
                return;
            }

            Vector3 localPos = Networking.LocalPlayer.GetPosition();
            Vector3 playerPos = currPlayer.GetPosition();

            if (checkInCollider)
            {
                for (int i = 0; i < voiceCollider.Length; ++i)
                {
                    if (voiceCollider[i].bounds.Contains(localPos) &&
                        voiceCollider[i].bounds.Contains(playerPos))
                    {
                        if (playerCount <= 30 && activePlayers < 20)
                        {
                            currPlayer.SetVoiceDistanceNear(0);
                            currPlayer.SetVoiceDistanceFar(60);
                            ++activePlayers;
                        }
                        else if (playerCount <= 40 && activePlayers < 12)
                        {
                            currPlayer.SetVoiceDistanceNear(0);
                            currPlayer.SetVoiceDistanceFar(60);
                            ++activePlayers;
                        }
                        else
                        {
                            currPlayer.SetVoiceDistanceNear(1);
                            currPlayer.SetVoiceDistanceFar(5);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < voiceCollider.Length; ++i)
                {
                    if (voiceCollider[i].bounds.Contains(localPos) == false &&
                        voiceCollider[i].bounds.Contains(playerPos) == true)
                    {
                        if (playerCount <= 30 && activePlayers < 20)
                        {
                            currPlayer.SetVoiceDistanceNear(0);
                            currPlayer.SetVoiceDistanceFar(60);
                            ++activePlayers;
                        }
                        else if (playerCount <= 40 && activePlayers < 12)
                        {
                            currPlayer.SetVoiceDistanceNear(0);
                            currPlayer.SetVoiceDistanceFar(60);
                            ++activePlayers;
                        }
                        else
                        {
                            currPlayer.SetVoiceDistanceNear(0);
                            currPlayer.SetVoiceDistanceFar(0);
                        }
                    }
                }
            }
        }
        playerIter++;
    }
}
