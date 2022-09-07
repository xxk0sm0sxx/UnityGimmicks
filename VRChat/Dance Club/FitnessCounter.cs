
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Note: This script calculates fitness scores based on hand movement
public class FitnessCounter : UdonSharpBehaviour
{
    public Vector3 leftPos1;
    Vector3 leftPos2;
    Vector3 leftPos3;
    Vector3 leftForce1;
    Vector3 leftForce2;

    public Vector3 rightPos1;
    Vector3 rightPos2;
    Vector3 rightPos3;
    Vector3 rightForce1;
    Vector3 rightForce2;

    public Vector3 headPos1;
    Vector3 headPos2;
    Vector3 headPos3;
    Vector3 headForce1;
    Vector3 headForce2;

    public float avgHeadHeight;
    public float squatHeight;
    public float betweenHeight;
    public float multiplier;
    public float headHeightRatio;

    public float elapsedTime;
    [SerializeField] public float updateWait;
    [SerializeField] public float interpWeight;

    public float interp;
    public GlobalFitnessScore gfs;

    public int iter;

    void Start()
    {
        leftPos1 = leftPos2 = leftPos3 = rightPos1 = rightPos2 = rightPos3 =
            leftForce1 = leftForce2 = rightForce1 = rightForce2 =
            headPos1 = headPos2 = headPos3 = headForce1 = headForce2 = new Vector3(0, 0, 0);

        avgHeadHeight = 2.0f;
        squatHeight = 2.0f;
        betweenHeight = 0;

        elapsedTime = 0.0f;

        interp = interpWeight * updateWait;

        iter = 0;
    }

    void UpdateTracking0()
    {
		//Get tracking data from controllers
        headPos3 = headPos2;
        headPos2 = headPos1;
        headPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        headForce2 = headPos2 - headPos3;
        headForce1 = headPos1 - headPos2;

        leftPos3 = leftPos2;
        leftPos2 = leftPos1;
        leftPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position - headPos1;

        leftForce2 = leftPos2 - leftPos3;
        leftForce1 = leftPos1 - leftPos2;

        rightPos3 = rightPos2;
        rightPos2 = rightPos1;
        rightPos1 = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position - headPos1;

        rightForce2 = rightPos2 - rightPos3;
        rightForce1 = rightPos1 - rightPos2;
    }

    void UpdateTracking1()
    {
        avgHeadHeight = (avgHeadHeight * (1.0f - interp)) + ((headPos1.y * 1.0f) * (interp));
        squatHeight = avgHeadHeight * 0.5f; //スクワットは身長の半分まで！！
        betweenHeight = squatHeight;

        //スクワットで界王拳
        headHeightRatio = 1.0f - ((headPos1.y - squatHeight) / betweenHeight);
        headHeightRatio = headHeightRatio < 0.0f ? 0.0f : headHeightRatio;
        multiplier = (headHeightRatio * 8.0f) + ((1.0f - headHeightRatio) * 1.0f); //身長半分まで近づいてると8倍界王拳！！
    }

    void UpdateTracking2()
    {
        //適当な計算、ルート無視
        float leftAcceleration = (leftForce2 - leftForce1).sqrMagnitude;
        float rightAcceleration = (rightForce2 - rightForce1).sqrMagnitude;
        float headAcceleration = (headForce2 - headForce1).sqrMagnitude;

        //頭の回転が物凄い増倍しないと影響がない
        float point = (multiplier * ((leftAcceleration * 20.0f) + (rightAcceleration * 20.0f) + (headAcceleration * 1000.0f) +
            (leftForce1.sqrMagnitude) + (rightForce1.sqrMagnitude) + (headForce1.sqrMagnitude * 500.0f)));

        //丁度気持ちよさそうな点数調整
        
        gfs.UpdateScore(point * 0.06f);
    }

    private void FixedUpdate()
    {
		//Perform processing across multiple frames
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime > updateWait)
        {
            if (iter == 0)
            {
                UpdateTracking0();
                iter += 1;
            }
            else if (iter == 1)
            {
                if (headForce1.sqrMagnitude > 0.01f || headForce2.sqrMagnitude > 0.01f ||
                    leftForce1.sqrMagnitude > 1.0f || leftForce2.sqrMagnitude > 1.0f ||
                    rightForce1.sqrMagnitude > 1.0f || rightForce2.sqrMagnitude > 1.0f)
                {
                    iter = 0;
                    elapsedTime -= updateWait;
                    return;
                }

                iter += 1;
            }
            else if (iter == 2)
            {
                UpdateTracking1();
                iter += 1;
            }
            else
            {
                UpdateTracking2();
                iter = 0;
                elapsedTime -= updateWait;
            }
        }
    }
}