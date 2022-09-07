
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NightToggle : UdonSharpBehaviour
{
    [UdonSynced]
    bool isNight;

    [SerializeField] public Material skyMaterial;
    [SerializeField] public Texture dayTexture;
    [SerializeField] public Texture nightTexture;

    [SerializeField] public Material[] materials;
    public Color[] colors;

    private void Start()
    {
        isNight = false;

        for (int i = 0; i < materials.Length; i++)
        {
            colors[i] = materials[i].GetColor("_Color");
        }
    }

    // Prevents people who are not the master from taking ownership
    public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
    {
        return requestedOwner.isMaster;
    }

    void ToggleNight()
    {
        if (isNight == false)
        {
            skyMaterial.mainTexture = dayTexture;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetColor("_Color", colors[i]);
            }
            return;
        }

        skyMaterial.mainTexture = nightTexture;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_Color", colors[i] * 0.25f);
        }
        return;
    }

    public override void OnDeserialization()
    {
        ToggleNight();
    }

    public override void Interact()
    {
        if (!Networking.IsMaster)
            return;
        else if (!Networking.IsOwner(gameObject)) // The object may have transfer ownership on collision checked which would allow people to take ownership by accident
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        isNight = !isNight;

        ToggleNight();

        RequestSerialization();
    }
}
