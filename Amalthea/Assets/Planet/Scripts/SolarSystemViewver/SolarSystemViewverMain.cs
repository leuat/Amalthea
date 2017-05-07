using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LemonSpawn
{

    public class SSVSettings
    {
        public static float SolarSystemScale = 500.0f;
        public static float PlanetSizeScale = 1.0f / 50f;
        //        public static float PlanetSizeScale = (float)(500.0f/RenderSettings.AU);
        public static int OrbitalLineSegments = 100;
        public static Vector2 OrbitalLineWidth = new Vector2(1.63f, 1.63f);
        public static float currentFrame = 0;
        public static float LineScale = 1;
        public static Color orbitLinesColor = new Color(0.3f, 0.7f, 1.0f, 1.0f);
        public static Color spaceCraftColor = new Color(1.0f, 0.5f, 0.2f, 1f);
        public static Color moonColor = new Color(0.5f, 0.7f, 1.0f, 0.9f);
        public static Color planetColor = new Color(0.9f, 0.7f, 0.3f, 0.9f);
        public static string audioClickPlanet = "GUI40Click";
        public static string audioHoverMenu = "GUIPop";
        public static float StarCamFixDistance = 20000;
        public static bool shadowText = true;
        public static string MainFont = "LektonCode";
        public static GameObject extraGameObject;
        public static float MiniCamDist = 3;
        public static float MiniCamFOV = 60;
        public static float MiniCamSize = 0.1f;


    }

    //    exp(1/2)  * exp(-1/2) = 
    /*
        public class DisplayPlanetMCAST {
            public Planet planet;
            public SerializedPlanet serializedPlanet;
            public GameObject go;
            public GameObject textMesh;
            public List<GameObject> orbitLines = new List<GameObject>();
    //        public bool isMoon = false;

            /*		private void CreateOrbitCircles() {
                        float radius = (float)planet.lsPlanet.pSettings.properties.pos.Length () * SSVSettings.SolarSystemScale;
                        for (int i = 0; i < SSVSettings.OrbitalLineSegments; i++) {
                            float t0 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)i;
                            float t1 = 2 * Mathf.PI / (float)SSVSettings.OrbitalLineSegments * (float)(i+1);
                            Vector3 from = new Vector3 (Mathf.Cos (t0), 0, Mathf.Sin (t0)) * radius;
                            Vector3 to = new Vector3 (Mathf.Cos (t1), 0, Mathf.Sin (t1)) * radius;

                            GameObject g = new GameObject ();
                            g.transform.parent = SolarSystemViewverMain.linesObject.transform;
                            LineRenderer lr = g.AddComponent<LineRenderer> ();
                            lr.material = (Material)Resources.Load ("LineMaterial");
                            lr.SetWidth (SSVSettings.OrbitalLineWidth.x, SSVSettings.OrbitalLineWidth.y);
                            lr.SetPosition (0, from);
                            lr.SetPosition (1, to);
                            orbitLines.Add (g);
                        }
                    }
            */
    /*        public void CreateTextMesh() {
                textMesh = new GameObject();
                textMesh.transform.parent = go.transform;
                textMesh.transform.localPosition = new Vector3(0,0,0);

                GUIText tm = textMesh.AddComponent<GUIText>();
                tm.color = new Color(0.3f, 0.6f, 1.0f,1.0f);
                tm.text = planet.lsPlanet.pSettings.name + "HALLA";
                tm.fontSize = 10;
            //    tm.font = (Font)Resources.Load("CaviarDreams");
            }
            */

    /*
    public void MaintainOrbits() {
        int maxFrames = serializedPlanet.Frames.Count;
        int currentFrame = (int)(SSVSettings.currentFrame*maxFrames);
        Color c = SSVSettings.orbitLinesColor;
        if (planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Spacecraft)
            c = SSVSettings.spaceCraftColor;

        int h = orbitLines.Count / 1;

        for (int i=0;i<orbitLines.Count;i++) {
            int f1 = (int)Mathf.Clamp((i-h)*SSVSettings.LineScale +currentFrame  ,0,maxFrames);
            int f2 = (int)Mathf.Clamp((i+1-h) * SSVSettings.LineScale + currentFrame , 0, maxFrames);
            if (f1 >= serializedPlanet.Frames.Count || f1<0)
                break;
            if (f2 >= serializedPlanet.Frames.Count || f2 < 0)
                break;
            LineRenderer lr = orbitLines[i].GetComponent<LineRenderer>();
            Frame sp = serializedPlanet.Frames[f1];
            Frame sp2 = serializedPlanet.Frames[f2];
            DVector from = new DVector (sp.pos_x, sp.pos_y,sp.pos_z) * SSVSettings.SolarSystemScale;
            DVector to = new DVector (sp2.pos_x, sp2.pos_y,sp2.pos_z) * SSVSettings.SolarSystemScale;


            lr.SetPosition (0, from.toVectorf());
            lr.SetPosition (1, to.toVectorf());

            float colorScale = Mathf.Abs(i-orbitLines.Count/2)/(float)orbitLines.Count*2;
            Color col = c*(1-colorScale);
            col.a = 1;
            lr.SetColors(col,col);
        }
    }

    public void DestroyOrbits() {
        foreach (GameObject go in orbitLines) {
            GameObject.Destroy(go);
            }
        orbitLines.Clear();


    }

    public void SetWidth(float w)
    {
        foreach (GameObject g in orbitLines)
        {
            LineRenderer lr = g.GetComponent<LineRenderer>();
            lr.SetWidth(SSVSettings.OrbitalLineWidth.x*w, SSVSettings.OrbitalLineWidth.y*w);
        }
    }


    public void CreateOrbitFromFrames(int maxLines) {

        DestroyOrbits();

        if (serializedPlanet.Frames.Count<2)
            return;     

        if (planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Moon)
            return;

        for (int i = 0; i < maxLines; i++) {
            if (i+1>=serializedPlanet.Frames.Count)
                break;
            Frame sp = serializedPlanet.Frames[i];
            Frame sp2 = serializedPlanet.Frames[i+1];
            DVector from = new DVector (sp.pos_x, sp.pos_y,sp.pos_z) * SSVSettings.SolarSystemScale;
            DVector to = new DVector (sp2.pos_x, sp2.pos_y,sp2.pos_z) * SSVSettings.SolarSystemScale;

            GameObject g = new GameObject ();
            g.transform.parent = SolarSystemViewverMain.linesObject.transform;
            LineRenderer lr = g.AddComponent<LineRenderer> ();
            lr.material = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
            lr.SetWidth (SSVSettings.OrbitalLineWidth.x, SSVSettings.OrbitalLineWidth.y);
            lr.SetPosition (0, from.toVectorf());
            lr.SetPosition (1, to.toVectorf());
            orbitLines.Add (g);
        }
    }



    public DisplayPlanetMCAST(GameObject g, Planet p, SerializedPlanet sp) {
        go = g;
        planet = p;
        serializedPlanet = sp;
//            CreateTextMesh();
        //CreateOrbitFromFrames ();
        //if (planet.lsPlanet.pSettings.name.ToLower().Contains("moon"))
        //    isMoon = true;
    }

    public void UpdatePosition() {
        planet.lsPlanet.pSettings.properties.pos*=SSVSettings.SolarSystemScale;
        planet.lsPlanet.pSettings.transform.position = planet.lsPlanet.pSettings.properties.pos.toVectorf();
        go.transform.position = planet.lsPlanet.pSettings.properties.pos.toVectorf();
        MaintainOrbits();
    }

}
*/
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
        public override void DisplayObjectText(DisplayPlanet dp)
        {
            if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
            {
                DisplayStarInfo(data.currentSystem);
                return;
            }
            else
    if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Spacecraft)
            {
                setText("txtPlanetName", "Spacecraft");
                setText("txtPlanetName2", "Spacecraft");
                setText("txtPlanetType", "Spacecraft");
                setText("txtPlanetInfo", "Spacecraft");
                return;
            }

            setText("txtPlanetType", dp.planet.lsPlanet.pSettings.planetType.name);
            setText("txtPlanetName", dp.planet.lsPlanet.pSettings.givenName);
            setText("txtPlanetName2", "(" + dp.planet.lsPlanet.pSettings.name + ")");

            string infoText = "";
            int radius = (int)(dp.planet.lsPlanet.pSettings.getActualRadius());
            int displayRadius = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / LemonSpawn.RenderSettings.GlobalRadiusScale * currentScale);
            float orbit = (dp.planet.lsPlanet.pSettings.properties.pos.toVectorf().magnitude);///(float)SSVSettings.SolarSystemScale);
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


        public void SlideScaleLines()
        {
            Slider slider = GameObject.Find("SliderScaleLines").GetComponent<Slider>();

            SSVSettings.LineScale = slider.value * 10;
            //   foreach (DisplayPlanetMCAST dp in dPlanets)
            //       dp.MaintainOrbits();
        }


        private float currentScale = 1;

        public void SlideScale()
        {
            Slider slider = GameObject.Find("SliderScale").GetComponent<Slider>();
            foreach (DisplayPlanetMCAST dp in data.dPlanets)
            {

                currentScale = slider.value * 10;

                //                int radius = (int)(dp.planet.lsPlanet.pSettings.getActualRadius());
                //              int displayRadius = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / RenderSettings.GlobalRadiusScale * currentScale);
                float t = 0.001f;
                if (currentScale < t)
                    currentScale = t;



                Vector3 newScale = Vector3.one * (0.00f + currentScale);
                dp.go.transform.localScale = Vector3.one * dp.planet.lsPlanet.pSettings.radius * 2.0f;
                //dp.SetWidth(newScale.x);
                dp.planet.lsPlanet.pSettings.transform.localScale = newScale;
                if (dp.planet.lsPlanet.pSettings.gameObject != null)
                    dp.planet.lsPlanet.pSettings.gameObject.transform.localScale = newScale;
                if (dp.planet.lsPlanet.pSettings.properties.terrainObject != null)
                    dp.planet.lsPlanet.pSettings.properties.terrainObject.transform.localScale = newScale;
            }
            Slide();
        }



        Vector3 euler = Vector3.zero;

        private Vector3 cameraAdd = Vector3.zero;


