using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Amalthea {

	public class SSVSettings {
		public static float SolarSystemScale = 1500.0f;
	public static float PlanetSizeScale = 1.0f / 100f;
//        public static float PlanetSizeScale = (float)(500.0f/RenderSettings.AU);
        public static int OrbitalLineSegments = 100;
		public static Vector2 OrbitalLineWidth = new Vector2 (1.63f, 1.63f);
        public static float currentFrame = 0;
        public static float LineScale = 1;
        public static Color orbitLinesColor = new Color(0.3f, 0.7f, 1.0f,1.0f);
        public static Color spaceCraftColor = new Color(1.0f, 0.5f, 0.2f, 1f);
        public static Color moonColor = new Color(0.5f, 0.7f, 1.0f, 0.9f);
        public static Color planetColor = new Color(0.9f, 0.7f, 0.3f, 0.9f);
        public static float starCameraScale = 0.00001f;
        public static GameObject extraGameObject;
        public static float MiniCamDist = 3;
        public static float MiniCamFOV = 60;
        public static float MiniCamSize = 0.1f;
        public static float ManagePlanetDistance = 2.0f;
        public static string audioClickPlanet = "GUI40Click";
        public static string audioHoverMenu = "GUIPop";
        public static float StarCamFixDistance = 20000;
        


        public static AudioClip loadAudio(string s)
        {
            return (AudioClip)Resources.Load("Effects/" + s);
        } 

        
    }


   

    public class SolarSystemViewverMain : LemonSpawn.World
    {


        public static AudioSource audioSource;
        private List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
        DisplayPlanet dpSun = null;
        private Vector3 mouseAccel = new Vector3();
        private Vector3 focusPoint = Vector3.zero;
        private Vector3 focusPointCur = Vector3.zero;
        private Vector3 focusPointCurStar = Vector3.zero;
        private float scrollWheel, scrollWheelAccel;
        private DisplayPlanet selected = null;
        public static GameObject linesObject = null;
        private GameObject pnlInfo = null;
        public static bool Reload = false;
        private float currentDistance;
        public GameObject starCamera;
        public bool initialized = false;
        private APlayer player = new APlayer();
        private StarSystem currentSystem = null, selectedSystem = null;
        
        private Font GUIFont;
        private GUIStyle guiStyle = new GUIStyle();
        private bool toggleLabels = true;

        private enum Mode { InterPlanetary, EditPlanet, Interstellar };
        private Mode currentMode = Mode.InterPlanetary;


        public float tempTime = 0;
        public float counter=0;

        private MenuItem mainMenu;

        static Vector2 menuSizeText = new Vector2(0.2f, 0.2f) * Screen.height;
        static Vector2 menuSizeImage = new Vector2(0.2f, 0.2f) * Screen.height;


        public void ClickManagePlanet()
        {


            if (currentMode == Mode.InterPlanetary)
            {
                currentMode = Mode.EditPlanet;
                setText("ToggleManageText", "Back to system");
            }
            else
            {
                currentMode = Mode.InterPlanetary;
                setText("ToggleManageText", "Manage planet");
            }
        }



        public DisplayPlanet findDisplayPlanet(LemonSpawn.Planet p)
        {
            foreach (DisplayPlanet dp in dPlanets)
                if (dp.planet.lsPlanet == p)
                    return dp;

            return null;
        }
        public DisplayPlanet findDisplayPlanetWithparent(LemonSpawn.Planet p)
        {
            foreach (DisplayPlanet dp in dPlanets)
                if (dp.planet.lsPlanet.pSettings.properties.parentPlanet == p)
                    return dp;

            return null;
        }


        protected void setText(string box, string text)
        {
            //  Debug.Log(box);
            GameObject.Find(box).GetComponent<Text>().text = text;
        }


        public void SelectPlanetMenu(System.Object o)
        {
            SelectPlanet((DisplayPlanet)o);
        }

        public static void PlaySound(string sound, float amp)
        {
            SolarSystemViewverMain.audioSource.PlayOneShot(SSVSettings.loadAudio(sound), amp);

        }

        private void DisplayStarInfo(StarSystem star)
        {
            setText("txtPlanetType", star.getCategory());
            setText("txtPlanetName", star.getName());
            setText("txtPlanetName2", "(" + star.noPlanets + " planets" + ")");
            string infoText2 = "";
            float radius2 = (int)(star.radius);
            //                int displayRadius2 = (int)((dp.planet.lsPlanet.pSettings.getActualRadius()) / LemonSpawn.RenderSettings.GlobalRadiusScale * currentScale);
            infoText2 += "Radius           : " + radius2.ToString("0.00") + " sun radii" + "\n";
            //          infoText += "Displayed Radius : " + displayRadius / radius + " x original radius\n";
            infoText2 += "Mass             : " + star.mass + " sun masses" + "\n";
            //            infoText += "Displayed Radius : " + displayRadius + " km \n";
            infoText2 += "Temperature      : " + (int)star.temperature + "K\n";
            //               infoText2 += dp.planet.lsPlanet.pSettings.planetType.PlanetInfo;
            setText("txtPlanetInfo", infoText2);

        }


        private void SelectPlanet(DisplayPlanet dp, bool trigger=true)
        {
            if (currentMode == Mode.Interstellar)
                return;

             selected = dp;
            focusPoint = dp.go.transform.position;
            pnlInfo.SetActive(true);

            if (currentDistance == 0)
                currentDistance = (dp.planet.lsPlanet.pSettings.gameObject.transform.position - MainCamera.transform.position).magnitude;

            if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
            {
                DisplayStarInfo(currentSystem);
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
                        int displayRadius = (int)((dp.planet.lsPlanet.pSettings.getActualRadius())/LemonSpawn.RenderSettings.GlobalRadiusScale*currentScale);
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

            if (trigger)
            {
                PlaySound(SSVSettings.audioClickPlanet, 0.5f);
                dp.Trigger();
            }

            if (currentMode == Mode.EditPlanet)
            {
                //mainCamera.transform.position = selected.planet.lsPlanet.pSettings.transform.position - Vector3.left * selected.planet.lsPlanet.pSettings.radius * SSVSettings.ManagePlanetDistance;
                mainCamera.transform.LookAt(selected.planet.lsPlanet.pSettings.transform.position);
            }


        }


        /*
                public void FocusOnPlanetClick()
                {
                    int idx = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().value;
                    string name = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().options[idx].text;

                    foreach (DisplayPlanet dp in dPlanets)
                        if (name.Contains(dp.planet.lsPlanet.pSettings.name))
                            SelectPlanet(dp);

                }
                */

        private void DeFocus()
        {
            selected = null;
            focusPoint = Vector3.zero;
            pnlInfo.SetActive(false);
            currentDistance = 0;
        }



/*        private void Initialize()
        {
            CreatePlanetMenu();
            initialized = true;
        }
        */
        private void RenderSolarSystemLabels()
        {
            foreach (DisplayPlanet dp in dPlanets)
            {
                guiStyle.normal.textColor = SSVSettings.planetColor;
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                {
                    Color c = SSVSettings.moonColor;
                    c.a = Mathf.Clamp(1 - 0.001f * (dp.go.transform.position - mainCamera.transform.position).magnitude, 0, 1);
                    guiStyle.normal.textColor = c;
                }
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                {
                    Color c = SSVSettings.planetColor;
                    c.a = Mathf.Clamp(1 - 0.00001f * (dp.go.transform.position - mainCamera.transform.position).magnitude, 0, 1);
                    guiStyle.normal.textColor = c;
                }
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Spacecraft)
                    guiStyle.normal.textColor = SSVSettings.spaceCraftColor;

                guiStyle.normal.textColor = guiStyle.normal.textColor * (1 + dp.timer);


                Vector3 pos = MainCamera.WorldToScreenPoint(dp.go.transform.position);
                //int width1 = dp.planet.lsPlanet.pSettings.givenName.Trim().Length;
                int width2 = dp.planet.lsPlanet.pSettings.name.Trim().Length;
                int fs = (int)Mathf.Clamp( 16 + (int)Mathf.Pow(dp.planet.lsPlanet.pSettings.radius, 0.6f) + (int)(dp.timer * 20f) ,8 ,35);
                guiStyle.fontSize = fs;
                //                if (pos.x >0 && pos.y<Screen.width && pos.y>0 && pos.y<Screen.height)
                if (pos.z > 0 && guiStyle.normal.textColor.a > 0)
                {
                    float ha = 30;
                    float gf = guiStyle.fontSize / 2;
                    GUI.Label(new Rect(pos.x - gf * dp.planet.lsPlanet.pSettings.givenName.Length / 2, Screen.height - pos.y - ha - gf, 250, 130), dp.planet.lsPlanet.pSettings.givenName, guiStyle);
                    guiStyle.fontSize = 12;

                    GUI.Label(new Rect(pos.x - (width2 / 2) * 4, Screen.height - pos.y + (int)(fs * 1.0) - ha, 250, 130), dp.planet.lsPlanet.pSettings.name, guiStyle);
                }

            }

        }

        private void RenderStarLabel(StarSystem star)
        {
//            guiStyle.normal.textColor = star.color;
            Color c = star.color;
            c.a = Mathf.Clamp(1 - 0.0025f * (star.position - starCamera.transform.position).magnitude, 0, 1);
            guiStyle.normal.textColor = c;
            if (guiStyle.normal.textColor.a <= 0)
                return;

            Vector3 pos = starCamera.GetComponent<Camera>().WorldToScreenPoint(star.position);
            int width2 = star.getName().Trim().Length;
            int fs = (int)(25 *(0.25f + 0.75f*c.a)) ;
            guiStyle.fontSize = fs;
            if (pos.z > 0)
            {
                float ha = 30;
                float gf = guiStyle.fontSize / 2;
                GUI.Label(new Rect(pos.x - gf * star.getName().Length / 2, Screen.height - pos.y - ha - gf, 250, 130), star.getName(), guiStyle);
                guiStyle.fontSize = 12;

                //GUI.Label(new Rect(pos.x - (width2 / 2) * 4, Screen.height - pos.y + (int)(fs * 1.0) - ha, 250, 130), dp.planet.lsPlanet.pSettings.name, guiStyle);
            }

        }

        private void RenderInterstellarLabels()
        {
            //RenderStarLabel(selectedSystem);
            foreach (StarSystem s in player.localSystems)
                RenderStarLabel(s);
        }



        private void RenderLabels()
        {
            if (!toggleLabels)
                return;


            GUI.skin.font = GUIFont;
            if (currentMode == Mode.InterPlanetary)
                RenderSolarSystemLabels();

            if (currentMode == Mode.Interstellar)
                RenderInterstellarLabels();
        }


        public void SlideTime()
        {
            Slider slider = GameObject.Find("Slider").GetComponent<Slider>();

            //SSVSettings.LineScale = slider.value * 10;
            tempTime = slider.value * 0.002f;

        }


        private float currentScale = 1;

        public void SlideScale()
        {


            Slider slider = GameObject.Find("SliderScale").GetComponent<Slider>();


            return;
            foreach (DisplayPlanet dp in dPlanets)
            {

                currentScale = slider.value * 10;

                //                int radius = (int)(dp.planet.pSettings.getActualRadius());
                //              int displayRadius = (int)((dp.planet.pSettings.getActualRadius()) / RenderSettings.GlobalRadiusScale * currentScale);
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
        }


        private void UpdateFocus()
        {
            if (Input.GetMouseButtonDown(0) && currentMode == Mode.InterPlanetary)
            {
                RaycastHit hit;
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    foreach (DisplayPlanet dp in dPlanets)
                    {
                        if (dp.go == hit.transform.gameObject)
                        {
                            SelectPlanet(dp);
                                                    }
                    }
                }
                else
                {
                    //if (!EventSystem.current.IsPointerOverGameObject() )
                    //    DeFocus();
                }
            }
        }

        Vector3 euler = Vector3.zero;

        private Vector3 cameraAdd = Vector3.zero;


        private void UpdateCamera()
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
            /*            focusPointCur += (focusPoint - focusPointCur) * 0.05f;
                        focusPointCurStar += (selectedSystem.position - focusPointCurStar) * 0.05f;
                        */
            float t = 0.1f;
            focusPointCur = (focusPoint*t + focusPointCur*(1-t));
            focusPointCurStar = (selectedSystem.position*t + focusPointCurStar*(1-t));

            mouseAccel *= 0.65f;

            euler += mouseAccel * 10f;
//            Debug.Log(theta);
            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.up, mouseAccel.x);
            starCamera.transform.RotateAround(focusPointCurStar, starCamera.transform.up, mouseAccel.x);
//            Debug.Log(selectedSystem.position);



            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) > 0.99)
                if (mouseAccel.y < 0)
                    mouseAccel.y = 0;
            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) < -0.99)
                if (mouseAccel.y > 0)
                    mouseAccel.y = 0;


            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.right, mouseAccel.y);
            mainCamera.transform.LookAt(focusPointCur);

            starCamera.transform.RotateAround(focusPointCurStar, starCamera.transform.right, mouseAccel.y);
            starCamera.transform.LookAt(focusPointCurStar);


