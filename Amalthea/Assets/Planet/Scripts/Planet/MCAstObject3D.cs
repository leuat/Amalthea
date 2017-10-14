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







        private GameObject InitializeLaser(Material m)
        {
            float sx = 0.5f;
            float sy = 0.5f;
            float sz = 2;
            LSMeshBox b = new LSMeshBox(
                new Vector3(sx, sy, sz),
                new Vector3(-sx,  sy, sz),
                new Vector3(-sx,  -sy, sz),
                new Vector3(sx, -sy, sz),
                new Vector3(sx,  -sy, -sz),
                new Vector3(-sx, -sy, -sz),
                new Vector3(-sx,  sy, -sz),
                new Vector3(sx,     sy, -sz), true);
            GameObject obj = b.Realize("Laser", m, 0, "Normal", true);
            return obj;

        }

        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {

            GameObject main = new GameObject(pSettings.properties.serializedPlanet.name);

            GameObject obj;
            string objName = pSettings.properties.serializedPlanet.objectString;
            if (objName.ToLower() == "laser")
                obj = InitializeLaser((Material)(Resources.Load(pSettings.properties.serializedPlanet.objectMaterial)));
            else 
                obj = (GameObject)GameObject.Instantiate(Resources.Load(pSettings.properties.serializedPlanet.objectString), new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0,90,0)));
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

            cameraAndPosition();
        }



    }

}