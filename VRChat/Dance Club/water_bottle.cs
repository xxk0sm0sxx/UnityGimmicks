
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class water_bottle : UdonSharpBehaviour
{
    public ParticleSystem particle;

    void Start()
    {

    }

    public override void OnPickupUseDown()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayParticle));
    }

    public override void OnPickupUseUp()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(StopParticle));
    }

    public void PlayParticle()
    {
        particle.Play();
    }

    public void StopParticle()
    {
        particle.Stop();
    }
}