/*            if (currentMode == Mode.Move)
            {


                if (selected != null && Mathf.Abs(scrollWheel) < 0.001)
                {
                    Vector3 dir = selected.planet.lsPlanet.pSettings.gameObject.transform.position - mainCamera.transform.position;
                    float dist = dir.magnitude;
                    //  Debug.Log("LOWER:" + dist + " c: " + currentDistance);
                    if (Mathf.Abs(dist - currentDistance) > 0.01)
                    {
                        float add = Mathf.Pow(dist - currentDistance, 0.5f);
                        cameraAdd += dir.normalized * add * 0.6f;
                        //                    mainCamera.transform.position = mainCamera.transform.position + dir.normalized * add;
                    }

                }

                mainCamera.transform.position = mainCamera.transform.position + cameraAdd;
            }*/
            if (currentMode == Mode.EditPlanet)
            {
                Vector3 dir = (mainCamera.transform.position - selected.planet.lsPlanet.pSettings.transform.position).normalized*-1;
                mainCamera.transform.position = selected.planet.lsPlanet.pSettings.transform.position - dir * selected.planet.lsPlanet.pSettings.radius * SSVSettings.ManagePlanetDistance;
            }
            cameraAdd *= 0.5f;
        }

        //        private void CreatePlanetHierarchy(DisplayPlanets dp) { }




        MenuItem CreateFileMenu(MenuLayout mLayout)
        {
            MenuItem m = new MenuItem(mainMenu, "File Menu", "File", null, menuSizeText, true, 1, mLayout, null, null);
            m.children.Add(new MenuItem(m, "File Menu", "Load", null, menuSizeText, true, 1, mLayout, null, null));
            m.children.Add(new MenuItem(m, "File Menu", "Save", null, menuSizeText, true, 1, mLayout, null, null));
            
            return m;
        }

        MenuItem CreateNavigation(MenuLayout mLayout)
        {
            MenuItem m = new MenuItem(mainMenu, "Navigation", "Navigation", null, menuSizeText, true, 1, mLayout, null, null);
            m.children.Add(new MenuItem(m, "Go to", "Go to", null, menuSizeText, true, 1, mLayout, null, null));

            return m;
        }

        void CreateMainMenu()
        {
            Color c = new Color(0.3f, 0.6f, 1.0f, 1.0f);
            GUIStyle s = new GUIStyle();
            s.font = (Font)Resources.Load("CaviarDreams");
            s.fontSize = 16;
            s.normal.textColor = c*4;
            s.alignment = TextAnchor.MiddleCenter;
            MenuLayout mLayout = new MenuLayout(16, s, c, c * 0.4f, MenuLayout.MenuStyle.SolidFrame);
            mainMenu = new Amalthea.MenuItem(null, "Main Menu", "", null, menuSizeText, false, 1, mLayout, null, null);

            mainMenu.children.Add(CreateFileMenu(mLayout));
            mainMenu.children.Add(new MenuItem(mainMenu, "File Menu", "Assets", null, menuSizeText, true, 1, mLayout, null, null));
//            new MenuItem()


        }

        private void CreatePlanetMenu(bool isInterPlanetary)
        {
            mainMenu.deleteFromChildren("SolarSystem");
            if (isInterPlanetary)
            {
                foreach (DisplayPlanet dp in dPlanets)
                    dp.CreatePlanetCamera();

                dpSun.CreateMenu("SolarSystem", mainMenu, menuSizeImage, true, 0.75f, mainMenu.layout, SelectPlanetMenu);
            }
            mainMenu.replaceItem("SolarSystem", 0);
        }



        private void CreatePlanetHierarchy()
        {
            // First, find the sun
            //            DisplayPlanet sun = dPlanets[0];
            dpSun = findDisplayPlanetWithparent(null);
            dpSun.children.Clear();
            foreach (DisplayPlanet dp in dPlanets)
            {
                if (dp.planet.lsPlanet.pSettings.properties.parentPlanet == dpSun.planet.lsPlanet)
                {
  //                  Debug.Log("Adding child " + dp.planet.lsPlanet.pSettings.name);
                    dpSun.children.Add(dp);
                }
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                foreach (DisplayPlanet sp in dPlanets)
                {
                    if (sp.planet.lsPlanet.pSettings.properties.parentPlanet == dp.planet.lsPlanet)
                    {

                        dp.children.Add(sp);
                    }
                }

            }
//            Debug.Log("CHILDREN COUNT" + dpSun.children.Count);
            // Find all who have this as a parent
        }



        private void PopulateWorld()
        {
            DestroyAllGameObjects();
            dPlanets.Clear();

            //solarSystem.InitializeFromScene();

            foreach (LemonSpawn.Planet p in solarSystem.planets)
            {
                GameObject go = p.pSettings.gameObject;

                Vector3 coolpos = p.pSettings.properties.pos.toVectorf();// new Vector3((float)p.pSettings.properties.pos.x, (float)p.pSettings.properties.pos.y, (float)p.pSettings.properties.pos.z);
                float ms = 1;
                float prevRadius = 0;
                if (p.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                {
                    ms = 2.0f / SSVSettings.PlanetSizeScale;
                    prevRadius = p.pSettings.transform.parent.gameObject.GetComponent<LemonSpawn.PlanetSettings>().radius;
                    //Debug.Log("IS MOON");
                }

                coolpos = Vector3.zero;
                go.transform.localPosition = coolpos * SSVSettings.SolarSystemScale*ms + coolpos.normalized*prevRadius;
                p.pSettings.properties.pos = new LemonSpawn.DVector(coolpos);
                //go.transform.localScale = Vector3.one * SSVSettings.PlanetSizeScale * p.pSettings.radius;
                //p.pSettings.atmoDensity = 0;

                GameObject hidden = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hidden.transform.parent = p.pSettings.transform;
                hidden.transform.localPosition = Vector3.zero;// coolpos * SSVSettings.SolarSystemScale*ms;
                hidden.transform.localScale = Vector3.one * p.pSettings.radius * 3f;
//                Debug.Log(p.pSettings.name);

                //if (p.pSettings.planetTypeName=="star" || p.pSettings.planetTypeName=="spacecraft")
                //    hidden.SetActive(false);

                hidden.GetComponent<MeshRenderer>().material = (Material)Resources.Load("HiddenMaterial");
                Planet ap = new Amalthea.Planet();
                ap.lsPlanet = p;
                if (p.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                    ap.stellarCategory = Globals.definitions.stellarCategories.Get("Planet");
                if (p.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                    ap.stellarCategory = Globals.definitions.stellarCategories.Get("Moon");
                if (p.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star) 
                    ap.stellarCategory = Globals.definitions.stellarCategories.Get("Star");
                dPlanets.Add(new DisplayPlanet(hidden, ap));
            }
            selected = dPlanets[0];
            System.Diagnostics.Stopwatch so = new System.Diagnostics.Stopwatch();
            so.Start();

            foreach (DisplayPlanet dp in dPlanets)
            {
                dp.UpdatePosition(tempTime, mainCamera.transform.position);
                dp.CreateOrbits(SSVSettings.OrbitalLineSegments);
            }
            Debug.Log("Orbits:  " + so.Elapsed);
            CreatePlanetHierarchy();
        }



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
            float w = 100;

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
            //            TestSlapDash();

            LemonSpawn.RenderSettings.planetCubeSphere = false;


            GUIFont = (Font)Resources.Load("CaviarDreams");
            guiStyle.font = GUIFont;
            Globals.Save();
            Globals.Initialize();
            audioSource = GameObject.Find("GUIAudio").GetComponent<AudioSource>() ;
//            Debug.Log(audioSource);
            //			CurrentApp = Verification.SolarSystemViewerName;
            LemonSpawn.RenderSettings.path = Application.dataPath + "/../";

            LemonSpawn.RenderSettings.UseThreading = true;
            LemonSpawn.RenderSettings.reCalculateQuads = false;
            LemonSpawn.RenderSettings.GlobalRadiusScale = SSVSettings.PlanetSizeScale;
            // Debug.Log(RenderSettings.GlobalRadiusScale);
            //RenderSettings.GlobalRadiusScale = 1;
            LemonSpawn.RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
            LemonSpawn.RenderSettings.sizeVBO = szWorld.resolution;
            LemonSpawn.RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
            LemonSpawn.RenderSettings.MoveCam = false;
            LemonSpawn.RenderSettings.ResolutionScale = szWorld.resolutionScale;
            LemonSpawn.RenderSettings.usePointLightSource = true;
            LemonSpawn.RenderSettings.logScale = true;
            LemonSpawn.RenderSettings.cullCamera = false;

            SSVSettings.extraGameObject = new GameObject("extra");


            satellite = GameObject.Find("Satellite");
            if (satellite != null)
                satellite.SetActive(false);

            pnlInfo = GameObject.Find("pnlInfo");
            pnlInfo.SetActive(false);
            solarSystem = new LemonSpawn.SolarSystem(sun, sphere, GameObject.Find("SolarSystem").transform, (int)szWorld.skybox);
            LemonSpawn.PlanetTypes.Initialize();
            SetupCloseCamera();
            MainCamera = mainCamera.GetComponent<Camera>();
            SzWorld = szWorld;

            setText("TextVersion", "Version: " + LemonSpawn.RenderSettings.version.ToString("0.00"));


            //            linesObject = new GameObject("Lines");
            //          CreateAxis();

            player.galaxy.Generate(50000, 0, 3000);
            currentSystem = player.galaxy.stars[0];
            player.AddToKnown(currentSystem);
            solarSystem.GenerateSolarSystem(currentSystem);
            selectedSystem = currentSystem;



            PopulateWorld();
            CreateMainMenu ();
            Update();
            CreatePlanetMenu(true);
            starCamera.transform.position = currentSystem.position + MainCamera.transform.position.normalized*SSVSettings.StarCamFixDistance * SSVSettings.starCameraScale;
            //           MainCamera.transform.position = (starCamera.transform.position - currentSystem.position) / SSVSettings.starCameraScale;
        }

        protected void GotoStarSystem()
        {

        }

        protected void DestroySolarSystem()
        {
            solarSystem.ClearStarSystem();
            solarSystem = null;
            DestroyAllGameObjects();
            selected = null;
        }



        private void UpdateZoom()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            scrollWheelAccel = Input.GetAxis("Mouse ScrollWheel") * 0.5f * -1;
            scrollWheel = scrollWheel * 0.9f + scrollWheelAccel * 0.1f;
            if (Mathf.Abs(scrollWheel) < 0.001)
                scrollWheel = 0;
            if (Mathf.Abs(scrollWheel) > 0) currentDistance = 0;
            //            Debug.Log(ScrollWheel);

            if (currentMode!=Mode.Interstellar)
            {
                Vector3 posM = MainCamera.transform.position;
                if (selected != null)
                {
                    posM -= selected.go.transform.position;
                    MainCamera.transform.position = posM * (1 + scrollWheel) + selected.go.transform.position;
                }

            }

            
            Vector3 posS = starCamera.transform.position;

            if (selectedSystem != null && MainCamera.transform.position.magnitude>SSVSettings.StarCamFixDistance)
            {
                posS -= selectedSystem.position;
                
                //                if (posS.magnitude>0.01)
                starCamera.transform.position = posS * (1 + scrollWheel) + selectedSystem.position;
//                if (starCamera.transform.position.magnitude < ss)
  //                  starCamera.transform.position = starCamera.transform.position.normalized * ss;

               // starCamera.transform.position = starCamera.transform.position + selectedSystem.position;

            }
/*            else
            {
                starCamera.transform.position = starCamera.transform.position * (1 + scrollWheel * scale);
                MainCamera.transform.position = MainCamera.transform.position * (1 + scrollWheel);
            }
            */
        }
        public void OnPostRender()
        {
            foreach (DisplayPlanet dp in dPlanets)
                dp.RenderGLOrbits(SSVSettings.OrbitalLineSegments);
        }

        protected void UpdateStarCamera()
        {
            float min;
            if (currentSystem != null)
            {
                min = Mathf.Min((starCamera.transform.position - currentSystem.position).magnitude, (starCamera.transform.position - selectedSystem.position).magnitude);
            }
            else
                min = (starCamera.transform.position - selectedSystem.position).magnitude;


            if (min>1)
            {
                if (currentMode == Mode.InterPlanetary)
                {
                    player.InterStellar();
                    // Coming from interplanetary to interstellar
                    DestroySolarSystem();
                    MainCamera.enabled = false;
                    currentSystem = null;
                    CreatePlanetMenu(false);
                }
                currentMode = Mode.Interstellar;
            } 
            else
            {
                if (currentMode == Mode.Interstellar)
                {
                    player.AddToKnown(selectedSystem); 
                    player.InterPlanetary();
                    Debug.Log("INTERPLANETARY");
                    // Coming from insterstellar to interplanetary
                    currentSystem = selectedSystem;

                    solarSystem = new LemonSpawn.SolarSystem(sun, sphere, GameObject.Find("SolarSystem").transform, (int)szWorld.skybox);
                    MainCamera.enabled = true;
                    solarSystem.GenerateSolarSystem(currentSystem);
 
                    PopulateWorld();
                    solarSystem.Update();
                    CreatePlanetMenu(true);

                    MainCamera.transform.position = (starCamera.transform.position - currentSystem.position) / SSVSettings.starCameraScale;
    
                }
                currentMode = Mode.InterPlanetary;


            }
//            starCamera.transform.rotation =  mainCamera.transform.rotation;
        //    starCamera.transform.position = mainCamera.transform.position * 0.00001f;
        }
        
        protected void SelectStarSystem()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = starCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                StarSystem winnerSystem = null;
                float winner = 1E30f;
                foreach (StarSystem ss in player.galaxy.stars)
                {
                    float d = LemonSpawn.Util.DistanceToLine(ray, ss.position);
                    if (d < winner)
                    {
                        winnerSystem = ss;
                        winner = d;
                    }
                }

                if (winnerSystem != null)
                {
                    /*GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    temp.transform.position = winnerSystem.position;
                    temp.transform.localScale = Vector3.one * 10;
                    temp.layer = 12;*/
                    selectedSystem = winnerSystem;
                    DisplayStarInfo(selectedSystem);

                }

            }

        }

        public override void Update()
        {
            UpdateFocus();
            UpdateCamera();
            UpdateZoom();
            if (solarSystem != null)
                solarSystem.Update();
            UpdateStarCamera();
    //        if (currentMode == Mode.Interstellar)

            player.Update(starCamera.transform.position, starCamera.transform.up);

            if (DisplayPlanet.performSelect!=null)
            {
                SelectPlanet(DisplayPlanet.performSelect);
                DisplayPlanet.performSelect = null;
            }

            if (LemonSpawn.RenderSettings.UseThreading)
            {
                for (int i=0;i<10;i++)
                LemonSpawn.ThreadQueue.MaintainThreadQueue();
            }

            // Always point to selected planet
            if (selected != null)
                SelectPlanet(selected, false);


            SelectStarSystem();


            foreach (DisplayPlanet dp in dPlanets)
                dp.UpdatePosition(tempTime, mainCamera.transform.position);

//            Debug.Log(LemonSpawn.ThreadQueue.currentThreads.Count + LemonSpawn.ThreadQueue.threadQueue.Count);


            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }


        }



        protected void OnGUI()
        {
            RenderLabels();
      
            if (mainMenu != null)
                mainMenu.Render(new Vector2(0.02f, -0.1f) * Screen.height);

        }


        private void DestroyAllGameObjects()
        {

            foreach (DisplayPlanet dp in dPlanets)
            {
                GameObject.Destroy(dp.go);
            }
            dPlanets.Clear();
        }

    }

}