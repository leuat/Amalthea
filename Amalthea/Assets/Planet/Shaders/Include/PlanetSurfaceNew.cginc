// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

#ifndef PlanetSurface
#define PlanetSurface


        uniform float3 surfaceNoiseSettings;
        uniform float3 surfaceNoiseSettings2;
        uniform float3 surfaceNoiseSettings3;
		uniform float3 surfaceNoiseSettings5;
		uniform float3 surfaceNoiseSettings6;
		uniform float3 surfaceVortex1;
		uniform float3 surfaceVortex2;
		float4x4 rotMatrix;
	
		uniform sampler2D _Craters;



/*		float getStandardPerlin(float3 pos, float scale, float power, float sub, int N) {
			float n = 0;
			float A = 0;
			float ms = scale;
			float3 shift= float3(0.123, 2.314, 0.6243);

			for (int i = 1; i <= N; i++) {
				float f = pow(2, i)*1.0293;
				float amp = (2 * pow(i,power)); 
				n += noisePerturbed(pos*f*ms + shift*f) / amp;
				A += 1/amp;
			}

			float v = clamp(n - sub*A, -1, 1);
			return v;	

		}
		*/


/*		float swissTurbulence(float3 p, float seed, int octaves,
			float lacunarity, float gain,
			float warp, float powscale, float offset)
		{
			float sum = 0;
			float freq = 1.0, amp = 1.0;
			float3 dsum = float3(0, 0, 0);
			for (int i = 0; i < octaves; i++)
			{
				float3 N;
				float F = perlinNoiseDeriv((p + warp * dsum)*freq, 1,1,N);

				float n = clamp((offset - powscale * abs(F)),-1000,1000);

				n = n * n;
				sum += amp * n;
				dsum = dsum + N * amp * -F;

				freq *= lacunarity;
				amp *= gain * saturate(sum);



			}
			return sum;
		}


		float getSwissFractal(in float3 p, float frequency, int octaves, float lacunarity, float offset, float gain, float powscale, float warp) {

			return swissTurbulence(p*frequency, 0, octaves,
				lacunarity, gain,
				warp, powscale, offset);

		}

		*/

		float4 getMultiFractal(in float3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO ) {

            float4 value = 0.0f;
            float weight = 1.0f;
            float w = surfaceNoiseSettings6.x;//-0.5;
            float3 vt = p * frequency;
            float f = 1;

//			float4 ss = float4(1, 1, 1,1);
			float4 ss = float4(1, 1, 1, 1);

            for (float octave = 0; octave < octaves; octave++)
            {
                 float4 signal = ss*initialO + noised(vt);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

                // Make the ridges.
                signal.x = abs(signal.x);
				signal.y = abs(signal.y);
				signal.z = abs(signal.z);
				signal.w = abs(signal.w);
				signal = ss*offs - signal;


//                signal *= signal;
				signal.x *= signal.x*weight;
				signal.y *= signal.y*weight;
				signal.z *= signal.z*weight;
				signal.w *= signal.w*weight;
				
                weight = signal.x * gain;
                weight = clamp(weight, 0, 1);

                value += (signal * pow(f, w));
//                value += (signal * pow(f, -1));
//                value += (signal * 1);
//                value += (signal * pow(f, 0.05));
//                value += (signal * pow(frequency, -1.0));
                vt = vt * lacunarity;
//                frequency *= lacunarity;
                 f *= lacunarity;
            }
            return value;
//			return ((value*1.25) -1);
        }



		float4 getSurfaceHeight(float3 pos, float scale, float octaves) {

			// return noise(pos * 10) * 5;
			scale = scale*(1 + surfaceVortex1.y*noise(pos*surfaceVortex1.x));
			scale = scale*(1 + surfaceVortex2.y*noise(pos*surfaceVortex2.x));
			float4 val = getMultiFractal(pos, scale*1.523, (int)octaves + 2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			return val;
			//val.x = pow(val, surfaceNoiseSettings3.z);

			//return clamp(val - surfaceNoiseSettings3.x, 0, 10);
			//return getStandardPerlin(pos, scale, 1, 0.5, 8);

		}




		float3 getHeightPosition(in float3 pos, in float scale, float heightScale, float octaves, out float3 Normal) {
			float4 v = getSurfaceHeight(mul(rotMatrix, pos), scale, octaves);
			Normal = v.yzw;
			return pos*fInnerRadius *(1 + v.x*heightScale);
//			return pos*fInnerRadius*(1+getSurfaceHeight(mul(rotMatrix, pos) , scale, octaves)*heightScale);
			
		}



		float3 getSurfaceNormal(float3 pos, float scale,  float heightScale, float normalScale, float3 tangent, float3 bn, float octaves, int N) {
//			float3 getSurfaceNormal(float3 pos, float scale, float heightScale, float normalScale) {
			float3 prev = 0;
//			pos = normalize(pos);
			float hs = heightScale;
			float3 Normal;
			float3 centerPos = getHeightPosition(normalize(pos), scale, hs, octaves, Normal);
			return normalize(Normal);
		}


		inline float LodSurface(in float3 p) {
			return surfaceNoiseSettings3.y;
//			return clamp(5000.0 / (length(p.xyz +v3Translate - _WorldSpaceCameraPos.xyz)), 4, surfaceNoiseSettings3.y);

		}

	//	inline float3 getPlanetSurfaceNormal(in float4 v) {
		float3 getPlanetSurfaceNormal(in float3 v, float3 t, float3 bn, float nscale, int N) {
			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			float octaves = LodSurface(v.xyz);

			return getSurfaceNormal(v, scale, heightScale, nscale, t,bn, octaves, N);
		}

		float3 getPlanetSurfaceNormalOctaves(in float3 v, float3 t, float3 bn, float nscale, int N, int octaves) {
			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;


			return getSurfaceNormal(v, scale, heightScale, nscale, t, bn, octaves, N);
		}


		float4 getPlanetSurfaceOnly(in float4 v) {

			

			float4 p = mul(unity_ObjectToWorld, v);
			p.xyz -= v3Translate;

			float octaves = surfaceNoiseSettings3.y;

			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			p.xyz = normalize(p.xyz);
			float3 Normal;
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves, Normal) + v3Translate;
			return mul(unity_WorldToObject, p);
		}

/*		float4 getPlanetSurfaceOnlyNoTranslate(in float4 v) {

			float4 p = mul(unity_ObjectToWorld, v);

			float octaves = surfaceNoiseSettings3.y;

			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			p.xyz = normalize(p.xyz);
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves);
			return mul(unity_WorldToObject, p);
		}
		*/
		#endif

