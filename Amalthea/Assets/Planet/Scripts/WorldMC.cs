using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Xml.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace LemonSpawn
{


    public class WorldMC : World
    {


        protected GameObject StarBackgroundSphere = null;
        protected double m_playSpeed = 0.0;
        protected Texture2D tx_background, tx_load, tx_record;
        protected int load_percent;
        protected GameObject helpPanel = null;
        protected GameObject settingsPanel = null;
        public GameObject debugPanel = null;
        protected MCAstSettings settings = new MCAstSettings();
        public GameObject slider;
        public static GameObject Slider;
        public static GameObject canvas;
        public static AudioSource audioSource = null;


        protected void PopulateGUISettings()
        {
            GameObject.Find("ScreenshotResolutionCmb").GetComponent<Dropdown>().value = settings.screenShotResolution;
            GameObject.Find("MovieResolutionCmb").GetComponent<Dropdown>().value = settings.movieResolution;
            GameObject.Find("GridSizeCmb").GetComponent<Dropdown>().value = settings.gridSize;
            GameObject.Find("ToggleCameraEffects").GetComponent<Toggle>().isOn = settings.cameraEffects;
            GameObject.Find("ToggleGPURendering").GetComponent<Toggle>().isOn = settings.UseGPURenderer;
            RenderSettings.GPUSurface = settings.UseGPURenderer;

        }

        protected void setText(string box, string text)
        {
          //  Debug.Log(box);
            GameObject.Find(box).GetComponent<Text>().text = text;
        }



        protected void PopulateSettingsFromGUI()
        {
            settings.screenShotResolution = GameObject.Find("ScreenshotResolutionCmb").GetComponent<Dropdown>().value;
            settings.movieResolution = GameObject.Find("MovieResolutionCmb").GetComponent<Dropdown>().value;
            settings.gridSize = GameObject.Find("GridSizeCmb").GetComponent<Dropdown>().value;
            settings.cameraEffects = GameObject.Find("ToggleCameraEffects").GetComponent<Toggle>().isOn;
            settings.UseGPURenderer = GameObject.Find("ToggleGPURendering").GetComponent<Toggle>().isOn;
            RenderSettings.GPUSurface = settings.UseGPURenderer;
            int actualGridSize = MCAstSettings.GridSizes[ settings.gridSize ];
            if (actualGridSize != RenderSettings.sizeVBO)
            {
                RenderSettings.sizeVBO = actualGridSize;
                solarSystem.Reset();
                AddMessage("New gridsize: Solar system reset");

            }
            effectCamera.GetComponent<Camera>().enabled = settings.cameraEffects;
        }


        protected void LoadSettings()
        {
            string fname = RenderSettings.path + RenderSettings.MCAstSettingsFile;
            if (File.Exists(fname))
            {
                settings = MCAstSettings.DeSerialize(fname);
                AddMessage("Settings file loaded : " + RenderSettings.MCAstSettingsFile);
                RenderSettings.UsePerPixelShading = settings.PerPixelShading;
                RenderSettings.GPUSurface = settings.UseGPURenderer;

            }
            else
            {
                AddMessage("Settings file created : " + RenderSettings.MCAstSettingsFile);
            }
        }

        protected void SaveSettings()
        {
			string fname = RenderSettings.path + RenderSettings.MCAstSettingsFile;
            MCAstSettings.Serialize(settings, fname);
//            AddMessage("Settings saved");
        }

        public void ClickOverview()
        {
            if (RenderSettings.renderType == RenderType.Normal)
                RenderSettings.renderType = RenderType.Overview;
            else
                RenderSettings.renderType = RenderType.Normal;
        }

        private double sliderScale = 10000.0;
        private SerializedCamera currentCamera = null;

        public bool isInCrash = false;

        public void Slide()
        {
            if (RenderSettings.MoveCam)
                return;
            double v = slider.GetComponent<Slider>().value;
            currentCamera = szWorld.getInterpolatedCamera(v/sliderScale, solarSystem.planets);
            if (currentCamera!=null) {
                szWorld.currentFrame = currentCamera.frame;

                    isInCrash = (currentCamera.status==1);
                if (isInCrash) 
                    noiseImage.SetActive(true);
                else
                   noiseImage.SetActive(false);
     
            }
        }

            


        private void setPlaySpeed(double v)
        {
            if (m_playSpeed == v)
            {
                //m_playSpeed = 0;
            }
            else
            {
                // Clear on play!
                if (RenderSettings.toggleSaveVideo)
                    ClearMovieDirectory();
                m_playSpeed = v;
            }

        }

        public void playNormal()
        {
            float speed = GameObject.Find("SliderPlaySpeed").GetComponent<Slider>().value;
            if (m_playSpeed == 0)
                setPlaySpeed((speed - 0.5f) * 0.01f);
            else m_playSpeed = 0;
        }

        public void playFast()
        {
            setPlaySpeed(0.0001);
        }


        public void UpdatePlaySpeed()
        {
            float speed = GameObject.Find("SliderPlaySpeed").GetComponent<Slider>().value;
            setPlaySpeed((speed-0.5f)*0.01f);
        }





        public void ClearStarSystem()
        {
            solarSystem.planets.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject go = transform.GetChild(i).gameObject;
                GameObject.Destroy(go);
                //	Debug.Log ("Destroying " + go.name);
            }


        }

        public void ExitSave()
        {
            SaveScreenshot();
            Application.Quit();
        }



        void CreateConfig(string fname)
        {

            SerializedMCAstObject p = new SerializedMCAstObject();
            SerializedWorld sz = new SerializedWorld();
            sz.Objects.Add(p);

            SerializedCamera c = new SerializedCamera();
            c.cam_x = 0;
            c.cam_y = 0;
            c.cam_z = -20000;
            /*		c.rot_x = 0;
                    c.rot_y = 0;
                    c.rot_z = 0;*/
            c.fov = 60;

            sz.Cameras.Add(c);


            SerializedWorld.Serialize(sz, fname);
        }

        public void ToggleSaveVideoCommand()
        {
            RenderSettings.toggleSaveVideo = GameObject.Find("ToggleSaveVideo").GetComponent<Toggle>().isOn;
        }
/*#if UNITY_STANDALONE

        public void LoadXmlFile()
        {
            string xml = GameObject.Find("XMLText").GetComponent<Text>().text;
            GameObject.Find("XMLText").GetComponent<Text>().text = " ";
            LoadXmlFile(xml);
        }
#endif

#if UNITY_STANDALONE_WIN
        public void LoadCommandLineXML()
        {

            string[] cmd = System.Environment.GetCommandLineArgs();
            if (cmd.Length > 1)
            {
                if (cmd[1] != "")
                    solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
            }

            //		LoadWorld("Assets/Planet/Resources/system1.xml", true);
            szWorld.IterateCamera();
            solarSystem.space.color = new Color(szWorld.sun_col_r, szWorld.sun_col_g, szWorld.sun_col_b);
            solarSystem.space.hdr = szWorld.sun_intensity;

        }

#endif

#if UNITY_STANDALONE_OSX
	public void LoadCommandLineXML() {
	
			
		System.IO.StreamWriter standardOutput = new System.IO.StreamWriter(System.Console.OpenStandardOutput());
		standardOutput.AutoFlush = true;
		System.Console.SetOut(standardOutput);
	
		string[] cmd = Util.GetOSXCommandParams();
		if (cmd.Length>1)  {
			if (cmd[1]!="")
			solarSystem.LoadWorld(Application.dataPath + "/../" + cmd[1], true, true, this);
		}
		
//		LoadWorld("Assets/Planet/Resources/system1.xml", true);
		szWorld.IterateCamera();
			solarSystem.space.color = new Color(szWorld.sun_col_r,szWorld.sun_col_g,szWorld.sun_col_b);
			solarSystem.space.hdr = szWorld.sun_intensity;
			
	}
	
#endif
*/
        public static void FatalError(string errorMessage) {
            if (FatalErrorPanel == null) {
                Debug.Log(errorMessage);
                Application.Quit();
                return;
                }
            FatalErrorPanel.SetActive(true);

/*          string p = Application.dataPath.Replace("/Contents","");

            string s = " files: ";
            DirectoryInfo info = new DirectoryInfo(p +".");
                FileInfo[] fileInfo = info.GetFiles();
                string first="";
            foreach (FileInfo f in fileInfo)  
                s+=f.Name + " ";
*/
            GameObject.Find("FatalErrorText").GetComponent<Text>().text = errorMessage;

        }


        public void SetupErrorPanel() {
            FatalErrorPanel = GameObject.Find("FatalError");
            if (FatalErrorPanel!=null)
                FatalErrorPanel.SetActive(false);

        }




        protected void GenerateTextures()
        {
            if (tx_background == null)
            {
                tx_background = new Texture2D(1, 1);
                tx_load = new Texture2D(1, 1);
                tx_background.SetPixel(0, 0, new Color(0, 0, 0, 1));
                tx_background.Apply();
                tx_load.SetPixel(0, 0, new Color(0.7f, 0.3f, 0.2f, 1));
                tx_load.Apply();
                // Create a circle


                int N = 512;
                tx_record = new Texture2D(N, N);
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                    {
                        Vector3 p = new Vector3(i / (float)N, j / (float)N, 0);
                        p -= new Vector3(0.5f, 0.5f);
                        float a = Mathf.Pow(1.8f - 2 * p.magnitude, 10);
                        tx_record.SetPixel(i, j, new Color(1, 0.2f, 0.2f, a));

                    }
                tx_record.Apply();
                //			tx_background = (Texture2D)Resources.Load ("cloudsTexture");

            }

        }

        protected void RenderProgressbar()
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tx_background);
            float h = 0.08f;
            int hei = (int)(Screen.height * h);
            float border = 0.05f;
            int rectwidth = (int)(Screen.width * (1 - 2 * border));
            GUI.DrawTexture(new Rect(Screen.width * border, Screen.height / 2 - hei, (int)(rectwidth / 100f * load_percent), 2 * hei), tx_load);
            GUI.Label(new Rect(Screen.width / 2 - 40, (int)(Screen.height * (2 / 3f)), 200, 200), RenderSettings.generatingText);

        }

        public void SaveScreenshot()
        {
            string warn = "";
            if (percent != 100)
                warn = " - WARNING: Image not fully processed (" + percent + "%)";

            string file = WriteScreenshot(RenderSettings.screenshotDir, MCAstSettings.Resolution[settings.screenShotResolution,0], MCAstSettings.Resolution[settings.screenShotResolution, 1]);
            AddMessage("Screenshot saved to " + RenderSettings.screenshotDir + file + warn);

        }

        public Vector2 blinkCrash = new Vector3(10, 0,0);

        protected void RenderClock() {
            if (szWorld.currentTime == 0)
                return;

            float W = Screen.width;
            float H = Screen.height;

            float w = 0.02f;
            float y = 0.02f;

            GUIStyle guiStyle = SSVAppSettings.guiStyle;
            guiStyle.normal.textColor = Color.white;
            float fs = Screen.height / 50;
            guiStyle.fontSize = (int)fs;


            GUI.Label(new Rect(w * W, y * H, 300, 30), "" + szWorld.currentTime.ToString("F3") + " " + szWorld.clockUnit, guiStyle);
            if (currentCamera!=null)
            GUI.Label(new Rect(w * W, y * H + fs*1.5f, 300, 30), currentCamera.displayMessage , guiStyle);


        }


        protected void RenderRuler()
        {
            if (szWorld.rulerTicks == 0)
                return;

            if (RenderSettings.textureRuler == null)
                RenderSettings.textureRuler = Util.createSolidTexture(RenderSettings.colorRuler);

            float W = Screen.width;
            float H = Screen.height;

            float w = 0.0f;
            float y = 0.1f;
            float h = 0.008f;

            float tickW = 0.002f;
            float tickH = 0.03f;

            float textY = y + tickH;
            float dx = (1 - 2 * w) / szWorld.rulerTicks;

            Rect r = new Rect(w * W, y * H, Screen.width - 2 * w * W, h * H);
            Rect full = new Rect(w * W, 0, Screen.width - 2 * w * W,Screen.height);

            GUI.DrawTexture(r, RenderSettings.textureRuler);

            if (full.Contains(Event.current.mousePosition))
            {

                float x = Event.current.mousePosition.x / Screen.width;
                float ddx = (szWorld.rulerEnd - szWorld.rulerStart);
                x = szWorld.rulerStart + ddx * x;

                GUI.Label(new Rect(Event.current.mousePosition.x+10, Event.current.mousePosition.y+10, 200, 30), "" + x.ToString("F2"));
            }

            //Draw ticks
            float start = w;
            float dval = (szWorld.rulerEnd - szWorld.rulerStart) / szWorld.rulerTicks;
            float val = szWorld.rulerStart;
            for (int i = 0; i < szWorld.rulerTicks + 1; i++)
            {
                GUI.DrawTexture(new Rect(start * W, y * H, tickW * W, tickH * H), RenderSettings.textureRuler);
                string text = val.ToString("F2") + " " + szWorld.rulerUnit;
                GUI.Label(new Rect(start * W, textY * H, 200, 30), text);
                start = start + dx;
                val += dval;
            }



        }



        protected void OnGUI()
        {

            GenerateTextures();
            solarSystem.RenderInformation();



            RenderRuler();
            RenderClock();

            if (RenderSettings.isVideo)
            {
                // Render blinking record sign
                if (RenderSettings.toggleSaveVideo && m_playSpeed>0)
                {
                    int s = Screen.width / 50;
                    int b = 50;
                    int t = (int)(Time.time * 2);
                    if (t % 2==0) {
                        GUI.DrawTexture(new Rect(Screen.width-b-s, Screen.height -b -s, s,s), tx_record);
                    } 
                }
                if (isInCrash) 
                {
                    GUIStyle crash = new GUIStyle();
                    crash.fontSize = 200;
                
                    crash.normal.textColor = Color.red;
                    crash.alignment = TextAnchor.UpperCenter;
                    blinkCrash.y-=Time.deltaTime*5f;
                    if (blinkCrash.y<=blinkCrash.x/2) {
                        GUI.Label(new Rect(Screen.width/2-300,Screen.height/2-200,600,400), "Crash detected!", crash);
                    }
                    if (blinkCrash.y<0) blinkCrash.y = blinkCrash.x;


                }   


                return;
            }

            // Generate Textures
            if (!hasScene)
                return;

            if (load_percent == 100)
                return;

            if (RenderSettings.toggleProgressbar)
                RenderProgressbar();



        }

        public void displayHelpPanel() {
            if (helpPanel != null)
            {
                helpPanel.SetActive(true);
                helpPanel.transform.SetAsLastSibling();
            }

        }

		public void closeHelpPanel() {
            if (helpPanel!=null)
            	helpPanel.SetActive(false);

        }
        public void displaySettingsPanel()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                settingsPanel.transform.SetAsLastSibling();
            }

        }

        public void closeSettingsPanel()
        {
            PopulateSettingsFromGUI();
            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            SaveSettings();

        }


        public void PopulateResolutionCombobox(string box)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i=0;i<MCAstSettings.Resolution.GetLength(0);i++)
            {
                string s = MCAstSettings.Resolution[i, 0] + "x" + MCAstSettings.Resolution[i, 1];
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }

            cbx.AddOptions(l);

        }

        public void PopulateIndexCombobox(string box, int[] lst)
        {
            Dropdown cbx = GameObject.Find(box).GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();

            for (int i = 0; i < lst.GetLength(0); i++)
            {
                string s = ""+ lst[i];
                ComboBoxItem ci = new ComboBoxItem();
                l.Add(new Dropdown.OptionData(s));
            }
            cbx.AddOptions(l);

        }


        private void SetupGUI()
        {
        	SetupErrorPanel();
			helpPanel = GameObject.Find("HelpPanel");
            if (helpPanel != null)
                helpPanel.SetActive(false);
			settingsPanel = GameObject.Find("SettingsPanel");
			if (settingsPanel != null)
                settingsPanel.SetActive(false);

			PlanetTypes.Initialize();

			if (settingsPanel != null)
                settingsPanel.SetActive(true); // must be true before populating

            LoadSettings();

            PopulateFileCombobox("ComboBoxLoadFile", "xml");
            PopulateResolutionCombobox("MovieResolutionCmb");
            PopulateResolutionCombobox("ScreenshotResolutionCmb");
            PopulateIndexCombobox("GridSizeCmb", MCAstSettings.GridSizes);
            displaySettingsPanel();
            PopulateGUISettings();
            



            if (settingsPanel != null)
                settingsPanel.SetActive(false);


        }



        public void PopulateFileCombobox(string box, string fileType) {
                Dropdown cbx = GameObject.Find (box).GetComponent<Dropdown>();
                cbx.ClearOptions();
                DirectoryInfo info = new DirectoryInfo(RenderSettings.path + RenderSettings.dataDir + ".");
                if (!info.Exists) {
                    FatalError("Could not find directory: " + RenderSettings.dataDir);
                    return;
                }
                FileInfo[] fileInfo = info.GetFiles();
                string first="";
            List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
            data.Add(new Dropdown.OptionData("-")); 
            foreach (FileInfo f in fileInfo)  {
                //string name = f.Name.Remove(f.Name.Length-4, 4);

                if (!f.Name.ToLower().Contains(fileType.ToLower()))
                    continue;
                    string[] lst = f.Name.Split('.');
                    if (lst[1].Trim().ToLower() == fileType.Trim().ToLower()) {


                        string text = lst[0].Trim();
                        if (!Verification.VerifyXML(RenderSettings.path + RenderSettings.dataDir + text + ".xml", Verification.MCAstName))
                            continue;


                        string name = f.Name;
                        string n = f.Name;
                        if (first == "")
                            first = f.Name;

                        data.Add(new Dropdown.OptionData(text));
                    }
                }
            cbx.AddOptions(data);


//            LoadFromXMLFile(RenderSettings.dataDir +  first);
 
        }





        protected void PopulateOverviewList(string box)
        {
            GameObject go = GameObject.Find(box);
            if (go==null)
                return;
            Dropdown cbx = go.GetComponent<Dropdown>();
            cbx.ClearOptions();
            List<Dropdown.OptionData> l = new List<Dropdown.OptionData>();
            l.Add(new Dropdown.OptionData("None"));
            foreach (Planet p in solarSystem.planets)
            {
                Dropdown.OptionData ci = new Dropdown.OptionData();
                ci.text = p.pSettings.name + " (" + p.pSettings.givenName + ")";
                string n = p.pSettings.name;
                if (n!="star")
                    l.Add(ci);
            }
            //      foreach (ComboBoxItem i in l)
            //          Debug.Log (i.Caption);

            cbx.AddOptions(l);

        }

        public void LoadFileFromMenu()
        {
            int idx = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().value;
            string name = GameObject.Find("ComboBoxLoadFile").GetComponent<Dropdown>().options[idx].text;
           	if (name=="-")
           		return;
			name =RenderSettings.dataDir + name + ".xml";

            LoadFromXMLFileMCAST(name, false, false);
            settings.previousFile = name;
            PopulateOverviewList("Overview");
            slider.GetComponent<Slider>().value = 0;
            GameObject.Find("SliderPlaySpeed").GetComponent<Slider>().value = SzWorld.defaultPlaySpeed;

        }
        
        public void FocusOnPlanetFromMenu()
        {
            int idx = GameObject.Find("Overview").GetComponent<Dropdown>().value;
            string name = GameObject.Find("Overview").GetComponent<Dropdown>().options[idx].text;
            FocusOnPlanet(name);

        }

        protected void ClearMovieDirectory()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(RenderSettings.path + RenderSettings.movieDir);
            if (!di.Exists) {
                FatalError("Could not find movie directory : " + RenderSettings.movieDir);
                return;
            }
            foreach (System.IO.FileInfo file in di.GetFiles()) file.Delete();
        }


      public GameObject noiseImage = null;
    public override void Start()
        {
            RenderSettings.path = Application.dataPath + "/../";
            CurrentApp = Verification.MCAstName;
            RenderSettings.GlobalRadiusScale = 0.995f;



            if (solarSystem == null)
    			solarSystem = new SolarSystem(sun, sphere, transform, (int)szWorld.skybox);

            canvas = GameObject.Find ("Canvas");
            noiseImage = GameObject.Find("NoiseImage");
            if (noiseImage != null)
               noiseImage.SetActive(false);
			base.Start();
            SetupGUI();


            SetupErrorPanel();

            StarBackgroundSphere = GameObject.Find("SunBackgroundSphere");



//            solarSystem.InitializeFromScene();

            GameObject.Find("TextVersion").GetComponent<Text>().text = "Version " + RenderSettings.version.ToString("0.00"); ;
            debugPanel = GameObject.Find("DebugPanel");
            debugPanel.SetActive(false);

            RenderSettings.MoveCam = false;
            //		slider = GameObject.Find ("Slider");
            //          Debug.Log(slider);
            if (slider != null)
                slider.SetActive(true);

            if (settings.previousFile != "")
            {
                LoadFromXMLFileMCAST(settings.previousFile);
                szWorld.IterateCamera();
                GameObject.Find("SliderPlaySpeed").GetComponent<Slider>().value = SzWorld.defaultPlaySpeed;


                //                szWorld.getInterpolatedCamera(0, solarSystem.planets);

            }



#if UNITY_STANDALONE
            //	LoadCommandLineXML();
#endif
        }
        public void LoadFromXMLFileMCAST(string filename, bool randomizeSeed = false, bool hierarchy=false)
        {
            AddMessage("Loading XML file: " + filename);


			if (!Verification.VerifyXML(RenderSettings.path + filename, Verification.MCAstName)) {
				AddMessage("ERROR: File " + filename + " is not a valid MCAst data file. Aborting. ", 2.5f);
				return;
			}

            base.LoadFromXMLFile(filename, randomizeSeed, hierarchy);
            displaySettingsPanel();
            PopulateSettingsFromGUI();
            closeSettingsPanel();
  //          Slide();

        }

            void setSun()
        {
            if (sun == null)
                return;

            sun.transform.rotation = Quaternion.FromToRotation(Vector3.forward, World.WorldCamera.toVectorf().normalized);
            sun.GetComponent<Light>().color = solarSystem.space.color;
        }

        // Update is called once per frame

        private void UpdateSlider()
        {



        }

        protected void UpdatePlay()
        {
            if (RenderSettings.MoveCam)
                return;

            if (slider == null)
                return;

            //          Debug.Log(Time.time + " " + m_playSpeed);
            if (solarSystem.planets.Count!=0)
            {
                float v = slider.GetComponent<Slider>().value;
                v += (float)(m_playSpeed*sliderScale);
                /*                if (v >= sliderScale)
                                {
                                    m_playSpeed = 0;
                                    v = 0;
                                }*/
                v = Mathf.Clamp(v, 0, (float)sliderScale);
//                Debug.Log("Playspeed after: " + m_playSpeed + " " + Time.time);
                slider.GetComponent<Slider>().value = v;
                canvas.SetActive(RenderSettings.RenderMenu);

                szWorld.getInterpolatedCamera(v, solarSystem.planets);
                if (RenderSettings.toggleSaveVideo)
                {
                    string f = WriteScreenshot(RenderSettings.movieDir,
                        MCAstSettings.Resolution[settings.movieResolution,0], MCAstSettings.Resolution[settings.movieResolution, 1]);
                    AddMessage("Movie frame saved to : " + RenderSettings.movieDir + f, 0.025f);
                }
                
            }

        }


        public void TogglePlanetTypes()
        {
            RenderSettings.ForceAllPlanetTypes++;
            if (RenderSettings.ForceAllPlanetTypes>=PlanetSettings.planetTypes.planetTypes.Count)
            {
                RenderSettings.ForceAllPlanetTypes = -1;
                AddMessage("Resetting to all planet types");
            }
            else
            {
                //AddMessage("Setting all planets to type : " + RenderSettings.ForceAllPlanetTypes +"  (" + PlanetSettings.planetTypes.planetTypes[RenderSettings.ForceAllPlanetTypes].Name + ")", 4);
            }
            LoadFromXMLFileMCAST(settings.previousFile, false);

        }

        public void ToggleFlyCamera() {
			RenderSettings.MoveCam = !RenderSettings.MoveCam;

        }

        public void RandomizeSeeds() {
			if (settings.previousFile!="") {
            		LoadFromXMLFileMCAST(settings.previousFile, true);
            		AddMessage("Planet seeds set to random value");
			}

        }


        public override void Update()
        {
            base.Update();

            if (StarBackgroundSphere!=null)
                StarBackgroundSphere.SetActive(RenderSettings.useAtmosphericStarSphere);
    
            //UpdateBlackHoleEffect(SpaceCamera.getPos().Normalize().toVectorf()*-1, mainCamera);
            

            if (Input.GetKey(KeyCode.Escape))
            {

                if (settingsPanel!=null && settingsPanel.activeSelf == true)
                {
                    settingsPanel.SetActive(false);
                }
                else
                if (helpPanel!=null && helpPanel.activeSelf == true)
                {
                    settingsPanel.SetActive(false);
                }
                else
                    FatalQuit();
            }


            UpdateMessages();
            UpdatePlay();
            //            return;

            if (noiseImage != null)
            {
                Rect r = noiseImage.GetComponent<RawImage>().uvRect;
                r.size = new Vector3(4 + Random.value * 5, 4 + Random.value * 5);
                noiseImage.GetComponent<RawImage>().uvRect = r;
            }

           
            if (!RenderSettings.debug)
                return;



            if (modifier)
                if (Input.GetKeyUp(KeyCode.P))
                {
                    debugPanel.SetActive(!debugPanel.activeSelf);

                    // Randomize seed

                }

            if (modifier)
                if (Input.GetKeyUp(KeyCode.L))
                {
                    RenderSettings.MoveCam = !RenderSettings.MoveCam;


                }
        }


        private int percent;


        protected void FocusOnPlanet(string n)
        {
            GameObject gc = mainCamera;
            //Camera c = gc.GetComponent<Camera>();
            Planet planet = null;
            foreach (Planet p in solarSystem.planets)
                if (p.pSettings.name == n)
                    planet = p;

            if (planet == null)
                return;

            DVector pos = planet.pSettings.properties.pos;
            float s = (float)(planet.pSettings.radius * szWorld.overview_distance / RenderSettings.AU);
            Vector3 dir = pos.toVectorf().normalized * s;
            Vector3 side = Vector3.Cross(Vector3.up, dir);

            pos = pos - new DVector(dir) - new DVector(side.normalized * s);
            pos.y += s;

            gc.GetComponent<SpaceCamera>().SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
            UpdateWorldCamera();
            Update();
            gc.GetComponent<SpaceCamera>().SetLookCamera(pos, planet.pSettings.gameObject.transform.position, Vector3.up);
            UpdateWorldCamera();

        }


        protected override void Log()
        {
            string s = "";
            float val = 1;
            if (ThreadQueue.orgThreads != 0)
                val = (ThreadQueue.threadQueue.Count / (float)ThreadQueue.orgThreads);

            percent = 100 - (int)(100 * val);


            if (percent == 100 && RenderSettings.ExitSaveOnRendered && ThreadQueue.currentThreads.Count == 0)
            {
                if (extraTimer-- == 0)
                    ExitSave();
            }
            load_percent = percent;

            if (RenderSettings.isVideo)
                s += "Progress: " + percent + " %\n";
            //s+="Height: " + stats.Height.ToString("0.00") + " km \n";
            //s+="Velocity: " + stats.Velocity.ToString("0.00") + " km/s\n";
            s += RenderSettings.extraText + "\n";
            foreach (Message m in messages)
                s += m.message + "\n";


            s+=RenderSettings.path;
            GameObject info = GameObject.Find("Logging");
            if (info != null)
                info.GetComponent<Text>().text = s;
        }


    }

}