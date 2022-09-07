
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SoundDetection : UdonSharpBehaviour
{
    public AudioSource audioSource;
    public GameObject[] freq1;
    public GameObject[] freq2;
    public GameObject[] freq3;
    public GameObject[] freq4;
    public GameObject[] freq5;
    public GameObject[] freq6;
    public GameObject[] freq7;
    public GameObject[] freq8;
    private float[] samples;
    private float[] avg;
    private float[] currValues;
    public float minScale;
    public float maxScale;
    float avgMax;
    float range;
    float currMax;

    void Start()
    {
        samples = new float[64];
        avg = new float[8];
        currValues = new float[8];
        avgMax = 0;
        range = maxScale - minScale;
        currMax = 0;
    }

    void SetSpectrumData(int idx, int low, int high)
    {
        for (int i = low; i < high; ++i)
        {
            avg[idx] += samples[i];
        }

        //avg[idx] = avg[idx] / (high - low);
        if (currMax < avg[idx])
            currMax = avg[idx];
    }

    private void FixedUpdate()
    {
        float fallOff = 1.0f - (4.0f * Time.deltaTime);
        float slowFallOff = 1.0f - (0.1f * Time.deltaTime);

        currMax *= slowFallOff;
        audioSource.GetSpectrumData(samples, 1, FFTWindow.BlackmanHarris);
        for (int i = 0; i < avg.Length; ++i)
        {
            avg[i] = 0;
        }

        SetSpectrumData(0, 0, 1);
        SetSpectrumData(1, 0, 2);
        SetSpectrumData(2, 1, 3);
        SetSpectrumData(3, 2, 5);
        SetSpectrumData(4, 3, 8);
        SetSpectrumData(5, 6, 20);
        SetSpectrumData(6, 12, 48);
        SetSpectrumData(7, 24, 64);
            
        for (int i = 0; i < currValues.Length; ++i)
        {
            float normalized = avg[i] / (currMax * 0.5f);
            if (currValues[i] < normalized )
            {
                currValues[i] = normalized;
            }
            else
            {
                currValues[i] *= fallOff;
            }
        }

        //float avgValue = 0;
        //for (int i = 0; i < currValues.Length; ++i)
        //{
        //    avgValue += currValues[i];
        //}
        //avgValue = avgValue / currValues.Length;

        //for (int i = 0; i < currValues.Length; ++i)
        //{
        //    currValues[i] = currValues[i] - avgValue;
        //}

        for (int i = 0; i < freq1.Length; ++i)
        {
            Vector3 scale = freq1[i].transform.localScale;
            scale.y = minScale + (currValues[0] * range);
            freq1[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq2.Length; ++i)
        {
            Vector3 scale = freq2[i].transform.localScale;
            scale.y = minScale + (currValues[1] * range);
            freq2[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq3.Length; ++i)
        {
            Vector3 scale = freq3[i].transform.localScale;
            scale.y = minScale + (currValues[2] * range);
            freq3[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq4.Length; ++i)
        {
            Vector3 scale = freq4[i].transform.localScale;
            scale.y = minScale + (currValues[3] * range);
            freq4[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq5.Length; ++i)
        {
            Vector3 scale = freq5[i].transform.localScale;
            scale.y = minScale + (currValues[4] * range);
            freq5[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq6.Length; ++i)
        {
            Vector3 scale = freq6[i].transform.localScale;
            scale.y = minScale + (currValues[5] * range);
            freq6[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq7.Length; ++i)
        {
            Vector3 scale = freq7[i].transform.localScale;
            scale.y = minScale + (currValues[6] * range);
            freq7[i].transform.localScale = scale;
        }

        for (int i = 0; i < freq8.Length; ++i)
        {
            Vector3 scale = freq8[i].transform.localScale;
            scale.y = minScale + (currValues[7] * range);
            freq8[i].transform.localScale = scale;
        }
    }
}
