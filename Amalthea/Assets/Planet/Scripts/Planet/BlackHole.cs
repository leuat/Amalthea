using UnityEngine;
using System.Collections;


namespace LemonSpawn
{

    public class BlackHole : Planet
    {

        private Material bhMaterial;

        public BlackHole()
        {

        }


        public BlackHole(PlanetSettings p)
        {
            pSettings = p;
        }


        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
            GameObject obj = new GameObject();
            obj.transform.parent = pSettings.gameObject.transform;

            bhMaterial = new Material(sky.shader);
            bhMaterial.CopyPropertiesFromMaterial(sky);

            pSettings.properties.distortionIntensity = 1f;
            RenderSettings.distortionIntensity = 1f;

            GameObject go = obj;
            go.name = "black hole";
            go.transform.localScale = Vector3.one * pSettings.radius;

            RenderSettings.useAtmosphericStarSphere = false;

            //pSettings.radius = 1000;

            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
//            starMaterial = new Material(sky.shader);
  //          starMaterial.CopyPropertiesFromMaterial(sky);
            mf.mesh = sphere;
            mr.material = (Material)Resources.Load("Black");

            



            pSettings.properties.extraColor = Constants.colorTemperatureToRGB(pSettings.temperature);
            bhMaterial.SetColor("_Color", pSettings.properties.extraColor);
            CreateAccretion();


        }

        protected void CreateAccretion()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Accretion disk";
            plane.transform.localScale = Vector3.one * pSettings.radius * 4;
            plane.GetComponent<MeshRenderer>().material = (Material)Resources.Load("AccretionDisk"); 

        }

        public override void Update()
        {
            //cameraAndPosition();
        }



    }

}
