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
        static public Texture2D GetRTPixels(RenderTexture rt,  Texture2D tex )
        {
            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            // Create a new Texture2D and read the RenderTexture image into it
            if (tex==null)
               tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

                // Restorie previously active render texture
            RenderTexture.active = currentActiveRT;
            return tex;
        }


        public static Texture2D createSolidTexture(Color c)
        {
            Texture2D t = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        public static Texture2D createFrameTexture(Color cf, Color cb, int size, int margin)
        {
            Texture2D t = new Texture2D(size, size, TextureFormat.ARGB32, false);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Color c = cb;
                    if (i <= margin || i > size - 1 - margin) c = cf;
                    if (j <= margin || j > size - 1 - margin) c = cf;
                    t.SetPixel(i, j, c);

                }
            }
            t.Apply();
            return t;
        }

    }
}




