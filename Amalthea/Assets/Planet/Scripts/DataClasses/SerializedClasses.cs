using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using UnityEngine.UI;

namespace LemonSpawn
{
    [System.Serializable]
    public class Frame
    {
        public int id;
        public double rot_x, rot_y, rot_z;
        public double pos_x;
        public double pos_y;
        public double pos_z;
        public double time;
        public float visible = 1;
        public float scale_x=1, scale_y=1, scale_z=1;
        public float color_r = 1, color_g = 1, color_b = 1;

        public string displayMessage = "";
        public string sound = "";
        public DVector pos()
        {
            return new DVector(pos_x, pos_y, pos_z);
        }

        public Vector3 color()
        {
            return new Vector3(color_r, color_g, color_b);
        }

        public Vector3 scale()
        {
            return new Vector3(scale_x, scale_y, scale_z);
        }

        public Vector3 rot()
        {
            return new Vector3((float)rot_x, (float)rot_y, (float)rot_z);
        }

    }

    [System.Serializable]
    public class SerializedMCAstObject
    {
        // General properties
        public string name;
        public string category;

        // Planet/star properties
        public float outerRadiusScale = 1.05f;
        public float radius = 5000; // Radius of planets
        public int seed = 0;
//        public int parentSeed = 0;
        public double pos_x, pos_y, pos_z;
        public double rot_x, rot_y, rot_z;
        public float temperature = 200;
        public float atmosphereDensity = 1;
        public float autoOrient = 1;


        public List<Frame> Frames = new List<Frame>();

        // Object properties
        public string objectString = "";
        public string objectMaterial = "";
        public float objectScale = 1;
        public float color_r = 0;
        public float color_g = 0;
        public float color_b = 0;


        public string forcePlanetSurface = "";

        public Color getColor()
        {
            if (color_r == 0 && color_g == 0 && color_b == 0)
                return Color.white;
            return new Color(color_r, color_g, color_b, 1.0f);
        }



        // Child list

        public List<SerializedMCAstObject> Objects;


        public PlanetSettings DeSerialize(GameObject g, float radiusScale)
        {
            PlanetSettings ps = g.AddComponent<PlanetSettings>();
            ps.outerRadiusScale = outerRadiusScale;
            //ps.transform.position.Set (pos_x, pos_y, pos_z);
            ps.properties.pos.x = pos_x;
            ps.properties.pos.y = pos_y;
            ps.properties.pos.z = pos_z;
            ps.properties.serializedPlanet = this;
            ps.rotation = rot_y % (2.0 * Mathf.PI);
            ps.temperature = temperature;
            ps.seed = seed;
            ps.properties.Frames = Frames;
            ps.radius = radius;
            if (RenderSettings.logScale) ps.radius = Mathf.Pow(ps.radius,RenderSettings.powScale);
            ps.radius *= radiusScale;

            ps.properties.orgPos.Set(ps.properties.pos);
//            ps.atmosphereDensity = Mathf.Clamp(atmosphereDensity, 0, RenderSettings.maxAtmosphereDensity);
            //	ps.atmosphereHeight = atmosphereHeight;
            foreach (Frame f in Frames)
                f.rot_y = f.rot_y % (2.0 * Mathf.PI);
//            if (planetType == null)
//                planetType = "";

            if (category == "star")
                ps.category = PlanetSettings.Categories.Star;
            if (category == "moon")
                ps.category = PlanetSettings.Categories.Moon;
            if (category == "3dobject")
                ps.category = PlanetSettings.Categories.Object3D;
            if (category == "planet")
                ps.category = PlanetSettings.Categories.Planet;
            if (category == "black hole")
                ps.category = PlanetSettings.Categories.BlackHole;
            if (category == "explosion")
                ps.category = PlanetSettings.Categories.Explosion;

            /*            if (planetType.ToLower() == "star" || planetType.ToLower() == "spacecraft")
                        {
                            ps.planetTypeName = planetType;
                        }
                        else*/
            if (ps.category == PlanetSettings.Categories.Moon || ps.category == PlanetSettings.Categories.Planet)
                ps.Randomize();
    
            return ps;
        }

        public SerializedMCAstObject()
        {

        }

        public SerializedMCAstObject(PlanetSettings ps)
        {
            outerRadiusScale = ps.outerRadiusScale;
            radius = ps.radius;
            pos_x = ps.properties.pos.x;
            pos_y = ps.properties.pos.y;
            pos_z = ps.properties.pos.z;
            temperature = ps.temperature;
            rot_y = ps.rotation;
            seed = ps.seed;
            atmosphereDensity = ps.atmosphereDensity;
            Frames = ps.properties.Frames;
            //planetType = ps.planetTypeName;
            //if (ps.properties.parentPlanet != null)
            //    parentSeed = ps.properties.parentPlanet.pSettings.seed;

        }


    }


