using UnityEngine;
using System.Collections;


namespace LemonSpawn
{

    public class Explosion : Planet
    {

        private Material material;

        private Billboards explosionBB = new Billboards();
        public Explosion()
        {

        }


        public Explosion(PlanetSettings p)
        {
            pSettings = p;
        }


        public override void Initialize(GameObject sun, Material ground, Material sky, Mesh sphere)
        {
            GameObject obj = new GameObject();
            obj.transform.parent = pSettings.gameObject.transform;

            GameObject go = obj;
            go.name = "explosion";

            Create(go);
        }



        protected void Create(GameObject obj)
        {
            float r = pSettings.properties.serializedPlanet.objectScale;
            Vector3 color = new Vector3(pSettings.properties.extraColor.r, pSettings.properties.extraColor.g, pSettings.properties.extraColor.b);

            //            color = Vector3.one;
            // Skal være 2
            explosionBB.billboards.Add(new LemonSpawn.Billboard(Vector3.zero, new Vector2( r, r), color));
            material = (Material)Resources.Load("ExplosionMaterial");
            int rndName = (int)(Random.value * 100000);
            explosionBB.Realize("Explosion"+rndName, material, 0).transform.parent = obj.transform.parent;

        }

        public override void Update()
        {
            cameraAndPosition();
            //material.SetColor("_Color", pSettings.properties.extraColor);

            Vector3[] c = explosionBB.lsMesh.mesh.normals;
            c[0] = new Vector3( pSettings.properties.extraColor.r, pSettings.properties.extraColor.g, pSettings.properties.extraColor.b);
            explosionBB.lsMesh.mesh.normals = c;
            material.SetFloat("_Size", pSettings.properties.scale.x);
        }



    }

}
