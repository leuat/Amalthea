using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class Billboard
    {
        public Vector3 pos;
        public Vector2 size;
        public Vector3 color;
        public Billboard(Vector3 p, Vector2 s, Vector3 c)
        {
            pos = p;
            size = s;
            color = c;
        }
    }



    public class Billboards
    {
        public LSMesh lsMesh;
        public GameObject gameObject;
        public Material material;
        public List<Billboard> billboards = new List<Billboard>();

         


        public GameObject Realize(string name, Material m, int layer)
        {
            LemonSpawn.Util.DestroyGameObject(name);
            lsMesh = CreateMesh(name);
            
            GameObject go = lsMesh.Realize(name, m, layer, "normal", false);
            go.layer = layer;
            return go;
        }
        public void UpdateMesh()
        {
            if (lsMesh == null)
                return;
            Mesh m = lsMesh.mesh;
            List<Vector3> posList = new List<Vector3>();
            m.GetVertices(posList);
            List<Vector3> nList = new List<Vector3>();
            m.GetNormals(nList);

            int i = 0;
            foreach (Billboard b in billboards)
            {
                posList[i] = b.pos;
                nList[i] = b.color;
                i++;
            }
            m.vertices = posList.ToArray();
            m.normals = nList.ToArray();
        }

        public LSMesh CreateMesh(string name)
        {
            LemonSpawn.LSMesh lsMesh = new LemonSpawn.LSMesh();
            int i = 0;
            Vector3 min = Vector3.one * 1E20f;
            Vector3 max = Vector3.one * -1E20f;
            foreach (Billboard b in billboards)
            {
                lsMesh.vertexList.Add(b.pos);
                float size = b.size.x;
                max = LemonSpawn.Util.Max(b.pos + Vector3.one * size, max);
                min = LemonSpawn.Util.Min(b.pos - Vector3.one * size, min);

                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                lsMesh.normalList.Add(b.color);
                lsMesh.uvList.Add(b.size);
                lsMesh.tangentList.Add(Vector4.zero);
                i++;
            }
            lsMesh.createMesh(false);
            lsMesh.mesh.name = "mesh";
            lsMesh.mesh.bounds = new Bounds((max + min) * 0.5f, (max - min));

            return lsMesh;


        }
    }
}
