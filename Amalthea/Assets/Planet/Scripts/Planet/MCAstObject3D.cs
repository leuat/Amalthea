using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LemonSpawn
{

    public class MCAstObject3D : Planet
    {

        private Material starMaterial;

        public MCAstObject3D(PlanetSettings p)
        {
            pSettings = p;
        }

        

        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {

            GameObject main = new GameObject(pSettings.properties.serializedPlanet.name);

            pSettings.properties.autoOrient = true;

            GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load(pSettings.properties.serializedPlanet.objectString), new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0,90,0)));
            main.transform.parent = pSettings.gameObject.transform;
            obj.transform.parent = main.transform;
            //GameObject go = obj;
            Material org = (Material)(Resources.Load(pSettings.properties.serializedPlanet.objectMaterial));
            Material m = new Material(org.shader);
            m.CopyPropertiesFromMaterial(org);
            m.color = new Color(pSettings.properties.serializedPlanet.color_r,
                pSettings.properties.serializedPlanet.color_g,
                pSettings.properties.serializedPlanet.color_b, 1);
            /*Material[] mats = obj.GetComponentInChildren<Renderer>().materials;
            for (int i=0;i<mats.Length;i++)
            {
                mats[i] = m;
            }
            obj.GetComponentInChildren<Renderer>().materials = mats;
            */
            Util.ReplaceAllMaterial(obj, m);

            obj.transform.localScale = Vector3.one * pSettings.properties.serializedPlanet.objectScale;

        }

        public override void Update()
        {

            //cameraAndPosition();
        }



    }

}