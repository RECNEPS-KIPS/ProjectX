Shader "Hidden/ChocDino/UIFX/BoxBlur-Reference"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	float _KernelRadius;

	v2f vert (appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	float4 Blur(v2f i, float2 texelSize)
	{
		float4 accum = tex2D(_MainTex, i.uv);

		float2 step = texelSize;
		int m = floor(_KernelRadius);
		float2 uv = i.uv;
		float2 offset = step;
		for (int i = 0; i < m; i++)
		{
			accum += tex2D(_MainTex, uv + offset);
			accum += tex2D(_MainTex, uv - offset);
			offset += step;
		}

		float remainder = _KernelRadius - m;
		if (remainder > 0.0)
		{
			float2 correction = step * remainder;
			accum += tex2D(_MainTex, uv + (step * m + correction)) * remainder;
			accum += tex2D(_MainTex, uv - (step * m + correction)) * remainder;
		}

		return accum / (_KernelRadius + _KernelRadius + 1.0);
	}

	float4 fragH(v2f i) : SV_Target
	{
		return Blur(i, float2(_MainTex_TexelSize.x, 0.0));
	}

	float4 fragV(v2f i) : SV_Target
	{
		return Blur(i, float2(0.0, _MainTex_TexelSize.y));
	}
	
	ENDCG

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
			"OutputsPremultipliedAlpha"="True"
		}

		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Name "BlurHorizontal"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragH
			ENDCG
		}

		Pass
		{
			Name "BlurVertical"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragV
			ENDCG
		}
	}
}