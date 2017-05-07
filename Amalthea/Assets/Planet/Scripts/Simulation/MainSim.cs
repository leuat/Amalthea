using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LemonSpawn
{

    public class Particle
    {
        public Vector3 P, V, A;
        public float mass = 1, T = 0;
        public void Move(float dt)
        {
            P = P + V * dt;
            V = V + A * dt;
            //            V = V * Mathf.Clamp(1-0.0001f*A.magnitude,0,1);
            A = Vector3.zero;

            T -= dt;
            
            T = Mathf.Clamp(T, 0, 50);
        }

        public Particle(Vector3 p, Vector3 v, float m)
        {
            P = p;
            V = v;
            mass = m;

        }

        public void Test()
        {
            Vector3 d = P;
        }
        public void Gravity(Vector3 op, float omass, float G, float size, float epsilon) { 

            Vector3 d = P - op;
            float r = Mathf.Max(d.magnitude, epsilon);
            d = d.normalized;
            if (r < size && r != epsilon)
            {
                //P = op + d * size * 1.01f;
                V *= 0.98f;
                T += 1;
            }


            float PP = -G * mass * omass;
            float F = PP / (r * r);//  - 0.1f*P/(Mathf.Pow(r,4));
            Force(d * F);
            //            o.Force(d * F * -1);

        }
        public void Gravity(Particle o, float G, float size, float epsilon)
        {
            Vector3 d = this.P - o.P;
            float r = Mathf.Max(d.magnitude, epsilon);
            d = d.normalized;
            if (r < size)
            {
                //o.P = this.P - d * size * 1.001f;
                float scale = 0.99f;
                V *= scale;
                o.V *= scale;
                T += o.V.magnitude*0.1f;
                o.T += V.magnitude * 0.1f;
            }

            float P = -G * mass * o.mass;
            float F = P / (r * r);//  - 0.1f*P/(Mathf.Pow(r,4));
            Force(d * F);
            o.Force(d * F * -1);

        }
        public void Force(Vector3 f)
        {
            A = A + f / mass;
        }
    }
    public class ParticleTree
    {

        public ParticleNode nodes;
        public Vector3 bmin, bmax;
        public Particles particles;

        public ParticleTree(Particles p)
        {
            particles = p;
        }

        public void Visualize(string name, bool showInternal)
        {
            Util.DestroyGameObject(name);
            GameObject parent = new GameObject(name);
            Material mat = (Material)Resources.Load("OctMaterial");
            nodes.Visualize(parent, mat, showInternal);
        }
        public void Update()
        {
            FindBounds();
            if (nodes == null)
                nodes = new ParticleNode();
            nodes.Init(bmin, bmax);
            foreach (Particle p in particles.particles)
            {
                nodes.InsertParticle(p);
            }
        }
        public void FindBounds()
        {
            bmin = Vector3.one * 1E20f;
            bmax = Vector3.one * -1E20f;
            foreach (Particle p in particles.particles)
            {
                bmin = Util.Min(p.P, bmin);
                bmax = Util.Max(p.P, bmax);
            }
        }

        public void Gravity(float G, float threshold)
        {
            ParticleNode.avgDepth = 0;
            nodes.CalculatetMass();
            foreach (Particle p in particles.particles)
                nodes.Gravity(p, G, threshold, 2, 1, 0);
            int N = particles.particles.Count;
            Debug.Log("Sum: " + ParticleNode.avgDepth/N + " supposed to be " + (float) Mathf.Log(N));
        }


    }

    public class ParticleNode
    {
        public Vector3 bmin, bmax;
        public Particle occupant = null;
        public int count;
        public Vector3 P = Vector3.zero;
        public float mass = 0;
        public ParticleNode[] children = null;
        public bool hasChildren = false;
        public Vector3 center;
        public List<Particle> list = new List<Particle>();
        public GameObject go;
        public void Init(Vector3 min, Vector3 max)
        {
            bmin = min;
            bmax = max;
            center = (max + min) / 2;
            hasChildren = false;
            mass = 0;
            count = 0;
            P = Vector3.zero;
            occupant = null;
            list.Clear();
//            if (go != null)
  //              GameObject.Destroy(go);
  //          go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //        go.GetComponent<MeshRenderer>().material = (Material)Resources.Load("OctMaterial");
            
        }

        public void CalculatetMass()
        {
            mass = 0;
            P = Vector3.zero;

            foreach (Particle p in list)
            {
                mass += p.mass;
                P = P + p.P * p.mass;
            }
            if (mass != 0)
                P = P / mass;
            else P = center;
            //P = center;
/*            if (go != null)
            {
                go.transform.position = P;
                go.transform.localScale = Vector3.one * mass;
            }*/
            if (hasChildren)    
                foreach (ParticleNode pn in children)
                    pn.CalculatetMass();

        }

        public bool ParticleIsWithin(Particle p)
        {
            if (p.P.x >= bmin.x && p.P.x <= bmax.x &&
                p.P.y >= bmin.y && p.P.y <= bmax.y &&
                p.P.z >= bmin.z && p.P.z <= bmax.z)
                return true;
            return false;
        }

        public static int avgDepth;

        public void Gravity(Particle p, float G, float threshold, float size, float epsilon, int depth)
        {
            if (mass == 0)
            {
                return;
            }

            float s = (bmax - bmin).magnitude;
            float d = (p.P - P).magnitude;
            if (d == 0)
                return;
            //if (Random.value > 0.999)
//                Debug.Log(s + " with d " + d + " so s/d = " + s/d + " and theta " + threshold);


            if (s / d <= threshold || !hasChildren)
            {
                p.Gravity(getP(), mass, G, size, epsilon);
                avgDepth++;//= Mathf.Max(avgDepth, depth);
                return;
            }

            //     if (hasChildren && s / d < threshold)
            if (hasChildren)
            {
                foreach (ParticleNode pn in children)
                    pn.Gravity(p, G, threshold, size, epsilon, depth + 1);
            }

        }

        public Vector3 getP()
        {
            return P;// / (float)count;
        }

        public void InsertParticle(Particle p)
        {
            // Ignore if without
            if (!ParticleIsWithin(p))
                return;

            list.Add(p);

//            mass += p.mass;
 //           count++;
  //          P += p.P;

            if (occupant == null && !hasChildren)
            {
                occupant = p;
                return;
            }

            if (children == null)
            {
                children = new ParticleNode[8];
                for (int i = 0; i < children.Length; i++)
                    children[i] = new LemonSpawn.ParticleNode();
            }
            Vector3 c = center;
            Vector3 c1 = bmin;
            Vector3 c2 = bmax;
            if (!hasChildren)
            {
                hasChildren = true;
                children[0].Init(c1, c);
                children[1].Init(new Vector3(c.x, c1.y, c1.z), new Vector3(c2.x, c.y, c.z));
                children[2].Init(new Vector3(c.x, c.y, c1.z), new Vector3(c2.x, c2.y, c.z));
                children[3].Init(new Vector3(c1.x, c.y, c1.z), new Vector3(c.x, c2.y, c.z));

                children[4].Init(new Vector3(c1.x, c1.y, c.z), new Vector3(c.x, c.y, c2.z));
                children[5].Init(new Vector3(c.x, c1.y, c.z), new Vector3(c2.x, c.y, c2.z));
                children[6].Init(c, c2);
                children[7].Init(new Vector3(c1.x, c.y, c.z), new Vector3(c.x, c2.y, c2.z));
            }
            foreach (ParticleNode pn in children)
            {
                pn.InsertParticle(p);
                if (occupant != null)
                    pn.InsertParticle(occupant);
            }
            occupant = null;

        }


        public void Visualize(GameObject parent, Material m, bool showInternal)
        {
            go = parent;
            if (!hasChildren || showInternal)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = P; //(bmin + bmax) / 2;
                go.transform.position = (bmin + bmax) / 2;
                go.transform.localScale = (bmax - bmin);
                go.transform.parent = parent.transform;
                go.GetComponent<MeshRenderer>().material = m;
            }
            if (hasChildren)
            {
                foreach (ParticleNode pn in children)
                    pn.Visualize(go, m, showInternal);
            }
        }

    }

    public class Particles
    {
        public List<Particle> particles = new List<Particle>();
        public Billboards billboards = new Billboards();

        public float findMass(float r)
        {
            float m = 0;
            foreach (Particle p in particles)
                if (p.P.magnitude < r)
                    m += p.mass;
            return m;
        }

        public void setEscapeVelocity(float G)
        {
            foreach (Particle p in particles)
            {
                float r = p.P.magnitude;
                float m = findMass(0.5f*r);
                float v = Mathf.Sqrt( G * m/r );
                Vector3 dir = Vector3.Cross(new Vector3(0, 1, 0), p.P);
                p.V = v * dir.normalized;
            }
        }

        public void UpdateMesh()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                billboards.billboards[i].pos = particles[i].P;
                float T = particles[i].T * 0.1f;
                billboards.billboards[i].color = new Vector3(1, T * 0.5f, T);
            }
            billboards.UpdateMesh();

        }
        public void Move(float dt)
        {
            foreach (Particle p in particles)
                p.Move(dt);
        }

        public void Create(int N, Vector3 size, Vector3 V, float bsize)
        {
            particles.Clear();
            for (int i = 0; i < N; i++)
            {
                Particle p = new Particle(Util.randomVector(size.x, size.y, size.z) - size * 0.5f,
                    Util.randomVector(V.x, V.y, V.z) - V * 0.5f, 1);
                //p.mass = 100 / (p.P.magnitude + 1);
                particles.Add(p);
                
            }
//            Particle center = new Particle(Vector3.zero, Vector3.zero, 10);
  //          particles.Add(center);

            Createbillboards(bsize);
        }




        public void Createbillboards(float bsize)
        {

            billboards.billboards.Clear();
            Vector3 color = new Vector3(1, 0.5f, 0.2f);
            Vector2 bSize = new Vector2(bsize, 0);
            foreach (Particle p in particles)
            {
                Billboard b = new Billboard(p.P, bSize, color);
                billboards.billboards.Add(b);
            }
            GameObject go = billboards.Realize("mesh", (Material)Resources.Load("BillboardStar"), 1);
        }

        public void GravityDirectSum(float G)
        {
            float size = 2;
            float epsilon = 1f;
            for (int i = 0; i < particles.Count; i++)
            {

                for (int j = i + 1; j < particles.Count; j++)
                {
                    particles[i].Gravity(particles[j], G, size, epsilon);


                }
            }
        }
    }



    public class MainSim : MonoBehaviour
    {

        private CameraRotator camRot;
        private Particles particles = new Particles();
        private float deltaTime = 0.1f;
        private bool abortThread = false;
        private ParticleTree tree;
        Thread thread;
        private float G = 0.6f;
        // Use this for initialization
        void Start()
        {
            camRot = new CameraRotator(GameObject.Find("MainCam").GetComponent<Camera>());
            float s = 250;
            float v = 1;
            particles.Create(100, new Vector3(s, 0.1f, s), new Vector3(v, 0.1f, v), 15);

            particles.setEscapeVelocity(G);
            thread = new Thread(new ThreadStart(ThreadedUpdate));
            thread.Start();
            tree = new ParticleTree(particles);
        }


        void ThreadedUpdate()
        {
            while (!abortThread)
            {
                //                particles.Gravity(1000);
                tree.Update();
                tree.Gravity(G, 4.0f);
                particles.Move(deltaTime);
                //
            }
        }


        private void OnApplicationQuit()
        {
            abortThread = true;
            thread.Abort();


        }
        // Update is called once per frame
        void Update()
        {
            camRot.UpdateCamera();
            particles.UpdateMesh();
            deltaTime = Time.deltaTime;

//            tree.Update();
  //         tree.Gravity(G, 4.0f);
          //  particles.GravityDirectSum(G);
    //        particles.Move(deltaTime);
           
            //particles.GravityDirectSum(G);
                tree.Visualize("tree", true);
        }
    }

}