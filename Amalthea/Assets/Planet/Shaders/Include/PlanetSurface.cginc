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



		float getStandardPerlin(float3 pos, float scale, float power, float sub, int N) {
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

		float perlinNoiseDeriv(float3 p, float sc, float sc2, out float3 N)
		{
			if (sc == 0)
				sc = 1.0f;
			float e = 0.09193;//*sc;
			N = float3(0, 0, 0);
			float F0 = noisePerturbed(float3(p.x, p.y, p.z));
			float Fx = noisePerturbed(float3(p.x + e, p.y, p.z));
			float Fy = noisePerturbed(float3(p.x, p.y + e, p.z));
			float Fz = noisePerturbed(float3(p.x, p.y, p.z + e));

			N = float3((Fx - F0) / e, (Fy - F0) / e, (Fz - F0) / e);

			float s = 0.8;
			N = normalize(N)*s;
			return F0;
		}


		float swissTurbulence(float3 p, float seed, int octaves,
			float lacunarity, float gain,
			float warp, float powscale, float offset)
		{
			float sum = 0;
			float freq = 1.0, amp = 1.0;
			float3 dsum = float3(0, 0, 0);
			for (int i = 0; i < octaves; i++)
			{
				float3 N;
				//float F = perlinNoiseDeriv((p + warp * dsum)*freq, 1,1,N);
				float4 vv = noised((p + warp * dsum)*freq);
				N = normalize(vv.yzw);
				float F = vv.x;


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



		float getMultiFractal(in float3 p, float frequency, int octaves, float lacunarity, float offs, float gain, float initialO ) {

            float value = 0.0f;
            float weight = 1.0f;
            float w = surfaceNoiseSettings6.x;//-0.5;
            float3 vt = p * frequency;
            float f = 1;
            for (float octave = 0; octave < octaves; octave++)
            {
                 float signal = initialO + noisePerturbed(vt);//perlinNoise2dSeamlessRaw(frequency, vt.x, vt.z,0,0,0,0);//   Mathf.PerlinNoise(vt.x, vt.z);

                // Make the ridges.
                signal = abs(signal);
                signal = offs - signal;


                signal *= signal;

                signal *= weight;
                weight = signal * gain;
                weight = clamp(weight, 0, 1);

                value += (signal * pow(f, w));
                vt = vt * lacunarity;
                 f *= lacunarity;
            }
            return value;
//			return ((value*1.25) -1);
        }



		float getSurfaceHeight(float3 pos, float scale, float octaves, float craterScale = 1) {

			// return noise(pos * 10) * 5;
			scale = scale*(1 + surfaceVortex1.y*noise(pos*surfaceVortex1.x));
			scale = scale*(1 + surfaceVortex2.y*noise(pos*surfaceVortex2.x));
//			float val = getMultiFractal(pos, scale*1.523, (int)octaves +2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			float val = getMultiFractal(pos, scale*1.523, (int)octaves , surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			//val = val*(0.1*getMultiFractal(pos, scale*13.523, (int)octaves-2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x) + 1);
//	val = val + val*0.15*getMultiFractal(pos, scale*23.523, (int)octaves-2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			//val = val + val * 0.10*getMultiFractal(pos, scale*23.523, (int)octaves - 3, 2.75, 1, 0.75, -0.5);
			//val = val + val * surfaceNoiseSettings4.y*getMultiFractal(pos, surfaceNoiseSettings4.z*23.523*scale, 7, 2.75, 0.75, 1.5, -0.5);
			
				// Next one is high detail settings, really cool
			//val = val + val * surfaceNoiseSettings4.y*getMultiFractal(pos, 23.523*scale, 7, 2.75, 0.75, 1.5, -0.5);
			// Next one is rivers and stuff
			 //val = val - val * 0.1*surfaceNoiseSettings4.z*clamp(getMultiFractal(pos, 6.523*scale*surfaceNoiseSettings5.y*4, 7, 2.75, 0.75, 1.5, -0.5)-surfaceNoiseSettings5.x,0,1);
			val = pow(val, surfaceNoiseSettings3.z);
			return clamp(val - surfaceNoiseSettings3.x, 0, 10);
			//return getStandardPerlin(pos, scale, 1, 0.5, 8);

		}



		float getSurfaceHeightSwiss(float3 pos, float scale, float octaves, float craterScale = 1) {

			//return noise(pos * 10)*5;


			scale = scale*(1 + surfaceVortex1.y*noise(pos*surfaceVortex1.x));
			scale = scale*(1 + surfaceVortex2.y*noise(pos*surfaceVortex2.x));
//			float val = getMultiFractal(pos, scale*1.523, (int)octaves + 2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, surfaceNoiseSettings2.x);
			float val = getSwissFractal(pos, scale*1.523, (int)octaves + 2, surfaceNoiseSettings.x, surfaceNoiseSettings.y, surfaceNoiseSettings.z, 1, 0.3);
			val = pow(val, surfaceNoiseSettings3.z);


/*			float pi = 3.14159265;
			float2 uv = float2(saturate(((atan2(pos.z, pos.x) / (1*pi)) + 1.0) / 2.0), (0.5 - (asin(pos.y) / pi)));			
			float craters = tex2Dlod(_Craters, float4(uv*1,0,0)).x*0.2;
			*/
			float craters = 0;
//			return craters;

			return clamp(val + craters*craterScale - surfaceNoiseSettings3.x, -0.1, 10);

		}

		float3 getHeightPosition(in float3 pos, in float scale, float heightScale, float octaves, float cs=1) {
			return pos*fInnerRadius *(1 + getSurfaceHeight(mul(rotMatrix, pos), scale, octaves,cs)*heightScale);
//			return pos*fInnerRadius*(1+getSurfaceHeight(mul(rotMatrix, pos) , scale, octaves)*heightScale);
			
		}



		float3 getSurfaceNormal(float3 pos, float scale,  float heightScale, float normalScale, float3 tangent, float3 bn, float octaves, int N) {
//			float3 getSurfaceNormal(float3 pos, float scale, float heightScale, float normalScale) {
			float3 prev = 0;
//			pos = normalize(pos);
			float hs = heightScale;
			float3 centerPos = getHeightPosition(normalize(pos), scale, hs, octaves);
			float3 norm = 0;

						for (float i=0;i<N;i++) {
							float3 disp = float3(cos(i/(N+0)*2.0*PI), 0, sin(i/(N+0)*2.0*PI));
							//float3 rotDisp = mul(tangentToWorld, disp);
							//float3 np = normalize(pos + mul(tangentToWorld, disp)*normalScale);
							//float3 np = normalize(pos + disp*normalScale);
							float3 np = normalize(pos + (disp.x*tangent + disp.z*bn) *normalScale);

							float3 newPos = getHeightPosition(np, scale, hs, octaves);

							if (length(prev)>0.1)
							{
								float3 n = normalize(cross(newPos - centerPos, prev - centerPos));
								float3 nn = n;
			//					if (dot(nn, normalize(pos)) < 0.0)
				//					nn *= -1;

								norm += nn;

							}
							prev = newPos;

						}
						

			return normalize(norm)*-1;
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
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves) + v3Translate;
			return mul(unity_WorldToObject, p);
		}

		float4 getPlanetSurfaceOnlyNoTranslate(in float4 v) {

			float4 p = mul(unity_ObjectToWorld, v);

			float octaves = surfaceNoiseSettings3.y;

			float scale = surfaceNoiseSettings2.z;
			float heightScale = surfaceNoiseSettings2.y;

			p.xyz = normalize(p.xyz);
			p.xyz = getHeightPosition(p.xyz, scale, heightScale, octaves);
			return mul(unity_WorldToObject, p);
		}

		#endif

