using UnityEngine;
using System.Collections;


namespace LemonSpawn {

    public class Star : Planet {

        private Material starMaterial;

        private Billboards corona = new Billboards();  
        public Star()
        {

        }      
        
           
        public Star(PlanetSettings p)
        {
            pSettings = p;
        }


        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
            GameObject obj = new GameObject();
            obj.transform.parent = pSettings.gameObject.transform;

            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            starMaterial = new Material(sky.shader);
            starMaterial.CopyPropertiesFromMaterial(sky);
            mf.mesh = sphere;
            mr.material = starMaterial;
            //pSettings.radius*=RenderSettings.GlobalRadiusScale;

            // Test for BH!
            pSettings.properties.distortionIntensity = 0;
            GameObject go = obj;
            go.name = "star";
            go.transform.localScale = Vector3.one * pSettings.radius;

            pSettings.properties.extraColor = Constants.colorTemperatureToRGB(pSettings.temperature);
            starMaterial.SetColor("_Color", pSettings.properties.extraColor);
            CreateCorona();


        }

        protected void CreateCorona()
        {
            float r = pSettings.radius*3;
            Vector3 color = new Vector3(pSettings.properties.extraColor.r, pSettings.properties.extraColor.g, pSettings.properties.extraColor.b) ;

//            color = Vector3.one;
            // Skal være 2
            corona.billboards.Add(new LemonSpawn.Billboard(Vector3.zero, new Vector2(2*r,2*r), color));
            corona.billboards.Add(new LemonSpawn.Billboard(Vector3.zero, new Vector2(10*r, 10*r), color*0.3f));
             
            corona.Realize("Solar Corona", (Material)Resources.Load("Corona"), 9);

        }

        public override void Update() {
            //cameraAndPosition();
        }



    }

}
