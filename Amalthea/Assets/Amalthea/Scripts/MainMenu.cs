using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LemonSpawn;
using Amalthea;

public class MainMenu : World {


    public override void Start()
    {
        base.Start();
        //       mainCamera.GetComponent<SpaceCamera>().enabled = false;
        LemonSpawn.RenderSettings.MoveCam = false;
        Globals.Initialize();


    }

    public override void Update()
    {
        base.Update();
        solarSystem.planets[0].pSettings.rotation -= 0.005 * Time.deltaTime;
        
    }

}
