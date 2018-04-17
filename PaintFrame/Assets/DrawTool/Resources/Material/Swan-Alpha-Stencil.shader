Shader "Swan/AlphaBlend_Stencil" {
		Properties{
			_Color("Main color",Color) = (1,1,1,1)
			_MainTex("Particle Texture", 2D) = "white" {}
			_light("亮度", float) = 1
			_Stencil("Stencil ID", Float) = 4
		}

			Category{
			Tags { "Queue" = "Transparent+200" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha One
			Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

		SubShader {
			Pass{
				Stencil
				{
					Ref[_Stencil]
					Comp Equal
					Pass Keep
				}
				Blend SrcAlpha OneMinusSrcAlpha
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;

				fixed4 _Color;
				float _light;

				struct appdata_t
				{
					float4 vertex   : POSITION;
					fixed4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					fixed4 color : COLOR;
				};

				v2f vert(appdata_t v)
				{
					v2f OUT;
					OUT.pos = UnityObjectToClipPos(v.vertex);
					OUT.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					OUT.color = v.color * _Color * _light;
					return OUT;
				}

				fixed4 frag(v2f i) : COLOR
				{
					fixed4 mainTex = tex2D(_MainTex, i.uv) * i.color;
					return mainTex;
				}
				ENDCG
			}
		}
	}
}