    /*	public class PlanetType {
            public string Name;
            public string CloudTexture;
        }
    */

    [System.Serializable]
    public class SerializedCamera
    {
        public double cam_x, cam_y, cam_z;
        //		public float rot_x, rot_y, rot_z;
        //		public float cam_theta, cam_phi;
        public double dir_x, dir_y, dir_z;
        public double up_x, up_y, up_z;
        public double scale_x, scale_y, scale_z;
        public double fov;
        public double time;
        public int frame;
        public string displayMessage = "";
        public int status = 0;
        public DVector getPos()
        {
            return new DVector(cam_x, cam_y, cam_z);
        }
        public DVector getUp()
        {
            return new DVector(up_x, up_y, up_z);
        }
        public DVector getDir()
        {
            return new DVector(dir_x, dir_y, dir_z);
        }
    }



    [System.Serializable]
    public class SerializedWorld
    {
        public List<SerializedMCAstObject> Objects = new List<SerializedMCAstObject>();
        public List<SerializedCamera> Cameras = new List<SerializedCamera>();
        public bool hasLengthContraction = false;
        public float sun_intensity = 0.1f;
        public float resolutionScale = 1.0f;
        public float global_radius_scale = 1;
        private int frame = 0;
        public float skybox = 0;
        public string uuid;
        public float displayTime = 1;
        public int EnvQuadLevel = 5;
        public int resolution = 64;
        public bool useSpaceCamera = true;
        public float overview_distance = 4;
        public int screenshot_width = 1024;
        public int screenshot_height = 1024;
        public int maxFrames = 0;
        public float defaultPlaySpeed = 0.6f;

        public float currentTime = 0;
        public float rulerStart = 0;
        public float rulerEnd = 0;
        public float rulerTicks;
        public string rulerUnit = "";

        public string clockUnit = "s";

        public bool isVideo()
        {
            if (Cameras.Count > 1)
                return true;
            return false;
        }




        public void SaveSerializedWorld(string filename, SolarSystem s, string _uuid)
        {
            Objects.Clear();
            foreach (Planet p in s.planets)
            {
                Objects.Add(new SerializedMCAstObject(p.pSettings));
            }
            uuid = _uuid;
            Serialize(this, filename);
        }


        public SerializedCamera getCamera(int i)
        {
            if (i >= 0 && i < Cameras.Count)
                return Cameras[i];
            if (i < 0)
                return null;
            if (i >= Cameras.Count)
                return Cameras[Cameras.Count - 1];
            return null;
        }


        public SerializedCamera getCamera(double t, int add)
        {

           
            for (int i = 0; i < Cameras.Count-1; i++)
            {
                if (t>=Cameras[i].time && t<Cameras[i+1].time)
                    return getCamera(i + add );
            }
            return null;
        }
        public float getCameraIndex(double t, int add) {
            for (int i = 0; i < Cameras.Count - 1; i++)
            {
                if (t >= Cameras[i].time && t < Cameras[i + 1].time)
                    return i + add;
            }
            return 0;
                }

/*        public SerializedCamera getInterpolatedCameraSpline(double t, List<Planet> planets)
        {
            // t in [0,1]
            if (Cameras.Count <= 1)
                return null;
            DVector pos, up;
            up = new DVector(Vector3.up);

            //			float n = t*(Cameras.Count-1);

            double maxTime = Cameras[Cameras.Count - 1].time;
            double time = t * maxTime;


            //			SerializedCamera a = getCamera(n-1);
            SerializedCamera p0 = getCamera(time, -1);
            SerializedCamera p1 = getCamera(time, 0);
            SerializedCamera p2 = getCamera(time, 1);
            SerializedCamera p3 = getCamera(time, 2);


            if (p2 == null || p1 == null || p0==null)
            {
                return p1;
            }

            double dt = 1.0 / (p2.time - p1.time) * (time - p1.time);
           
       

            pos = Util.CatmullRom(dt, p0.getPos(), p1.getPos(), p2.getPos(), p3.getPos());
            up = Util.CatmullRom(dt, p0.getUp(), p1.getUp(), p2.getUp(), p3.getUp());
            DVector dir = Util.CatmullRom(dt, p0.getDir(), p1.getDir(), p2.getDir(), p3.getDir());


            foreach (Planet p in planets)
            {
                p.InterpolatePositions(p1.frame, dt);
            }

            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, dir.toVectorf(), up.toVectorf());
            return p1;
        }
*/
        public int currentFrame;
        public bool isNewFrame = false;

