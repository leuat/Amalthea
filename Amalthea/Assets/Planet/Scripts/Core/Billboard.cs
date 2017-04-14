using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn {

    public class Billboard
    {
        public Vector3 pos;
        public Vector2 size;
        public Vector3 color;
    }



    public class Billboards
    {
        public LSMesh lsMesh;
        public GameObject gameObject;
        public Material material;
        public List<Billboard> billboards = new List<Billboard>();

        public void Realize(Material m)
        {

        }

    }
}