using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace LemonSpawn {
    public class AdditionalLineRenderer : MonoBehaviour {

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
        public void OnPostRender()
        {
            SolarSystemViewverMain SSV = GameObject.Find("Camera").GetComponent<SolarSystemViewverMain>();
            SSV.OnPostRender();
        }
    }
}