Shader "Custom/SimpleWater"
{
    Properties

    {

		//  color of the water below its shadow.
		_ShalowColor("Shallow Color", Color) = (0.325, 0.807, 0.971, 0.725)

		// color of the water below is at its deepest.
		_DeepColor("DeepColor", Color) = (0.086, 0.407, 1, 0.749)

		// Maximum distance for the color gradient to be the deepest one.
		_DepthMaxDistance("Depth Maximum Distance", Float) = 1

		// Color to render the foam
		_FoamColor("Foam Color", Color) = (1,1,1,1)

		// Noise texture
		_SurfaceNoise("Surface Noise", 2D) = "white" {}

		// Speed, in UVs per second the noise will scroll. Only the xy components are used.
		_SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)

		// Values in the noise texture above this cutoff are rendered on the surface.
		_SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777

		// Red and green channels of this texture are used to offset the
		// noise texture to create distortion in the waves.
		_SurfaceDistortion("Surface Distortion", 2D) = "white" {}	

		// Multiplies the distortion by this value.
		_SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

		// Control the distance that surfaces below the water will contribute
		// to foam being rendered.
		_FoamMaxDistance("Foam Maximum Distance", Float) = 0.4

		_FoamMinDistance("Foam Minimum Distance", Float) = 0.04	
		
		_Intensity("intensity", Float) = 0.1        
        _XSpeed("Flow Speed", Float) = -0.2         
        _WaveScale("Wave Scale", Float) = 0.1       
        _WaveStrength("Wave Strength", Float) = 0.1  
		_Specular("Specular", Float) = 0.1
		_Gloss("Gloss", Float) = 0.1
		_SpecularColor("Specular Color", Color) = (1,1,1,1)
		_FoamDepth("Foam Depth", Float) = 0.2
		_Foam("Foam", 2D) = "bump" {} 
		_FoamFactor("Foam Factor", Float) = 0.5 
		_NormalScale("Normal Scale", Float) = 0.1
		_RimColor("RimColor", Color) = (1,1,1,1)
		_RimPower("RimPower", Range(0.000001, 3.0)) = 0.1

    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"LightMode" = "UniversalForward"
		}

        Pass
        {
			// Transparent "normal" blending.
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM
			#define SMOOTHSTEP_AA 0.01

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			

			// Blends two colors using the same algorithm that our shader is using
			// to blend with the screen. This is usually called "normal blending",
			// and is similar to how software like Photoshop blends two layers.
			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}


            struct appdata
            {
                float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;	
				float2 noiseUV : TEXCOORD0;
				float2 distortUV : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 viewNormal : NORMAL;
				float2 uv : TEXCOORD3;
				float3 worldNormal : NORMAL1;
				float3 worldPos: TEXCOORD4;
            };

			sampler2D _SurfaceNoise;
			float4 _SurfaceNoise_ST;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SurfaceDistortion;
			float4 _SurfaceDistortion_ST;
			float _Intensity;
            float _XSpeed;
            float _WaveScale;
            float _WaveStrength;
			float _Specular;
			float _Gloss;
			float4 _Form_ST;

			float calculateSurfaceHeight(float x, float z, float scale)
            {
                float y = 0.0;
                y += (sin(x * 1.0 / scale + _Time.y * 1.0) + sin(x * 2.3 / scale + _Time.y * 1.5) + sin(x * 3.3 / scale + _Time.y * 0.4)) / 3.0;
                y += (sin(z * 0.2 / scale + _Time.y * 1.8) + sin(z * 1.8 / scale + _Time.y * 1.8) + sin(z * 2.8 / scale + _Time.y * 0.8)) / 3.0;
                return y;
            }

            v2f vert (appdata v)
            {
				v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				v.vertex.y += _WaveStrength * calculateSurfaceHeight(v.vertex.x, v.vertex.z, _WaveScale);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
				o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
				o.viewNormal = COMPUTE_VIEW_NORMAL;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }


			float4 _ShalowColor;
			float4 _DeepColor;
			float4 _FoamColor;

			float _DepthMaxDistance;
			float _FoamMaxDistance;
			float _FoamMinDistance;
			float _SurfaceNoiseCutoff;
			float _SurfaceDistortionAmount;

			float2 _SurfaceNoiseScroll;

			float4 _FoamOffset;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraNormalsTexture;
			float _FoamDepth;
			sampler2D _Foam;
			float _FoamFactor;
			fixed4 _RimColor;
			float _RimPower;
			float4 _SpecularColor;

            float4 frag (v2f i) : SV_Target
            {

				float3 worldNormal = i.worldNormal;
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float NdotV = saturate(dot(worldNormal,viewDir));

				// Retrieve the current depth value of the surface behind the
				// pixel we are currently rendering.
				half4 screenPos = i.screenPos;
				half eyeDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( screenPos ))));
				half eyeDepthToScreenPos = abs( eyeDepth - screenPos.w );
		
				float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(screenPos));
				
				// Modulate the amount of foam we display based on the difference
				// between the normals of our water surface and the object behind it.
				// Larger differences allow for extra foam to attempt to keep the overall
				// amount consistent.
				float3 normalDot = saturate(dot(existingNormal, i.viewNormal));
				float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
				float foamDepthDifference = saturate(eyeDepthToScreenPos / foamDistance);

				float surfaceNoiseCutoff = foamDepthDifference * _SurfaceNoiseCutoff;

				float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

				// Distort the noise UV based off the RG channels (using xy here) of the distortion texture.
				// Also offset it by time, scaled by the scroll speed.
				float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, 
				(i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);
				float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

				// Use smoothstep to ensure we get some anti-aliasing in the transition from foam to surface.
				// Uncomment the line below to see how it looks without AA.
				// float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
				float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

				float4 surfaceNoiseColor = _FoamColor;
				surfaceNoiseColor.a *= surfaceNoise;

				// Calculate the color of the water based on the depth using our two gradient colors.
				float waterDepthDifference = saturate(eyeDepthToScreenPos / _DepthMaxDistance);
				float4 waterColor = lerp(_ShalowColor, _DeepColor, waterDepthDifference);

				// Calculate the _FormColor and the diffuse color with the interplation
				// interplation is calculated by the green channel of the data(sample _Foam on constructing a uv)
				// and params: _FoamFactor
				half foamMask = 1 - eyeDepthToScreenPos + _FoamDepth;
				half3 foam1 = tex2D(_Foam, i.uv + worldNormal.xy);
				float temp_output = ( saturate( foam1.g * foamMask - _FoamFactor));

				
				fixed3 diffuse = _LightColor0.rgb *waterColor*NdotV ;
				diffuse = lerp( diffuse , _FoamColor, temp_output);

				// RimLight effect: When the direction of sight is not perpendicular to the surface,
				// the smaller the angle, the more obvious the rim color will be

				float rim = 1 - max(0, dot(viewDir, worldNormal));

				fixed3 rimColor = _RimColor * pow(rim, 1 / _RimPower);

				// Blinn-Phong lighting effect 
				// sourced from https://www.jordanstevenstechart.com/lighting-models
				fixed3 halfDir = normalize(lightDir + viewDir);
                fixed3 specular =  _Specular * pow(max(0,dot(worldNormal, halfDir)), _Gloss) * _SpecularColor;
				waterColor = fixed4((max(0,dot(worldNormal, lightDir))*diffuse.rgb+specular+ rimColor),1);

				// Use normal alpha blending to combine the foam with the surface.
				return alphaBlend(surfaceNoiseColor, waterColor);
				// return waterColor;
            }
            ENDCG
        }
    }
}