
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BlueSlideTeleport : UdonSharpBehaviour
{
    public GameObject point;
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.LocalPlayer.TeleportTo(point.transform.position, point.transform.rotation);
    }
}
