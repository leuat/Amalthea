﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;


namespace Amalthea
{


    [System.Serializable]
    public class ResourceType : BaseCollection
    {
        public Color color = Color.white;
        public ResourceType() { }
        public ResourceType(string n, Color c)
        {
            name = n;
            color = c;
        }
    }

    [System.Serializable]
    public class Resource
    {
        public ResourceType resourceType;
        public float amount;

        public Resource(ResourceType rt, float v)
        {
            resourceType = rt;
            amount = v;
        }
    }

    [System.Serializable]
    public class MinableResource
    {
        public Resource resource;
        public Vector3 position = Vector3.zero;
    }

    [System.Serializable]
    public class CityType : BaseCollection
    {
        public Color color = Color.yellow;
        public CityType(string n, Color c)
        {
            name = n;
            color = c;
        }
        public CityType() { }
    }

    [System.Serializable]
    public class City
    {
        public int seed;
        public Vector3 position;
        public string name;
        public float level;
        public CityType cityType;


    }

    [System.Serializable]
    public class TypeCollection<T> where T : BaseCollection
    {
        public List<T> list = new List<T>();
        public T Get(string n)
        {
            foreach (T t in list)
                if (t.name == n)
                    return t;
            return null;
        }

    }

    [System.Serializable]
    public class Definitions
    {
        public TypeCollection<CityType> cityTypes = new TypeCollection<CityType>();
        public TypeCollection<ResourceType> resourceTypes = new TypeCollection<ResourceType>();
        public TypeCollection<StellarCategory> stellarCategories = new TypeCollection<StellarCategory>();

        public void InitTemp()
        {
            cityTypes.list.Add(new CityType("Civic", new Color(1, 1, 0.4f)));
            resourceTypes.list.Add(new ResourceType("Iron", new Color(0.5f, 0.5f, 0.5f)));

            stellarCategories.list.Add(new StellarCategory("Moon", new Color(0.3f, 0.5f, 1.0f, 0.5f), 1f));
            stellarCategories.list.Add(new StellarCategory("Planet", new Color(1.0f, 0.8f, 0.3f, 0.5f), 1.2f));
            stellarCategories.list.Add(new StellarCategory("Star", new Color(1.0f, 0.3f, 0.3f, 0.5f), 1.2f));
        }

