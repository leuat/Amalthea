Shader "LemonSpawn/Ground"
{
	Properties
	{

		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo map", 2D) = "white" {}
	_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

	_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

	_Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
		_ParallaxMap("Height Map", 2D) = "black" {}

	_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

	_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

	_DetailMask("Detail Mask", 2D) = "white" {}

	_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
	_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}

	[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

		// UI-only data
		[HideInInspector] _EmissionScaleUI("Scale", Float) = 0.0
		[HideInInspector] _EmissionColorUI("Color", Color) = (1,1,1)

		// Blending state
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
	}




		CGINCLUDE
#define UNITY_SETUP_BRDF_INPUT MetallicSetup
		ENDCG



		SubShader
	{
		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
		LOD 150


		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
	{
		Name "FORWARD"
		Tags{ "LightMode" = "ForwardBase" }
		Blend[_SrcBlend][_DstBlend]
		ZWrite[_ZWrite]


		CGPROGRAM
#pragma target 4.0
		// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
#pragma exclude_renderers gles

		// -------------------------------------

#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _EMISSION
#pragma shader_feature _METALLICGLOSSMAP 
#pragma shader_feature ___ _DETAIL_MULX2
#pragma shader_feature _PARALLAXMAP
		//#pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE

#pragma multi_compile_fwdbase
#pragma multi_compile_fog

#pragma multi_compile LS_GPU_SURFACE LS_CPU_SURFACE

//#pragma LS_GPU_SURFACE
//#pragma LS_CPU_SURFACE

#pragma vertex LvertForwardBase
#pragma fragment LfragForwardBase

#include "UnityStandardCore.cginc"
#include "Include/IQnoise.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"



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
		float3 posWorld					: TEXCOORD10;
		float3 tangent : TEXCOORD11;

	};


	sampler2D _Mountain, _Basin, _Top, _Surface;




VertexOutputForwardBaseGround LvertForwardBase(VertexInput v)
	{
		VertexOutputForwardBaseGround o;


		float3 normalWorld = normalize(mul(unity_ObjectToWorld, v.vertex).xyz - v3Translate);
#ifdef LS_GPU_SURFACE
		float4 groundVertex = getPlanetSurfaceOnly(v.vertex);
		v.vertex = groundVertex;
#else
		float4 groundVertex = v.vertex;
		normalWorld = normalize(UnityObjectToWorldNormal(v.normal));

#endif
		UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBaseGround, o);
		float4 capV = groundVertex;
		float4 posWorld = mul(unity_ObjectToWorld, capV);
		
#ifdef _TANGENT_TO_WORLD
		o.tangent = v.tangent.xyz;
#endif
								  //		posWorld.x = 0;
		o.posWorld = posWorld.xyz;

		float wh = (length(o.posWorld.xyz - v3Translate) - fInnerRadius);

#ifdef LS_GPU_SURFACE
		capV.x *= scaleFactor.x;
		capV.y *= scaleFactor.y;
		capV.z *= scaleFactor.z;
#else
/*		o.posWorld.x/= scaleFactor.x;
		o.posWorld.y /= scaleFactor.y;
		o.posWorld.z /= scaleFactor.z;
		o.posWorld.xyz = capV.xyz;*/
#endif
		o.pos = UnityObjectToClipPos(capV);

		o.tex = TexCoords(v);
		o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);

//		o.n1 = normalWorld;
#ifdef _TANGENT_TO_WORLD
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

		float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
		o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
		o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];
#else
		o.tangentToWorldAndPackedData[0].xyz = 0;
		o.tangentToWorldAndPackedData[1].xyz = 0;
		o.tangentToWorldAndPackedData[2].xyz = normalWorld;
#endif
		//We need this for shadow receving
		TRANSFER_SHADOW(o);

		// Static lightmaps
#ifndef LIGHTMAP_OFF
		o.ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		o.ambientOrLightmapUV.zw = 0;
		// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
#elif UNITY_SHOULD_SAMPLE_SH
#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
		o.ambientOrLightmapUV.rgb = 0;
#elif (SHADER_TARGET < 30)
		o.ambientOrLightmapUV.rgb = ShadeSH9(half4(normalWorld, 1.0));
#else
		// Optimization: L2 per-vertex, L0..L1 per-pixel
		o.ambientOrLightmapUV.rgb = ShadeSH3Order(half4(normalWorld, 1.0));
