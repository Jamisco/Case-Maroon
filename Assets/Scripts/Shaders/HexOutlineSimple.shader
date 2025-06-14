Shader "Unlit/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineSize("Outline Size", Range(1,10)) = 1 // Add outline size control
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
        }
 
        Blend SrcAlpha OneMinusSrcAlpha
 
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineSize;
 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                fixed outline = 0;
                
                // Sample in a larger area based on OutlineSize
                for(int j = 1; j <= _OutlineSize; j++)
                {
                    // Check horizontal and vertical neighbors
                    fixed leftPixel = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x * j, 0)).a;
                    fixed upPixel = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y * j)).a;
                    fixed rightPixel = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x * j, 0)).a;
                    fixed bottomPixel = tex2D(_MainTex, i.uv + float2(0, -_MainTex_TexelSize.y * j)).a;
                    
                    // Check diagonal neighbors
                    fixed topRightPixel = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x * j, _MainTex_TexelSize.y * j)).a;
                    fixed topLeftPixel = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x * j, _MainTex_TexelSize.y * j)).a;
                    fixed bottomRightPixel = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x * j, -_MainTex_TexelSize.y * j)).a;
                    fixed bottomLeftPixel = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x * j, -_MainTex_TexelSize.y * j)).a;
                    
                    outline = max(outline, (1 - leftPixel * upPixel * rightPixel * bottomPixel * 
                                             topRightPixel * topLeftPixel * bottomRightPixel * bottomLeftPixel) * col.a);
                }
 
                return lerp(col, _OutlineColor, outline);
            }
            ENDCG
        }
    }
}