        public void InterpolatePlanetFrames(double t, List<Planet> pl)
        {

            int totalFrames = maxFrames;
            if (totalFrames < 2)
            {
                foreach (Planet p in pl)
                {
                    p.pSettings.properties.pos = p.pSettings.properties.orgPos;
                   // if (p.pSettings.properties.parentPlanet != null)
                   //     p.pSettings.properties.pos -= p.pSettings.properties.parentPlanet.pSettings.properties.pos;
  //                  Debug.Log(p.pSettings.properties.pos.toVectorf().x);
                }
                return;
            }
            int frame = (int)(totalFrames*t);
            if (currentFrame == frame)
                isNewFrame = false;
            else
                isNewFrame = true;

            currentFrame = frame;
            double dt = (totalFrames*t - frame);

            foreach (Planet p in pl)
            {
                p.InterpolatePositions(frame, dt, isNewFrame);
//                Debug.Log(p.pSettings.properties.pos.toVectorf());
            }


        }



        public SerializedCamera getInterpolatedCamera(double t, List<Planet> planets)
        {
            // t in [0,1]
            if (Cameras.Count <= 1)
                return null;
            DVector pos, up;
            up = new DVector(Vector3.up);


            double maxTime = Cameras[Cameras.Count - 1].time;
            double time = t * maxTime;

            SerializedCamera b = getCamera(time, 0);
            SerializedCamera c = getCamera(time, 1);
            if (c == null)
                return b;

            double dt = 1.0 / (c.time - b.time) * (time - b.time);

            pos = b.getPos() + (c.getPos() - b.getPos()) * dt;
            up = b.getUp() + (c.getUp() - b.getUp()) * dt;
            currentTime = (float)(b.time + (c.time - b.time) * dt);

            RenderSettings.fov = (float)(b.fov + (c.fov - b.fov) * dt);

            DVector dir = b.getDir() + (c.getDir() - b.getDir()) * dt;

            //            Debug.Log("INTERPOLATE");
            if (b.frame == currentFrame)
                isNewFrame = false;
            else
                isNewFrame = true;
            foreach (Planet p in planets)
            {
                p.InterpolatePositions(b.frame, dt, isNewFrame);
            }




            World.MainCamera.GetComponent<SpaceCamera>().SetLookCamera(pos, dir.toVectorf(), up.toVectorf());
            return b;
        }
        
        public void IterateCamera()
        {

            if (frame >= Cameras.Count)
                return;

            //Debug.Log("JAH");

            SerializedCamera sc = Cameras[frame];
            //gc.GetComponent<SpaceCamera>().SetCamera(new Vector3(sc.cam_x, sc.cam_y, sc.cam_z), Quaternion.Euler (new Vector3(sc.rot_x, sc.rot_y, sc.rot_z)));
            DVector up = new DVector(sc.up_x, sc.up_y, sc.up_z);
            DVector pos = new DVector(sc.cam_x, sc.cam_y, sc.cam_z);
			SpaceCamera spc = World.MainCamera.GetComponent<SpaceCamera>();
            if (spc!=null)
            	spc.SetLookCamera(pos, sc.getDir().toVectorf(), up.toVectorf());



            //c.fieldOfView = sc.fov;



            //Atmosphere.sunScale = Mathf.Clamp(1.0f / (float)pos.Length(), 0.0001f, 1);
            frame++;
        }


        public SerializedWorld()
        {

        }


        public static SerializedWorld DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
            TextReader textReader = new StreamReader(filename);
            SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(SerializedWorld sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedWorld));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }

		public static SerializedWorld DeSerializeString(string data)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(SerializedWorld));
            //TextReader textReader = new StreamReader(filename);
            StringReader sr = new StringReader(data);
            SerializedWorld sz = (SerializedWorld)deserializer.Deserialize(sr);
            sr.Close();
            return sz;
        }

    }


	[System.Serializable]
    public class MCAstSettings
    {
        public static int[,] Resolution = new int[11, 2] { 
            { 320, 200 }, { 640, 480 }, { 800, 600 }, { 1024, 768 }, { 1280, 1024 }, { 1600, 1200 },
            { 800, 480 }, { 1024, 600 }, { 1280, 720 }, { 1680, 1050 }, { 2048, 1080 } };

        public static int[] GridSizes = new int[6] { 16, 32, 48, 64, 80, 96 };


        public int movieResolution = 1;
        public int gridSize = 1;
        public int screenShotResolution = 4 ;
        public bool advancedClouds = false;
        public bool cameraEffects = true;
        public string previousFile = "";
        public bool PerPixelShading = true;
        public bool UseGPURenderer = true;

        public static MCAstSettings DeSerialize(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(MCAstSettings));
            TextReader textReader = new StreamReader(filename);
            MCAstSettings sz = (MCAstSettings)deserializer.Deserialize(textReader);
            textReader.Close();
            return sz;
        }
        static public void Serialize(MCAstSettings sz, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MCAstSettings));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, sz);
            textWriter.Close();
        }
    } 


}