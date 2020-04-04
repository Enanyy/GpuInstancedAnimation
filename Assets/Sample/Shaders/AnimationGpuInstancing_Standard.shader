// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "AnimationGpuInstancing/Standard" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[NoScaleOffset] _AnimTex("Animation Texture", 2D) = "white" {}
		[HideInInspector][PerRendererData] _CurrentFrame("", Int) = 0
		[HideInInspector][PerRendererData] _PreviousFrame("", Int) = 0
		[HideInInspector][PerRendererData] _FadeStrength("", Range(0,1)) = 0

		[HideInInspector] _PixelCountPerFrame("", Int) = 0
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "UnityCG.cginc"

		#pragma multi_compile_instancing
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AnimTex;
		float4 _AnimTex_TexelSize;

		struct Input {
			float2 uv_MainTex;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		half _Glossiness;
		half _Metallic;

		int _PixelCountPerFrame;

		UNITY_INSTANCING_BUFFER_START(Props)

		UNITY_DEFINE_INSTANCED_PROP(int, _CurrentFrame)
#define _CurrentFrame_arr Props
		UNITY_DEFINE_INSTANCED_PROP(int, _PreviousFrame)
#define _PreviousFrame_arr Props
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
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		void vert(inout appdata v, out Input o)
        {
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			UNITY_INITIALIZE_OUTPUT(Input, o);

			int currentFrame = UNITY_ACCESS_INSTANCED_PROP(_CurrentFrame_arr, _CurrentFrame);
			
			int clampedIndex = currentFrame * _PixelCountPerFrame;
			
			float4x4 bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x);
			float4x4 bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y);
			float4x4 bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z);
			float4x4 bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w);

			float4 currentVertex =
				mul(bone1Matrix, v.vertex) * v.boneWeight.x +
				mul(bone2Matrix, v.vertex) * v.boneWeight.y +
				mul(bone3Matrix, v.vertex) * v.boneWeight.z +
				mul(bone4Matrix, v.vertex) * v.boneWeight.w;

			float4 currentNormal =
				mul(bone1Matrix, v.normal) * v.boneWeight.x +
				mul(bone2Matrix, v.normal) * v.boneWeight.y +
				mul(bone3Matrix, v.normal) * v.boneWeight.z +
				mul(bone4Matrix, v.normal) * v.boneWeight.w;

			float fadeStrength = UNITY_ACCESS_INSTANCED_PROP(_FadeStrength_arr, _FadeStrength);
             //fadeStrength由外部C#传入，对于所有顶点都是一样的，不存在并行运算时某个顶点先计算完成需要等待其他顶点的情况
            if (fadeStrength  >= 0)
            {
			    currentFrame = UNITY_ACCESS_INSTANCED_PROP(_PreviousFrame_arr, _PreviousFrame);
			    clampedIndex = currentFrame * _PixelCountPerFrame;
			    bone1Matrix = GetMatrix(clampedIndex, v.boneIndex.x);
			    bone2Matrix = GetMatrix(clampedIndex, v.boneIndex.y);
			    bone3Matrix = GetMatrix(clampedIndex, v.boneIndex.z);
			    bone4Matrix = GetMatrix(clampedIndex, v.boneIndex.w);

			    float4 previousVertex = mul(bone1Matrix, v.vertex) * v.boneWeight.x +
										mul(bone2Matrix, v.vertex) * v.boneWeight.y +
										mul(bone3Matrix, v.vertex) * v.boneWeight.z +
										mul(bone4Matrix, v.vertex) * v.boneWeight.w;
			    float4 previousNormal = mul(bone1Matrix, v.normal) * v.boneWeight.x +
										mul(bone2Matrix, v.normal) * v.boneWeight.y +
										mul(bone3Matrix, v.normal) * v.boneWeight.z +
										mul(bone4Matrix, v.normal) * v.boneWeight.w;

			    v.vertex = previousVertex * (1 - fadeStrength) + currentVertex * fadeStrength;
			    v.normal = previousNormal * ( 1 - fadeStrength) + currentNormal * fadeStrength;
            }
            else
            {   
                v.vertex = currentVertex;
                v.normal = currentNormal;         
            }

           
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
