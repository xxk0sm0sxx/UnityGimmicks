
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SplashParent : UdonSharpBehaviour
{
    public Splash[] pool;

    void Start()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = transform.GetChild(i).GetComponent<Splash>();
        }
    }

    public void SpawnSplash(Vector3 pos,  int id)
    {
        int playerCount = VRCPlayerApi.GetPlayerCount();
        int max = pool.Length / playerCount;
        for (int i = 0; i < max; ++i)
        {
            int j = id * max + i;
            while (j >= pool.Length)
                j -= pool.Length;
            
            if (pool[j].Active() == false)
            {
                pool[j].Spawn(pos);
                break;
            }
        }
    }
}
