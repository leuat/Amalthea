using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{
    public class SSVData : SSVDataBase
    {
        public List<DisplayPlanet> dPlanets = new List<DisplayPlanet>();
        public DisplayPlanet selected = null;
        public DisplayPlanet dpSun = null;
        public Vector3 focusPointCurStar = Vector3.zero;
        public StarSystem currentSystem = null, selectedSystem = null;
        public APlayer player = new APlayer();
        public float currentDistance;

        public enum Mode { InterPlanetary, EditPlanet, Interstellar };
        public Mode currentMode = Mode.InterPlanetary;


        public override void Initialize()
        {

        }

        // Update is called once per frame
        public override void Update()
        {
            UpdatePositions();
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



        public void CreatePlanetHierarchy()
        {
            // First, find the sun
            //            DisplayPlanet sun = dPlanets[0];
            
            dpSun = findDisplayPlanetWithparent(null);
            dpSun.children.Clear();
            foreach (DisplayPlanet dp in dPlanets)
            {
                if (dp.planet.lsPlanet.pSettings.properties.parentPlanet == dpSun.planet.lsPlanet)
                {
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
        }
        /*
        public void RenderDisplayPlanetLabels(Camera MainCamera)
        {
            foreach (DisplayPlanet dp in dPlanets)
            {
                SSVAppSettings.guiStyle.normal.textColor = SSVSettings.planetColor;
                if (dp.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Moon)
                    SSVAppSettings.guiStyle.normal.textColor = SSVSettings.moonColor;
                if (dp.planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Spacecraft)
                    SSVAppSettings.guiStyle.normal.textColor = SSVSettings.spaceCraftColor;


                Vector3 pos = MainCamera.WorldToScreenPoint(dp.go.transform.position);
                int width1 = dp.planet.lsPlanet.pSettings.givenName.Trim().Length;
                int width2 = dp.planet.lsPlanet.pSettings.name.Trim().Length;
                int fs = 16 + (int)Mathf.Pow(dp.planet.lsPlanet.pSettings.radius, 0.6f);
                SSVAppSettings.guiStyle.fontSize = fs;
                //                if (pos.x >0 && pos.y<Screen.width && pos.y>0 && pos.y<Screen.height)
                if (pos.z > 0)
                {
                    float ha = 50;
                    GUI.Label(new Rect(pos.x - (width1 / 2) * 10, Screen.height - pos.y - ha, 250, 130), dp.planet.lsPlanet.pSettings.givenName, SSVAppSettings.guiStyle);
                    SSVAppSettings.guiStyle.fontSize = 12;

                    GUI.Label(new Rect(pos.x - (width2 / 2) * 4, Screen.height - pos.y + (int)(fs * 1.0) - ha, 250, 130), dp.planet.lsPlanet.pSettings.name, SSVAppSettings.guiStyle);
                }

            }
        }
        */

        public void RenderSolarSystemLabels(GameObject mainCamera)
        {
            foreach (DisplayPlanet dp in dPlanets)
            {
                GUIStyle guiStyle = SSVAppSettings.guiStyle;


                guiStyle.normal.textColor = dp.planet.stellarCategory.color;
                Color c = dp.planet.stellarCategory.color;
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                {
                    c.a = Mathf.Clamp(1 - 0.001f * (dp.go.transform.position - mainCamera.transform.position).magnitude, 0, 1);
                    guiStyle.normal.textColor = c;
                }
                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                {
                    c.a = Mathf.Clamp(1 - 0.00005f * (dp.go.transform.position - mainCamera.transform.position).magnitude, 0, 1);
                    guiStyle.normal.textColor = c;
                }
//                if (dp.planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Spacecraft)
  //                  guiStyle.normal.textColor = SSVSettings.spaceCraftColor;

                guiStyle.normal.textColor = guiStyle.normal.textColor * (1 + dp.timer);


                Vector3 pos = mainCamera.GetComponent<Camera>().WorldToScreenPoint(dp.go.transform.position);
                //int width1 = dp.planet.lsPlanet.pSettings.givenName.Trim().Length;
                int width2 = dp.planet.lsPlanet.pSettings.name.Trim().Length;
                int fs = (int)Mathf.Clamp(16 + (int)Mathf.Pow(dp.planet.lsPlanet.pSettings.radius, 0.6f) + (int)(dp.timer * 20f), 8, 35);
                guiStyle.fontSize = fs;
                Color black = Color.black;
                black.a = guiStyle.normal.textColor.a;
                //                if (pos.x >0 && pos.y<Screen.width && pos.y>0 && pos.y<Screen.height)
                if (pos.z > 0 && guiStyle.normal.textColor.a > 0)
                {
                    float ha = 30;
                    float gf = guiStyle.fontSize / 2;
                    c = guiStyle.normal.textColor;
                    guiStyle.normal.textColor = black;
                    int a = 2;
                    GUI.Label(new Rect(pos.x - gf * dp.planet.lsPlanet.pSettings.givenName.Length / 2 + a, Screen.height - pos.y - ha - gf + a, 250, 130), dp.planet.lsPlanet.pSettings.givenName, guiStyle);
                    guiStyle.fontSize = 12;
                    GUI.Label(new Rect(pos.x - (width2 / 2) * 4 + a, Screen.height - pos.y + (int)(fs * 1.0) - ha + a, 250, 130), dp.planet.lsPlanet.pSettings.name, guiStyle);
                    guiStyle.fontSize = fs;


                    guiStyle.normal.textColor = c;


                    GUI.Label(new Rect(pos.x - gf * dp.planet.lsPlanet.pSettings.givenName.Length / 2, Screen.height - pos.y - ha - gf, 250, 130), dp.planet.lsPlanet.pSettings.givenName, guiStyle);
                    guiStyle.fontSize = 12;

                    GUI.Label(new Rect(pos.x - (width2 / 2) * 4, Screen.height - pos.y + (int)(fs * 1.0) - ha, 250, 130), dp.planet.lsPlanet.pSettings.name, guiStyle);
                }

            }

        }




        public void DestroyAllGameObjects()
        {

            foreach (DisplayPlanet dp in dPlanets)
            {
                dp.DestroyOrbits();
                GameObject.Destroy(dp.go);
            }
        }

        public void CreateOrbitalLines()
        {
            foreach (DisplayPlanet dp in dPlanets)
                dp.CreateOrbitsFromRadius(SSVAppSettings.MaxOrbitalLines);

        }


        protected DisplayPlanet FindDisplayPlanetFromGameObject(GameObject go)
        {
            foreach (DisplayPlanet dp in dPlanets)
            {
                if (dp.planet.lsPlanet.pSettings.gameObject == go)
                    return dp;
            }
            return null;
        }


        public void UpdatePositions()
        {
            foreach (DisplayPlanet dp in dPlanets)
            {
                dp.UpdatePosition();
            }
        }
        
        // This one is needed for flat xml objects
        public void OrganizePlanetGameObjectsByName()
        {
            
            foreach (DisplayPlanet dp in dPlanets)
            {
                GameObject g = dp.planet.lsPlanet.pSettings.gameObject;
                string n = g.name;
                //Debug.Log(g.name);
                if (n.ToLower().Contains("planet") && !n.ToLower().Contains("moon"))
                {
                    g.transform.parent = findDisplayPlanetWithparent(null).planet.lsPlanet.pSettings.transform;//   GameObject.Find("The star").transform;
                }
                if (n.ToLower().Contains("moon"))
                {
                    string[] lst = n.Split(' ');
                    string parentPlanet = lst[3] + " " + lst[4];
                    //string parent = ""
                    g.transform.parent = GameObject.Find(parentPlanet).transform;
                }
                // ALSO set parent planet link (for menu)
                if (g.transform.parent != null)
                {
                    DisplayPlanet dparent = FindDisplayPlanetFromGameObject(g.transform.parent.gameObject);
                    if (dparent != null)
                    {
                        dp.planet.lsPlanet.pSettings.properties.parentPlanet = dparent.planet.lsPlanet;

                        dp.planet.lsPlanet.pSettings.properties.distance = (float)dp.planet.lsPlanet.pSettings.gameObject.transform.position.magnitude;
                        dp.planet.lsPlanet.pSettings.properties.pos = dp.planet.lsPlanet.pSettings.properties.pos - dparent.planet.lsPlanet.pSettings.properties.pos;
                        dp.planet.lsPlanet.pSettings.properties.orgPos = dp.planet.lsPlanet.pSettings.properties.pos;
                        dp.planet.lsPlanet.pSettings.properties.distance = (float)dp.planet.lsPlanet.pSettings.properties.pos.Length();
                    }
                    // Also update properties distance


                }
                
            }

        }


    }
}
