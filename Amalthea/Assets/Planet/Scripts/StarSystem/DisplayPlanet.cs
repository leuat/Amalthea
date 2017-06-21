using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class DisplayPlanet
    {
        public DisplayPlanet parent;
        public List<DisplayPlanet> children = new List<DisplayPlanet>();
        public static DisplayPlanet performSelect = null;
        public PlanetInstance planet;
        public GLLines lines = null;
        public string displayMessage = "";
        public GameObject go;
        public float timer = 0;
//        public List<Vector3> orbitLines = new List<Vector3>();
//        public Color orbitColor;
        public Camera miniCamera;
        public GameObject miniCameraGO;
        RenderTexture renderTexture = new RenderTexture(128, 128, 16, RenderTextureFormat.Default);
        public Texture2D texImage;



        // X Y coordinates + margin of camera
        public static Vector3 cameraBegin = new Vector3();

        public void setCameraEnabled(bool isEnabled)
        {
            miniCamera.enabled = isEnabled;
        }

        public void CreateMenu(string n, MenuItem item, Vector2 size, bool isHorizontal, float sc, MenuLayout ml, MenuItem.Callback callback)
        {
            MenuItem mi = new MenuItem(item, n, "", texImage, size, isHorizontal, sc, ml, callback, (System.Object)this);
            item.children.Add(mi);
            foreach (DisplayPlanet dp in children)
                dp.CreateMenu(n, mi, size * sc, !isHorizontal, sc, ml, callback);
        }

        public void CreatePlanetCamera()
        {
            if (miniCamera == null)
            {
                miniCameraGO = new GameObject();
                miniCameraGO.transform.parent = SSVAppSettings.extraGameObject.transform;

                miniCamera = miniCameraGO.AddComponent<Camera>();
                //miniCamera.rect = new Rect(0.01f, 1 - 0.5f, 0.2f, 0.2f);

            }
            miniCamera.enabled = true;

            miniCamera.transform.position = planet.lsPlanet.pSettings.transform.position -
            (planet.lsPlanet.pSettings.transform.position.normalized + miniCamera.transform.right * 15 - Vector3.up * 15).normalized * SSVAppSettings.MiniCamDist * planet.lsPlanet.pSettings.radius;
            miniCamera.transform.up = new Vector3(0.5f, 1, 0).normalized;
            miniCamera.fieldOfView = SSVAppSettings.MiniCamFOV;
            miniCamera.transform.LookAt(planet.lsPlanet.pSettings.transform.position);
            miniCamera.targetTexture = renderTexture;
            //            miniCamera.rect = new Rect(0, 0, 0.2, 0.2);
            miniCamera.farClipPlane = 1000f;
            miniCamera.nearClipPlane = 0.1f;
            miniCamera.clearFlags = CameraClearFlags.SolidColor;
			miniCamera.cullingMask = GameObject.Find ("Camera").GetComponent<Camera> ().cullingMask;
            miniCamera.backgroundColor = new Color(0, 0, 0, 0);// Color.black;
			GameObject sc = GameObject.Find("StarCamera");
//			sc.SetActive(false);
            miniCamera.Render();
	//		sc.SetActive(true);

            texImage = Util.GetRTPixels(renderTexture, texImage);
            miniCamera.enabled = false;
            //miniCamera.targetTexture = null;

        }

        public void UpdateCamera()
        {
            if (miniCamera == null)
                return;
/*            miniCamera.transform.position = planet.lsPlanet.pSettings.transform.position +
                Vector3.left * SSVAppSettings.MiniCamDist * planet.lsPlanet.pSettings.radius;
            miniCamera.transform.LookAt(planet.lsPlanet.pSettings.transform.position);*/
        }


        public void Trigger()
        {
            timer = 1;
        }

        public DisplayPlanet(GameObject g, PlanetInstance p)
        {
            go = g;
            planet = p;
            if (p.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                planet.stellarCategory = Globals.definitions.stellarCategories.Get("Planet");
            if (p.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon)
                planet.stellarCategory = Globals.definitions.stellarCategories.Get("Moon");
            if (p.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
                planet.stellarCategory = Globals.definitions.stellarCategories.Get("Star");
            if (p.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.BlackHole)
                planet.stellarCategory = Globals.definitions.stellarCategories.Get("BlackHole");


            if (planet.stellarCategory == null)
                planet.stellarCategory = Globals.definitions.stellarCategories.Get("UFO");
        }

/*        public void DestroyOrbits()
        {
            orbitLines.Clear();

        }
        */


        public void DrawGUI(Vector3 rect, float size)
        {
            int h = Screen.height;

            float ss = timer * 0.04f;

            Rect r = new Rect((rect.x - ss) * h, (rect.y - ss) * h, (size + 2 * ss) * h, (size + 2 * ss) * h);

            if (GUI.Button(r, "", new GUIStyle()))
            {
                performSelect = this;
            }
            GUI.DrawTexture(r, texImage, ScaleMode.ScaleToFit);



            //            GUI.Label(r, planet.lsPlanet.pSettings.name);

            //            Debug.Log(miniCamera.rect):;
            //            Debug.Log("child " +children.Count);
            //            Vector3 newRect = rect;
            //            Debug.Log("rect: " + miniCamera.rect + " for planet " + planet.lsPlanet.pSettings.name);
            float s = size;
            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
            {
                s /= 2;
                rect.x += s / 2;
                rect.y += s;
            }
            //            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
            //                Debug.Log(children.Count);

            foreach (DisplayPlanet dp in children)
            {
                if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Star)
                    rect.x += s;// + newRect.z;

                if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
                {
                    rect.y += s;// + rect.z;
                }
                dp.DrawGUI(rect, s);

            }
        }


        public void CreateOrbitsFromRadius(int maxLines, GLLineRenderer glr)
        {

            //DestroyOrbits();

            lines = new GLLines();

            //if (planet.pSettings.category == PlanetSettings.Categories.Moon)
            //    return;
            lines.color = planet.stellarCategory.color * (0.5f + 1.0f * UnityEngine.Random.value);
            lines.color.a = 0.5f;

            Vector3 center = planet.lsPlanet.pSettings.transform.parent.position;
            //float d = (planet.lsPlanet.pSettings.transform.position - center).magnitude;
            //Quaternion q = Quaternion.Euler(planet.lsPlanet.pSettings.eulerX, 0, planet.lsPlanet.pSettings.eulerZ);
            for (int i = 0; i < maxLines; i++)
            {
                float t0 = (float)i / (maxLines + 1) * 2 * Mathf.PI;


                Vector3 p = planet.lsPlanet.pSettings.getOrbit(t0);// *SSVAppSettings.SolarSystemScale;
                Vector3 dp = getDisplayPosition(p);

                lines.points.Add(dp);


            }
            glr.lines.Add(lines);
        }

        public Vector3 getDisplayPosition(Vector3 pos)
        {
            float ms = 1;
            float prevRadius = 0;
            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Moon && planet.lsPlanet.pSettings.properties.parentPlanet!=null)
            {
                ms = 2.0f / SSVAppSettings.PlanetSizeScale;
                prevRadius = planet.lsPlanet.pSettings.properties.parentPlanet.pSettings.radius;
            }
            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet  && planet.lsPlanet.pSettings.properties.parentPlanet!=null)
            {
                ms = 1.0f;// / SSVAppSettings.PlanetSizeScale;
                if (planet.lsPlanet.pSettings.transform.parent != null)
                {
                    float scale = 2;
                    if (planet.lsPlanet.pSettings.properties.parentPlanet.pSettings.category == PlanetSettings.Categories.BlackHole)
                        scale = 10;
                        prevRadius = planet.lsPlanet.pSettings.properties.parentPlanet.pSettings.radius * scale;
                }
            }
            // Scales moons
            //            Debug.Log(pos + " for " + planet.lsPlanet.pSettings.gameObject.name);
            return pos * SSVAppSettings.SolarSystemScale * ms + pos.normalized * prevRadius;

        }

        public void UpdatePosition()
        {
//            Debug.Log("UpdatePos: " + planet.lsPlanet.pSettings.properties.pos.toVectorf().x);
            Vector3 p = getDisplayPosition(planet.lsPlanet.pSettings.properties.pos.toVectorf());
            planet.lsPlanet.pSettings.transform.localPosition = p;
            // Displace orbitallines
            if (lines!=null)
                lines.displacement = planet.lsPlanet.pSettings.transform.parent.position;

        }

        public void UpdatePosition(float t, Vector3 cam)
        {
            planet.lsPlanet.pSettings.setPosition(t);

            planet.lsPlanet.pSettings.transform.localPosition = getDisplayPosition(planet.lsPlanet.pSettings.properties.pos.toVectorf());

            float scale = Mathf.Clamp((go.transform.position - cam).magnitude * 0.003f, 3, 50);

            go.transform.localScale = Vector3.one * planet.lsPlanet.pSettings.radius * 2f;
            //            go.transform.localScale = Vector3.one * Mathf.Clamp(planet.lsPlanet.pSettings.radius, 2, 1000) * 2f * scale;
            timer -= Time.deltaTime * 10f;
            timer = Mathf.Clamp(timer, 0, 1);

            UpdateCamera();

        }

        public static explicit operator DisplayPlanet(UnityEngine.Object v)
        {
            throw new NotImplementedException();
        }


    }
}