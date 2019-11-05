Shader "Custom/BackGround"
{
    Properties
    {
        //_Color ("Color", Color) = (1,0,0,1)
		_Off ("Off", Float) = 0
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			fixed4 _Colors[10];
			float _Off;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				int degree = i.vertex.y / 10 + _Off; //pixel(0~1800) -> degree(0~180)
				float radian = (float)degree * 3.141592f / 180.0f;
				uint rawIdx = degree / 360;
				int centerIdx = rawIdx % 10; //color array index
				int nextIdx = (rawIdx + 1) % 10; //color array next index
				int preIdx = rawIdx <= 0 ? 0 : (rawIdx - 1) % 10; //color array previous index
				fixed4 mainColor = _Colors[centerIdx];
				fixed4 subColor = sin(radian) > 0 ? _Colors[preIdx] : _Colors[nextIdx];
				float rate = 0.75f - 0.25f * cos(radian); //0.5f~1.0f period
				
				fixed4 col = mainColor * rate + subColor * (1.0f - rate);
				return col;
			}
			ENDCG
		}
    }
}
