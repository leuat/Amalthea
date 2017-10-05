using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LemonSpawn
{


    public class SpaceAtmosphere
    {
        Material mat;
        GameObject sun;
        public Color color;
        public float m_g = -0.990f;             // The Mie phase asymmetry factor, must be between 0.999 to -0.999
        public float hdr = 0.1f;
        public SpaceAtmosphere(Material m, GameObject s, Color col, float h)
        {
            mat = m;
            sun = s;
            color = col;
            hdr = h;
        }


        public void Update()
        {
            mat.SetVector("v3LightPos", sun.transform.forward * -1.0f);
            mat.SetColor("sunColor", color);
            mat.SetFloat("fHdrExposure", hdr * Atmosphere.sunScale);
            mat.SetFloat("g", m_g);
            mat.SetFloat("g2", m_g * m_g);

        }


    }
    

    public class SolarSystem
    {

        public Material spaceMaterial;
        public Material groundMaterial;
        public Transform transform;
        private GameObject sun;
        private Mesh sphere;
        public Star star;
        public SpaceAtmosphere space;
        public List<Planet> planets = new List<Planet>();
        public List<GameObject> cameraObjects = new List<GameObject>();

        // Closest active planet
        public static Planet planet = null;

        // Use this for initialization

        private void InitializeSurfaces()
        {
            groundMaterial = (Material)Resources.Load("GroundMaterial");


            if (!RenderSettings.GPUSurface)
                Shader.DisableKeyword("LS_GPU_SURFACE");
            //                groundMaterial = (Material)Resources.Load("GroundMaterial");
            else
            {
                
                Shader.EnableKeyword("LS_GPU_SURFACE");

                /*                if (RenderSettings.UsePerPixelShading)
                                    groundMaterial = (Material)Resources.Load("GroundMaterialGPU");
                                else
                                    groundMaterial = (Material)Resources.Load("GroundMaterialGPUVertexOnly");
                                    */
            }

        }

        Planet AddPlanet(Planet parentPlanet, float parentD, float CurrentRadius, System.Random rnd, Transform parent, PlanetSettings.Categories cat, float Luminosity, float i, float rMin, float rMax, float tilt, string name, bool isStar)
        {
                
                GameObject go = new GameObject();
            go.transform.parent = parent;
            //sz.global_radius_scale = RenderSettings.GlobalRadiusScale;
            PlanetSettings ps = go.AddComponent<PlanetSettings>();
            if (!isStar)
            {
                ps.properties.setRotation((float)((rnd.NextDouble() - 0.5) * tilt), (float)((rnd.NextDouble() - 0.5) * tilt));
                ps.properties.t0 = (float)(rnd.NextDouble() * Mathf.PI * 2);
            }
            ps.seed = rnd.Next();
            ps.properties.parentPlanet = parentPlanet;
            
            ps.category = cat;
//            ps.properties.rotationSpeed = (float)rnd.NextDouble() * 1f; ;
            ps.properties.distance = CurrentRadius;
            if (!isStar)
            {
                ps.setPosition(0);
                go.transform.position = ps.properties.pos.toVectorf();

                //            go.transform.rotation = Quaternion.Euler((float)((rnd.NextDouble()-0.5)*30), 0, (float)((rnd.NextDouble() - 0.5) * 30));
                float rad = Mathf.Pow((float)rnd.NextDouble(), 2);
                ps.radius = RenderSettings.fromActualRadius((rMin + rad * (rMax - rMin)));
            }
            else
                ps.radius = rMin;
            //            Debug.Log("Radius: " + ps.radius / RenderSettings.GlobalRadiusScale);
            //                RenderSettings.
            //           if (RenderSettings.logScale) ps.radius = Mathf.Pow(ps.radius, RenderSettings.powScale);
            //           ps.radius *= RenderSettings.GlobalRadiusScale;
            if (!isStar)
            {
                float D = (float)ps.properties.pos.Length();
                ps.temperature = Constants.getBlackBodyTemperature(Luminosity, (parentD + D) * (float)Constants.AU, ps.properties.albedo);
                ps.temperature += rnd.Next() % 100;
            }
            else
            {
                ps.temperature = rMax;
                ps.properties.extraColor = Constants.colorTemperatureToRGB(ps.temperature);
//                ps.properties.extraColor = new Color(0.9f, 0.7f, 0.5f);
//                Debug.Log(ps.properties.extraColor);
            }
            ps.Randomize();

            go.name = name;// ps.givenName;
            Planet p = InitializeObject(ps);

            p.pSettings.properties.parent = go;


            if (ps.category == PlanetSettings.Categories.Star)
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("Sun"), sphere);
            else
            if (ps.category == PlanetSettings.Categories.Object3D)
            {
                p.Initialize(sun, groundMaterial, (Material)Resources.Load(ps.properties.serializedPlanet.objectMaterial), sphere);
            }
            else
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("SkyMaterial"), sphere);

            planets.Add(p);
            return p;
        }

