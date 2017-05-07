// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "LemonSpawn/Billboard"
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
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
		LOD 200
		Blend srcalpha oneminussrcalpha
		cull off
		Zwrite off
		Ztest off
		
		LOD 200

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
	uniform float3 upVector;

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
		float3 up = upVector;
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



	// Fragment Shader -----------------------------------------------
	float4 FS_Main(FS_INPUT input) : COLOR
	{


		float4 val = tex2D(_SpriteTex, input.tex0);
		float3 c = input.normal;
		val.x *= c.x;
		val.y *= c.y;
		val.z *= c.z;
		val *= 2;
		val.a = max(val.x, val.y);
		val.a =max(val.a, val.z);
		return val;
	}

		ENDCG
	}
	}
}
