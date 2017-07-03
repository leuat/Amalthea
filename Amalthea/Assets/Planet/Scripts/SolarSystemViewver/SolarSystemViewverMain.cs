using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

namespace LemonSpawn
{

    public class SSVSettings
    {
        public static int OrbitalLineSegments = 100;
        public static float currentFrame = 0;
        public static float LineScale = 1;


    }    
    public class SolarSystemViewverMain : SSVAppBase
    {

        public static bool Reload = false;

        private void DisplayStarInfo(StarSystem star)
        {
            setText("txtPlanetType", star.getCategory());
            setText("txtPlanetName", star.getName());
            setText("txtPlanetName2", "(" + star.noPlanets + " planets" + ")");
            string infoText2 = "";
            //                int displayRadius2 = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / LemonSpawn.RenderSettings.GlobalRadiusScale * currentScale);
            infoText2 += "Radius           : " + star.radius.ToString("0.00") + " sun radii" + "\n";
            //          infoText += "Displayed Radius : " + displayRadius / radius + " x original radius\n";
            infoText2 += "Mass             : " + star.mass + " sun masses" + "\n";
            //            infoText += "Displayed Radius : " + displayRadius + " km \n";
            infoText2 += "Temperature      : " + (int)star.temperature + "K\n";
            //               infoText2 += dp.planet.lsPlanet.pSettings.planetType.PlanetInfo;
            setText("txtPlanetInfo", infoText2);
        }
        private void DisplayBHInfo(StarSystem star)
        {
            setText("txtPlanetType", "Black hole");
            setText("txtPlanetName", star.getName());
            setText("txtPlanetName2", "(" + star.noPlanets + " planets" + ")");
            string infoText2 = "";
            //                int displayRadius2 = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / LemonSpawn.RenderSettings.GlobalRadiusScale * currentScale);
            infoText2 += "Radius           : " + star.radius.ToString("0.00") + " sun radii" + "\n";
            //          infoText += "Displayed Radius : " + displayRadius / radius + " x original radius\n";
            infoText2 += "Mass             : " + star.mass + " sun masses" + "\n";
            //            infoText += "Displayed Radius : " + displayRadius + " km \n";
            infoText2 += "Event horizon (Not done)   : " + (int)star.temperature + "K\n";
            //               infoText2 += dp.planet.lsPlanet.pSettings.planetType.PlanetInfo;
            setText("txtPlanetInfo", infoText2);
        }

        public override void DisplayObjectText(DisplayPlanet dp)
        {
            if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
            {
                DisplayStarInfo(data.currentSystem);
                return;
            }
            if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.BlackHole)
            {
                DisplayBHInfo(data.currentSystem);
                return;
            }
            else
    if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Object3D)
            {
                setText("txtPlanetName", "3D Object");
                setText("txtPlanetName2", "");
                setText("txtPlanetType", "");
                setText("txtPlanetInfo", "");
                return;
            }

            setText("txtPlanetType", dp.planet.lsPlanet.pSettings.planetType.name);
            setText("txtPlanetName", dp.planet.lsPlanet.pSettings.givenName);
            setText("txtPlanetName2", "(" + dp.planet.lsPlanet.pSettings.name + ")");

            string infoText = "";
            int radius = (int)(dp.planet.lsPlanet.pSettings.getActualRadius());
            int displayRadius = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / LemonSpawn.RenderSettings.GlobalRadiusScale * currentScale);
            float orbit = (dp.planet.lsPlanet.pSettings.properties.pos.toVectorf().magnitude);///(float)SSVAppSettings.SolarSystemScale);
            infoText += "Orbital distance : " + orbit + "Au\n\n";
            infoText += "Orbital period   : " + LemonSpawn.Constants.getFormattedTimeFromSeconds(dp.planet.lsPlanet.pSettings.getOrbitalPeriod()) + "\n";
            infoText += "Radius           : " + radius + "km\n";
            //          infoText += "Displayed Radius : " + displayRadius / radius + " x original radius\n";
            infoText += "Mass             : " + LemonSpawn.Constants.getFormattedMass(dp.planet.lsPlanet.pSettings.getMass()) + "\n";
            //            infoText += "Displayed Radius : " + displayRadius + " km \n";
            infoText += "Temperature      : " + (int)dp.planet.lsPlanet.pSettings.temperature + "K\n";
            infoText += dp.planet.lsPlanet.pSettings.planetType.PlanetInfo;
            setText("txtPlanetInfo", infoText);




        }

        private void UpdateOverviewClick()
        {
            GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().value = 0;

        }

        /*        public void FocusOnPlanetClick()
                {
                    int idx = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().value;
                    string name = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().options[idx].text;

                    foreach (DisplayPlanetMCAST dp in dPlanets)
                        if (name.Contains(dp.planet.lsPlanet.pSettings.name))
                            SelectPlanet(dp);

                }

                public void ZoomPlanet()
                {
                    //SolarSystemViewverZoom.SzWorld = SzWorld;
                    RenderSettings.currentSZWorld = SzWorld;
                    Application.LoadLevel(4);
                }
                */

        public void ToggleLabels()
        {
            SSVAppSettings.toggleLabels = !SSVAppSettings.toggleLabels;
        }


        private void RenderLabels()
        {
            if (!SSVAppSettings.toggleLabels)
                return;
            GUI.skin.font = SSVAppSettings.GUIFont;
            data.RenderSolarSystemLabels(mainCamera);
        }


