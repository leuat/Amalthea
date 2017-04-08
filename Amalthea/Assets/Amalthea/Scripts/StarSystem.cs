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
        public List<Planet> planets = new List<Planet>();

        void Generate()
        {
            System.Random rnd = new System.Random(seed);

            
        
        }



    }
}