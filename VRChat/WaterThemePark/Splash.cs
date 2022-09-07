
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Note: Networked object pool for networked water splash effect
public class Splash : UdonSharpBehaviour
{
    Vector3 newPos;
    double elapsedTime;
    public GameObject model;
    bool localActive;

    [UdonSynced] Vector3 startPosition;
    [UdonSynced] bool active;
    [UdonSynced] double startTime;
    [UdonSynced] int owner;

    void Start()
    {
        localActive = false;
        active = false;
        model.SetActive(false);
    }

    public bool Active()
    {
        return localActive;
    }

    public override void OnDeserialization()
    {
        if (active == true && localActive == false)
        {
            transform.position = startPosition;
            localActive = true;
            model.SetActive(true);
        }
        if (active == false && localActive == true)
        {
            Free();
        }
    }

    private void Update()
    {
        if (active == true && localActive == false)
        {
            transform.position = startPosition;
            localActive = true;
            model.SetActive(true);
        }
        if (active == false && localActive == true)
        {
            Free();
        }

        if (localActive)
        {
            elapsedTime = Networking.GetServerTimeInSeconds() - startTime;
            
            if (elapsedTime > 2.0f)
            {
                Free();
            }
        }
    }

    void Free()
    {
        if (active == false)
            return;

        localActive = false;
        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            active = false;
        model.SetActive(false);

        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            RequestSerialization();
        }
    }

    public void Spawn(Vector3 in_startPosition)
    {
        if (Networking.GetOwner(gameObject) != Networking.LocalPlayer)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        owner = Networking.LocalPlayer.playerId;
        startTime = Networking.GetServerTimeInSeconds();
        startPosition = in_startPosition;
        transform.position = startPosition;
        active = true;
        localActive = true;
        model.SetActive(true);

        RequestSerialization();
    }
}
