Shader "LemonSpawn/GasPlanet" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_CloudTex("Base (RGB)", 2D) = "white" {}
	_CloudTex2("Base (RGB)", 2D) = "white" {}
	}
		SubShader{
		//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		Tags{ "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Tags{ "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		LOD 400


		Lighting On
		Cull off
		ZWrite on
		ZTest on
		//Blend SrcAlpha OneMinusSrcAlpha
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



	struct v2f
	{
		float4 pos : POSITION;
		float4 texcoord : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float4 uv : TEXCOORD2;
		float3 worldPosition : TEXCOORD3;
		float3 c0 : TEXCOORD4;
		float3 c1 : TEXCOORD5;
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
		o.worldPosition = v.vertex;//mul (_Object2World, v.vertex).xyz;
								   //o.worldPosition = mul (_Object2World, v.vertex).xyz;

		float3 vv = mul(unity_ObjectToWorld, v.vertex).xyz;
		vv = normalize(vv - v3Translate)*fInnerRadius*1.01;

		//  			   	  getGroundAtmosphere(v.vertex, o.c0, o.c1);
		getGroundAtmosphere(mul(unity_WorldToObject,vv + v3Translate), o.c0, o.c1);


		TRANSFER_VERTEX_TO_FRAGMENT(o);

		return o;
	}
	// Calculates the Mie phase function


	float getNoiseOctaves(float3 pos, float freq, float per, int N, float t) {
		float total = 0;
		float amplitude = 1;
		float maxAmplitude = 0;
		float3 shift = float3(0.1234123, 0.2123521, 0.2591723);
		for (int i = 0; i < N; i++) {
			float v = noise(pos * 0.3*freq + shift*_Time*t*cos(i - t*0.64)*sin(i + t));//cos(t*_Time*0.1)*sin(i*t*_Time*0.00712));
																					   //        double v = raw_3d( (x) * freq, (y) * freq, (z) * freq );

			total += v*amplitude;
			freq *= 2;
			maxAmplitude += amplitude;
			amplitude *= per;
			//p = sqrt(p);

		}


		return total / maxAmplitude;

	}


	float getStormThreshold(float3 pos, float add) {
		float sc = ls_cloudscale*2;
		float dec = 0.85;

		pos.y *= 1.5;
		pos.x -= add*0.1;
		float s1 =1*abs(noise(sc * pos)) - dec;
		float s2 = 1 * abs(noise(sc * (pos+ float3(1.234, 0.2545,2.425)))) - dec;
		float s3 = 1 * abs(noise(sc * (pos + float3(-19.234, -0.4545, 5.425)))) - dec;

		float s = max(s1*s2*s3, 0);
		return clamp(5*ls_cloudscattering * s,0,1);

	}


	float3 getGasClouds(float3 normal, float3 pos, float add) {

//		add *= 10;
//		float yy = pos.y*cos(add) + pos.x*sin(add);

		float yy = pos.y;
		float scale = ls_cloudthickness*3.5;

		float ns = 0.02;

		float3 p = 13*float3(ns*normal.x, yy + 10.1234, ns*normal.z)*scale;

//		return ls_cloudcolor*clamp(getNoiseOctaves(1*p, 2, 0.5, 8, 0) - 0.3, 0,1) ;


		float n = clamp(1 - abs(normal.y), 0, 1);
//		float n = 1;

		float amp = 0.1*n;// noise(4.23*pos);
		float stormThreshold = getStormThreshold(pos, add);
			
		float stormy = 100 * noise(10*normal)*stormThreshold;// *noise(float3(0.35, 23.234, 0.52));

		float3 pp = pos;
//		pp.x += add * 0.5;


		pp.x += add * 0.05;
		p.y += amp*(0.3*noise(scale*320.1234*pp));
		
		pp.z += add * 0.03;
		p.y += amp*0.3*noise(scale*456.34*pp);
		pp.z += add * 0.1;
		p.y += +amp*0.5*noise(scale*8.545*pp);
		pp.z += add * 0.05;
		p.y+=amp*0.5*noise(scale * 56 * pp);
		pp.x += add * 0.01;
		p.y += amp*0.2*noise(scale*1856.34*pp);

/*		pp.x += add * 0.001;
		p.y += amp*0.05*noise(scale*12856.34*pp);
		*/
		p.y += stormy;
		p.x += 0.1*stormy*noise(pos*25);
		
		//p.y += 0.1*stormy*add//


		//float val = pow(1.2*abs(getNoiseOctaves(p, 8, 0.5, 4, 0)),1)*0.5;

		float val = pow(1.2* abs(getMultiFractal2(p, 0.1, 12, 2.5, 0.57, 4, -0.5)),1);        


		// Add dark bankds


		//val += 3*stormThreshold;

		val *= (pow(n,2));
		float t = clamp(pow(noise(4.234*p), 0.5), 0, 1);
		//t = 0;
		float3 c = t*ls_cloudcolor + (1 - t)*topColor;
//		float3 c= ls_cloudcolor;

		val = clamp(val-ls_cloudSubScale, 0, 1);

		float dval = 0.1*pow(2 * abs(getMultiFractal2(p*2.6554, 0.1, 12, 2.5, 0.57, 4, -0.5)), 1);

		dval = clamp(dval - ls_cloudSubScale*0.5, 0, 1);

		return c*val -c.bgr*dval;


	}

	float getGasCloudShadow(float3 start, float3 direction, float stepLength, float normal, float add) {

		int N = 4;

		float3 p = float3(0,0,0);

		float val = -10;
		//			[uroll]
		for (int i = 0; i<N; i++) {
			float v = 5*length(getGasClouds(float3(0,0,1), start, add));
//			float h = abs(length(p) - 1);
	//		v = exp(-(h * 200))*v;

			val += v;
			p = p + direction*stepLength*0.01;
		}
//		return 0;
		return val / (float)(N);

	}





	fixed4 frag(v2f i) : COLOR{

		float3 worldSpacePosition = i.worldPosition - v3Translate * 0;
		float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);

		float globalLight = clamp(dot(i.normal, normalize(lightDir)),0,1);


		//			globalLight = pow(globalLight, 1);


		float3 shift = float3(1.2314, 0.6342, 0.96123)*23.4;

		float3 pos = normalize(worldSpacePosition);
		float4 c = float4(0,0,0,1);

		int N = 10;
		float add = _Time*0.1;


		c.rgb = getGasClouds(i.normal, pos,add)*ls_cloudintensity;
		
//		float shadow = getGasCloudShadow(pos, normalize(lightDir), ls_shadowscale, i.normal, add);

//		shadow = clamp(shadow*1, 0.0, 1.0);

	//	c.rgb *= (1.0 - shadow);

	
		c.rgb = atmColor(i.c0, i.c1) + c.rgb *globalLight;


		return c;
	}
		ENDCG
	}
	}
		Fallback "Diffuse"
}
