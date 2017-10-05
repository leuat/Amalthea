using UnityEngine;


namespace LemonSpawn
{

    [System.Serializable]
    public class SerializedCRTSettings
    {
        [Range(0, 2048)]
        public float pixels = 320f;
        [Range(-1, 1)]
        public float radialDistort = 1.0f;
        [Range(0, 2f)]
        public float vignetteDistance = 0.2f;
        [Range(0, 1)]
        public float tintStrength = 0.5f;
        [Range(0, 1)]
        public float pixelBlend = 0.5f;
        [Range(0, 8)]
        public float gamma = 1f;
        [Range(-2, 2)]
        public float correction = 0;
        [Range(0, 1)]
        public float tint_r;
        [Range(0, 1)]
        public float tint_g;
        [Range(0, 1)]
        public float tint_b;
        [Range(0, 1)]
        public float monoRange = 0.5f;
        [Range(0, 1)]
        public float monoStrength = 0;
        public int monoType = 0;


        public string settings_id;


        public Color tint()
        {
            return new Color(tint_r, tint_g, tint_b);
        }

        //    [Range(0, 1)]
    }
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]

    /*    public class MonoColor
        {
            public Color[] colors;
            public MonoColor(Color[] c)
            {
                colors = c;
            }
        }
        */
    public class CRTScreen : MonoBehaviour
    {
        public Shader shader;
        private Material _material;
        private int pixelWidth = -1;
        //        public Color tint = Color.white;
        public SerializedCRTSettings settings = new SerializedCRTSettings();


        public static Vector3[] MonoColorsCGA1 = new Vector3[]
        { new Vector3(0,0,0), new Vector3(1, 1, 1), new Vector3(1,0,1), new Vector3(0,1,1), new Vector3(1,0,1) };
        public static Vector3[] MonoColorsCGA2 = new Vector3[]
        { new Vector3(0,0,0), new Vector3(1, 1, 0), new Vector3(0,1,0), new Vector3(1,0,0), new Vector3(0,1,0) };
        public static Vector3[] MonoColorsGameBoy = new Vector3[]
        { new Vector3(15f/255f,56f/255f,15/255f), new Vector3(155/255f, 188/255f, 15/255f),
            new Vector3(48/255f,98/255f,48/255f), new Vector3(139/255f,172/255f,15/255f), new Vector3(48f/255f,98/255f,48/255f) };

        public static Vector3[][] MonoColors = new Vector3[][] { MonoColorsCGA1, MonoColorsCGA2, MonoColorsGameBoy };



        protected Material material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }

        private int FindPixelWidth()
        {
            if (Screen.width < 1024)
                return 1;
            if (Screen.width < 1600)
                return 2;

            return 2;
            if (Screen.width < 2048)
                return 4;
            if (Screen.width < 3000)
                return 8;
            if (Screen.width < 5000)
                return 16;

            return 1;

         }



        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (shader == null) return;
            if (pixelWidth == -1)
                pixelWidth = FindPixelWidth();


            Material mat = material;
            mat.SetFloat("_pixels", settings.pixels);
            mat.SetFloat("_pixelsColorX", Screen.width / pixelWidth);//  pixelsColor);
            mat.SetFloat("_pixelsColorY", Screen.height / pixelWidth);//  pixelsColor);
            mat.SetFloat("_radialDistort", settings.radialDistort);
            mat.SetFloat("_vignetteDistance", settings.vignetteDistance);
            mat.SetFloat("_pixelBlend", settings.pixelBlend);
            mat.SetColor("_tint", settings.tint());
            mat.SetFloat("_tintStrength", settings.tintStrength);
            mat.SetFloat("_gamma", settings.gamma);
            mat.SetFloat("_correction", settings.correction);
            mat.SetFloat("_monoRange", settings.monoRange);
            mat.SetFloat("_monoStrength", settings.monoStrength);

//          mat.SetFloat("_line", Mathf.Sin(Time.time * 1.2341f)*0.01f);
//            mat.SetFloat("_line", Random.value*0.001f);
            mat.SetFloat("_line", 0.001f);

            Vector3[] cols = MonoColors[settings.monoType];
            for (int i=0;i<cols.Length;i++)
            {
                mat.SetVector("_monoColor" + i, cols[i]);
            }


            Graphics.Blit(source, destination, mat, 0);
        }

        void OnDisable()
        {
            if (_material)
            {
                DestroyImmediate(_material);
            }
        }
    }
}