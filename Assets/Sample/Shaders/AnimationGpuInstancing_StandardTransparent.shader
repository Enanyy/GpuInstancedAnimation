// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "AnimationGpuInstancing/StandardTransparent" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[NoScaleOffset] _AnimTex("Animation Texture", 2D) = "white" {}
		[HideInInspector] [PerRendererData] _CurrentFrame("", Int) = 0
		[HideInInspector][PerRendererData] _PreviousFrame("", Int) = 0
		[HideInInspector][PerRendererData] _FadeStrength("", Range(0,1)) = 0
		[HideInInspector] _PixelCountPerFrame("", Int) = 0
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
		#include "UnityCG.cginc"

		#pragma multi_compile_instancing
		#pragma target 3.0
		struct Input {
			float2 uv_MainTex;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		half _Glossiness;
		half _Metallic;
		sampler2D _MainTex;
		sampler2D _AnimTex;
		float4 _AnimTex_TexelSize;
		int _PixelCountPerFrame;

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(int, _CurrentFrame)
#define _CurrentFrame_arr Props
			UNITY_DEFINE_INSTANCED_PROP(int, _PreviousFrame)
#define _PreviousFrame_arr Props
			UNITY_DEFINE_INSTANCED_PROP(int, _BlendFrame)
#define _BlendFrame_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _BlendDirection)
#define _BlendDirection_arr Props
			UNITY_DEFINE_INSTANCED_PROP(float, _FadeStrength)
#define _FadeStrength_arr Props
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _Color_arr Props
			UNITY_INSTANCING_BUFFER_END(Props)

			float4 GetUV(int index)
		{
			int row = index / (int)_AnimTex_TexelSize.z;
			int col = index % (int)_AnimTex_TexelSize.z;

			return float4(col / _AnimTex_TexelSize.z, row / _AnimTex_TexelSize.w, 0, 0);
		}

		float4x4 GetMatrix(int startIndex, float boneIndex)
		{
			int matrixIndex = startIndex + boneIndex * 3;

			float4 row0 = tex2Dlod(_AnimTex, GetUV(matrixIndex));
			float4 row1 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 1));
			float4 row2 = tex2Dlod(_AnimTex, GetUV(matrixIndex + 2));
			float4 row3 = float4(0, 0, 0, 1);

			return float4x4(row0, row1, row2, row3);
		}

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			half4 boneIndex : TEXCOORD2;
			fixed4 boneWeight : TEXCOORD3;
			fixed4 boneHeight : TEXCOORD4;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		struct VertexData
		{
			float4 vertex;
			float4 normal;
		};
		VertexData GetVertex(int currentFrame, appdata v)
		{
			VertexData o;
			int clampedIndex = currentFrame * _PixelCountPerFrame;

			float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x);
			float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y);
			float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z);
			float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w);

			o.vertex =
				mul(bone1Matrix, v.vertex) * v.boneWeight.x +
				mul(bone2Matrix, v.vertex) * v.boneWeight.y +
				mul(bone3Matrix, v.vertex) * v.boneWeight.z +
				mul(bone4Matrix, v.vertex) * v.boneWeight.w;

			o.normal =
				mul(bone1Matrix, v.normal) * v.boneWeight.x +
				mul(bone2Matrix, v.normal) * v.boneWeight.y +
				mul(bone3Matrix, v.normal) * v.boneWeight.z +
				mul(bone4Matrix, v.normal) * v.boneWeight.w;


			return o;
		}
		float GetHeightWeight(appdata v)
		{
			float heightWeight = v.boneHeight.x * v.boneWeight.x +
				v.boneHeight.y * v.boneWeight.y +
				v.boneHeight.z * v.boneWeight.z +
				v.boneHeight.w * v.boneWeight.w;
			return saturate(heightWeight);
		}
		void vert(inout appdata v, out Input o)
		{
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			UNITY_INITIALIZE_OUTPUT(Input, o);

			int currentFrame = UNITY_ACCESS_INSTANCED_PROP(_CurrentFrame_arr, _CurrentFrame);
			int blendFrame = UNITY_ACCESS_INSTANCED_PROP(_BlendFrame_arr, _BlendFrame);

			VertexData current = GetVertex(currentFrame, v);

			float fadeStrength = UNITY_ACCESS_INSTANCED_PROP(_FadeStrength_arr, _FadeStrength);
			//fadeStrength由外部C#传入，对于所有顶点都是一样的，不存在并行运算时某个顶点先计算完成需要等待其他顶点的情况
			if (fadeStrength >= 0)
			{
				int previousFrame = UNITY_ACCESS_INSTANCED_PROP(_PreviousFrame_arr, _PreviousFrame);

				VertexData previous = GetVertex(previousFrame, v);

				current.vertex = previous.vertex * (1 - fadeStrength) + current.vertex * fadeStrength;
				current.normal = previous.normal * (1 - fadeStrength) + current.normal * fadeStrength;

				if (blendFrame > 0)
				{
					VertexData blend = GetVertex(blendFrame, v);
					blend.vertex = previous.vertex * (1 - fadeStrength) + blend.vertex * fadeStrength;
					blend.normal = previous.normal * (1 - fadeStrength) + blend.normal * fadeStrength;

					float heightWeight = GetHeightWeight(v);

					float blendDirection = UNITY_ACCESS_INSTANCED_PROP(_BlendDirection_arr, _BlendDirection);

					current.vertex = blend.vertex * abs(1 - heightWeight - blendDirection) + current.vertex * abs(heightWeight - blendDirection);
					current.normal = blend.vertex * abs(1 - heightWeight - blendDirection) + current.normal * abs(heightWeight - blendDirection);
				}
			}
			else
			{
				if (blendFrame > 0)
				{
					VertexData blend = GetVertex(blendFrame, v);
					float heightWeight = GetHeightWeight(v);

					float blendDirection = UNITY_ACCESS_INSTANCED_PROP(_BlendDirection_arr, _BlendDirection);

					current.vertex = blend.vertex * abs(1 - heightWeight - blendDirection) + current.vertex * abs(heightWeight - blendDirection);
					current.normal = blend.vertex * abs(1 - heightWeight - blendDirection) + current.normal * abs(heightWeight - blendDirection);
				}
			}


			v.vertex = current.vertex;
			v.normal = current.normal;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
