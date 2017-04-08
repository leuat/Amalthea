using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Amalthea {

    [System.Serializable]
    public class BaseCollection
    {
        public string name;
    }

    [System.Serializable]
    public class XMLSerializable
    {
        public static XMLSerializable DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(XMLSerializable));
            TextReader textReader = new StreamReader(filename);
            XMLSerializable sz = (XMLSerializable)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(XMLSerializable sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLSerializable));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, (XMLSerializable)sz);
            textWriter.Close();
        }

    }




    public class Util {
       
        
    }
}