        public static Definitions DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Definitions));
            TextReader textReader = new StreamReader(filename);
            Definitions sz = (Definitions)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(Definitions sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Definitions));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, (Definitions)sz);
            textWriter.Close();
        }

    }

    public class Globals
    {
        public static Definitions definitions = new Definitions();

        public static void Initialize()
        {
            definitions = (Definitions)Definitions.DeSerialize("definitions.xml");
        }
        public static void Save()
        {
            definitions.InitTemp();
            Definitions.Serialize(definitions, "definitions.xml");
        }

    }
    [System.Serializable]
    public class StellarCategory : BaseCollection
    {
        public Color color;
        public float width;
        public StellarCategory() { }
        public StellarCategory(string n, Color c, float w)
        {
            name = n;
            color = c;
            width = w;
        }
    }


    [System.Serializable]
    public class SerializedPlanet
    {
        public int seed;
        public Vector3 position = Vector3.zero;
        public float rotation = 0;
        public float radius;
        public string planetTypeName;
    }

    public class Planet
    {
        public SerializedPlanet serializedPlanet;
        public LemonSpawn.Planet lsPlanet;
        public LemonSpawn.PlanetType planetType;
        public StellarCategory stellarCategory;
    }


    [System.Serializable]
    public class StarSystem
    {
        public int seed;
        public int id;
        public Vector3 position = Vector3.zero;
        public Color color;
        public float temperature;
        public float radius;
        private string name = "";


        public float mass;
        public float noPlanets;
        float density;

        public string getName()
        {
            if (name == "")
                name = LemonSpawn.Util.getRandomName(new System.Random(seed), "Kvorsk");
            return name;
        }

        public StarSystem(int i, int sd, Vector3 p,float t, float r, float pNoPlanets)
        {
            radius = r;
            seed = sd;
            id = i;
            position = p;
            temperature = t;
            radius = r;
            noPlanets = pNoPlanets;
            color = LemonSpawn.Constants.colorTemperatureToRGB(t);
            name = "";
            density = 1400;
            mass =  (float)(4 / 3.0 * Mathf.PI * density * Mathf.Pow((float)LemonSpawn.Constants.sunR*radius, 3) * Mathf.Pow(1000, 3)/LemonSpawn.Constants.massSun);
        }

        public string getCategory()
        {
            string s = "";
            if (temperature <= 2000)
                s = s + "cool ";
            if (temperature > 8000)
                s = s + "hot ";

            if (mass<0.001)
            {
                s += "brown dwarf";
            }
            if (mass >= 0.001 && mass<0.5)
            {
                if (temperature < 4500) 
                    s += "red dward";
                else
                    s += "white dwarf";
            }
            if (mass >= 0.5 && mass < 5)
            {
                if (radius > 8)
                    s+= "giant";
                else
                    s += "main sequence star";

            }
            if (mass >= 5 )
            {
               s += "supergiant";
            }

            return s;
    }


        public static LemonSpawn.LSMesh CreateMesh(string name, List<StarSystem> list, float val1, float val2, System.Random rnd, Material m, Color overrideColor, int layer)
        {
            LemonSpawn.LSMesh lsMesh = new LemonSpawn.LSMesh();
            int i = 0;
            Vector3 oc = new Vector3(overrideColor.r, overrideColor.g, overrideColor.b);
            Vector3 min = Vector3.one * 1E20f;
            Vector3 max = Vector3.one * -1E20f;
            foreach (StarSystem s in list)
            {
                lsMesh.vertexList.Add(s.position);
                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                if (oc.magnitude==0)
                lsMesh.normalList.Add(new Vector3(s.color.r, s.color.g, s.color.b));
                else
                    lsMesh.normalList.Add(oc);

                float size = s.radius * val1 + (float)rnd.NextDouble() * val2;
                max = LemonSpawn.Util.Max(s.position + Vector3.one * size, max);
                min = LemonSpawn.Util.Max(s.position - Vector3.one * size, min);

                lsMesh.uvList.Add(new Vector2(size, 0));
                lsMesh.tangentList.Add(Vector4.zero);
                i++;
            }
            lsMesh.createMesh(false);
            lsMesh.mesh.name = "mesh";
            lsMesh.mesh.bounds = new Bounds((max + min) * 0.5f, (max - min));
         //   markingMaterial = (Material)Resources.Load("StarMarkingMaterial");

            LemonSpawn.Util.DestroyGameObject(name);

            GameObject go = lsMesh.Realize(name, m, layer, "normal", false);
            go.layer = layer;
            return lsMesh;

        }
    }

    [System.Serializable]
    public class Galaxy
    {
        public int seed;
        public List<StarSystem> stars = new List<StarSystem>();
        public Mesh mesh;
        public LemonSpawn.LSMesh lsMesh;

        public LemonSpawn.LSMesh lsMeshNebulae;

        public GameObject galaxyGO;
        public Material galaxyMaterial;
        public Material nebulaeMaterial;
        public int category = 0;



        public void GenerateNebulae(int no, System.Random rnd, int lyWidth, int ba, int width)
        {
//            lsMeshNebulae = new LemonSpawn.LSMesh();
            List<StarSystem> ls = new List<StarSystem>();
            for (int i=0;i<no;i++) {
                Vector3 pos = LemonSpawn.Util.randomVector(rnd, lyWidth, lyWidth, lyWidth) - Vector3.one * lyWidth / 2;
                StarSystem ss = new StarSystem(i, 1, pos, 1, 1, 0);
                ss.color = new Color(1-(float)rnd.NextDouble()*0.5f, 1-(float)rnd.NextDouble() * 0.5f, 1-(float)rnd.NextDouble() * 0.5f)*(0.1f+ 0.35f*(float)rnd.NextDouble());
                ls.Add(ss);
            }

            nebulaeMaterial = (Material)Resources.Load("NebulaeMaterial");
            lsMeshNebulae = StarSystem.CreateMesh("nebulae", ls, ba, width, rnd, nebulaeMaterial, Color.black,12);


        
    }


        public void Generate(int no, int se, float lyWidth)
        {
            seed = se;
            System.Random rnd = new System.Random(seed);
            stars.Clear();
            // Always one in zero
            stars.Add(new StarSystem(0, rnd.Next(), Vector3.zero, 3800, 1, 9));
            for (int i = 0; i < no - 1; i++)
            {
                int noPlanets = 2 + rnd.Next() % 9;
                float starT = Mathf.Pow((float)rnd.NextDouble(), 2);
                float starR = Mathf.Pow((float)rnd.NextDouble(), 4);
                StarSystem s = new StarSystem(i+1, rnd.Next(), LemonSpawn.Util.randomVector(rnd, lyWidth, lyWidth, lyWidth) - Vector3.one * lyWidth / 2,
                     2000 +starT* 12000f, 0.1f + starR * 10f, noPlanets);
                stars.Add(s);
            }
            galaxyMaterial = (Material)Resources.Load("StarMaterial");
            StarSystem.CreateMesh("galaxy", stars, 0.5f, 0, rnd, galaxyMaterial, Color.black,12);
            GenerateNebulae(2000, rnd, 4000, 10, 350);
        }



        public List<StarSystem> FindLocalSystems(Vector3 point, float radiusLy)
        {
            List<StarSystem> l = new List<StarSystem>();
            foreach (StarSystem s in stars)
            {
                if ((s.position - point).magnitude < radiusLy)
                    l.Add(s);

            }
            return l;
        }



    }


    class APlayer {
        int galaxySeed;
        public Galaxy galaxy = new Galaxy();
        public List<StarSystem> knownSystems = new List<StarSystem>();
        public List<StarSystem> localSystems = new List<StarSystem>();
        private Material markingMaterial = null;
        private Material sunGlareMaterial = null;
        private StarSystem currentSystem;

        public void Update(Vector3 currentPos, Vector3 up)
        {
            localSystems = galaxy.FindLocalSystems(currentPos, 500f);
            galaxy.galaxyMaterial.SetVector("upVector", up);
            if (galaxy.nebulaeMaterial)
            galaxy.nebulaeMaterial.SetVector("upVector", up);
            if (markingMaterial != null)
               markingMaterial.SetVector("upVector", up);
            if (sunGlareMaterial)
                sunGlareMaterial.SetVector("upVector", up);

        }

        public void AddToKnown(StarSystem s)
        {
            if (!knownSystems.Contains(s))
                knownSystems.Add(s);
        }


        public void InterStellar()
        {
            if (markingMaterial == null)
                markingMaterial = (Material)Resources.Load("StarMarkingMaterial");
             StarSystem.CreateMesh("knownMarker", knownSystems, 10,0, new System.Random(), markingMaterial, Color.cyan,12);

            LemonSpawn.Util.DestroyGameObject("sunGlare");
        
        }

        private void CreateSunGlare()
        {
            Color color = 0.6f * currentSystem.color;
            color.a = 1;
            List<StarSystem> ls = new List<StarSystem>();
            StarSystem s1 = new StarSystem(0, 0, Vector3.zero, 1, currentSystem.radius, 1);
            s1.color = color;
            ls.Add(s1);
            StarSystem s2 = new StarSystem(0, 0, Vector3.zero, 1, currentSystem.radius*10, 1);
            s2.color = color * 0.3f;
            ls.Add(s2);
            sunGlareMaterial = (Material)Resources.Load("StarGlow");
            StarSystem.CreateMesh("ASunGlare", ls, 2000, 0, new System.Random(), sunGlareMaterial, Color.black,0);
        }

        public void InterPlanetary(StarSystem newSystem)
        {
            currentSystem = newSystem;
            LemonSpawn.Util.DestroyGameObject("knownMarker");
            CreateSunGlare();
        }

        


    }

}