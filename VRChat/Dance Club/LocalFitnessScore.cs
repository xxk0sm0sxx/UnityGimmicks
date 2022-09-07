
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LocalFitnessScore : UdonSharpBehaviour
{
    public bool localActive;

    [UdonSynced] public bool active;
    public int idx;
    [UdonSynced] public int ownerId;

    [UdonSynced] public string ownerName;
    [UdonSynced] public float score;
    public float pingScore;

    public GlobalFitnessScore gfs;
    ParticleSystem.EmitParams param;

    void Start()
    {
        localActive = false;
        active = false;
        ownerName = "";
        score = 0;
        gfs = transform.parent.GetComponent<GlobalFitnessScore>();
        ownerId = -1;
        pingScore = 0;
        param = new ParticleSystem.EmitParams();
        param.applyShapeToPosition = true;
    }

    public bool Active()
    {
        return localActive;
    }

    public float Score()
    {
        return score;
    }

    public override void OnDeserialization()
    {
        if (ownerId == -1)
        {
            //Debug.Log("OnDeserialization OwnerID -1: playerId: " + Networking.LocalPlayer.playerId.ToString() + " idx: " + idx.ToString() + " ownerId: " + ownerId.ToString() + " pingScore: " + pingScore.ToString());
            return;
        }
        if (active == true && localActive == false)
        {
            localActive = true;
        }
        if (active == false && localActive == true)
        {
            Free();
        }

        if (localActive && ownerId < Networking.LocalPlayer.playerId &&
            ownerName.Length > 0 && ownerName == Networking.LocalPlayer.displayName)
        {
            gfs.localScore = score;
            gfs.AssignLFS(idx);
            //Debug.Log("OnDeserialization playerId: " + Networking.LocalPlayer.playerId.ToString() + " ownerId: " + ownerId.ToString());
        }

        int intScore = (int)(score / 3.0f);
        //Debug.Log("DeserializeEmit playerId: " + Networking.LocalPlayer.playerId.ToString() + " idx: " + idx.ToString() + " ownerId: " + ownerId.ToString() + " pingScore: " + pingScore.ToString() + " intScore: " + intScore.ToString());
        if (pingScore < (intScore * 3))
        {
            Vector3 pos = VRCPlayerApi.GetPlayerById(ownerId).GetPosition();
            pingScore = intScore * 3;
            
            param.position = pos;
            gfs.ps.Emit(param, 1);

        }
    }

    public void UpdateScore(float in_score)
    {
        score = in_score;

        int intScore = (int)(score / 3.0f);
        if (pingScore < (intScore * 3))
        {
            if (ownerId != gfs.localId)
            {
                gfs.AssignLFS(idx);
                return;
            }
            //Debug.Log("ReqSeq playerId: " + Networking.LocalPlayer.playerId.ToString() + " idx: " + idx.ToString() + " ownerId: " + ownerId.ToString() + " pingScore: " +pingScore.ToString() + " intScore: " + intScore.ToString());
            Vector3 pos = VRCPlayerApi.GetPlayerById(ownerId).GetPosition();
            pingScore = intScore * 3;

            param.position = pos;
            gfs.ps.Emit(param, 1);
            RequestSerialization();
        }
    }

    void Free()
    {
        if (active == false)
            return;

        localActive = false;
        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
            active = false;

        if (Networking.GetOwner(gameObject) == Networking.LocalPlayer)
        {
            //Debug.Log("Free ReqSeq playerId: " + Networking.LocalPlayer.playerId.ToString() + " idx: " + idx.ToString() + " ownerId: " + ownerId.ToString());

            RequestSerialization();
        }
    }

    public void Assign()
    {
        if (Networking.GetOwner(gameObject) != Networking.LocalPlayer)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        ownerName = Networking.LocalPlayer.displayName;
        ownerId = Networking.LocalPlayer.playerId;
        active = true;
        localActive = true;
        //Debug.Log("AssignReqSeq playerId: " + Networking.LocalPlayer.playerId.ToString() + " idx: " + idx.ToString() + " ownerId: " + ownerId.ToString());

        RequestSerialization();
    }
}
