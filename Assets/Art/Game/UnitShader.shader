Shader "Sprites/UnitShader"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)

		_ColorPalette ("Color Palette", 2D) = "white" {}
		_PaletteSize ("Palette Size", Int) = 24
		_PaletteIndex ("Palette Index", Int) = 1
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _ColorPalette;
			uniform float4 _ColorPalette_TexelSize;
			sampler2D _AlphaTex;
			float _AlphaSplitEnabled;
			int _PaletteSize;
			int _PaletteIndex;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				// Get main texture color
				fixed4 color = tex2D (_MainTex, uv);

				// Check all palette colors
				for (int y=0; y < _PaletteSize; ++y)
				{
					// Get palette color
					float2 pCoord = float2(0, y);
					float2 paletteUV = (pCoord+0.5) * _ColorPalette_TexelSize.xy;
					paletteUV.y = 1 - paletteUV.y;
					fixed4 paletteColor = tex2D(_ColorPalette, paletteUV);
					
					half3 delta = abs(color.rgb - paletteColor.rgb);
					if (length(delta) < 0.1)
					{
						// Color matches palette, set new color
						pCoord = float2(0, y+(_PaletteIndex * _PaletteSize));
						paletteUV = (pCoord+0.5) * _ColorPalette_TexelSize.xy;
						paletteUV.y = 1 - paletteUV.y;
						color.rgb = tex2D(_ColorPalette, paletteUV).rgb;
					}
				}

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}