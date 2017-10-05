Shader "LemonSpawn/AccretionDisk" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		cull off
		Zwrite off
		Pass{
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "Include/Utility.cginc"
#include "Include/Atmosphere.cginc"

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;
	float4 _Color;
	float amplitude;

	struct v2f {
		float4 pos : POSITION;
		//  fixed4 color : COLOR;
		float2 uv : TEXCOORD0;
		float4 opos: TEXCOORD1;

	};


	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.opos = v.vertex;
		o.uv = v.texcoord;//TRANSFORM_TEX (v.texcoord, _MainTex);
		return o;
	}

	float getNoiseOctaves(float3 pos, float freq, float per, int N, float t) {
		float total = 0;
		float amplitude = 1;
		float maxAmplitude = 0;
		float3 shift = float3(0.1234123, 0.2123521, 0.2591723);
		for (int i = 0; i < N; i++) {
			float v = noise(pos * 0.3*freq + shift*_Time*t*cos(i - t*0.64)*sin(i + t));//cos(t*_Time*0.1)*sin(i*t*_Time*0.00712));
																					   //        double v = raw_3d( (x) * freq, (y) * freq, (z) * freq );

			total += 0.2/v*amplitude;
			freq *= 2;
			maxAmplitude += amplitude;
			amplitude *= per;
			//p = sqrt(p);

		}


		return total / maxAmplitude;

	}


	fixed4 frag(v2f i) : COLOR{
		//      	float3 lightDir = _WorldSpaceLightPos0;
		float r = length(i.uv - float2(0.5,0.5));

	float2 rad2 = i.uv - float2(0.5, 0.5);
		float3 rad = float3(rad2.x, 0, rad2.y);

		float3 tangent = cross(float3(0, 1, 0), normalize(rad));

		float s = 0.1;
//		float3 t2 =15*float3(tangent.x,rad.x,0);
		float3 t2 = 2 * float3(r+ s*tangent.x, 0, r+s*tangent.z);

		float c = getNoiseOctaves(t2,10,1,5,_Time*0.41);
		//	c = clamp(c, 0, 0.65)*1.15;

		float radius = 0.4;
		float width = 0.09;

		float dr = radius - r;

		float v = exp(-(dr / width)*(dr / width));


//		if (r<radius1 || r>radius2)
//			discard;
		//     c.a = 1;
		float4 col = _Color;
		float val = 3*pow(c.x,2) - 0.0;


		return _Color *val*v;
		}
			ENDCG

		}
	}
}
