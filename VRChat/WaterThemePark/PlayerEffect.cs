
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

//Note: Player effect script for pool world. Toggles underwater effect and splashes.
public class PlayerEffect : UdonSharpBehaviour
{
    Vector3 headPos1;
    Vector3 headPos2;

    float elapsedTime;
    [SerializeField] public float updateWait;

    Vector3 leftPos1;
    Vector3 leftPos2;

    Vector3 rightPos1;
    Vector3 rightPos2;

    public GameObject waterHeightObj;
    float waterHeight;
    public SplashParent bigSplashParent;
    public SplashParent smallSplashParent;

    Vector3 lookRay;

    bool isUnderwater;
    public AudioSource underwaterSound;
    public AudioSource swimSound;

    void Start()
    {
        leftPos1 = leftPos2 = rightPos1 = rightPos2 = headPos1 = headPos2 = new Vector3(0, 10, 0);

        elapsedTime = 0.0f;
        waterHeight = waterHeightObj.transform.position.y;

        isUnderwater = false;
        underwaterSound.Stop();
    }

    private void FixedUpdate()
    {
        transform.position = Networking.LocalPlayer.GetPosition();

        elapsedTime += Time.deltaTime;
        if (elapsedTime > updateWait)
        {
            elapsedTime -= updateWait;

            headPos2 = headPos1;
            headPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

            leftPos2 = leftPos1;
            leftPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

            rightPos2 = rightPos1;
            rightPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

			//Creates water splash based on motion
            CalculateSplash();
        }

        CheckIsUnderwater();
    }

    void CheckIsUnderwater()
    {
        if (isUnderwater == false && headPos1.y <= waterHeight)
        {
            isUnderwater = true;
            underwaterSound.Play();
            return;
        }

        if (isUnderwater == true && headPos1.y > waterHeight)
        {
            isUnderwater = false;
            underwaterSound.Stop();
        }
    }

    void CalculateSplash()
    {
        if (leftPos2.y > waterHeight && leftPos1.y <= waterHeight)
        {
            Vector3 splashPos = leftPos1;
            splashPos.y = waterHeight;
            smallSplashParent.SpawnSplash(splashPos, Networking.LocalPlayer.playerId);
        }

        if (rightPos2.y > waterHeight && rightPos1.y <= waterHeight)
        {
            Vector3 splashPos = rightPos1;
            splashPos.y = waterHeight;
            smallSplashParent.SpawnSplash(splashPos, Networking.LocalPlayer.playerId);
        }
        
        if (headPos2.y > waterHeight && headPos1.y <= waterHeight)
        {
            Vector3 force = headPos1 - headPos2;
            if (force.sqrMagnitude > 0.1)
            {
                Vector3 splashPos = headPos1;
                splashPos.y = waterHeight;
                bigSplashParent.SpawnSplash(splashPos, Networking.LocalPlayer.playerId);
            }
        }

        if (headPos2.y <= waterHeight && headPos1.y > waterHeight)
        {
            Vector3 force = headPos1 - headPos2;
            if (force.sqrMagnitude > 0.1)
            {
                Vector3 splashPos = headPos1;
                splashPos.y = waterHeight;
                bigSplashParent.SpawnSplash(splashPos, Networking.LocalPlayer.playerId);
            }
        }
    }

    public override void InputJump(bool value, UdonInputEventArgs args)
    {
		//Press jump to swim in water
        if (Networking.LocalPlayer.GetPosition().y <= waterHeight )
        {
            Vector3 v = Networking.LocalPlayer.GetVelocity();
            Networking.LocalPlayer.SetVelocity(new Vector3(v.x, 2.0f, v.z));

            if (isUnderwater)
            {
                swimSound.Play();
            }
        }
    }
}