#if AMALTHEA
        public void GenerateSolarSystem(Amalthea.StarSystem starSystem)
        {
            System.Random rnd = new System.Random(starSystem.seed);

            //            PlanetSettings starPs = new PlanetSettings();
            //            starPs.category = LemonSpawn.PlanetSettings.Categories.Star;
            planets.Clear();

      

            Planet star = AddPlanet(null, 0, 0, rnd, transform, PlanetSettings.Categories.Star, 0, 0, RenderSettings.fromActualRadius((float)Constants.sunR)* starSystem.radius,
                starSystem.temperature, 25, "Star", true);
            star.pSettings.density = 1400;
            star.pSettings.givenName = starSystem.getName() ;
            star.pSettings.name = starSystem.getName();
            //            Debug.Log("Sun mass: " + star.pSettings.getMass());

            //          Debug.Log("Star T,R:" + star.pSettings.temperature + " , " + star.pSettings.radius/Constants.sunR);
            float lum = Constants.LumFromT(star.pSettings.getActualRadius()*1000, star.pSettings.temperature);
                               //            noPlanets = 0;
                               //            Debug.Log("Transform:" + transform);
            float R = (float)(0.2 + rnd.NextDouble()*0.5);

            /*            double M = star.pSettings.getMass();
                        float ve = Mathf.Sqrt((float)(2 * Constants.G * M / (1 * Constants.AU)));
                        Debug.Log("Earths speed: " + ve);
                        */
            //            noPlanets = 1;
            System.Diagnostics.Stopwatch so = new System.Diagnostics.Stopwatch();
            so.Start();

            for (int i=0;i < starSystem.noPlanets;i ++)
            {
                float max = 40000;
                if (rnd.NextDouble() > 0.8)
                    max *= 3;
                Planet p = AddPlanet(star, 0,R, rnd, star.pSettings.transform, PlanetSettings.Categories.Planet, lum,i, 2000, max,25, "Planet " + (i+1),false);
                int noMoons = rnd.Next() % (int)(Mathf.Log(p.pSettings.getActualRadius()*0.5f));
                float Rm = (float)(0.0002f + rnd.NextDouble() * 0.0004f);
//                Debug.Log("radius " + p.pSettings.getActualRadius() + " name " + p.pSettings.givenName + " with mass " + p.pSettings.getMass());
                for (int j=0;j<noMoons;j++)
                {
                    Planet m = AddPlanet(p, R, Rm, rnd, p.pSettings.transform, PlanetSettings.Categories.Moon, lum, i, 100, p.pSettings.getActualRadius()*0.3f,60, "Moon " + (j+1),false);
                    
                    Rm += (float)(0.0001 + rnd.NextDouble()*0.0002);

                }
                R += (float)(0.4 + rnd.NextDouble() * (i + 1));
            }
            Debug.Log("El: " + so.Elapsed);

        }

