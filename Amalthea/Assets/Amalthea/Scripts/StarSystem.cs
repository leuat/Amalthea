using System.Collections;
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

            stellarCategories.list.Add(new StellarCategory("Moon", new Color(0.3f, 0.5f, 1.0f , 0.5f), 1f));
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
        public Vector3 position = Vector3.zero;
        public Color color;
        public float temperature;
        public float radius;
        public string name;
        public StarSystem(int s, Vector3 p, Color c, float t, float r, string n)
        {
            radius = r;
            seed = s;
            position = p;
            temperature = t;
            radius = r;
            color = c;
            name = n;
        }

    }

    [System.Serializable]
    public class Galaxy
    {
        public int seed;
        public List<StarSystem> stars = new List<StarSystem>();
        public Mesh mesh;
        public LemonSpawn.LSMesh lsMesh;
        public void Generate(int no, int se, float lyWidth)
        {
            seed = se;
            System.Random rnd = new System.Random(seed);
            stars.Clear();
            Color c = new Color();
            // Always one in zero
            stars.Add(new StarSystem(rnd.Next(), Vector3.zero, new Color(1.0f, 0.9f, 0.7f), 5800, 1, LemonSpawn.Util.getRandomName(rnd)));
            for (int i=0;i<no-1;i++)
            {
                c.r = 0.5f + (float)(rnd.NextDouble() * 0.5);
                c.b = 0.5f + (float)(rnd.NextDouble() * 0.5);
                c.g = Mathf.Max(c.r, 0.5f + (float)(rnd.NextDouble() * 0.5));
                StarSystem s = new StarSystem(rnd.Next(), LemonSpawn.Util.randomVector(rnd, lyWidth, lyWidth, lyWidth) - Vector3.one * lyWidth / 2,
                    c, 5800, 0.1f + (float)rnd.NextDouble() * 2f, LemonSpawn.Util.getRandomName(rnd));
                stars.Add(s);
            }
            CreateMesh();            
        }

        public void CreateMesh()
        {
            lsMesh = new LemonSpawn.LSMesh();
            int i = 0;
            foreach (StarSystem s in stars)
            {
                lsMesh.vertexList.Add(s.position);
                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                lsMesh.faceList.Add(i);
                lsMesh.normalList.Add(new Vector3(s.color.r, s.color.g, s.color.b));
                lsMesh.uvList.Add(new Vector2(s.radius,0));
                lsMesh.tangentList.Add(Vector4.zero);
                i++;
            }
            lsMesh.createMesh(false);
            Debug.Log(lsMesh.mesh.vertexCount);
            mesh = lsMesh.mesh;
            mesh.name = "GalaxyMesh";

            GameObject go = lsMesh.Realize("Galaxy", (Material)Resources.Load("StarMaterial"), 0, "normal", false);
            go.layer = 12;
            
        }



    }
}