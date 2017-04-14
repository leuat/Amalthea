using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Amalthea
{

    public class DisplayPlanet
    {
        public DisplayPlanet parent;
        public List<DisplayPlanet> children = new List<DisplayPlanet>();
        public static DisplayPlanet performSelect = null;
        public Planet planet;
        public GameObject go;
        public float timer = 0;
        public List<Vector3> orbitLines = new List<Vector3>();
        private static Material lineRenderer;
        public Color orbitColor;
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
                miniCameraGO.transform.parent = SSVSettings.extraGameObject.transform;

                miniCamera = miniCameraGO.AddComponent<Camera>();
                //miniCamera.rect = new Rect(0.01f, 1 - 0.5f, 0.2f, 0.2f);

            }
            miniCamera.enabled = true;

            miniCamera.transform.position = planet.lsPlanet.pSettings.transform.position -
            (planet.lsPlanet.pSettings.transform.position.normalized + miniCamera.transform.right * 15 - Vector3.up * 15).normalized * SSVSettings.MiniCamDist * planet.lsPlanet.pSettings.radius;
            miniCamera.transform.up = new Vector3(0.5f, 1, 0).normalized;
            miniCamera.fieldOfView = SSVSettings.MiniCamFOV;
            miniCamera.transform.LookAt(planet.lsPlanet.pSettings.transform.position);
            miniCamera.targetTexture = renderTexture;
            //            miniCamera.rect = new Rect(0, 0, 0.2, 0.2);
            miniCamera.farClipPlane = 100000;
            miniCamera.clearFlags = CameraClearFlags.SolidColor;
			miniCamera.cullingMask = GameObject.Find ("Camera").GetComponent<Camera> ().cullingMask;
            miniCamera.backgroundColor = new Color(0, 0, 0, 0);// Color.black;
			GameObject sc = GameObject.Find("StarCamera");
//			sc.SetActive(false);
            miniCamera.Render();
//			sc.SetActive(true);

            texImage = Util.GetRTPixels(renderTexture, texImage);
            miniCamera.enabled = false;
            //miniCamera.targetTexture = null;

        }

        public void UpdateCamera()
        {
            if (miniCamera == null)
                return;
            miniCamera.transform.position = planet.lsPlanet.pSettings.transform.position +
                Vector3.left * SSVSettings.MiniCamDist * planet.lsPlanet.pSettings.radius;
            miniCamera.transform.LookAt(planet.lsPlanet.pSettings.transform.position);
        }


        public void Trigger()
        {
            timer = 1;
        }

        public DisplayPlanet(GameObject g, Planet p)
        {
            go = g;
            planet = p;
        }

        public void DestroyOrbits()
        {
            orbitLines.Clear();

        }



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


        public void RenderGLOrbits(int maxLines)
        {

            Vector3 center = planet.lsPlanet.pSettings.transform.parent.position;
            float d = (planet.lsPlanet.pSettings.transform.position - center).magnitude;
            if (lineRenderer == null)
                lineRenderer = new Material(Shader.Find("Particles/Alpha Blended"));
            lineRenderer.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(orbitColor);

            for (int i = 0; i < orbitLines.Count; i++)
            {
                Vector3 from = orbitLines[i] + center;
                Vector3 to = orbitLines[(i + 1) % orbitLines.Count] + center;



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

            orbitColor = planet.stellarCategory.color * (0.5f + 1.0f * UnityEngine.Random.value);
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
            }
            if (planet.lsPlanet.pSettings.category == LemonSpawn.PlanetSettings.Categories.Planet)
            {
                ms = 1.0f;// / SSVSettings.PlanetSizeScale;
                prevRadius = planet.lsPlanet.pSettings.transform.parent.gameObject.GetComponent<LemonSpawn.PlanetSettings>().radius*2;
            }
            // Scales moons
            return pos * SSVSettings.SolarSystemScale * ms + pos.normalized * prevRadius;

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