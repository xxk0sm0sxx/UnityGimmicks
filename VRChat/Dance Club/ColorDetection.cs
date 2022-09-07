
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Note: This script runs a portion of it's functions every frame to reduce lag

public class ColorDetection : UdonSharpBehaviour
{
    Camera cam;
    public Texture2D texture;

    public Material topColorMat;
    public Material midColorMat;
    public Material botColorMat;
    public Material fastColorMat;

    public Color topAvgColor;
    public Color midAvgColor;
    public Color botAvgColor;
    public Color fastAvgColor;

    private Color topTargetColor;
    private Color midTargetColor;
    private Color botTargetColor;
    private Color fastTargetColor;

    public Color defaultTopColor;
    public Color defaultMidColor;
    public Color defaultBotColor;
    public Color defaultFastColor;

    public Material light1Mat;
    public Material light2Mat;
    public Color defaultLight1Color;
    public Color defaultLight2Color;

    public float rate;
    public Color destColor1;
    public Color destColor2;
    public Color floorColor1;
    public Color floorColor2;

    public float th, ts, tv;
    public float _s, _v;
    public float _power, _mul;

    public SoundNormalize soundNormalize;

    public float elapsedTime;
    [SerializeField] public float updateWait;

    Color[] colors;

    public Kinel.VideoPlayer.Scripts.KinelVideoScript videoPlayer;

    public int iter;
    public bool doGetPixel;
    public int iter2;

    float topR;
    float topG;
    float topB;
    int topCount;

    float midR;
    float midG;
    float midB;
    int midCount;

    float botR;
    float botG;
    float botB;
    int botCount;

    int skipY;
    int skipX;

    int thirdHeight;

    float topAvgR;
    float topAvgG;
    float topAvgB;

    float slowPercentage;
    float slowInverse;

    float fastPercentage;
    float fastInverse;

    float h, h2, s, v;

    Color topColor;

    float midAvgR;
    float midAvgG;
    float midAvgB;

    Color midColor;
    float botAvgR;
    float botAvgG;
    float botAvgB;

    Color botColor;
    Color fastColor;

    float r1;

    void Start()
    {
        cam = GetComponent<Camera>();

        topTargetColor = defaultTopColor;
        midTargetColor = defaultMidColor;
        botTargetColor = defaultBotColor;
        fastTargetColor = defaultFastColor;

        light1Mat.color = defaultLight1Color;
        light2Mat.color = defaultLight2Color;
        rate = 0;
        destColor1 = defaultLight1Color;
        floorColor1 = defaultLight2Color;
        destColor2 = defaultLight2Color;
        floorColor2 = defaultLight1Color;

        iter = 0;
        iter2 = 0;
        doGetPixel = true;
        elapsedTime = 0;
    }

    Color convert(Color c, float gamma, float multiply)
    {
        Color r = new Color(c.r * multiply, c.g * multiply, c.b * multiply);
        r = new Color(Mathf.Pow(r.r, gamma), Mathf.Pow(r.g, gamma), Mathf.Pow(r.b, gamma));
        r = new Color(r.r / multiply, r.g / multiply, r.b / multiply);
        r = new Color(Mathf.Clamp(r.r, 0.0f, 1.0f), Mathf.Clamp(r.g, 0.0f, 1.0f), Mathf.Clamp(r.b, 0.0f, 1.0f));
        return r;
    }

    Color normalize(Color c)
    {
        float max = c.r;
        if (max < c.g)
            max = c.g;
        if (max < c.b)
            max = c.b;

        if (max < 0.05f)
            return c;

        return new Color(c.r / max, c.g / max, c.b / max);
    }

    float easeInOutQuint(float x) {
        return x< 0.5 ? 16 * x* x* x* x* x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }

    void OnPostRender()
    {
        if (doGetPixel == false)
            return;

        texture.ReadPixels(cam.pixelRect, 0, 0);
        texture.Apply();

        colors = texture.GetPixels();

        doGetPixel = false;
    }

    void Update1()
    {
		//Initialization
        topR = 0;
        topG = 0;
        topB = 0;
        topCount = 0;

        midR = 0;
        midG = 0;
        midB = 0;
        midCount = 0;

        botR = 0;
        botG = 0;
        botB = 0;
        botCount = 0;

        skipY = 2;
        skipX = 2;

        thirdHeight = texture.height / 3;
	
		//Retrive color from video pixel
        for (int y = 0; y < texture.height; y += skipY)
        {
            for (int x = 0; x < texture.width; x += skipX)
            {
                int idx = y * texture.width + x;
                Color c = colors[idx];

                if (y < thirdHeight)
                {
                    botR += c.r;
                    botG += c.g;
                    botB += c.b;
                    ++botCount;
                }
                else if (y < (thirdHeight * 2))
                {
                    midR += c.r;
                    midG += c.g;
                    midB += c.b;
                    ++midCount;
                }
                else
                {
                    topR += c.r;
                    topG += c.g;
                    topB += c.b;
                    ++topCount;
                }
            }
        }
    }

