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
        public static int MaxOrbitalLines = 60;
        public static int FontSize = 12 * Screen.height / 512;
        public static bool useAbsolutePositions = false;
        public static Vector2 menuSizeText = new Vector2(0.2f, 0.1f) * Screen.height;
        public static Vector2 menuSizeImage = new Vector2(0.2f, 0.1f) * Screen.height;
        public static Vector2 menuSizePlanet = new Vector2(0.1f, 0.1f) * Screen.height;

        public static string audioClickPlanet = "GUI40Click";
        public static string audioHoverMenu = "GUIPop";


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
        protected CameraRotator cameraRotator, starCameraRotator;
        protected bool Initialized = false;
        

        // Use this for initialization
        public override void Start()
        {
            MainCamera = mainCamera.GetComponent<Camera>();

            data.Initialize();
            cameraRotator = new CameraRotator(MainCamera);
            starCameraRotator = new CameraRotator(starCamera.GetComponent<Camera>());
            pnlInfo = GameObject.Find("pnlInfo");
            pnlInfo.SetActive(false);
            Globals.definitions.InitTemp();
            GameObject go = new GameObject("AudioSource");
            audioSource = go.AddComponent<AudioSource>();

            RenderSettings.path = Application.dataPath + "/../";

            RenderSettings.UseThreading = true;
            RenderSettings.reCalculateQuads = false;
            RenderSettings.GlobalRadiusScale = SSVAppSettings.PlanetSizeScale;
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

        protected void UpdateBlackHoleEffect()
        {
            Vector3 pos = MainCamera.WorldToScreenPoint(Vector3.zero);
            if (pos.z > 0 && data.dpSun != null && data.dpSun.planet.lsPlanet.pSettings.properties.distortionIntensity != 0)
            {
                mainCamera.GetComponent<BlackHoleEffect>().enabled = true;
                BlackHoleEffect.centerPoint = new Vector3(pos.x, pos.y);
                BlackHoleEffect.centerPoint.x /= Screen.width;
                BlackHoleEffect.centerPoint.y /= Screen.height;
                BlackHoleEffect.intensity = data.dpSun.planet.lsPlanet.pSettings.properties.distortionIntensity;
                BlackHoleEffect.size = 1.0f / mainCamera.transform.position.magnitude;

                // Set cameras
            }
            else
            {
                BlackHoleEffect.intensity = 0;
                mainCamera.GetComponent<BlackHoleEffect>().enabled = false;
            }

            float add = 0;
            if (data.dpSun != null)
                add = data.dpSun.planet.lsPlanet.pSettings.radius * 1;

            MainCamera.nearClipPlane = MainCamera.transform.position.magnitude + add;
            Camera NearCam = GameObject.Find("FrontBlackHoleCamera").GetComponent<Camera>();
            NearCam.farClipPlane = MainCamera.nearClipPlane;

            NearCam.enabled = true;
            if (NearCam.farClipPlane < 0)
            {
                NearCam.enabled = false;
                MainCamera.nearClipPlane = 0.1f;
            }

        }

        public override void Update()
        {
            if (Initialized)
                data.Update();

            cameraRotator.UpdateCamera();
            starCameraRotator.UpdateCamera();
            starCameraRotator.ForceCamera(cameraRotator);

            if (data.selected != null) {
                cameraRotator.focusPoint = data.selected.planet.lsPlanet.pSettings.transform.position;
                //starCameraRotator.focusPoint = data.selected.planet.lsPlanet.pSettings.transform.position;
            }

            if (Initialized)
            {
                UpdateFocus();
                UpdateZoom();
                solarSystem.Update();
            }

        
            if (RenderSettings.UseThreading)
                ThreadQueue.MaintainThreadQueue();

            UpdateBlackHoleEffect();
        }
        /*
                public void OnPostRender()
                {
                    foreach (DisplayPlanet dp in data.dPlanets)
                        dp.RenderGLOrbits();
                }
                */

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
                Debug.Log("blah " + ray.origin + "  " + ray.direction);
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("HIT" + hit.transform.gameObject.name);
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
                PlaySound(SSVAppSettings.audioClickPlanet, 0.5f);
                dp.Trigger();
            }

            /*            if (data.currentMode == SSVData,Mode.EditPlanet)
                        {
                            //mainCamera.transform.position = selected.planet.lsPlanet.pSettings.transform.position - Vector3.left * selected.planet.lsPlanet.pSettings.radius * SSVSettings.ManagePlanetDistance;
                            mainCamera.transform.LookAt(selected.planet.lsPlanet.pSettings.transform.position);
                        }
            */
        }


        protected void DestroyAll()
        {
            data.DestroyAllGameObjects();
            data.dPlanets.Clear();
        }




        protected void PopulateWorld()
        {
            DestroyAll();
            //Debug.Break();
            foreach (Planet p in solarSystem.planets)
            {
                GameObject go = p.pSettings.gameObject;
                p.pSettings.properties.orgPos = p.pSettings.properties.pos;

                GameObject hidden = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                hidden.transform.localScale = Vector3.one * p.pSettings.radius * 2f;
                hidden.transform.parent = p.pSettings.transform;

                hidden.GetComponent<MeshRenderer>().material = (Material)Resources.Load("HiddenMaterial");

                PlanetInstance pi = new LemonSpawn.PlanetInstance(p, Globals.definitions.stellarCategories.Get("planet"));

                DisplayPlanet dp = new DisplayPlanet(hidden, pi);
                dp.parent = data.findDisplayPlanet(dp.planet.lsPlanet.pSettings.transform.parent.gameObject.name);
                data.dPlanets.Add(dp);

               
            }

        }



        protected void CreatePlanetMenu(bool isInterPlanetary)
        {
            mainMenu.deleteFromChildren("SolarSystem");
            if (isInterPlanetary)
            {
                foreach (DisplayPlanet dp in data.dPlanets)
                    dp.CreatePlanetCamera();

                data.dpSun.CreateMenu("SolarSystem", mainMenu, SSVAppSettings.menuSizePlanet, true, 0.75f, mainMenu.layout, SelectPlanetMenu);
            }
            mainMenu.replaceItem("SolarSystem", 0);
        }


        public void OnPostRender()
        {
            //            foreach (DisplayPlanet dp in data.dPlanets)
            //              dp.RenderGLOrbits();
            data.glr.Render();
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

            if (data.selectedSystem != null && MainCamera.transform.position.magnitude > SSVAppSettings.StarCamFixDistance)
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