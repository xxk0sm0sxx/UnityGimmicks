
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorActivate : UdonSharpBehaviour
{
    [SerializeField] public bool initialState;

    private void Start()
    {
        transform.GetChild(0).gameObject.SetActive(initialState);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if(player.isLocal)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