/*        private void UpdateCamera()
        {
            float s = 1.0f;
            float theta = 0.0f;
            float phi = 0.0f;

            //            if (Input.GetMouseButton(1))
            {
                //                Debug.Log("TEST");
                //              theta = s * Input.GetAxis("Mouse X");
                //            phi = s * Input.GetAxis("Mouse Y") * -1.0f;
                theta = s * Input.GetAxis("Horizontal");
                phi = s * Input.GetAxis("Vertical") * -1.0f;
            }
            mouseAccel += new Vector3(theta, phi, 0);
            float t = 0.1f;
            focusPointCur = (focusPoint * t + focusPointCur * (1 - t));

            //focusPointCurStar = (selectedSystem.position * t + focusPointCurStar * (1 - t));

            mouseAccel *= 0.65f;

            euler += mouseAccel * 10f;
            //            Debug.Log(theta);
            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.up, mouseAccel.x);

            Vector3 starFocus = focusPointCurStar;// + focusPointCur * SSVSettings.starCameraScale;

            starCamera.transform.RotateAround(starFocus, starCamera.transform.up, mouseAccel.x);
            //            Debug.Log(selectedSystem.position);



            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) > 0.99)
                if (mouseAccel.y < 0)
                    mouseAccel.y = 0;
            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) < -0.99)
                if (mouseAccel.y > 0)
                    mouseAccel.y = 0;


            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.right, mouseAccel.y);

            starCamera.transform.RotateAround(starFocus, starCamera.transform.right, mouseAccel.y);
            mainCamera.transform.LookAt(focusPointCur);
            starCamera.transform.LookAt(starFocus);
            if (currentMode == Mode.InterPlanetary)
                starCamera.transform.rotation = mainCamera.transform.rotation;
            //			starCamera.transform.forward = mainCamera.transform.forward

        }
*/

        



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

            Color c = new Color(0.3f, 0.4f, 1.0f, 1.0f);
            GameObject g = new GameObject();
            LineRenderer lr = g.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
            lr.SetWidth(w, w);
            lr.SetPosition(0, f);
            lr.SetPosition(1, t);
            Color cc1 = c * c1;
            Color cc2 = c * c2;
            cc1.a = 0.4f;
            cc2.a = 0.4f;
            lr.SetColors(cc1, cc2);
        }

        private void CreateAxis()
        {
            float w = 10000;

            CreateLine(Vector3.zero, Vector3.up * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.up * w * -1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.right * w * -1, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward * w, 1, 0.2f, 5);
            CreateLine(Vector3.zero, Vector3.forward * w * -1, 1, 0.2f, 5);

        }

        public static GameObject satellite = null;

        public void TestSlapDash()
        {
            SlapDash d = new SlapDash();
            System.Random rnd = new System.Random(4);
            foreach (Language l in d.languages)
            {
                string s = "";

                for (int i = 0; i < 100; i++)
                    s += l.GenerateWord(rnd) + "  ";
                Debug.Log(s);

            }


        }

        public override void Start()
        {
            //            TestSlapDash();
            base.Start();
            SSVAppSettings.GUIFont = (Font)Resources.Load("Fonts/LektonCode");
            SSVAppSettings.guiStyle.font = SSVAppSettings.GUIFont;

            CurrentApp = Verification.SolarSystemViewerName;

            SSVSettings.extraGameObject = new GameObject("extra");


            satellite = GameObject.Find("Satellite");
            if (satellite != null)
                satellite.SetActive(false);

            solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);
            PlanetTypes.Initialize();
            SetupCloseCamera();
            PopulateFileCombobox("ComboBoxLoadFile", "xml");
            SzWorld = szWorld;
            slider = GameObject.Find("Slider");

            setText("TextVersion", "Version: " + RenderSettings.version.ToString("0.00"));

            CreateAxis();

            data.player.galaxy.Generate(50000, 0, 3000);
            data.currentSystem = data.player.galaxy.stars[0];
            data.player.AddToKnown(data.currentSystem);
            //solarSystem.GenerateSolarSystem(currentSystem);
            data.selectedSystem = data.currentSystem;



            if (Reload == true)
            {
                Debug.Log("RELOADING");
                SzWorld = RenderSettings.currentSZWorld;
                szWorld = SzWorld;
                solarSystem.LoadSZWold(this, szWorld, false, RenderSettings.GlobalRadiusScale);
            }

            CreateMainMenu();

            //			LoadData ();
        }



        public override void Update()
        {
            base.Update();
            

            UpdatePlay();
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
                if (dp.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Spacecraft ||
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
                    //scale = SSVSettings.SolarSystemScale;

                    if (dist2 < scale && spaceCraft.planet.lsPlanet.pSettings.radius < winner.planet.lsPlanet.pSettings.radius)
                    {
                        if (spaceCraft.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Spacecraft)
                            dist2 = 0;
                        //                        Debug.Log(spaceCraft.planet.lsPlanet.pSettings.radius + " vs " + winner.planet.lsPlanet.pSettings.radius);

                        spaceCraft.planet.lsPlanet.pSettings.gameObject.transform.position = winner.go.transform.position +
                           dir.normalized * (scale * 1.0001f + 1 * dist2 * SSVSettings.SolarSystemScale / 10.0f);
                    }
                }

            }

        }

        MenuItem CreateFileMenu(MenuLayout mLayout)
        {
            MenuItem m = new MenuItem(mainMenu, "File Menu", "File", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null);
            m.children.Add(new MenuItem(m, "File Menu", "Load", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));
            m.children.Add(new MenuItem(m, "File Menu", "Save", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));

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
            s.font = (Font)Resources.Load(SSVSettings.MainFont);
            s.fontSize = 16;
            s.normal.textColor = c * 4;
            s.alignment = TextAnchor.MiddleCenter;
            MenuLayout mLayout = new MenuLayout(16, s, c, c * 0.4f, MenuLayout.MenuStyle.SolidFrame);
            mLayout.background = (Texture2D)Resources.Load("GUIButton1");
            mainMenu = new MenuItem(null, "Main Menu", "", null, SSVAppSettings.menuSizeText, false, 1, mLayout, null, null);

            mainMenu.children.Add(CreateFileMenu(mLayout));
            mainMenu.children.Add(new MenuItem(mainMenu, "File Menu", "Assets", null, SSVAppSettings.menuSizeText, true, 1, mLayout, null, null));
            //            new MenuItem()


        }



        protected void OnGUI()
        {
            RenderLabels();
            if (mainMenu != null)
                mainMenu.Render(new Vector2(0.02f, -0.1f) * Screen.height);

        }

        public void LoadFileFromMenu()
        {
            DeFocus();

            int idx = GameObject.Find("ComboBoxLoadFile").GetComponent<UnityEngine.UI.Dropdown>().value;
            string name = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().options[idx].text;
            if (name == "-")
                return;
            name = RenderSettings.dataDir + name + ".xml";

            LoadFromXMLFile(name);


            szWorld.useSpaceCamera = false;
            PopulateOverviewList("Overview");
            PopulateWorld();
            //foreach (DisplayPlanet dp in data.dPlanets)
            //    dp.CreateOrbitFromFrames(100);

            Slide();
            data.CreatePlanetHierarchy();
            CreatePlanetMenu(false);
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

        public void playNormal()
        {
            setPlaySpeed(0.000025f);

        }

        public void playFast()
        {
            setPlaySpeed(0.0002f);
        }


        protected void UpdatePlay()
        {
            //          Debug.Log(Time.time + " " + m_playSpeed);
            if (m_playSpeed > 0 && solarSystem.planets.Count != 0)
            {
                float v = slider.GetComponent<Slider>().value;
                v += (float)m_playSpeed;
                if (v >= 1)
                {
                    m_playSpeed = 0;
                    v = 1;
                }
                //                Debug.Log("Playspeed after: " + m_playSpeed + " " + Time.time);
                slider.GetComponent<Slider>().value = v;

                Slide();
            }

        }
    }

}