#endif
        public SolarSystem(GameObject pSun, Mesh s, Transform t, int skybox)
        {
            sun = pSun;
            sphere = s;
            transform = t;
            spaceMaterial = (Material)Resources.Load("SpaceMaterial");
            InitializeSurfaces();

            space = new SpaceAtmosphere(spaceMaterial, sun, Color.white, 0.1f);


            SetSkybox(skybox);

        }


        public void RenderInformation()
        {

            foreach (Planet p  in planets)
            {
                GUIStyle guiStyle = SSVAppSettings.guiStyle;
                guiStyle.normal.textColor = Color.red; //guiStyle.normal.textColor;




                Vector3 pos = World.MainCamera.GetComponent<Camera>().WorldToScreenPoint(p.pSettings.gameObject.transform.position);
                //int width1 = dp.planet.lsPlanet.pSettings.givenName.Trim().Length;
                if (pos.z > 0 && guiStyle.normal.textColor.a > 0)
                {
                
                    {
                        guiStyle.fontSize = Screen.height/50;
                        if (p.pSettings.properties.serializedPlanet != null)
                        {
                            guiStyle.normal.textColor = p.pSettings.properties.serializedPlanet.getColor();//  new Color(0.5f, 0.7f, 1.0f);
                            if (World.SzWorld.currentFrame < p.pSettings.properties.serializedPlanet.Frames.Count)
                            {
                                Frame frame = p.pSettings.properties.serializedPlanet.Frames[World.SzWorld.currentFrame];
                                string info = frame.displayMessage;
                                if (info != null && info != "")
                                    GUI.Label(new Rect(pos.x, Screen.height - pos.y, 250, 130), info, guiStyle);


                            }

                        }
                    }

                }

            }

        }

        public void reInitializeGround()
        {
            InitializeSurfaces();
            foreach (Planet p in planets)
                p.pSettings.atmosphere.ReinitializeGroundMaterial(groundMaterial);
        }

        void setSun()
        {
            //		if (World.WorldCamera
            if (sun == null)
                return;
            sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
            sun.GetComponent<Light>().color = space.color;
        }


        public void ReplaceMaterial(GameObject g, Material mat, PlanetSettings ps)
        {
            Renderer r = g.GetComponent<Renderer>();
            if (r != null)
                r.material = mat;

            ps.atmosphere.InitAtmosphereMaterial(mat);
            ps.atmosphere.initGroundMaterial(true,mat);


            foreach (Transform child in g.transform)
            {
                ReplaceMaterial(child.gameObject, mat, ps);
            }
        }

        public void toggleGPUSurface()
        {
            RenderSettings.GPUSurface = !RenderSettings.GPUSurface;
            InitializeSurfaces();
            foreach (Planet p in planets)
            {
               // Debug.Log("WTF");
               if (groundMaterial != null)
                p.pSettings.atmosphere.ReinitializeGroundMaterial(groundMaterial);
//                ReplaceMaterial(p.pSettings.properties.terrainObject, groundMaterial, p.pSettings);
                GameObject.DestroyImmediate(p.pSettings.properties.terrainObject);
            }


        }

        public void findClosestPlanet()
        {
            //if (planets.Count > 0)
            //    planet = planets[0];

            float min = 1E30f;
            foreach (Planet p in planets)
            {
                if (p.pSettings.category == PlanetSettings.Categories.Planet)
                {
                    float l = (p.pSettings.gameObject.transform.position).magnitude - p.pSettings.radius;
                    if (l < min)
                    {
                        planet = p;
                        min = l;
                    }
                }
            }

        }



        private Planet InitializeObject(PlanetSettings ps)
        {
            if (ps.category == PlanetSettings.Categories.Star)
                return new Star(ps);
            if (ps.category == PlanetSettings.Categories.BlackHole)
                return new BlackHole(ps);
            if (ps.category == PlanetSettings.Categories.Object3D)
                return new MCAstObject3D(ps);
            if (ps.category == PlanetSettings.Categories.Explosion)
                return new Explosion(ps);


            return new Planet(ps);
        }

        public void InitializePlanet(GameObject go, PlanetSettings ps)
        {
            Planet p = InitializeObject(ps);
            p.pSettings.properties.orgPos.Set(p.pSettings.properties.pos);
//            GameObject go = new GameObject();
  //          go.transform.parent = transform;
            //            ps.gameObject = go;
            //sz.global_radius_scale = RenderSettings.GlobalRadiusScale;
            ps.transform.parent = transform;
            p.pSettings.properties.parent = go;
            if (ps.category == PlanetSettings.Categories.Star)
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("Sun"), sphere);
            else
            if (ps.category == PlanetSettings.Categories.Object3D)
            {
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("SpaceCraftMaterial"), sphere);
            }
            else
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("SkyMaterial"), sphere);

            planets.Add(p);

        }


        public void InitializeFromScene()
        {

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                if (go.activeSelf)
                {
                    PlanetSettings ps = go.GetComponent<PlanetSettings>();
                    if (ps == null)
                    {
                        cameraObjects.Add(go);
                        continue;
                    }
                    Planet p = InitializeObject(ps);


                    p.pSettings.properties.orgPos.Set(go.transform.position);
                    p.pSettings.properties.pos.Set(go.transform.position);
                    go.transform.parent = transform;
                    p.pSettings.properties.parent = go;
                    //p.pSettings.planetType = PlanetSettings.planetTypes.planetTypes[p.pSettings.planetTypeIndex];
                    //                   if (RenderSettings.GPUSurface)
                    ///                        p.pSettings.planetType = PlanetSettings.planetTypes.planetTypes[0];
                    //				p.pSettings.planetType = PlanetType.planetTypes[1];


                    if (ps.category == PlanetSettings.Categories.Star)
                        p.Initialize(sun, groundMaterial, (Material)Resources.Load("Sun"), sphere);
                    else
                    if (ps.category == PlanetSettings.Categories.Object3D)
                    {
                        p.Initialize(sun, groundMaterial, (Material)Resources.Load("SpaceCraftMaterial"), sphere);
                    }
                    else
                        p.Initialize(sun, groundMaterial, (Material)Resources.Load("SkyMaterial"), sphere);

                    planets.Add(p);

                }
            }

            RenderSettings.ResolutionScale = World.SzWorld.resolutionScale;

            //space.color = new Color(World.SzWorld.sun_col_r, World.SzWorld.sun_col_g, World.SzWorld.sun_col_b);
            space.hdr = World.SzWorld.sun_intensity;
        }




        public void Update()
        {
            setSun();
            if (space != null)
                space.Update();

            findClosestPlanet();

            if (planet != null)
                planet.ConstrainCameraExterior();


            foreach (Planet p in planets)
            {
//                Debug.Log(p.pSettings.gameObject.name);
                p.Update();
            }

            // Set closest clippping plane
            if (planet != null)
            {
                if (planet.pSettings.atmosphere != null)
                    planet.pSettings.atmosphere.setClippingPlanes();

               // UnityEngine.RenderSettings.reflectionIntensity = UnityEngine.RenderSettings.reflectionIntensity * 0.99f + planet.pSettings.m_reflectionIntensity * 0.01f;

            }

        }


        public void Reset()
        {
            foreach (Planet p in planets)
                p.Reset();
        }


        private void ForceNoneHierarchy(SerializedMCAstObject so, DVector current)
        {
            so.pos_x += current.x;
            so.pos_y += current.y;
            so.pos_z += current.z;
            current = current + new DVector(so.pos_x, so.pos_y, so.pos_z);
            foreach (SerializedMCAstObject nso in so.Objects)
            {
                ForceNoneHierarchy(nso, current);
            }
        }

        public void LoadSZWold(World world, SerializedWorld sz, bool randomizeSeeds, float scale, bool hierarchy)
        {
            SetSkybox((int)sz.skybox);
            if (RenderSettings.ignoreXMLResolution)
            {
                sz.resolutionScale = World.SzWorld.resolutionScale;
                sz.resolution = World.SzWorld.resolution;

            }
            else
            {
                RenderSettings.sizeVBO = Mathf.Clamp(sz.resolution, 32, 128);
                RenderSettings.ResolutionScale = sz.resolutionScale;

            }
            World.SzWorld = sz;

            RenderSettings.ScreenshotX = sz.screenshot_height;
            RenderSettings.ScreenshotY = sz.screenshot_width;
            World.hasScene = true;
            RenderSettings.isVideo = sz.isVideo();
            if (RenderSettings.isVideo == true)
                RenderSettings.ExitSaveOnRendered = false;


            //		RenderSettings.isVideo = false;
            if (WorldMC.Slider != null)
                WorldMC.Slider.SetActive(RenderSettings.isVideo);
            InitializeSurfaces();
            //Debug.Log("WHOO");

            Util.DestroyGameObject("SolarSystem");
            
            GameObject parent = new GameObject("SolarSystem");



            foreach (SerializedMCAstObject sp in sz.Objects)
            {
                if (!hierarchy)
                    ForceNoneHierarchy(sp, new DVector(0,0,0));

                InitializeSZObject(sp, parent, randomizeSeeds, scale, hierarchy);
            }
 
            world.setWorld(sz);

        }

        private void InitializeSZObject(SerializedMCAstObject sp, GameObject parent, bool randomizeSeeds, float scale, bool hierarchy)
        {
            World.SzWorld.maxFrames = Mathf.Max(sp.Frames.Count, World.SzWorld.maxFrames);
            GameObject go = new GameObject(sp.name);
            go.transform.parent = parent.transform;
            //sz.global_radius_scale = RenderSettings.GlobalRadiusScale;
            PlanetSettings ps = sp.DeSerialize(go, scale);
            if (randomizeSeeds)
            {
                ps.seed = (int)(Random.value * 10000f);
                ps.Randomize();
            }
            Planet p = InitializeObject(ps);

            p.pSettings.properties.serializedPlanet = sp;


            p.pSettings.properties.parent = go;


            if (ps.category == PlanetSettings.Categories.Star)// && World.CurrentApp != Verification.MCAstName)
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("Sun"), sphere);
            else
            if (ps.category == PlanetSettings.Categories.Object3D)
            {
                p.Initialize(sun, groundMaterial, (Material)Resources.Load(sp.objectMaterial), sphere);
            }
            else
                p.Initialize(sun, groundMaterial, (Material)Resources.Load("SkyMaterial"), sphere);

            
            planets.Add(p);
            GameObject newP = p.pSettings.gameObject;
            if (!hierarchy)
                newP = parent;
            foreach (SerializedMCAstObject child in sp.Objects)
            {
               

                InitializeSZObject(child, newP, randomizeSeeds, scale, hierarchy);
            }


        }

        public void LoadWorld(string data, bool isFile, bool ExitOnSave, World world, bool randomizeSeeds = false, bool hierarchy=false)
        {
            ClearStarSystem();
            SerializedWorld sz;
            if (isFile)
            {
                //			RenderSettings.extraText = data;

                if (!System.IO.File.Exists(data))
                {
                    //RenderSettings.extraText = ("ERROR: Could not find file :'" + data + "'");
                    World.FatalError("Could not load file: " + data);
                    return;
                }
                sz = SerializedWorld.DeSerialize(data);
            }
            else
                sz = SerializedWorld.DeSerializeString(data);
            RenderSettings.ExitSaveOnRendered = ExitOnSave;
            RenderSettings.extraText = "";

            LoadSZWold(world, sz, randomizeSeeds, RenderSettings.GlobalRadiusScale, hierarchy);
        }
        public void ClearStarSystem()
        {
            planets.Clear();
/*            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                if(go.GetComponent<PlanetSettings>()!=null)
                    GameObject.Destroy(go);
                //	Debug.Log ("Destroying " + go.name);
            }
            */


        }
       
        public static void SetSkybox(int s)
        {
            /*string skybox = "Skybox3";
            s = s % 7;

            if (s == 1) skybox = "Skybox4";
            if (s == 2) skybox = "Skybox5";
            if (s == 3) skybox = "Skybox2";
            if (s == 4) skybox = "Skybox7";
            if (s == 5) skybox = "Skybox8";
            if (s == 6) skybox = "Skybox9";
*/
			string skybox = "Skybox" + s;
            Debug.Log("Ignoring setting skybox " + s);
//            UnityEngine.RenderSettings.skybox = (Material)Resources.Load(skybox);

        }


    }
}