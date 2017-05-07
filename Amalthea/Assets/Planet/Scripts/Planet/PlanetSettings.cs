﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;


namespace LemonSpawn {




    // Hidden properties
    public class PlanetProperties
    {
        public int currentLayer = 10;
        public string currentTag = "Normal";
        public double currentDistance;
        public float albedo = 0.3f;
        public float t0 = 0;
        public bool autoOrient = false; // Auto orient to face direction
        public Quaternion rotationMatrix;
        public float rotationSpeed = 0;
        public void setRotation(float x, float z)
        {
            rotationMatrix = Quaternion.Euler(x, 0, z);
        }

        public DVector pos = new DVector();
        public float distance;
        public DVector orgPos = new DVector();
        public DVector posInKm;
        public GameObject terrainObject, parent, environmentObject;
        public Vector3 localCamera;
        public Planet parentPlanet;
        public SerializedPlanet serializedPlanet;
        public List<Frame> Frames = new List<Frame>();
        public Plane[] cameraPlanes;
        public GPUSurface gpuSurface;
        public Color extraColor = new Color(1,1,1,1);
    }
    // Public settings
    public class PlanetSettings : MonoBehaviour {

        public enum Categories { Planet, Moon, Star, Spacecraft};

        [Header("Planet settings")]
        public int seed;
        public double rotation;
        public float Gravity;
        public string givenName = "";
        public int maxQuadNodeLevel;
        public Categories category = Categories.Planet;
 //       public int planetTypeIndex;
        public bool castShadows = true;
        public bool hasSea = false;
		[Header("Lacunarity, Offset, Gain")]
		public Vector3 ExpSurfSettings = new Vector3(2.5f, 1.0f, 1.5f);
		[Header("InitialOffset, Surface Height, Surface Scale")]
		public Vector3 ExpSurfSettings2 = new Vector3(0.6f, 0.01f, 3.2451f);
		[Header("Height sub, octaves, power")]
		public Vector3 ExpSurfSettings3 = new Vector3(0.3f, 10,1);
        [Header("Noise Perturb, Sub Surface Amplitude, Sub Surface Scale")]
        public Vector3 ExpSurfSettings4 = new Vector3(1000, 0, 0);
        [Header("Vortex #1: scale, amp")]
        public Vector3 SurfaceVortex1 = new Vector3(0.0f, 0, 0);
        [Header("Vortex #2: scale, amp")]
        public Vector3 SurfaceVortex2 = new Vector3(0.0f, 0, 0);

        public Vector3 ExpSurfSettings5 = new Vector3(0.5f, 0.1f, 0);
        [Header("Swiss: power add, warp")]
        public Vector3 ExpSurfSettings6 = new Vector3(1.5f, 0.5f, 0);
        [Header("Swiss: offset, gain")]

        // Public stuff to be exposed
        [Header("Atmosphere settings")]
        public float atmosphereDensity = 1.0f;
//        public float atmosphereHeight = 1.025f;
        public float outerRadiusScale = 1.025f;
        public float m_reflectionIntensity = 0;
        public Vector3 m_atmosphereWavelengths = new Vector3(0.65f, 0.57f, 0.475f);
        public float metallicity = 0;
        public float specularity = 0;
        public float density = 5513;
        public string atmosphereString ="normal";


        public float m_hdrExposure = 1.5f;
        public float m_ESun = 10.0f;            // Sun brightness constant
        public float radius = 5000;
        public float temperature = 300f;

        [Space(10)]
        [Header("Ground settings")]
        public float hillyThreshold = 0.980f;
        public float liquidThreshold = 0.0005f;
        public float topThreshold = 0.006f;
        public float basinThreshold = 0.0015f;
        public float globalTerrainHeightScale = 2.0f;
        public float globalTerrainScale = 4.0f;
        public Color m_surfaceColor, m_surfaceColor2;
        public Texture2D m_surfaceTexture;
        public Color m_basinColor, m_basinColor2;
        public Texture2D m_basinTexture;
		public Color m_topColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        public Texture2D m_topTexture;
        public Color m_hillColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        public Texture2D m_hillTexture;
        public Color m_waterColor = new Color(0.6f, 0.8f, 0.9f, 1.0f);
        public Color emissionColor;
        public Texture2D bumpMap;


    [Space(10)]
        [Header("Environment settings")]
        public bool hasEnvironment = false;
        public int environmentDensity = 0;
        public QuadEnvironmentType quadEnvironmentType;


        [Space(10)]
        [Header("Ring settings")]
        public bool hasRings;
        public Color ringColor = Color.white;
        public float ringScale = 1;
        public float ringAmplitude = 1;
        public Vector3 ringRadius = new Vector3(0.2f, 0.45f,0);

        [Space(10)]
        [Header("Cloud settings")]
        public float bumpScale = 1.0f;
		public float cloudRadius = 1.02f;
        public float renderedCloudRadius = 1.03f;
        public Color cloudColor = new Color(0.7f,0.8f,1f);
		public CloudSettings cloudSettings = new CloudSettings();
        public bool hasFlatClouds = false;
        public bool hasBillboardClouds = false;
        public bool hasVolumetricClouds = false;

