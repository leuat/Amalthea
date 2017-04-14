// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "LemonSpawn/Star" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Scale("Scale", Range(0,1)) = 0.1
	}
	SubShader {
//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Tags {"Queue"="Transparent+1105" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400


		Lighting On
        Cull Back
        ZWrite on
        ZTest on
//        Blend One One//SrcAlpha OneMinusSrcAlpha
       Pass
         {

	Tags { "LightMode" = "ForwardBase" }
         
             CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPosition)
             
             #pragma target 3.0
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fwdbase
                         
             #include "UnityCG.cginc"
             #include "AutoLight.cginc"
#include "Include/Utility.cginc"
#include "Include/Atmosphere.cginc"

        sampler2D _MainTex;
        float4 _Color;
        float _Scale;



		float getNoiseOctaves(float3 pos, float freq, float per, int N, float t) {
			float total = 0;
			float amplitude = 1;
			float maxAmplitude = 0;
			float3 shift = float3(0.1234123, 0.2123521, 0.2591723);
			for (int i = 0; i < N; i++) {
				float v = noise(pos * 0.3*freq + shift*_Time*t*cos(i-t*0.64)*sin(i+t));//cos(t*_Time*0.1)*sin(i*t*_Time*0.00712));
				//        double v = raw_3d( (x) * freq, (y) * freq, (z) * freq );
				
				total += v*amplitude;
				freq *= 2;
				maxAmplitude += amplitude;
				amplitude *= per;
				//p = sqrt(p);

			}


			return total / maxAmplitude;

		}

             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float4 uv : TEXCOORD2;
                 float3 worldPosition : TEXCOORD3;
                 LIGHTING_COORDS(6,7)
             };
              
             
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos( v.vertex);
                 o.uv = v.texcoord;

				 o.uv.xy = pos2uv(v.vertex.xyz);
				 o.uv.xy *= stretch;

                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 				 o.worldPosition = v.vertex;//mul (_Object2World, v.vertex).xyz;
 				 //o.worldPosition = mul (_Object2World, v.vertex).xyz;

 				 float3 vv = mul(unity_ObjectToWorld, v.vertex).xyz;
 				 vv = normalize(vv-v3Translate)*fInnerRadius*1.01;

//  			   	  getGroundAtmosphere(v.vertex, o.c0, o.c1);
  				

                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             			// Calculates the Mie phase function

             				



		float getSun(float3 pos, int N, float scale) {

			
			float3 p = pos;

			float val = 0;
//			[uroll]
			float A = 0;
			for (int i=0;i<N;i++){
				float k = (2*i+1)*scale;
				float amp = 1/((i+1)*k);
				float v = abs(noise(p*k));
				val += amp*v;
				A+=amp;
			}
			
			return 0.2/((val/A)+0.1);

			}

                            
		fixed4 frag(v2f i) : COLOR {

			float3 worldSpacePosition = i.worldPosition - v3Translate*0;
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
			float3 centerDirection = normalize(_WorldSpaceCameraPos);

			float globalLight = clamp(dot(i.normal, normalize(lightDir))+0.25,0,1);


//			globalLight = pow(globalLight, 1);


			float3 shift = float3(1.2314, 0.6342, 0.96123)*23.4;

			float3 pos = normalize(worldSpacePosition);
			float4 c;

			float ms = 50.23*_Scale;
			//float val = 1-2*pow(getMultiFractal2(pos, ms, 8, 2.5, 0.56, 4, -0.5),3);
			float val = (getNoiseOctaves(pos*73.23, 1, 1, 7,1))*1.0 -0.0;

			float s = 0.8;
			float t1 = getNoiseOctaves(pos * 4.5, 2, 0, 1,0.83456) - s;
			t1 = max(t1, 0.0);
			val = val - t1;

			float t2 = getNoiseOctaves(pos * 2.123, 1.5, 1, 8,1.2354) - 0.6;
			t2 = max(t2, 0.0);
			val = val + t2*15;

			float theta = 1.0 - pow(dot(normalize(viewDirection), pos),0.4);
//			pColor = vec4(unColor + total - theta, 1.0);
			val -= theta;
			val *= 1.2;
			c.a = 1;
			c.rgb = _Color.xyz*val;


			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }