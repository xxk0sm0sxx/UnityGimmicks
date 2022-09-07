
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HeightAdjust : UdonSharpBehaviour
{
    Vector3 oriPos;

    private void Start()
    {
        oriPos = transform.position;
    }

    private void FixedUpdate()
    {
        var headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        float y = headData.position.y;
        transform.position = new Vector3(oriPos.x, y, oriPos.z);
    }
}
