
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SlideRing : UdonSharpBehaviour
{
    public Animator animator;

    void Start()
    {
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        animator.Play("Base Layer.BlueSlide", 0, 0);
    }

    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }
}
