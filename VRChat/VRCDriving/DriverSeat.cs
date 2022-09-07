
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DriverSeat : UdonSharpBehaviour
{
    public DriftCar driftCar;
    Vector3 originalPos;
    bool seated;
    public float targetHeight;

    VRCPlayerApi seatedPlayer;
    public bool canDrive;

    void Start()
    {
        originalPos = transform.localPosition;
        seated = false;
    }

    public void AutoAdjustHeight()
    {
        Vector3 globalHead = seatedPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Vector3 localHead = transform.parent.worldToLocalMatrix.MultiplyPoint(globalHead);

        Vector3 newPos = transform.localPosition;
        newPos.y += targetHeight - localHead.y;
        newPos.z += transform.localPosition.z - localHead.z;
        Debug.Log(globalHead.ToString() + " : " + localHead.ToString());
        transform.localPosition = newPos;
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        base.OnStationEntered(player);
        seatedPlayer = player;
        SendCustomEventDelayedSeconds("AutoAdjustHeight", 1.0f);

        if (canDrive == false)
            return;
        if (player.isLocal)
        {
            driftCar.Handle();
            seated = true;
        }
        else
        {
            driftCar.HandleByOther();
        }
    }

    public override void OnStationExited(VRCPlayerApi player)
    {
        base.OnStationExited(player);
        seatedPlayer = null;
        transform.localPosition = originalPos;

        if (canDrive == false)
            return;

        driftCar.Stop();
        seated = false;
    }

    private void FixedUpdate()
    {
        if (seated == false)
            return;
    }
}