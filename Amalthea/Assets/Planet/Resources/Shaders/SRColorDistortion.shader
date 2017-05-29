Shader "LemonSpawn/SRColorDistortion" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_bwBlend("Black & White blend", Range(0, 1)) = 0
	}
		SubShader{
		Pass{
		CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag

#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
	uniform float _bwBlend;
	uniform float _aspectRatio;
	uniform float3 _moveDirection;
	uniform float3 _focusPoint;
	uniform float _lightSpeed;

	uniform float3 _viewDirection, _up, _right;

	//vertex:vert



	float4 frag(v2f_img i) : COLOR{
		float4 c = tex2D(_MainTex, i.uv);

//		float weight = dot(normalize(_MoveDirection))

		



		float2 uv = i.uv - float2(0.5, 0.5);

		float3 viewDirection = normalize(_viewDirection +   uv.x*_right + uv.y*_up);

		float dott = dot(viewDirection, normalize(_moveDirection));

		float s = sign(dott);

	/*	float v = length(moveDirection);
		float gamma = 1 / sqrt(1 - v*v / (c*c));

		
		float nu = waveLengthFromRGB(c.rgb);

//		nu 

		c.rgb = rgbFromWaveLength(nu);
*/
		dott *= clamp(0.2*length(_moveDirection)/c,0,1);

		c.r *= 1 + dott ;
		c.b *= 1 + -dott;

		return c;
	}
		ENDCG
	}
	}
}