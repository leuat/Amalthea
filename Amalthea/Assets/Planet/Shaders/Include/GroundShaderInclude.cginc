struct VertexOutputForwardBaseGround
{
	float4 pos							: SV_POSITION;
	float4 tex							: TEXCOORD0;
	half3 eyeVec 						: TEXCOORD1;
	half4 tangentToWorldAndPackedData[3]    : TEXCOORD2;	// [3x3:tangentToWorld | 1x3:viewDirForParallax]
	half4 ambientOrLightmapUV			: TEXCOORD5;	// SH or Lightmap UV
	SHADOW_COORDS(6)
		UNITY_FOG_COORDS(7)
		float3 c0 : TEXCOORD8;
	float3 c1 : TEXCOORD9;
	float3 n1 : TEXCOORD10;
	//	float4 vpos  : TEXCOORD11;
	// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
	//#if UNITY_SPECCUBE_BOX_PROJECTION
	float3 posWorld					: TEXCOORD11;
	//#endif
	float3 posWorld2 				: TEXCOORD12;
	float3 tangent : TEXCOORD13;
	float3 binormal: TEXCOORD14;
};


sampler2D _Mountain, _Basin, _Top, _Surface;