    void Update2()
    {
		//Do color processing
        topAvgR = topR / topCount;
        topAvgG = topG / topCount;
        topAvgB = topB / topCount;

        midAvgR = midR / midCount;
        midAvgG = midG / midCount;
        midAvgB = midB / midCount;

        botAvgR = botR / botCount;
        botAvgG = botG / botCount;
        botAvgB = botB / botCount;

        topColor = new Color(topAvgR, topAvgG, topAvgB);
        topColor = convert(topColor, _power, _mul);
        Color.RGBToHSV(topColor, out h, out s, out v);
        th = h;
        ts = s;
        tv = v;
        if (s > _s) s = 1.0f;
        if (v > _v) v = 1.0f;
        topColor = Color.HSVToRGB(h, s, v);
    }

    void Update3()
    {
        midColor = new Color(midAvgR, midAvgG, midAvgB);
        midColor = convert(midColor, _power, _mul);
        Color.RGBToHSV(midColor, out h, out s, out v);
        if (s > _s) s = 1.0f;
        if (v > _v) v = 1.0f;
        midColor = Color.HSVToRGB(h, s, v);

        botColor = new Color(botAvgR, botAvgG, botAvgB);
        botColor = convert(botColor, _power, _mul);
        Color.RGBToHSV(botColor, out h, out s, out v);
        if (s > _s) s = 1.0f;
        if (v > _v) v = 1.0f;
        botColor = Color.HSVToRGB(h, s, v);
    }

    void Update4()
    {
        h2 = h + 0.5f;
        if (h2 > 1.0f)
            h2 -= 1.0f;

        v = 0.9f;

		//Vibe colors back and forth
        if (soundNormalize.forward)
        {
            if (soundNormalize.mode == 0)
            {
                destColor2 = Color.HSVToRGB(h, 1.0f, v);
                floorColor2 = Color.HSVToRGB(h2, 1.0f, v); ;
            }
            else
            {
                destColor2 = Color.HSVToRGB(h2, 1.0f, v);
                floorColor2 = Color.HSVToRGB(h, 1.0f, v); ;
            }
        }
        else
        {
            if (soundNormalize.mode == 0)
            {
                destColor1 = Color.HSVToRGB(h, 1.0f, v);
                floorColor1 = Color.HSVToRGB(h2, 1.0f, v); ;
            }
            else
            {
                destColor1 = Color.HSVToRGB(h2, 1.0f, v);
                floorColor1 = Color.HSVToRGB(h, 1.0f, v); ;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!videoPlayer.videoStartedPlaying)
        {
            topColorMat.color = defaultTopColor;
            midColorMat.color = defaultMidColor;
            botColorMat.color = defaultBotColor;
            fastColorMat.color = defaultFastColor;
            light1Mat.color = defaultLight1Color;
            light2Mat.color = defaultLight2Color;
            rate = 0;
            destColor1 = defaultLight1Color;
            floorColor1 = defaultLight2Color;
            destColor2 = defaultLight2Color;
            floorColor2 = defaultLight1Color;

            iter = 0;
            iter2 = 0;
            return;
        }
		
        {
			//The following part has to be done every frame
            if (iter2 == 0)
            {
                slowPercentage = 0.5f * Time.fixedDeltaTime * 2;
                slowInverse = 1.0f - slowPercentage;

                fastPercentage = 1.0f * Time.fixedDeltaTime * 2;
                fastInverse = 1.0f - fastPercentage;

                topAvgColor = topAvgColor * slowInverse + topColor * slowPercentage;
                topColorMat.color = topAvgColor;

                midAvgColor = midAvgColor * slowInverse + midColor * slowPercentage;
                midColorMat.color = midAvgColor;
                botAvgColor = botAvgColor * slowInverse + botColor * slowPercentage;
                botColorMat.color = botAvgColor;

                fastColor = midColor;

                fastAvgColor = fastAvgColor * fastInverse + fastColor * fastPercentage;
                fastColorMat.color = fastAvgColor;

                iter2 = 1;
            }
            else
            {
                if (soundNormalize.forward)
                {
                    if (soundNormalize.bpmInterp > 0 && soundNormalize.bpmInterp < 1)
                    {
                        r1 = easeInOutQuint(soundNormalize.bpmInterp);
                    }

                    light1Mat.color = light1Mat.color * (1.0f - r1) + destColor1 * r1;
                    light2Mat.color = light2Mat.color * (1.0f - r1) + floorColor1 * r1;
                }
                else
                {
                    if (soundNormalize.bpmInterp > 0 && soundNormalize.bpmInterp < 1)
                    {
                        r1 = easeInOutQuint(soundNormalize.bpmInterp);
                    }
                    light1Mat.color = light1Mat.color * r1 + floorColor2 * (1.0f - r1);
                    light2Mat.color = light2Mat.color * r1 + destColor2 * (1.0f - r1);
                }

                iter2 = 0;
            }
        }

        elapsedTime += Time.fixedDeltaTime;
		
		//Do a part of processing across multiple frames to reduce lag
        if (elapsedTime > updateWait)
        {
            if (iter == 0)
            {
                doGetPixel = true;
                iter += 1;
                this.GetComponent<Camera>().Render();
            }
            else if (iter == 1)
            {
                if (doGetPixel == true)
                    return;

                Update1();
                iter += 1;
            }
            else if (iter == 2)
            {
                Update2();
                iter += 1;
            }
            else if (iter == 3)
            {
                Update3();
                iter += 1;
            }
            else
            {
                Update4();
                iter = 0;
                elapsedTime -= updateWait;
            }
        }
    }
}
