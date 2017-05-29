// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "LemonSpawn/Corona3D" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_Scale("Scale", Range(0,1)) = 0.1
	}
		SubShader{
		//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent+1000" }
		LOD 400


		Lighting On
		Cull back
		ZWrite off
		ZTest off
	   Blend srcalpha one
		Pass
	{

		Tags{ "LightMode" = "ForwardBase" }

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
			float v = 0.15 / (noise(pos * 0.3*freq + shift*_Time*t*cos(i - t*0.64)*sin(i + t)) + 0.02);//cos(t*_Time*0.1)*sin(i*t*_Time*0.00712));
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


	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;

		o.uv.xy = pos2uv(v.vertex.xyz);
		o.uv.xy *= stretch;

		o.normal = normalize(v.normal).xyz;
		o.texcoord = v.texcoord;
		o.worldPosition = v.vertex;//mul (_Object2World, v.vertex).xy
		o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;

		float3 vv = mul(unity_ObjectToWorld, v.vertex).xyz;
		vv = normalize(vv - v3Translate)*fInnerRadius*1.01;

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
		for (int i = 0; i < N; i++) {
			float k = (2 * i + 1)*scale;
			float amp = 1 / ((i + 1)*k);
			float v = abs(noise(p*k));
			val += amp*v;
			A += amp;
		}

		return 0.2 / ((val / A) + 0.1);

	}



	float findCorona(float3 pos, float or, float add) {
		float r = length(pos);
		float theta = atan2(pos.y, pos.x);
		float phi = acos(pos.z / r);
		float3 d3 = float3(cos(theta)+add, cos(phi)-add , 1.5324);


		d3.x += 01.4*noise(pos*0.0174);
		d3.y += 01.4*noise(pos*0.0164);

		float val = 0.01*pow(noise(d3 * 240),1);
		val += 0.01*pow(noise(d3 * 25), 1);
		val *= 0.5;
		// Damp, holes etc

		val *= 2 * noise(0.03*pos);

		// Modify with radius

		float mr = length(pos);

		float dr = 5 * clamp(1 - r / or,0,1);
		val = clamp(val * dr,0,1);
		//		val = clamp(val * 1.2 * (1-dr) - 0.00, 0, 1);


				return val * 1;
			}


			float integrateCorona(float3 origin, float3 ray, float steplength, float outerRadius, float innerRadius, float add) {
				int cnt = 0;
				float3 pos = origin + ray*steplength;
				float val = 0;
				while (length(pos) <= outerRadius && cnt<130 && length(pos)>innerRadius) {
					cnt++;
					pos += ray*steplength;

					val += findCorona(pos, outerRadius, add);
					val = clamp(val, 0, 1);

				}

				return val;// 35 * val / cnt;
			}



			fixed4 frag(v2f i) : COLOR{

				float3 worldSpacePosition = i.worldPosition - v3Translate * 0;
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
				float3 centerDirection = normalize(_WorldSpaceCameraPos);

				float globalLight = clamp(dot(i.normal, normalize(lightDir)) + 0.25,0,1);


				//			globalLight = pow(globalLight, 1);


				float3 pos = normalize(worldSpacePosition);
				float4 c;

				float3 origin = worldSpacePosition;
				float3 ray = -viewDirection;

				float innerRadius = 112;

				c = float4(0, 0, 0, 0);
				float t1, t2;

				float val = integrateCorona(origin, ray, 1, length(worldSpacePosition), innerRadius, _Time*0.4);
				val *= 1.1*(clamp(abs(dot(i.normal, -viewDirection)) - 0.0,0,1));
				c = val*_Color;



				return c;
			}
				ENDCG
			}
	}
		Fallback "Diffuse"
}