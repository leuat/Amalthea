using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{

    public class GLLines
    {
        public List<Vector3> points = new List<Vector3>();
        public Color color = Color.white;
        public Vector3 displacement = Vector3.zero;
        public void Render()
        {
            GL.Color(color);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 from = points[i] + displacement; 
                Vector3 to = points[(i + 1) % points.Count] + displacement;

                GL.Vertex3(from.x, from.y, from.z);
                GL.Vertex3(to.x, to.y, to.z);
            }

        }
    }

    public class GLLineRenderer
    {
        public List<GLLines> lines = new List<GLLines>();
        private static Material lineRenderer;

        // Must be called on onUpdatePostRenderer eller whatnot

        public void Clear()
        {
            lines.Clear();
        }

        public void Render()
        {
            if (lineRenderer == null)
                lineRenderer = new Material(Shader.Find("Particles/Alpha Blended"));

            lineRenderer.SetPass(0);
            GL.Begin(GL.LINES);
            foreach (GLLines l in lines)
                l.Render();
            GL.End();


        }

    }

}