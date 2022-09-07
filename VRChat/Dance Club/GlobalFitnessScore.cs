
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

//Note: This script networks the fitness scores of all players
public class GlobalFitnessScore : UdonSharpBehaviour
{
	//A global pool of local scores for each joiner
    public LocalFitnessScore[] lfs;
    public FitnessCounter fs;
    public int iter;
    public float localScore;
    public float globalScore;
    public float accumScore;

    public int localId;

    public int maxId;

    public int shownOwnScore;
    public int shownTeamScore;

    public ParticleSystem ps;

    [SerializeField] public Text localPointText;
    [SerializeField] public Text globalPointText;

    void Start()
    {
        for (int i = 0; i < lfs.Length; i++)
        {
            lfs[i] = transform.GetChild(i).GetComponent<LocalFitnessScore>();
            lfs[i].idx = i;
        }

        iter = 0;
        localScore = 0;
        globalScore = 0;
        accumScore = 0;
        maxId = 0;
        localId = 999;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer == player)
        {
            //Debug.Log("OnPlayerJoined playerId: " + Networking.LocalPlayer.playerId.ToString() + " localId: " + localId.ToString());
            AssignLFS(player.playerId);
        }

        if (player.playerId > maxId)
            maxId = player.playerId;
    }

    public void AssignLFS(int id)
    {
        if (id < lfs.Length)
        {
            if (id != localId)
            {
                //Debug.Log("AssignLFS playerId: " + Networking.LocalPlayer.playerId.ToString() + " id: " + id.ToString());
                lfs[id].Assign();
                localId = id;
            }
        }
    }

    public void UpdateScore(float addScore)
    {
        if (localId >= lfs.Length)
            return;
        
        //Debug.Log("UpdateScore playerId: " + Networking.LocalPlayer.playerId.ToString() + " localId: " + localId.ToString() + " LFS id: " + lfs[localId].ownerId.ToString());

        localScore += addScore;
        lfs[localId].UpdateScore(localScore);
    }

    private void FixedUpdate()
    {
        if (lfs[iter].Active())
            accumScore += lfs[iter].Score();

        iter++;

        if (iter > maxId || iter >= lfs.Length)
        {
            globalScore = accumScore;
            accumScore = 0;
            iter = 0;

            int shownScore = (int)(localScore);
            shownScore *= 10;
            localPointText.text = "Your Fitness Score\n君の運動スコア\n \n< " + shownScore.ToString() + "点 > ";

            shownScore = (int)(globalScore);
            shownScore *= 10;
            shownTeamScore = shownScore;
            globalPointText.text = "Team Fitness Score\nみんなの運動スコア\n \n< "+ shownScore.ToString() + " 点 > ";
        }
    }
}
