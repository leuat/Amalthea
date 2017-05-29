Shader "LemonSpawn/BlackHole" {
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
	uniform float2 _centerPoint;
	uniform float _size;

//vertex:vert

	float2 lensPoint(float strength, float radius, float2 t) {
		//		CVector C2DImage::lensPoint(float strength, float radius, int x, int
		//		y) {



		float Ds = 1;
		float Dd = 0.5;
		float Dds = Dd + Ds;


		float2 alpha = float2(0, 0);
		float2 eta = float2 (t.x, t.y);
		float w =15;
		//  float xx = 10.0/float(w);///Dd;
		int n = 0;
//		float scale = 0.01*(0.001/ _size);

		float pStr = 4 *(0.001 / _size);
		float scale = 0.1/w;
//		strength *= _size * 2000;
//		strength *= _size*250;
		for (int i = 0; i<w; i++)
			for (int j = 0; j<w; j++) {

				float2 etam = scale*float2(float(i - w / 2), float(j - w / 2)*1.5* _aspectRatio);
				etam = etam + eta;

				float2 diff = eta - etam;

				//if (etam.x > 0 && etam.x < 1 && etam.y>0 && etam.y < 1) 
				{

					float pointSource = -clamp(1 - (pStr * length(etam - _centerPoint)), 0, 1);

					alpha = alpha + normalize(diff) * 1.0 * pointSource;

					n += 1.0;
				}
			}

		alpha = strength*alpha*1.0 * Dds / Ds / (n);

		return alpha;
	}


	float4 frag(v2f_img i) : COLOR{
		float4 c = tex2D(_MainTex, i.uv + lensPoint(_bwBlend, 0, i.uv));

		

		return c;
	}
		ENDCG
	}
	}
}