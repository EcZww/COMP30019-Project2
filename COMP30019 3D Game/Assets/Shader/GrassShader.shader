 Shader "Custom/GrassShader"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_TipColor("Tip Color", Color) = (1, 1, 1, 1)
		_BladeTexture("Blade Texture", 2D) = "white" {}

		_BladeWidthMin("Blade Width (Min)", Range(0, 0.1)) = 0.02
		_BladeWidthMax("Blade Width (Max)", Range(0, 0.1)) = 0.05
		_BladeHeightMin("Blade Height (Min)", Range(0, 2)) = 0.1
		_BladeHeightMax("Blade Height (Max)", Range(0, 2)) = 0.2

		_BladeSegments("Blade Segments", Range(1, 10)) = 3
		_BladeBendDistance("Blade Forward Amount", Float) = 0.38
		_BladeBendCurve("Blade Curvature Amount", Range(1, 4)) = 2

		_BendDelta("Bend Variation", Range(0, 1)) = 0.2

		_Density_of_Grass("Density of Grass", Range(0.01, 2)) = 0.1

		_WindMap("Wind Offset Map", 2D) = "bump" {}
		_WindVelocity("Wind Velocity", Vector) = (0.01, 0, 0.01, 0)
		_WindFrequency("Wind Pulse Frequency", Range(0, 1)) = 0.01

		_ShadowColor ("Shadow Color", Color) = (0.35,0.4,0.45,1.0)

		_SpecColor("Specular", Color) = (1.0, 1.0, 1.0, 1.0)
        _Smoothness("Gloss", Range(8.0, 256)) = 20
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100
		Cull Off

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f
			#define BLADE_SEGMENTS 4
			
			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _TipColor;
				sampler2D _BladeTexture;

				float _BladeWidthMin;
				float _BladeWidthMax;
				float _BladeHeightMin;
				float _BladeHeightMax;
				float _BladeHeight;
				float _BladeWidth;

				float _BladeBendDistance;
				float _BladeBendCurve;

				float _BendDelta;

				float _Density_of_Grass;

				sampler2D _WindMap;
				float4 _WindMap_ST;
				float4 _WindVelocity;
				float  _WindFrequency;
				float4 _SpecColor;
				float _Smoothness;

				float4 _ShadowColor;
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex  : POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex  : SV_POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside  : SV_InsideTessFactor;
			};

			struct GeomData
			{
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normal : NORMAL;
			};

			


			// Following functions from Roystan's code:
			// (https://github.com/IronWarrior/UnityGrassGeometryShader)

			// Randomness in shaders, sourced from http://answers.unity.com/answers/624136/view.html
			// Extended discussion on this function can be found at the following link:
			// Returns a number in the 0...1 range.
			float rand(float3 co)
			{
				return frac(sin( dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
			}

			// rotation matrix that rotates around the provided axis with given angle, sourced from:
			// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
			float3x3 angleAxis3x3(float angle, float3 axis)
			{
				float c, s;
				sincos(angle, s, c);

				float t = 1 - c;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;

				return float3x3
				(
					t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
			}

			// Regular vertex shader used by typical shaders.
			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.vertex = TransformObjectToHClip(v.vertex.xyz);
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.uv = v.uv;
				return o;
			}

			// Vertex shader which just passes data to tessellation stage.
			VertexOutput tessVert(VertexInput v)
			{
				VertexOutput o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.uv = v.uv;
				return o;
			}

			// Vertex shader which translates from object to world space.
			VertexOutput geomVert (VertexInput v)
            {
				VertexOutput o; 
				o.vertex = float4(TransformObjectToWorld(v.vertex), 1.0f);
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.uv = v.uv;
                return o;
            }

			//  Function to control the factor that will be used to create the point in
			//  tessletation stage. The large factor will make the grass denser.
			float tessellationEdgeFactor(VertexInput vert0, VertexInput vert1)
			{
				float3 v0 = vert0.vertex.xyz;
				float3 v1 = vert1.vertex.xyz;
				float edgeLength = distance(v0, v1);
				return edgeLength / _Density_of_Grass;
			}

			// Tessellation hull and domain shaders derived from Catlike Coding's tutorial:
			// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

			// The patch constant function is where we create new control
			// points on the patch.
			TessellationFactors patchConstantFunc(InputPatch<VertexInput, 3> patch)
			{
				TessellationFactors f;

				f.edge[0] = tessellationEdgeFactor(patch[1], patch[2]);
				f.edge[1] = tessellationEdgeFactor(patch[2], patch[0]);
				f.edge[2] = tessellationEdgeFactor(patch[0], patch[1]);
				f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0f;

				return f;
			}

			// The hull function operates on each patch (in our case, a patch is a triangle),
			// and outputs new control points for the other tessellation stages.

			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("patchConstantFunc")]
			VertexInput hull(InputPatch<VertexInput, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			// The domain function interpolates the properties of the vertices (position, normal, etc.)
			// to create new vertices.
			[domain("tri")]
			VertexOutput domain(TessellationFactors factors, OutputPatch<VertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				VertexInput data;

				#define DOMAIN_INTERPOLATE(fieldname) data.fieldname = \
					patch[0].fieldname * barycentricCoordinates.x + \
					patch[1].fieldname * barycentricCoordinates.y + \
					patch[2].fieldname * barycentricCoordinates.z;

				DOMAIN_INTERPOLATE(vertex)
				DOMAIN_INTERPOLATE(normal)
				DOMAIN_INTERPOLATE(tangent)
				DOMAIN_INTERPOLATE(uv)

				return tessVert(data);
			}

			// Geometry functions derived from Roystan's tutorial:
			// https://roystan.net/articles/grass-shader.html

			// This function applies a transformation (during the geometry shader),
			// converting to clip space in the process.
			GeomData TransformGeomToClip(float3 pos, float3 offset, float3x3 transformationMatrix, float2 uv,float3 normal)
			{
				GeomData o;
				o.normal = normal;
				o.pos = TransformObjectToHClip(pos + mul(transformationMatrix, offset));
				o.uv = uv;
				o.worldPos = TransformObjectToWorld(pos + mul(transformationMatrix, offset));

				return o;
			}

			// This is the geometry shader. For each vertex on the mesh, a leaf
			// blade is created by generating additional vertices.
			[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
			void geom(point VertexOutput input[1], inout TriangleStream<GeomData> triStream)
			{


				
					float3 root = input[0].vertex.xyz;

					float3 normal = input[0].normal;
					float4 tangent = input[0].tangent;
					float3 bitangent = cross(normal, tangent.xyz) * tangent.w;

					float3x3 tangentToLocal = float3x3
					(
						tangent.x, bitangent.x, normal.x,
						tangent.y, bitangent.y, normal.y,
						tangent.z, bitangent.z, normal.z
					);

					// Rotate around the y-axis a random amount.
					float3x3 randRotMatrix = angleAxis3x3(rand(root) * UNITY_TWO_PI, float3(0, 0, 1.0f));

					// Rotate around the bottom of the blade a random amount.
					float3x3 randBendMatrix = angleAxis3x3(rand(root.zzx) * _BendDelta * UNITY_PI * 0.5f, float3(-1.0f, 0, 0));

					float2 windUV = root.xz * _WindMap_ST.yz + _WindMap_ST.xy + normalize(_WindVelocity.yzw) * _WindFrequency * _Time.y;
					float2 windSampleMap = (tex2Dlod(_WindMap, float4(windUV, 0, 0)).xy * 2 - 1) * length(_WindVelocity);

					float3 windAxis = normalize(float3(windSampleMap.x, windSampleMap.y, 0));
					float3x3 windMatrix = angleAxis3x3(UNITY_PI * windSampleMap, windAxis);

					// Transform the grass blades to the correct tangent space.
					float3x3 baseTransformationMatrix = mul(tangentToLocal, randRotMatrix);
					float3x3 tipTransformationMatrix = mul(mul(mul(tangentToLocal, windMatrix), randBendMatrix), randRotMatrix);


					float width  = lerp(_BladeWidthMin, _BladeWidthMax, rand(root.xzy));
					float height = lerp(_BladeHeightMin, _BladeHeightMax, rand(root.zyx) );
					float forward = rand(root.yyz) * _BladeBendDistance;

					// Create blade segments by adding two vertices at once.
					for (int i = 0; i < BLADE_SEGMENTS; ++i)
					{
						float t = i / (float)BLADE_SEGMENTS;
						float3 offset = float3(width * (1 - t), pow(t, _BladeBendCurve) * forward, height * t);

						float3x3 transformationMatrix = (i == 0) ? baseTransformationMatrix : tipTransformationMatrix;
						triStream.Append(TransformGeomToClip(root, float3( offset.x, offset.y, offset.z), transformationMatrix, float2(0, t),normal));
						triStream.Append(TransformGeomToClip(root, float3(-offset.x, offset.y, offset.z), transformationMatrix, float2(1, t),normal));
					}

					// Add the final vertex at the tip of the grass blade.
					triStream.Append(TransformGeomToClip(root, float3(0, forward, height), tipTransformationMatrix, float2(0.5, 1),normal));

				
			}
		ENDHLSL

		// This pass draws the grass blades generated by the geometry shader.
        Pass
        {
			Name "GrassPass"
			Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
			#pragma require geometry
			#pragma require tessellation tessHW

			//#pragma vertex vert
			#pragma vertex geomVert
			#pragma hull hull
			#pragma domain domain
			#pragma geometry geom
            #pragma fragment frag
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            float4 frag (GeomData input) : SV_Target
			
            {
				// Blinn-Phong lighting model

				real3 positionWS = input.worldPos;
				Light mainLight = GetMainLight();
				real3 lightColor = mainLight.color; 
				real3 normalWS = normalize(input.normal);
				real3 lightDir = normalize(mainLight.direction);
				float4 albedo = tex2D(_BladeTexture, input.uv) ;
				real3 ambient = UNITY_LIGHTMODEL_AMBIENT * 2 * albedo;
				real3 diffuse = saturate(dot(lightDir,normalWS)) * lightColor * albedo;
				real3 viewDirectionWS = normalize(GetCameraPositionWS() - positionWS);
				real3 halfVector = normalize(viewDirectionWS + lightDir);
				real3 specular = pow(saturate(dot(halfVector, normalWS)), _Smoothness) * lightColor * saturate(_SpecColor);

				float3 light = ambient + diffuse + specular;

				// lerp function to achieve a gradient color from the root  to the top
				
				return real4( light,1.0f) * lerp(_BaseColor, _TipColor, input.uv.y);
                 
			}

			ENDHLSL
		}
    }
}