#endif
		// Add approximated illumination from non-important point lights
#ifdef VERTEXLIGHT_ON
		o.ambientOrLightmapUV.rgb += Shade4PointLights(
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, posWorld, normalWorld);
#endif
#endif

#ifdef DYNAMICLIGHTMAP_ON
		o.ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

#ifdef _PARALLAXMAP
		TANGENT_SPACE_ROTATION;
		half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(groundVertex));
		o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
		o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
		o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;
#endif

		UNITY_TRANSFER_FOG(o,o.pos);

		getGroundAtmosphere(groundVertex, o.c0, o.c1);

		return o;
	}


	uniform float hillyThreshold;





	inline float3 getTex(sampler2D t, in float2 uv) {
//		return 1;
		//								return float3(1,1,1);
		//		uv *= ;
		float3 c = tex2D(t, uv)*0.25;
		//								c += tex2D(t, 0.5323*uv);
		c += tex2D(t, 0.2213*uv)*0.33;

		c += tex2D(t, 0.0513*uv)*0.33;

		c /= 0.5;
		return c;
	}


	inline float widthDistance(float3 pos) {
		return clamp(10 * length(pos - _WorldSpaceCameraPos) / fInnerRadius, 0.1, 10);
	}






	float3 getSurfaceColor(float h, float hill, float perlinGround, float posY, float2 uv) {
		
		float3 mColor = ((1 - perlinGround)*middleColor + middleColor2*perlinGround);
		//	float3 bColor = ((1-tt)*basinColor + basinColor2*tt*r_noise(normalize(i.vpos.xyz),2.1032,3));


		float3 hColor = mColor;//getTex(_Surface, i.tex.xy);//float3(1,1,1);//s.diffColor;
							   //	float3 hillColor = s.diffColor;
							   //if (dd < 0.98 )
							   //	hColor = float3(0.2, 0.2 ,0.2);
		float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position




		float fCameraHeight = length(v3CameraPos);
		float camH = clamp(fCameraHeight - fInnerRadius, 0, 1);
		//	float wh = (length(i.posWorld.xyz - v3Translate) - fInnerRadius);
		//	wh = 0.0;
		//									float modulatedHillyThreshold = hillyThreshold* atan2(i.posWorld.z , i.posWorld.y);
		float modulatedTopThreshold = topThreshold*(1 - posY*1.1);
		float modulatedHillyThreshold = hillyThreshold;// clamp(hillyThreshold - 1 * posY, 0, 1);


		hColor = mixHeight(hColor, basinColor*getTex(_Basin, uv), 500, basinThreshold, h);

		hColor = mixHeight(hColor, basinColor2*getTex(_Basin, uv), 3000, liquidThreshold, h);
		hColor = mixHeight(topColor*getTex(_Top, uv), hColor, 1000, modulatedTopThreshold, h);


		//float dd = dot(normalize(i.posWorld2.xyz), normalize(s.normalWorld * 1));

		hColor = mixHeight(hColor, hillColor*getTex(_Mountain, uv), 10, modulatedHillyThreshold, hill);

		return hColor;
	}

	half4 LfragForwardBase(VertexOutputForwardBaseGround i) : SV_Target
	{
		FRAGMENT_SETUP(s)

#ifdef LS_GPU_SURFACE
	float3 pfix = i.posWorld.xyz;
#else
		float3 pfix = i.posWorld - v3Translate;
		pfix = float3(pfix.x  / scaleFactor.x, pfix.y / scaleFactor.y,pfix.z / scaleFactor.z);
		pfix += v3Translate;
#endif

		float h = (length(pfix - v3Translate) - fInnerRadius) / fInnerRadius;// - liquidThreshold;
#ifdef LS_GPU_SURFACE
	float3 binormal = normalize(cross(i.posWorld - v3Translate, i.tangent));

	float3 realN = getPlanetSurfaceNormal(i.posWorld - v3Translate, i.tangent, binormal, widthDistance(i.posWorld.xyz),3);
	s.normalWorld = realN;
#endif
	//UnityLight mainLight = MainLight(s.normalWorld);
	UnityLight mainLight = MainLight();
	mainLight.dir = v3LightPos;
	half atten = SHADOW_ATTENUATION(i);

	half occlusion = Occlusion(i.tex.xy);

	float rness = s.smoothness;

	UnityGI gi = FragmentGI(
		s.posWorld, occlusion, i.ambientOrLightmapUV, atten, rness, s.normalWorld, s.eyeVec, mainLight);


	//	float dd = dot(normalize(mul(rotMatrixInv, i.posWorld2.xyz)), normalize(s.normalWorld * 1 + i.n1 * 0));

	
	float3 ppos = normalize(pfix- v3Translate);


	float perlinGround = pow(clamp(noise(normalize(ppos)*3.1032) + 0.3, 0, 1), 2);

	float hill = dot(normalize(ppos), normalize(s.normalWorld * 1));

	float posY = (clamp(2 * abs(asin(ppos.y) / 3.14159), 0, 1));


	float3 diff = getSurfaceColor(h, hill, perlinGround, posY, i.tex.xy);


	float4 spc = _Color*0.65;// float4(1, 1, 1, 1);// *metallicity;// *specularity * 1;
	half4 c = UNITY_BRDF_PBS(diff, s.specColor, s.oneMinusReflectivity, rness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI(diff, s.specColor, s.oneMinusReflectivity, rness, s.normalWorld, -s.eyeVec, occlusion, gi);
	c.rgb += Emission(i.tex.xy);



	c.rgb = groundColor(i.c0, i.c1, c.rgb, s.posWorld, 1.0);// *groundClouds;

															//											c.rgb = modulatedHillyThreshold;
															//												c.rgb = float3(1,0,0)*modd;
															//return float4(ppos.xyz,1);
															//s.alpha = 1;
	return OutputForward(c, s.alpha);
	}
		ENDCG
	}
		// ------------------------------------------------------------------

		Pass
	{
		Name "FORWARD_DELTA"
		Tags{ "LightMode" = "ForwardAdd" }
		Blend[_SrcBlend] One
		Fog{ Color(0,0,0,0) } // in additive pass fog should be black
		ZWrite Off
		ZTest LEqual

		CGPROGRAM
#pragma target 3.0
		// GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
#pragma exclude_renderers gles

		// -------------------------------------


#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2
#pragma shader_feature _PARALLAXMAP

#pragma multi_compile_fwdadd_fullshadows
#pragma multi_compile_fog

#pragma vertex vertForwardAdd
#pragma fragment fragForwardAdd

#include "UnityStandardCore.cginc"

		ENDCG
	}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass{
		Name "ShadowCaster"
		Tags{ "LightMode" = "ShadowCaster" }

		ZWrite On ZTest LEqual

		CGPROGRAM
#pragma target 3.0
		// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
#pragma exclude_renderers gles

		// -------------------------------------


#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma multi_compile_shadowcaster

#pragma vertex vertShadowCaster
#pragma fragment fragShadowCaster

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "Include/Atmosphere.cginc"
#include "Include/PlanetSurface.cginc"

		// Do dithering for alpha blended shadows on SM3+/desktop;
		// on lesser systems do simple alpha-tested shadows
#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#if !((SHADER_TARGET < 30) || defined (SHADER_API_MOBILE) || defined(SHADER_API_D3D11_9X) || defined (SHADER_API_PSP2) || defined (SHADER_API_PSM))
#define UNITY_STANDARD_USE_DITHER_MASK 1
#endif
#endif

		// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

		// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif


		half4		_Color;
	half		_Cutoff;
	sampler2D	_MainTex;
	float4		_MainTex_ST;
#ifdef UNITY_STANDARD_USE_DITHER_MASK
	sampler3D	_DitherMaskLOD;
#endif

	struct VertexInput
	{
		float4 vertex	: POSITION;
		float3 normal	: NORMAL;
		float2 uv0		: TEXCOORD0;
	};

#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
	struct VertexOutputShadowCaster
	{
		V2F_SHADOW_CASTER_NOPOS
#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
			float2 tex : TEXCOORD1;
#endif
	};
#endif


	// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
	// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
	// some platforms, and then things don't go well.


	void vertShadowCaster(VertexInput v,
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		out VertexOutputShadowCaster o,
#endif
		out float4 opos : SV_POSITION)
	{

		v.vertex = getPlanetSurfaceOnly(v.vertex);

		TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
			o.tex = TRANSFORM_TEX(v.uv0, _MainTex);
#endif
	}


	half4 fragShadowCaster(
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
		VertexOutputShadowCaster i
#endif
#ifdef UNITY_STANDARD_USE_DITHER_MASK
		, UNITY_VPOS_TYPE vpos : VPOS
#endif
	) : SV_Target
	{
#if defined(UNITY_STANDARD_USE_SHADOW_UVS)
		half alpha = tex2D(_MainTex, i.tex).a * _Color.a;
#if defined(_ALPHATEST_ON)
	clip(alpha - _Cutoff);
#endif
#if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#if defined(UNITY_STANDARD_USE_DITHER_MASK)
	// Use dither mask for alpha blended shadows, based on pixel position xy
	// and alpha level. Our dither texture is 4x4x16.
	half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25,alpha*0.9375)).a;
	clip(alphaRef - 0.01);
