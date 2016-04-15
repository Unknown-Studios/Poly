using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Sky : MonoBehaviour
{
    public static Sky instance;

    public static Color Color;
    public static Color TopColor;
    public CullShader[] CullMaterials;

    public string CurWeather;

    public SeasonClass[] Seaso;

    public bool ShowSkybox = true;

    public Material SkyboxMaterial;

    public List<Material> SnowMaterials;

    public Texture2D starTexture;

    public Color SunColor;
    public TimeOfDay[] TID = new TimeOfDay[4];

    public float TOD;

    public WeatherClass[] Weathe;

    //private GameObject SunShafts;
    public Material[] WindMaterials;

    public float Blend;
    private float angle;
    private Color color;
    private ParticleSystem currentPar;

    private float currentPercent;

    private int CurreWeather;

    private float Exponent;

    private float SkyIntensity = 1.0f;

    private float snow;

    private float speed;
    private GameObject WC;

    public TimeOfDay GetCurrentTOD(float time)
    {
        TimeOfDay tod = new TimeOfDay();
        for (int i = 0; i < TID.Length; i++)
        {
            //If the tod is less than the end (NORMAL)
            if (TID[i].Start < TID[i].End)
            {
                if (TID[i].Start < time && TID[i].End > time)
                {
                    tod = TID[i];
                }
            }
            //If the tod is higher than the end (NIGHT)
            else if (TID[i].Start > TID[i].End)
            {
                if (TID[i].Start < time || TID[i].End > time)
                {
                    tod = TID[i];
                }
            }
        }
        if (tod == null)
        {
            return null;
        }
        return tod;
    }

    private int CalcWeather()
    {
        SeasonClass CurrentSeason = Seaso[Game.Season - 1];
        int we = 0;
        int old = 0;
        for (int i = 0; i < CurrentSeason.weatherChance.Length; i++)
        {
            old += CurrentSeason.weatherChance[i];
            if (Game.Weather <= old)
            {
                we = i;
            }
        }

        return we;
    }

    private void LateUpdate()
    {
        if (currentPar != null)
        {
            ParticleSystem.Particle[] p = new ParticleSystem.Particle[currentPar.particleCount];
            int l = currentPar.GetParticles(p);

            int i = 0;
            while (i < l)
            {
                p[i].velocity = Wind.windVector3;
                i++;
            }

            currentPar.SetParticles(p, l);
        }
    }

    private void OnDisable()
    {
        foreach (Material mat in WindMaterials)
        {
            mat.SetVector("_Wind", Vector2.zero);
        }
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void Start()
    {
        RenderSettings.skybox = SkyboxMaterial;
        Game.time = 7.0f / 24.0f;

        starTexture = new Texture2D(500, 500);
        for (int x = 0; x < starTexture.width; x++)
        {
            for (int y = 0; y < starTexture.height; y++)
            {
                Color col = Color.black;
                if (Random.Range(0, 2500) == 1)
                {
                    col = Color.white;
                }
                starTexture.SetPixel(x, y, col);
            }
        }
        starTexture.Apply();

        foreach (CullShader mat in CullMaterials)
        {
            if (Game.noGraphics)
            {
                mat.CullMaterial.SetFloat("_CullDistance", 0.0f);
            }
            else
            {
                mat.CullMaterial.SetFloat("_CullDistance", mat.CullDistance);
            }
        }
    }

    private void Update()
    {
        if (!ShowSkybox)
        {
            RenderSettings.skybox = null;
            return;
        }
        else if (RenderSettings.skybox != SkyboxMaterial)
        {
            RenderSettings.skybox = SkyboxMaterial;
        }

        TOD = Game.time * 24.0f;

        angle = (Game.time * 360) - 90;
        transform.localEulerAngles = new Vector3(angle, 0, 0);

        Blend = TOD % 1f;

        TimeOfDay CTOD = GetCurrentTOD(TOD);

        if (CTOD != null)
        {
            if (TOD - CTOD.Start < 1)
            {
                Exponent = Mathf.Clamp01(TOD - CTOD.Start);
            }
            else if (CTOD.End - TOD < 1)
            {
                Exponent = Mathf.Clamp01(CTOD.End - TOD);
            }
            else
            {
                Exponent = 1;
            }

            Color = Color.Lerp(Color, CTOD.Color, Blend);
            TopColor = Color.Lerp(TopColor, CTOD.TopColor, Blend);

            RenderSettings.skybox.SetColor("_SkyColor1", TopColor);
            RenderSettings.skybox.SetColor("_SkyColor2", Color);
            RenderSettings.fogColor = Color;

            RenderSettings.skybox.SetColor("_SunColor", SunColor);
            RenderSettings.skybox.SetFloat("_SkyExponent1", Exponent);

            RenderSettings.ambientLight = SunColor;
            GetComponent<Light>().color = SunColor;
        }

        foreach (Material mat in WindMaterials)
        {
            mat.SetVector("_Wind", Wind.wind);
        }

        if (Sync.instance != null)
        {
            Sync.Wind = Wind.wind;
            Sync.WindSpeed = Wind.speed;
            Sync.TimeOfDay = TOD;
        }
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 1000;
        RenderSettings.fogEndDistance = 2500;

        Weather();
    }

    private void Weather()
    {
        if (Game.Weather != currentPercent)
        {
            currentPercent = Game.Weather;
            CurreWeather = Mathf.Clamp(CalcWeather(), 0, Weathe.Length);
        }
        Game.WeatherLength = Weathe.Length;
        WeatherClass CurrentWeather = Weathe[CurreWeather];
        if (CurWeather != CurrentWeather.Name)
        {
            CurWeather = CurrentWeather.Name;
            Destroy(WC);
        }
        if (!WC && CurrentWeather.ParticleSystem)
        {
            WC = Instantiate(CurrentWeather.ParticleSystem);
            WC.name = CurrentWeather.Name;
            currentPar = WC.GetComponent<ParticleSystem>();
        }
        if (WC)
        {
            Vector3 WeatherPos;
            if (Camera.main)
            {
                WeatherPos = Camera.main.transform.position;
            }
            else
            {
                WeatherPos = Vector3.zero;
            }
            WeatherPos.y += 25;
            WC.transform.position = WeatherPos;
        }
        int target = 0;
        if (CurrentWeather.Name.Contains("Snow"))
        {
            target = 1;
        }
        else
        {
            target = 0;
        }

        foreach (Material m in SnowMaterials)
        {
            snow = Mathf.Lerp(snow, target, Time.deltaTime / 30);
            Color s = new Color(snow, snow, snow);
            m.SetColor("White", s);
        }

        SkyIntensity = Mathf.Lerp(SkyIntensity, CurrentWeather.SkyIntensity, Time.deltaTime);
        GetComponent<Light>().intensity = SkyIntensity;
        RenderSettings.skybox.SetFloat("_SkyIntensity", SkyIntensity);
    }

    [Serializable]
    public class CullShader
    {
        public float CullDistance;
        public Material CullMaterial;
    }

    [Serializable]
    public class SeasonClass
    {
        public string Name;
        public int[] weatherChance;
    }

    [Serializable]
    public class TimeOfDay
    {
        public Color Color = Color.white;
        public int End;
        public string name;
        public int Start;
        public Color TopColor = Color.white;
    }

    [Serializable]
    public class WeatherChance
    {
        public int clear;
        public int rain;
        public int rain_heavy;
        public int snow;
        public int snow_heavy;
    }

    [Serializable]
    public class WeatherClass
    {
        public string Name;
        public GameObject ParticleSystem;
        public float SkyIntensity = 1.0f;
    }
}