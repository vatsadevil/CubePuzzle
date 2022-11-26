Shader "Unlit/CustomShader1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("MainColor", Color) = (1,1,1,1)
        _Steps ("Steps", int) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Steps;
            float4 _MainColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // float3 dir = i.vertex - _WorldSpaceLightPos0.xyz;    
                float ang = dot(normalize(i.normal), dir);
                ang = clamp(ang, 0, 1);
                ang = floor(ang * _Steps)/_Steps;
                col = col * ang;
                col = col * _MainColor;
                return col;
            }
            ENDCG
        }
    }
}