        [System.NonSerialized]
        public SettingsTypes planetType;
//        [System.NonSerialized]
        public string planetTypeName = "";
        public Atmosphere atmosphere;
        public Sea sea;
        public PlanetProperties properties = new PlanetProperties();
        public Surface surface;

        public static PlanetTypes planetTypes;


        public double getMass()
        {
            return 4 / 3.0 * Mathf.PI * density * Mathf.Pow(getActualRadius(), 3) *Mathf.Pow(1000,3);
        }

        public float getVe(double M)
        {
            return 0.001f * Mathf.Sqrt((float)(2 * Constants.G * M / (properties.distance * Constants.AU))) / properties.distance;

        }

        public float getOrbitalPeriod()
        {
            if (properties.parentPlanet == null)
                return 0;
            double M = properties.parentPlanet.pSettings.getMass();
            double ve = Mathf.Sqrt((float)(2 * Constants.G * M / (properties.distance * Constants.AU)));
            return (float)(2 * Mathf.PI * properties.distance * Constants.AU / ve);///Constants.secondsPerYear);
        }

        public void setPosition(float tval)
        {
            if (properties.parentPlanet == null)
                return;
            double M = properties.parentPlanet.pSettings.getMass();
            //Debug.Log("Escvel for : " + ve + " withg radius " + getActualRadius() + " name " + givenName + " with mass " + getMass());
            float t = getVe(M) * tval;
            properties.pos.Set(getOrbit(t));
        }
        public Vector3 getOrbit(float t)
        {
            return properties.rotationMatrix * new Vector3(Mathf.Cos(t + properties.t0), 0, Mathf.Sin(t + properties.t0)) * properties.distance;

        }

        public float getActualRadius() {
            float actualRadius = radius/RenderSettings.GlobalRadiusScale;
            if (RenderSettings.logScale)
                actualRadius = Mathf.Pow(actualRadius, 1.0f/RenderSettings.powScale);
            return actualRadius;

        }


        public void setLayer(int layer, string tag)
        {
            properties.currentTag = tag;
            properties.currentLayer = layer;
        }

        public void tagGameObject(GameObject go)
        {
            //   Util.tagAll(pSettings.parent, "Normal", 10);
            go.tag = properties.currentTag;
            go.layer = properties.currentLayer;

        }

        public void tagGameObjectAll(GameObject go)
        {
            //   Util.tagAll(pSettings.parent, "Normal", 10);
            go.tag = properties.currentTag;
            go.layer = properties.currentLayer;
            foreach (Transform t in go.transform)
                tagGameObjectAll(t.gameObject);

        }

        public void InitializeAtmosphereMaterials(GameObject go)
        {
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                atmosphere.InitAtmosphereMaterial(mr.material);
            }


            foreach (Transform t in go.transform)
                InitializeAtmosphereMaterials(t.gameObject);

        }





        public Frame getFrame(int i) {
			if (i>=0 && i< properties.Frames.Count)
				return properties.Frames[i];
			return null;
		}
		
		public void Initialize() {
		
			if (hasSea) {
				sea = new Sea();
			}
            maxQuadNodeLevel = RenderSettings.maxQuadNodeLevel;

			
			surface = new Surface(this);
            
			
		}


	


