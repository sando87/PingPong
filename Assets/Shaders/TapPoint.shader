Shader "Custom/TapPoint"
{
    Properties
    {
		_Color("Color", Color) = (1,0,0,1)
        _Rounds ("Rounds", Vector) = (1,0,0,1)
		_Rotate ("Rotate", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			float4 _Rounds;
			float _Rotate;

			v2f vert(appdata v)
			{
				v2f o;
				float angle = _Rotate * v.uv.x;
				float scale = _Rounds[v.uv.y];
				float4 rotVert = float4(cos(angle) * scale, sin(angle) * scale, 0, 1);
				o.vertex = UnityObjectToClipPos(rotVert);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return _Color;
			}

			ENDCG
		}
    }
}
