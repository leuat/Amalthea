using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        
    }

    //    exp(1/2)  * exp(-1/2) = 

    public class DisplayPlanet {
		public Planet planet;
//        public LemonSpawn.SerializedPlanet serializedPlanet;
		public GameObject go;
        public GameObject textMesh;
		public List<Vector3> orbitLines = new List<Vector3>();
        private static Material lineRenderer;
        public Color orbitColor;


        public DisplayPlanet(GameObject g, Planet p)
        {
            go = g;
            planet = p;
            //            CreateTextMesh();
            //CreateOrbitFromFrames ();
           // if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
             //   isMoon = true;
        }

        /*        public void MaintainOrbits() {
                    int maxFrames = serializedPlanet.Frames.Count;
                    int currentFrame = (int)(SSVSettings.currentFrame*maxFrames);
                    Color c = SSVSettings.orbitLinesColor;

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
*/
                public void DestroyOrbits() {
                    orbitLines.Clear();

                }

        /*                public void SetWidth(float w)
                        {
                            foreach (GameObject g in orbitLines)
                            {
                                LineRenderer lr = g.GetComponent<LineRenderer>();
                                lr.SetWidth(SSVSettings.OrbitalLineWidth.x*w, SSVSettings.OrbitalLineWidth.y*w);
                            }
                        }
                        */



        public void RenderGLOrbits(int maxLines)
        {

            Vector3 center = planet.lsPlanet.pSettings.transform.parent.position;
            float d = (planet.lsPlanet.pSettings.transform.position - center).magnitude;
            if (lineRenderer==null)
                //                lineRenderer = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
                lineRenderer = new Material(Shader.Find("Particles/Alpha Blended"));
            //            return;

            lineRenderer.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(orbitColor);

            for (int i = 0; i < orbitLines.Count; i++)
            {
                //float t0 = (float)i / (maxLines + 1) * 2 * Mathf.PI;
                //float t1 = (float)(i + 1) / (maxLines + 1) * 2 * Mathf.PI;


                //Vector3 from = center + new Vector3(Mathf.Cos(t0), 0, Mathf.Sin(t0)) * d;
                //Vector3 to = center + new Vector3(Mathf.Cos(t1), 0, Mathf.Sin(t1)) * d;
                Vector3 from = orbitLines[i] + center;
                Vector3 to = orbitLines[(i+1)%orbitLines.Count] + center;



                //            lineMat.SetPass(0);
                GL.Vertex3(from.x, from.y, from.z);
                GL.Vertex3(to.x, to.y, to.z);
            }
            GL.End();
        }

        public void CreateOrbits(int maxLines)
        {

            DestroyOrbits();

            //if (planet.pSettings.category == PlanetSettings.Categories.Moon)
            //    return;

            orbitColor = planet.stellarCategory.color * (0.5f + 1.0f*Random.value);
            orbitColor.a = 0.5f;

            Vector3 center = planet.lsPlanet.pSettings.transform.parent.position;
            float d = (planet.lsPlanet.pSettings.transform.position - center).magnitude;
            //Quaternion q = Quaternion.Euler(planet.lsPlanet.pSettings.eulerX, 0, planet.lsPlanet.pSettings.eulerZ);
            for (int i = 0; i < maxLines; i++)
            {
                float t0 = (float)i / (maxLines + 1) * 2 * Mathf.PI;


                Vector3 p = planet.lsPlanet.pSettings.getOrbit(t0);// *SSVSettings.SolarSystemScale;
                Vector3 dp = getDisplayPosition(p);

                    //planet.lsPlanet.pSettings.properties.rotationMatrix* new Vector3(Mathf.Cos(t0), 0, Mathf.Sin(t0))*d;
                orbitLines.Add(dp);
                

            }
        }

        public Vector3 getDisplayPosition(Vector3 pos)
        {
            float ms = 1;
            float prevRadius = 0;
            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
            {
                ms = 2.0f / SSVSettings.PlanetSizeScale;
                prevRadius = planet.lsPlanet.pSettings.transform.parent.gameObject.GetComponent<LemonSpawn.PlanetSettings>().radius;
                //Debug.Log("IS MOON");
            }

            return  pos * SSVSettings.SolarSystemScale * ms + pos.normalized * prevRadius;

        }


        public void UpdatePosition(float t, Vector3 cam) {
            //            planet.serializedPlanet.position = planet.lsPlanet.pSettings.properties.pos.toVectorf();
            //            planet.serializedPlanet.position*=SSVSettings.SolarSystemScale;
            /*            planet.lsPlanet.pSettings.properties.pos = planet.lsPlanet.pSettings.properties.orgPos * SSVSettings.SolarSystemScale; 

                        planet.lsPlanet.pSettings.transform.position = planet.lsPlanet.pSettings.properties.pos.toVectorf();
                        go.transform.position = planet.lsPlanet.pSettings.properties.pos.toVectorf();*/

//          if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
  //              return;
            planet.lsPlanet.pSettings.setPosition(t);


            planet.lsPlanet.pSettings.transform.localPosition = getDisplayPosition(planet.lsPlanet.pSettings.properties.pos.toVectorf());

            float scale = Mathf.Clamp((go.transform.position - cam).magnitude*0.003f, 1, 25);

           
            go.transform.localScale = Vector3.one * planet.lsPlanet.pSettings.radius * 3f*scale;


        }

    }

    public class SolarSystemViewverMain : LemonSpawn.World
    {



        private List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
        private Vector3 mouseAccel = new Vector3();
        private Vector3 focusPoint = Vector3.zero;
        private Vector3 focusPointCur = Vector3.zero;
        private float scrollWheel, scrollWheelAccel;
        private DisplayPlanet selected = null;
        public static GameObject linesObject = null;
        private GameObject pnlInfo = null;
        public static bool Reload = false;
        private float currentDistance;
        private Font GUIFont;
        private GUIStyle guiStyle = new GUIStyle();
        private bool toggleLabels = true;

        public float tempTime = 0;


        protected void setText(string box, string text)
        {
            //  Debug.Log(box);
            GameObject.Find(box).GetComponent<Text>().text = text;
        }


        private void SelectPlanet(DisplayPlanet dp)
        {
             selected = dp;
            focusPoint = dp.go.transform.position;
            pnlInfo.SetActive(true);

            if (currentDistance == 0)
                currentDistance = (dp.planet.lsPlanet.pSettings.gameObject.transform.position - MainCamera.transform.position).magnitude;

            if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
            {
                setText("txtPlanetName", "Star");
                setText("txtPlanetName2", "Star");
                setText("txtPlanetType", "Star");
                setText("txtPlanetInfo", "Star");
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

            UpdateOverviewClick();
        }


        private void UpdateOverviewClick()
        {

            GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().value = 0;

        }

        public void FocusOnPlanetClick()
        {
            int idx = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().value;
            string name = GameObject.Find("Overview").GetComponent<UnityEngine.UI.Dropdown>().options[idx].text;

            foreach (DisplayPlanet dp in dPlanets)
                if (name.Contains(dp.planet.lsPlanet.pSettings.name))
                    SelectPlanet(dp);

        }

        public void ZoomPlanet()
        {
            //SolarSystemViewverZoom.SzWorld = SzWorld;
            //RenderSettings.currentSZWorld = SzWorld;
            //Application.LoadLevel(4);
        }

        private void DeFocus()
        {
            selected = null;
            focusPoint = Vector3.zero;
            pnlInfo.SetActive(false);
            currentDistance = 0;
        }


        public void ToggleLabels()
        {
            toggleLabels = !toggleLabels;
        }

        private void RenderLabels()
        {
            if (!toggleLabels)
                return;
            GUI.skin.font = GUIFont;
            foreach (DisplayPlanet dp in dPlanets)
            {
                guiStyle.normal.textColor = SSVSettings.planetColor;
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                {
                    Color c = SSVSettings.moonColor;
                    c.a = Mathf.Clamp(1-0.001f*(dp.go.transform.position - mainCamera.transform.position).magnitude, 0, 1);
                    guiStyle.normal.textColor = c;
                }
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Spacecraft)
                    guiStyle.normal.textColor = SSVSettings.spaceCraftColor;




                Vector3 pos = MainCamera.WorldToScreenPoint(dp.go.transform.position);
                int width1 = dp.planet.lsPlanet.pSettings.givenName.Trim().Length;
                int width2 = dp.planet.lsPlanet.pSettings.name.Trim().Length;
                int fs = 16 + (int)Mathf.Pow(dp.planet.lsPlanet.pSettings.radius, 0.6f);
                guiStyle.fontSize = fs;
                //                if (pos.x >0 && pos.y<Screen.width && pos.y>0 && pos.y<Screen.height)
                if (pos.z > 0)
                {
                    float ha = 50;
                    GUI.Label(new Rect(pos.x - (width1 / 2) * 10, Screen.height - pos.y - ha, 250, 130), dp.planet.lsPlanet.pSettings.givenName, guiStyle);
                    guiStyle.fontSize = 12;

                    GUI.Label(new Rect(pos.x - (width2 / 2) * 4, Screen.height - pos.y + (int)(fs * 1.0) - ha, 250, 130), dp.planet.lsPlanet.pSettings.name, guiStyle);
                }

            }
        }


        public void SlideTime()
        {
            Slider slider = GameObject.Find("Slider").GetComponent<Slider>();

            //SSVSettings.LineScale = slider.value * 10;
            tempTime = slider.value * 1.01f;

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
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    foreach (DisplayPlanet dp in dPlanets)
                    {
                        if (dp.go == hit.transform.gameObject)
                            SelectPlanet(dp);
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

            if (Input.GetMouseButton(1))
            {
//                Debug.Log("TEST");
                theta = s * Input.GetAxis("Mouse X");
                phi = s * Input.GetAxis("Mouse Y") * -1.0f;
            }
            mouseAccel += new Vector3(theta, phi, 0);
            focusPointCur += (focusPoint - focusPointCur) * 0.1f;
            mouseAccel *= 0.9f;

            euler += mouseAccel * 10f;
//            Debug.Log(theta);
            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.up, mouseAccel.x);
            


            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) > 0.99)
                if (mouseAccel.y < 0)
                    mouseAccel.y = 0;
            if ((Vector3.Dot(mainCamera.transform.forward, Vector3.up)) < -0.99)
                if (mouseAccel.y > 0)
                    mouseAccel.y = 0;


            mainCamera.transform.RotateAround(focusPointCur, mainCamera.transform.right, mouseAccel.y);
            mainCamera.transform.LookAt(focusPointCur);

            if (selected != null && Mathf.Abs(scrollWheel) < 0.001)
            {
                Vector3 dir = selected.planet.lsPlanet.pSettings.gameObject.transform.position - mainCamera.transform.position;
                float dist = dir.magnitude;
                //  Debug.Log("LOWER:" + dist + " c: " + currentDistance);
                if (Mathf.Abs(dist - currentDistance) > 0.01)
                {
                    float add = dist - currentDistance;
                    cameraAdd += dir.normalized * add * 0.06f;
                    //                    mainCamera.transform.position = mainCamera.transform.position + dir.normalized * add;
                }

            }

            mainCamera.transform.position = mainCamera.transform.position + cameraAdd;
            cameraAdd *= 0.6f;
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

            foreach (DisplayPlanet dp in dPlanets)
                dp.CreateOrbits(SSVSettings.OrbitalLineSegments);

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

        public void TestSlapDash()
        {
            System.Random rnd = new System.Random(4);
            LemonSpawn.SlapDash d = new LemonSpawn.SlapDash();
            foreach (LemonSpawn.Language l in d.languages)
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
            GUIFont = (Font)Resources.Load("CaviarDreams");
            guiStyle.font = GUIFont;
            Globals.Save();
            Globals.Initialize();
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


            linesObject = new GameObject("Lines");
            CreateAxis();

            solarSystem.GenerateSolarSystem(6);
            PopulateWorld();

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

            Vector3 pos = MainCamera.transform.position;
            if (selected != null)
            {
                pos -= selected.go.transform.position;
                MainCamera.transform.position = pos * (1 + scrollWheel) + selected.go.transform.position;
            }
            else
                MainCamera.transform.position = pos * (1 + scrollWheel);

        }
        public void OnPostRender()
        {
            foreach (DisplayPlanet dp in dPlanets)
                dp.RenderGLOrbits(SSVSettings.OrbitalLineSegments);
        }

        public override void Update()
        {
            UpdateFocus();
            UpdateCamera();
            UpdateZoom();
            solarSystem.Update();

//            tempTime += 0.000000015f;

            if (LemonSpawn.RenderSettings.UseThreading)
                LemonSpawn.ThreadQueue.MaintainThreadQueue();

            // Always point to selected planet
            if (selected != null)
                SelectPlanet(selected);


            foreach (DisplayPlanet dp in dPlanets)
                dp.UpdatePosition(tempTime, mainCamera.transform.position);

//            UpdatePlay();
//            ForceSpaceCraft();

            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }


        }



        protected void OnGUI()
        {
            RenderLabels();
        }


        private void DestroyAllGameObjects()
        {

            foreach (DisplayPlanet dp in dPlanets)
            {
                GameObject.Destroy(dp.go);
            }
        }

    }

}