        public void Randomize(int count, string forcedPlanetType) {
            System.Random r = new System.Random(seed);

            givenName = Util.getRandomName(r,"Kvorsk");

            
            sea = new Sea();
            hasFlatClouds = true;
            if (forcedPlanetType != null)
                planetType = PlanetTypes.p.FindPlanetType(forcedPlanetType);
            // Or else find a random one


            if (planetType == null)
                planetType = PlanetTypes.p.getRandomPlanetType(r, getActualRadius(), temperature);
            planetTypeName = planetType.name;
            planetType.Realize(r);
            planetType.setParameters(this, r);


            //radius*=RenderSettings.GlobalRadiusScale;
            //Debug.Log(atmosphereDensity);
        }



/*		public void RandomizeOld(int count) {
			System.Random r = new System.Random(seed);
            temperature = (float)r.NextDouble()*500f + 100;

			if (count>=2)
				planetType = planetTypes.getRandomPlanetType(r, radius, temperature);
			else
				planetType = planetTypes.getPlanetType("Terra");; // First two are ALWAYS TERRA

			if (planetType == null)
				return;

            if (RenderSettings.ForceAllPlanetTypes != -1)
				planetType = planetTypes.planetTypes[RenderSettings.ForceAllPlanetTypes];


            //int atm = r.Next()%AtmosphereWavelengths.Length;
            //Debug.Log("Atmosphere index: " + atm);
            string atm = planetType.atmosphere[r.Next()%planetType.atmosphere.Length];
			m_atmosphereWavelengths = PlanetType.getAtmosphereValue(atm);

			bumpMap = (Texture2D)Resources.Load ("Meaty_Normal");


			m_surfaceColor = Util.VaryColor(planetType.color, planetType.colorVariation, r);
			m_surfaceColor2 = Util.VaryColor(planetType.color, planetType.colorVariation, r);
            m_waterColor = planetType.seaColor;


			m_basinColor = Util.VaryColor(planetType.basinColor, planetType.basinColorVariation, r);
			m_basinColor2 = Util.VaryColor(planetType.basinColor, planetType.basinColorVariation, r);

//			Debug.Log("Surface color:" + m_surfaceColor + " " + m_surfaceColor2);
//			Debug.Log("basin color:" + m_basinColor + " " + m_basinColor2);
	
			m_topColor = planetType.topColor;//m_basinColor*1.2f;
//			Debug.Log("TOPColor: " + m_topColor);									

            //metallicity = 0.01f*(float)r.NextDouble();
            metallicity = 0;
							
			hasRings = false;
			if (radius>RenderSettings.RingRadiusRequirement && r.NextDouble() < RenderSettings.RingProbability) {
				hasRings = true;
				ringColor.r = 0.7f+  0.3f*(float)r.NextDouble();
				ringColor.g = 0.7f+  0.3f*(float)r.NextDouble();
				ringColor.b = 0.7f+  0.3f*(float)r.NextDouble();
				ringScale = 0.6f + 2.5f * (float)r.NextDouble();
				ringRadius.x = 0.15f + 0.15f*(float)r.NextDouble();
				ringRadius.y = 0.25f + 0.20f*(float)r.NextDouble();
				
			}
            m_hdrExposure = 1.5f;
            m_ESun = 10;


            globalTerrainHeightScale = (1.1f + 1.1f * (float)r.NextDouble())*planetType.surfaceHeightModifier;
            globalTerrainScale = (1.0f + (float)(6 * r.NextDouble()))*planetType.surfaceScaleModifier;


//            atmosphereHeight = 1.019f;
  //          outerRadiusScale = 1.025f;

//             < atmosphereHeight > 1.019 </ atmosphereHeight >
  //  < outerRadiusScale > 1.025 </ outerRadiusScale >

  			atmosphereDensity = planetType.atmosphereDensity;
            if (planetType.cloudType!="") 
			{
				clouds = (Texture2D)Resources.Load (Constants.Clouds[r.Next()%Constants.Clouds.Length]);
                hasFlatClouds = true;
                hasVolumetricClouds = false;
                if (planetType.cloudType=="terra")
                    cloudSettings.RandomizeTerra(r);
                if (planetType.cloudType == "gas")
                    cloudSettings.RandomizeGas(r);
                cloudColor.Set(planetType.cloudColor.r, planetType.cloudColor.g, planetType.cloudColor.b);

            }
			if (planetType.sealevel>0 ) {
				sea = new Sea();
                liquidThreshold = planetType.sealevel;
			}
			
			surface = new Surface(this);
			
		}
		
		*/

        public float getHeight() {
			return properties.localCamera.magnitude - radius;
		}
		public float getScaledHeight() {
			return (properties.localCamera.magnitude - radius)/radius;
		}
		public float getPlanetSize() {
            return radius;
        }
        public PlanetSettings() {
//			surface = new Surface(this);
            properties.gpuSurface = new GPUSurface(this);
			
		}
		

        public void UpdateParameters(System.Random r)
        {
            // DO various stuff with the random parameters
            if (atmosphereString == null)
                atmosphereString = "normal";
            string[] atmCandidates= atmosphereString.Split(',');
            string atm = atmCandidates[r.Next() % atmCandidates.Length].Trim();
            m_atmosphereWavelengths = AtmosphereType.getAtmosphereValue(atm);



        }

		
		public void Update() {
			if (properties.posInKm == null) {
                properties.posInKm = new DVector();
			}

            if (properties.gpuSurface != null)
                properties.gpuSurface.Update();

            properties.posInKm.x = properties.pos.x;
            properties.posInKm.y = properties.pos.y;
            properties.posInKm.z = properties.pos.z;
            properties.posInKm.Scale(RenderSettings.AU);


            properties.localCamera = World.WorldCamera.Sub (properties.posInKm).toVectorf();// - transform.position;
			Quaternion q = 	Quaternion.Euler(new Vector3(0, -(float)(rotation/(2*Mathf.PI)*360f),0));
            properties.localCamera = q* properties.localCamera;


            if (!World.SzWorld.useSpaceCamera && World.MainCameraObject!=null)
                {
                properties.localCamera = World.MainCameraObject.transform.position;// - f.transform.position;
                //Debug.Log(properties.localCamera);
               // Debug.Log(properties.localCamera);
                }

//            ExpSurfSettings4.x = (int)ExpSurfSettings4.x;

			if (World.CloseCamera != null)
                properties.cameraPlanes = GeometryUtility.CalculateFrustumPlanes(World.CloseCamera);


        }
		
	}
	

}