
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

//Note: This script not only normalizes volume, but also badly estimate a rough BPM for the current song
//This uses volume to attempt to detect beats, since FFT is too heavy for Quest users. This runs great on Quest.
//This also does processing across multiple frames for speed.
public class SoundNormalize : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public float[] samples;
    public float targetLevel;
    public float range;
    public float vol;
    public float avgVol;
    public float currLevel;
    public float output;

    public float bpm;
    public float bpmInterp;
    public float bpmInterpOffset;
    public float minBpm;
    public float maxBpm;
    public float bpmDt;
    public float bpmInterpDt;
    public float currBPS;
    public bool forward;

    public float mode;

    public Text text;

    public Transform[] gameObjs;
    public int iter;

    public float elapsedTime;
    [SerializeField] public float updateWait;

    public float avgMax;
    public float interp1;
    public float interp2;

    public float change;
    public float diff;

    public float currentBpm;

    void Start()
    {
        samples = new float[8];
        vol = 1.0f;
        avgVol = targetLevel;
        bpmDt = 0;
        currBPS = 0;
        bpmInterpOffset = 0;
        mode = 0;
        bpmInterp = 0;
        iter = 0;

        elapsedTime = 0;

        interp1 = 4.0f * updateWait;
    }

    void CalculateBPM()
    {
        currBPS = (60.0f / bpm) * 4.0f;

        //if (bpmInterpDt/currBPS < 0.5f)
        //{
        //    bpmInterpOffset = bpmInterpDt;
        //}
        //else
        //{
        //    bpmInterpOffset = bpmInterpDt - currBPS;
        //}
        bpmInterpOffset = bpmInterpOffset * 0.9f + bpmInterpDt * 0.1f;
    }

    void ChangeSize()
    {
        change = 0.0f;
        if (avgVol > 0.001f)
        {
            change = currLevel / (avgVol*1.5f);
            if (change > 1.0f)
            {
                change = 1.0f;
                CalculateBPM();
            }
            else
            {
                change = 0;
            }
        }
    }

    void Update1()
    {
        audioSource.GetOutputData(samples, 1);

        avgMax = 0;
        for (int i = 0; i < samples.Length; ++i)
        {
            avgMax += samples[i] * samples[i];
        }
    }

    void Update2()
    {
        avgMax /= samples.Length;
        avgMax = Mathf.Sqrt(avgMax);

        avgVol = avgVol * (1.0f - interp1) + avgMax * interp1;

        diff = avgVol - targetLevel;

        currLevel = avgMax;

        if (bpmInterpDt > (currBPS * 2))
        {
            bpmInterpDt -= (currBPS * 2);
            if (avgVol < 0.15f)
            {
                mode = 0;
            }
            else
            {
                mode = 1;
            }
        }
    }

    void Update3()
    {
        if (diff > range)
        {
            vol = vol * (1.0f - ((diff - range) * 0.3f));
        }
        else if (diff < 0.0f)
        {
            vol += 0.05f * updateWait;
        }

        if (vol < 0.1f)
            vol = 0.1f;
        else if (vol > 1.0f)
            vol = 1.0f;

        audioSource.volume = vol;
    }

    private void FixedUpdate()
    {
        if (audioSource == null)
        {
            iter = 0;
            return;
        }

        {
            bpmDt += Time.fixedDeltaTime;
        }
        {
            bpmInterpDt += Time.fixedDeltaTime;

            float rate = (bpmInterpDt - bpmInterpOffset) / (currBPS);
            if (rate < 0)
                rate += 2.0f;

            if (rate > 1.0f)
            {
                forward = false;
                bpmInterp = (2.0f - rate);
            }
            else
            {
                forward = true;
                bpmInterp = rate;
            }
        }

        {
            if (change >= 1.0f)
            {
                currentBpm = 60.0f / bpmDt;

                if (currentBpm <= maxBpm)
                {
                    while (currentBpm < minBpm)
                    {
                        currentBpm *= 2.0f;
                    }

                    bpmDt = 0.0f;

                    bpm = bpm * 0.9f + currentBpm * 0.1f;
                }
            }
        }

        {
            interp2 = 1.0f * Time.fixedDeltaTime;

            if (change > output)
            {
                output = change;
            }
            else
            {
                output -= interp2;
            }

            foreach (var obj in gameObjs)
            {
                if (obj.gameObject.activeSelf == false)
                    return;

                obj.localScale = new Vector3(output * 2.0f, output * 2.0f, output * 2.0f);
            }
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime > updateWait)
        {
            if (iter == 0)
            {
                Update1();

                iter += 1;
            }
            else if (iter == 1)
            {
                Update2();
                iter += 1;
            }
            else if (iter == 2)
            {
                ChangeSize();
                iter += 1;
            }
            else
            {
                Update3();
                iter = 0;
                elapsedTime -= updateWait;
            }
        }
    }
}
