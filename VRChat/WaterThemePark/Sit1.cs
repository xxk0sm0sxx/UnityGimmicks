
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Sit1 : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }
}