/*        public void SlideScaleLines()
        {
            Slider slider = GameObject.Find("SliderScaleLines").GetComponent<Slider>();

            SSVAppSettings.LineScale = slider.value * 10;
            //   foreach (DisplayPlanetMCAST dp in dPlanets)
            //       dp.MaintainOrbits();
        }
        */

        private float currentScale = 1;

        public void SlideScale()
        {
            Slider slider = GameObject.Find("SliderScale").GetComponent<Slider>();
            foreach (DisplayPlanet dp in data.dPlanets)
            {
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star )
                    continue;
                currentScale = slider.value * 10;

                //                int radius = (int)(dp.planet.lsPlanet.pSettings.getActualRadius());
                //              int displayRadius = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / RenderSettings.GlobalRadiusScale * currentScale);
                float t = 0.001f;
                if (currentScale < t)
                    currentScale = t;



                                Vector3 newScale = Vector3.one * (0.00f + currentScale);
                /*                dp.go.transform.localScale = Vector3.one * dp.planet.lsPlanet.pSettings.radius * 2.0f;
                                //dp.SetWidth(newScale.x);
                                dp.planet.lsPlanet.pSettings.transform.localScale = newScale;
                                if (dp.planet.lsPlanet.pSettings.gameObject != null)
                                    dp.planet.lsPlanet.pSettings.gameObject.transform.localScale = newScale;

                                if (dp.planet.lsPlanet.pSettings.properties.terrainObject != null)
                                    dp.planet.lsPlanet.pSettings.properties.terrainObject.transform.localScale = newScale;
                                    */

                Util.ReScaleDetach(dp.planet.lsPlanet.pSettings.gameObject, newScale);
            }
            Slide();
        }



        Vector3 euler = Vector3.zero;

        private Vector3 cameraAdd = Vector3.zero;






        /*        public void CreateFakeOrbits(int steps, float stepLength)
                {
                    foreach (SerializedPlanet sp in szWorld.Planets)
                    {
                        int frame = 0;

                        float t0 = Random.value * 2 * Mathf.PI;
                        float radius = new Vector3((float)sp.pos_x, (float)sp.pos_y, (float)sp.pos_z).magnitude;
                        float modifiedStepLength = stepLength / Mathf.Sqrt(radius);
                        float rot = Random.value * 30f + 10f;
                        sp.Frames.Clear();
                        for (int i = 0; i < steps; i++)
                        {
                            float perturb = Mathf.Cos(i / (float)steps * 30.234f);
                            float rad = radius * (0.2f * perturb + 1);
                            Vector3 pos = new Vector3(Mathf.Cos(t0), 0, Mathf.Sin(t0)) * rad;
                            Frame f = new Frame();
                            f.pos_x = pos.x;
                            f.pos_y = pos.y;
                            f.pos_z = pos.z;
                            f.rotation = frame / rot;
                            f.id = frame;
                            sp.Frames.Add(f);
                            frame++;
                            t0 += modifiedStepLength;
                        }
                    }
                }

                */
        private void CreateLine(Vector3 f, Vector3 t, float c1, float c2, float w)
        {

            Color c = new Color(0.3f, 0.4f, 1.0f, 0.5f);

            GLLines l = new GLLines();
            l.points.Add(f);
            l.points.Add(t);
            l.color = c;
            data.glr.lines.Add(l);

        }

        private void CreateAxis()
        {
            float w = 30000;

            CreateLine(Vector3.zero, Vector3.up * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.up * w * -1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right * w * -1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward * w * -1, 1, 0.2f, 5);

        }

        public static GameObject satellite = null;


        public override void Start()
        {
            base.Start();
            SSVAppSettings.useAbsolutePositions = true;
            RenderSettings.planetCubeSphere = false;
            SSVAppSettings.GUIFont = (Font)Resources.Load("Fonts/LektonCode");
            SSVAppSettings.guiStyle.font = SSVAppSettings.GUIFont;

            CurrentApp = Verification.SolarSystemViewerName;

            SSVAppSettings.extraGameObject = new GameObject("extra");


            satellite = GameObject.Find("Satellite");
            if (satellite != null)
                satellite.SetActive(false);

            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
            PlanetTypes.Initialize();
            SetupCloseCamera();
            SzWorld = szWorld;
            slider = GameObject.Find("Slider");

            setText("TextVersion", "Version: " + RenderSettings.version.ToString("0.00"));

            CreateAxis();

            data.player.galaxy.Generate(50000, 1, 3000);
            data.currentSystem = data.player.galaxy.stars[0];
            data.player.AddToKnown(data.currentSystem);
            //solarSystem.GenerateSolarSystem(currentSystem);
            data.selectedSystem = data.currentSystem;



            if (Reload == true)
            {
                Debug.Log("RELOADING");
                SzWorld = RenderSettings.currentSZWorld;
                szWorld = SzWorld;
                solarSystem.LoadSZWold(this, szWorld, false, RenderSettings.GlobalRadiusScale, true);
            }

            CreateMainMenu();

        }



        public override void Update()
        {
                
            base.Update();
            UpdatePlay();
            if (Initialized)
                ForceSpaceCraft();

            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }


        }



        protected List<DisplayPlanet> getSpaceCrafts(List<DisplayPlanet> planets)
        {
            List<DisplayPlanet> spaceCrafts = new List<DisplayPlanet>();
            foreach (DisplayPlanet dp in data.dPlanets)
            {
                if (dp.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Object3D ||
                    dp.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Moon)
                {
                    //                    Debug.Log(dp.planet.lsPlanet.pSettings.name);

                    spaceCrafts.Add(dp);
                }
                else
                    planets.Add(dp);
            }

            return spaceCrafts;
        }



        protected void ForceSpaceCraft()
        {
            List<DisplayPlanet> planets = new List<DisplayPlanet>(); ;
            List<DisplayPlanet> spaceCrafts = getSpaceCrafts(planets);

            for (int i = 0; i < spaceCrafts.Count; i++)
            {
                DisplayPlanet spaceCraft = spaceCrafts[i];
                DisplayPlanet winner = null;
                float winnerLength = 1E30f;
                for (int j = 0; j < planets.Count; j++)
                {
                    DisplayPlanet dp = planets[j];
                    if (dp != spaceCraft)
                    {

                        float dist = (dp.go.transform.position - spaceCraft.go.transform.position).magnitude;
                        if (dist < winnerLength)
                        {
                            winnerLength = dist;
                            winner = dp;
                        }
                    }

                }

                /*                Vector3 newScale = Vector3.one * (0.00f + currentScale);
                                dp.go.transform.localScale = Vector3.one * dp.planet.lsPlanet.pSettings.radius * 2.0f;
                                dp.SetWidth(newScale.x);
                                dp.planet.lsPlanet.pSettings.transform.localScale = newScale;
                                */
                if (winner != null)
                {
                    Vector3 dir = (winner.go.transform.position - spaceCraft.go.transform.position) * -1;
                    float dist2 = dir.magnitude;
                    float scale = winner.go.transform.parent.transform.localScale.x * winner.planet.lsPlanet.pSettings.radius * 2f;
                    //scale = SSVAppSettings.SolarSystemScale;

                    if (dist2 < scale && spaceCraft.planet.lsPlanet.pSettings.radius < winner.planet.lsPlanet.pSettings.radius)
                    {
                        if (spaceCraft.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Object3D)
                            dist2 = 0;
                        //                        Debug.Log(spaceCraft.planet.lsPlanet.pSettings.radius + " vs " + winner.planet.lsPlanet.pSettings.radius);

                        spaceCraft.planet.lsPlanet.pSettings.gameObject.transform.position = winner.go.transform.position +
                           dir.normalized * (scale * 1.0001f + 1 * dist2 * SSVAppSettings.SolarSystemScale / 10.0f);
                    }
                }

            }

        }

        void CreateLoadFileMenu(MenuItem loadMenu, string fileType) { 
            List<string> flist = Util.FindFilesInDirectory(RenderSettings.path + RenderSettings.dataDir, fileType);
            foreach (string s in flist)
            {
                MenuItem file = new MenuItem(loadMenu, "File Menu", s.Replace(".xml",""), null, SSVAppSettings.menuSizeText + new Vector2(1.2f, 0), false, 2, loadMenu.layout, LoadSolarSystemFromXML, s);
                loadMenu.children.Add(file);
            }


        }


        MenuItem CreateFileMenu(MenuLayout mLayout)
        {
            MenuItem m = new MenuItem(mainMenu, "File Menu", "File", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null);
            MenuItem load = new MenuItem(m, "File Menu", "Load", null, SSVAppSettings.menuSizeText, false, 1, mLayout, null, null);
            CreateLoadFileMenu(load, "xml");
            m.children.Add(load);




            //m.children.Add(new MenuItem(m, "File Menu", "Save", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));

            return m;
        }

        MenuItem CreateNavigation(MenuLayout mLayout)
        {
            MenuItem m = new MenuItem(mainMenu, "Navigation", "Navigation", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null);
            m.children.Add(new MenuItem(m, "Go to", "Go to", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));

            return m;
        }

        void CreateMainMenu()
        {
            Color c = new Color(0.3f, 0.6f, 1.0f, 1.0f);
            GUIStyle s = new GUIStyle();
            s.font = (Font)Resources.Load(SSVAppSettings.MainFont);
            s.fontSize = SSVAppSettings.FontSize;
            s.normal.textColor = c * 4;
            s.alignment = TextAnchor.MiddleCenter;
            MenuLayout mLayout = new MenuLayout(16, s, c, c * 0.4f, MenuLayout.MenuStyle.SolidFrame);
            mLayout.background = (Texture2D)Resources.Load("GUI/GUIButton1");
            mainMenu = new MenuItem(null, "Main Menu", "", null, SSVAppSettings.menuSizeText, false, 1, mLayout, null, null);

            mainMenu.children.Add(CreateFileMenu(mLayout));
            mainMenu.children.Add(new MenuItem(mainMenu, "File Menu", "Assets", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));
            //            new MenuItem()


        }



        new void OnGUI()
        {

            if (mainMenu != null && MenuItem.isLock==false)
                mainMenu.Render(new Vector2(0.02f, 0.1f) * Screen.height);

            if (!Initialized)
                return;
            RenderLabels();

        }


        public void LoadSolarSystemFromXML(System.Object o)
        {
            Initialized = false;
//            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);

            string name = (string)o;
            name = RenderSettings.dataDir + name;// + ".xml";
            data.DestroyAllGameObjects();
            LoadFromXMLFile(name,false,true);


            szWorld.useSpaceCamera = false;
            PopulateOverviewList("Overview");
            PopulateWorld();


            //            Debug.Log(solarSystem.planets.Count);

            //          data.OrganizePlanetGameObjectsByName();
            data.OrganizeDisplayPlanetsFromGameObject();
            //foreach (DisplayPlanet dp in data.dPlanets)
            //    dp.CreateOrbitFromFrames(100);

            Slide();

            data.CreatePlanetHierarchy();
            //Update();
            solarSystem.Update();

            CreatePlanetMenu(true);
            data.CreateOrbitalLines();

            CreateAxis();
            Initialized = true;
            //Debug.Log("")
        }



        public void Slide()
        {
            //            if (szWorld.getMaxFrames()<=2)
            //              return;
            float v = Mathf.Clamp(slider.GetComponent<Slider>().value, 0.01f, 0.99f);
            SSVSettings.currentFrame = v;
            szWorld.InterpolatePlanetFrames(v, solarSystem.planets);
            foreach (DisplayPlanet dp in data.dPlanets)
            {
                dp.UpdatePosition();
            }
            ForceSpaceCraft();
        }


        private void setPlaySpeed(float v)
        {
            if (m_playSpeed == v)
            {
                m_playSpeed = 0;
            }
            else
            {
                m_playSpeed = v;
            }

        }
        /*
        public void playNormal()
        {
            setPlaySpeed(0.000025f);

        }

        public void playFast()
        {
            setPlaySpeed(0.0002f);
        }*/

        public void UpdatePlaySpeed()
        {
            float speed = GameObject.Find("SliderPlaySpeed").GetComponent<Slider>().value;
            setPlaySpeed((speed-0.5f)*0.01f);
        }



        protected void UpdatePlay()
        {
            //          Debug.Log(Time.time + " " + m_playSpeed);
            if (/*m_playSpeed > 0 && */solarSystem.planets.Count != 0)
            {
                float v = slider.GetComponent<Slider>().value;
                v += (float)m_playSpeed;
                v = Mathf.Clamp(v, 0, 1);
                //                Debug.Log("Playspeed after: " + m_playSpeed + " " + Time.time);
                slider.GetComponent<Slider>().value = v;

                Slide();
            }

        }
    }

}