#else
	clip(alpha - _Cutoff);
#endif
#endif
#endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

	SHADOW_CASTER_FRAGMENT(i)
	}

		ENDCG
	}
		// ------------------------------------------------------------------
		//  Deferred pass
		Pass
	{
		Name "DEFERRED"
		Tags{ "LightMode" = "Deferred" }

		CGPROGRAM
#pragma target 3.0
		// TEMPORARY: GLES2.0 temporarily disabled to prevent errors spam on devices without textureCubeLodEXT
#pragma exclude_renderers nomrt gles


		// -------------------------------------

#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _EMISSION
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2
#pragma shader_feature _PARALLAXMAP

#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON

#pragma vertex vertDeferred
#pragma fragment fragDeferred

#include "UnityStandardCore.cginc"







		ENDCG
	}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
	{
		Name "META"
		Tags{ "LightMode" = "Meta" }

		Cull Off

		CGPROGRAM
#pragma vertex vert_meta
#pragma fragment frag_meta

#pragma shader_feature _EMISSION
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2

#include "UnityStandardMeta.cginc"
		ENDCG
	}
	}

		SubShader
	{
		Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
		LOD 150

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
	{
		Name "FORWARD"
		Tags{ "LightMode" = "ForwardBase" }

		Blend[_SrcBlend][_DstBlend]
		ZWrite[_ZWrite]

		CGPROGRAM
#pragma target 2.0

#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _EMISSION 
#pragma shader_feature _METALLICGLOSSMAP 
#pragma shader_feature ___ _DETAIL_MULX2
		// SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

#pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE

#pragma multi_compile_fwdbase
#pragma multi_compile_fog

#pragma vertex vertForwardBase
#pragma fragment fragForwardBase

#include "UnityStandardCore.cginc"

		ENDCG
	}
		// ------------------------------------------------------------------
		//  Additive forward pass (one light per pass)
		Pass
	{
		Name "FORWARD_DELTA"
		Tags{ "LightMode" = "ForwardAdd" }
		Blend[_SrcBlend] One
		Fog{ Color(0,0,0,0) } // in additive pass fog should be black
		ZWrite Off
		ZTest LEqual

		CGPROGRAM
#pragma target 2.0

#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2
		// SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
#pragma skip_variants SHADOWS_SOFT

#pragma multi_compile_fwdadd_fullshadows
#pragma multi_compile_fog

#pragma vertex vertForwardAdd
#pragma fragment fragForwardAdd

#include "UnityStandardCore.cginc"

		ENDCG
	}
		// ------------------------------------------------------------------
		//  Shadow rendering pass
		Pass{
		Name "ShadowCaster"
		Tags{ "LightMode" = "ShadowCaster" }

		ZWrite On ZTest LEqual

		CGPROGRAM
#pragma target 2.0

#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma skip_variants SHADOWS_SOFT
#pragma multi_compile_shadowcaster

#pragma vertex vertShadowCaster
#pragma fragment fragShadowCaster

#include "UnityStandardShadow.cginc"

		ENDCG
	}

		// ------------------------------------------------------------------
		// Extracts information for lightmapping, GI (emission, albedo, ...)
		// This pass it not used during regular rendering.
		Pass
	{
		Name "META"
		Tags{ "LightMode" = "Meta" }

		Cull Off

		CGPROGRAM
#pragma vertex vert_meta
#pragma fragment frag_meta

#pragma shader_feature _EMISSION
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2

#include "UnityStandardMeta.cginc"
		ENDCG
	}


	}


		//	FallBack "VertexLit"
		CustomEditor "StandardShaderGUI"
}
