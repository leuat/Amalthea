﻿Shader "LemonSpawn/Corona"
{
	Properties
	{
		_SpriteTex("Base (RGB)", 2D) = "white" {}
	_Size("Size", Range(0, 3)) = 0.5
	}

		SubShader
	{
		Pass
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "4000" }
		LOD 200
		Blend srcalpha one
		cull off
		Zwrite off
		Ztest off


		CGPROGRAM
#pragma target 5.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#pragma geometry GS_Main
#include "UnityCG.cginc" 

		// **************************************************************
		// Data structures												*
		// **************************************************************
		struct GS_INPUT
	{
		float4	pos		: POSITION;
		float3	normal	: NORMAL;
		float2  tex0	: TEXCOORD0;
	};

	struct FS_INPUT
	{
		float4	pos		: POSITION;
		float3 normal	: NORMAL;
		float2  tex0	: TEXCOORD0;
		float3 extra : TEXCOORD1;
	};


	// **************************************************************
	// Vars															*
	// **************************************************************

	float _Size;
	float4x4 _VP;
	//Texture2D _SpriteTex;
	//SamplerState sampler_SpriteTex;
	sampler _SpriteTex;

	// **************************************************************
	// Shader Programs												*
	// **************************************************************

	// Vertex Shader ------------------------------------------------
	GS_INPUT VS_Main(appdata_base v)
	{
		GS_INPUT output = (GS_INPUT)0;

		output.pos = mul(unity_ObjectToWorld, v.vertex);
		output.normal = v.normal;
		output.tex0 = v.texcoord;

		return output;
	}


	// Geometry Shader -----------------------------------------------------
	[maxvertexcount(4)]
	void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
	{
		float3 up = UNITY_MATRIX_IT_MV[1].xyz; //upVector;
		float3 look = _WorldSpaceCameraPos - p[0].pos;
		//look.y = 0;
		look = normalize(look);
		float3 right = cross(up, look);

		float halfS = 1.5 * _Size *max(p[0].tex0.x, 0.5);

		float4 v[4];
		v[0] = float4(p[0].pos + halfS * right - halfS * up, 1.0f);
		v[1] = float4(p[0].pos + halfS * right + halfS * up, 1.0f);
		v[2] = float4(p[0].pos - halfS * right - halfS * up, 1.0f);
		v[3] = float4(p[0].pos - halfS * right + halfS * up, 1.0f);

		float4x4 vp = UNITY_MATRIX_MVP;//UnityObjectToClipPos(unity_WorldToObject);
		FS_INPUT pIn;
		pIn.pos = mul(vp, v[0]);
		pIn.tex0 = float2(1.0f, 0.0f);
		pIn.normal = p[0].normal;
		pIn.extra = p[0].tex0.x;
		triStream.Append(pIn);

		pIn.pos = mul(vp, v[1]);
		pIn.tex0 = float2(1.0f, 1.0f);
		triStream.Append(pIn);

		pIn.pos = mul(vp, v[2]);
		pIn.tex0 = float2(0.0f, 0.0f);
		triStream.Append(pIn);

		pIn.pos = mul(vp, v[3]);
		pIn.tex0 = float2(0.0f, 1.0f);
		triStream.Append(pIn);
	}




	inline float iqhash(float n)
	{
		return frac(sin(n)*753.5453123);

	}

	float noise(float3 x)
	{

		float3 p = floor(x);
		float3 f = frac(x);

		f = f*f*(3.0 - 2.0*f);
		float n = p.x + p.y*157.0 + 113.0*p.z;

		return lerp(lerp(lerp(iqhash(n + 0.0), iqhash(n + 1.0), f.x),
			lerp(iqhash(n + 157.0), iqhash(n + 158.0), f.x), f.y),
			lerp(lerp(iqhash(n + 113.0), iqhash(n + 114.0), f.x),
				lerp(iqhash(n + 270.0), iqhash(n + 271.0), f.x), f.y), f.z);


	}

	float flare(float2 t, float3 opos, float add) {


/*		float3 p = float3(t.x, 0, t.y);
		p = mul((float3x3)unity_ObjectToWorld, p);
		t.x = p.x;
		t.y = p.z;
		*/
//		t.x = opos.x;
	//	t.y = opos.z;

		t.x -= 0.5;
		t.y -= 0.5;
		float s = 4;

		float d = clamp(1-6*length(t), 0, 1);
		if (d < 0)
			return 0;

		t.x += 0.1*(1-d)*noise(s*float3(t.x+add, t.y, 0));
		t.y += 0.1*(1-d)*noise(s*float3(t.x - add*0.95, t.y, 0));

		t.x += 0.15*(1 - d)*noise(s*3.23*float3(t.x + 0.05*cos(add), t.y-0.134*add, 0));
//		t.y += 0.5*(1 - d)*noise(s*float3(t.x - add*0.95, t.y, 0));


		float tan = atan2(t.y, t.x);

		float3 pos = 124.234*float3(t.x, tan, add*0.1);

/*		float4x4 vp = UNITY_MATRIX_MVP;//UnityObjectToClipPos(unity_WorldToObject);
		float4 np = float4(pos, 1);
		pos = mul(vp, np).xyz;

		*/
		float v = noise(1*pos);
		v *= d;
		float s2 = 23;
		float a = add*0.2;
		float modifier = noise(s2*float3(t.y, t.x-0.323*a, 0.66*t.x + a)) - 0.35;
		//if (modifier>0)
			v *= (modifier);

		v = clamp(v*3,0,1);
		return v;
	}

	// Fragment Shader -----------------------------------------------
	float4 FS_Main(FS_INPUT input) : COLOR
	{
		float2 t = input.tex0;
		float add = _Time;

/*		float s = 5;
		float scale = 1 + 0.1*noise(float3(s*t.x, s*t.y, -0.1234));

		float3 np1 = scale*32.35*float3(t.x-1.234*add, t.y + sin(add), 0.1234);
		float3 np2 = scale*31.35*float3(t.x - 0.634*cos(add), t.y - 0.92*cos(add), 0.3234);
		float v1 =  noise(np1);
		float v2 = noise(np2);
		float d = clamp(0.2*length(t - float2(0.5, 0.5)),0,1);*/
		float2 texP = t;// +0.0*d*float2(v1, v2);
		float4 val = 0;// tex2D(_SpriteTex, texP);
		float3 c = input.normal;
		val *= 2;
		val+= flare(t, normalize(input.pos),_Time);
		

		val.x *= c.x;
		val.y *= c.y;
		val.z *= c.z;
//		val.z=d;
		val.a = max(val.x, val.y);
		val.a = max(val.a, val.z);
//		val.x=clamp
		return val;
	}

		ENDCG
	}
	}
}
