using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{

    public class DisplayPlanetMCAST : DisplayPlanet
    {

        private SerializedMCAstObject serializedPlanet;
        public DisplayPlanetMCAST(GameObject g, PlanetInstance p) : base(g,p)
        {
        }


        public void CreateOrbitFromFrames(int maxLines)
        {
            return;


            if (serializedPlanet.Frames.Count < 2)
                return;

            if (planet.lsPlanet.pSettings.category == PlanetSettings.Categories.Moon)
                return;

            for (int i = 0; i < maxLines; i++)
            {
                if (i + 1 >= serializedPlanet.Frames.Count)
                    break;
                Frame sp = serializedPlanet.Frames[i];
                Frame sp2 = serializedPlanet.Frames[i + 1];
                DVector from = new DVector(sp.pos_x, sp.pos_y, sp.pos_z) * SSVAppSettings.SolarSystemScale;
                DVector to = new DVector(sp2.pos_x, sp2.pos_y, sp2.pos_z) * SSVAppSettings.SolarSystemScale;

                GameObject g = new GameObject();
//                g.transform.parent = SolarSystemViewverMain.linesObject.transform;
                LineRenderer lr = g.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Particles/Additive"));//(Material)Resources.Load ("LineMaterial");
//                lr.SetWidth(SSVAppSettings.OrbitalLineWidth.x, SSVAppSettings.OrbitalLineWidth.y);
                lr.SetPosition(0, from.toVectorf());
                lr.SetPosition(1, to.toVectorf());
                //orbitLines.Add(g);
            }
        }

      


        /*        public void MaintainOrbits()
                {
                    int maxFrames = serializedPlanet.Frames.Count;
                    int currentFrame = (int)(SSVAppSettings.currentFrame * maxFrames);
                    Color c = SSVAppSettings.orbitLinesColor;
                    if (planet.pSettings.category == PlanetSettings.Categories.Spacecraft)
                        c = SSVAppSettings.spaceCraftColor;

                    int h = orbitLines.Count / 1;

                    for (int i = 0; i < orbitLines.Count; i++)
                    {
                        int f1 = (int)Mathf.Clamp((i - h) * SSVAppSettings.LineScale + currentFrame, 0, maxFrames);
                        int f2 = (int)Mathf.Clamp((i + 1 - h) * SSVAppSettings.LineScale + currentFrame, 0, maxFrames);
                        if (f1 >= serializedPlanet.Frames.Count || f1 < 0)
                            break;
                        if (f2 >= serializedPlanet.Frames.Count || f2 < 0)
                            break;
                        LineRenderer lr = orbitLines[i].GetComponent<LineRenderer>();
                        Frame sp = serializedPlanet.Frames[f1];
                        Frame sp2 = serializedPlanet.Frames[f2];
                        DVector from = new DVector(sp.pos_x, sp.pos_y, sp.pos_z) * SSVAppSettings.SolarSystemScale;
                        DVector to = new DVector(sp2.pos_x, sp2.pos_y, sp2.pos_z) * SSVAppSettings.SolarSystemScale;


                        lr.SetPosition(0, from.toVectorf());
                        lr.SetPosition(1, to.toVectorf());

                        float colorScale = Mathf.Abs(i - orbitLines.Count / 2) / (float)orbitLines.Count * 2;
                        Color col = c * (1 - colorScale);
                        col.a = 1;
                        lr.SetColors(col, col);
                    }
                }
                */
    }
}