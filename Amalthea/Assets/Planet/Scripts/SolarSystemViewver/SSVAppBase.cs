using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LemonSpawn
{
    public class SSVAppSettings
    {
        public static float SolarSystemScale = 500.0f;
        public static float PlanetSizeScale = 1.0f / 50f;
        //        public static float PlanetSizeScale = (float)(500.0f/RenderSettings.AU);
        public static float StarCamFixDistance = 20000;
        public static bool shadowText = true;
        public static string MainFont = "LektonCode";
        public static GameObject extraGameObject;
        public static float MiniCamDist = 3;
        public static float MiniCamFOV = 60;
        public static float MiniCamSize = 0.1f;

        public static Vector2 menuSizeText = new Vector2(0.2f, 0.2f) * Screen.height;
        public static Vector2 menuSizeImage = new Vector2(0.2f, 0.2f) * Screen.height;


        public static Font GUIFont;
        public static GUIStyle guiStyle = new GUIStyle();
        public static bool toggleLabels = true;

        public static AudioClip loadAudio(string s)
        {
            return (AudioClip)Resources.Load("Effects/" + s);
        }
    }


    public class SSVAppBase : WorldMC
    {

        protected SSVData data = new SSVData();

        protected MenuItem mainMenu;
        public GameObject starCamera;
        protected static AudioSource audioSource;
        protected GameObject pnlInfo = null;
        protected CameraRotator cameraRotator;
        // Use this for initialization
        public override void Start()
        {
            MainCamera = mainCamera.GetComponent<Camera>();

            data.Initialize();
            cameraRotator = new CameraRotator(MainCamera);
            pnlInfo = GameObject.Find("pnlInfo");
            pnlInfo.SetActive(false);
            Globals.definitions.InitTemp();
            GameObject go = new GameObject("AudioSource");
            audioSource = go.AddComponent<AudioSource>();

            RenderSettings.path = Application.dataPath + "/../";

            RenderSettings.UseThreading = true;
            RenderSettings.reCalculateQuads = false;
            RenderSettings.GlobalRadiusScale = SSVSettings.PlanetSizeScale;
            // Debug.Log(RenderSettings.GlobalRadiusScale);
            //RenderSettings.GlobalRadiusScale = 1;
            RenderSettings.maxQuadNodeLevel = m_maxQuadNodeLevel;
            RenderSettings.sizeVBO = szWorld.resolution;
            RenderSettings.minQuadNodeLevel = m_minQuadNodeLevel;
            RenderSettings.MoveCam = false;
            RenderSettings.ResolutionScale = szWorld.resolutionScale;
            RenderSettings.usePointLightSource = true;
            RenderSettings.logScale = true;
            szWorld.useSpaceCamera = false;



        }



        // Update is called once per frame
        public override void Update()
        {
            data.Update();
            cameraRotator.UpdateCamera();
            UpdateFocus();
            UpdateZoom();
            solarSystem.Update();

            if (RenderSettings.UseThreading)
                ThreadQueue.MaintainThreadQueue();
        }

        public static void PlaySound(string sound, float amp)
        {
            SolarSystemViewverMain.audioSource.PlayOneShot(SSVAppSettings.loadAudio(sound), amp);

        }



        public virtual void DisplayObjectText(DisplayPlanet dp)
        {

        }

        public void DeFocus()
        {
            data.selected = null;
            cameraRotator.focusPoint = Vector3.zero;
            pnlInfo.SetActive(false);
            data.currentDistance = 0;
        }

        public void UpdateFocus()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    foreach (DisplayPlanet dp in data.dPlanets)
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


        public void SelectPlanet(DisplayPlanet dp, bool trigger = true)
        {
            if (data.currentMode == SSVData.Mode.Interstellar)
                return;

            data.selected = dp;
            cameraRotator.focusPoint = dp.go.transform.position;
            pnlInfo.SetActive(true);

            if (data.currentDistance == 0)
                data.currentDistance = (dp.planet.lsPlanet.pSettings.gameObject.transform.position - MainCamera.transform.position).magnitude;


            DisplayObjectText(dp);

            if (trigger)
            {
                PlaySound(SSVSettings.audioClickPlanet, 0.5f);
                dp.Trigger();
            }

            /*            if (data.currentMode == SSVData,Mode.EditPlanet)
                        {
                            //mainCamera.transform.position = selected.planet.lsPlanet.pSettings.transform.position - Vector3.left * selected.planet.lsPlanet.pSettings.radius * SSVSettings.ManagePlanetDistance;
                            mainCamera.transform.LookAt(selected.planet.lsPlanet.pSettings.transform.position);
                        }
            */
        }
        protected void PopulateWorld()
        {
            data.DestroyAllGameObjects();
            data.dPlanets.Clear();

            //solarSystem.InitializeFromScene();


            foreach (Planet p in solarSystem.planets)
            {
                GameObject go = p.pSettings.gameObject;

                Vector3 coolpos = new Vector3((float)p.pSettings.properties.pos.x, (float)p.pSettings.properties.pos.y, (float)p.pSettings.properties.pos.z);
                go.transform.position = coolpos * SSVSettings.SolarSystemScale;
                p.pSettings.properties.pos = new DVector(coolpos);
                //go.transform.localScale = Vector3.one * SSVSettings.PlanetSizeScale * p.pSettings.radius;
                //p.pSettings.atmoDensity = 0;

                GameObject hidden = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hidden.transform.position = coolpos * SSVSettings.SolarSystemScale;
                hidden.transform.localScale = Vector3.one * p.pSettings.radius * 2f;
                hidden.transform.parent = p.pSettings.transform;
                //if (p.pSettings.planetTypeName=="star" || p.pSettings.planetTypeName=="spacecraft")
                //    hidden.SetActive(false);

                hidden.GetComponent<MeshRenderer>().material = (Material)Resources.Load("HiddenMaterial");

                PlanetInstance pi = new LemonSpawn.PlanetInstance(p, Globals.definitions.stellarCategories.Get("planet"));
                data.dPlanets.Add(new DisplayPlanet(hidden, pi));
            }

        }
        protected void CreatePlanetMenu(bool isInterPlanetary)
        {
            mainMenu.deleteFromChildren("SolarSystem");
            if (isInterPlanetary)
            {
                foreach (DisplayPlanet dp in data.dPlanets)
                    dp.CreatePlanetCamera();

                data.dpSun.CreateMenu("SolarSystem", mainMenu, SSVAppSettings.menuSizeImage, true, 0.75f, mainMenu.layout, SelectPlanetMenu);
            }
            mainMenu.replaceItem("SolarSystem", 0);
        }

        public void SelectPlanetMenu(System.Object o)
        {
            SelectPlanet((DisplayPlanet)o);
        }

        private void UpdateZoom()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

//            if (Mathf.Abs(scrollWheel) > 0) currentDistance = 0;
            //            Debug.Log(ScrollWheel);

            if (data.currentMode != SSVData.Mode.Interstellar)
            {
                Vector3 posM = MainCamera.transform.position;
                if (data.selected != null)
                {
                    posM -= data.selected.go.transform.position;
                    MainCamera.transform.position = posM * (1 + cameraRotator.scrollWheel) + data.selected.go.transform.position;
                }

            }


            Vector3 posS = starCamera.transform.position;

            if (data.selectedSystem != null && MainCamera.transform.position.magnitude > SSVSettings.StarCamFixDistance)
            {
                posS -= data.selectedSystem.position;

                //                if (posS.magnitude>0.01)
                starCamera.transform.position = posS * (1 + cameraRotator.scrollWheel) + data.selectedSystem.position;
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

    }
}