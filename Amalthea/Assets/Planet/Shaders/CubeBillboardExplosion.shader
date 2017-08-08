Shader "LemonSpawn/CubeBillboardExplosion" {
	Properties{
		_Size("Size", Float) = 1.0
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
		Pass{

		Blend srcalpha one
		cull off
		Zwrite off
		Ztest off

		CGPROGRAM

#include "UnityCG.cginc" 
#include "Include/Utility.cginc"
#include "Include/Atmosphere.cginc"


#pragma vertex vert  
#pragma fragment frag

		// User-specified uniforms            
	uniform float _Size;
	uniform float4 _Color;

	struct vertexInput {
		float4 vertex : POSITION;
		float4 tex : TEXCOORD0;
	};
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 tex : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input)
	{
		vertexOutput output;

		output.pos = mul(UNITY_MATRIX_P,
			mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
			+ float4(input.vertex.x, input.vertex.y, 0.0, 0.0)
			* float4(_Size, _Size, 1.0, 1.0));

		output.tex = input.tex;

		return output;
	}

	float4 frag(vertexOutput input) : COLOR
	{
		//return tex2D(_MainTex, float2(input.tex.xy));
		float2 p = input.tex - float2(0.5, 0.5);
		float d = length(p);
		float v = atan(p.x / p.y) + _Time*1.23;
		float l = length(p) - _Time*0.15;
		d += 0.01*noise(float3(l * 100,v * 10, 0.124)) + 0.005*noise(float3(l * 294.23, v * 32.13, 0.124));;
		float val = exp(-d*d * 500)*1.2;
		float4 c = _Color*val;

		return c;

	}

		ENDCG
	}
	}
}