Shader "Custom/PaintableShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _PaintTex ("Paint Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        sampler2D _PaintTex;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the base texture
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);

            // Sample the paint texture
            fixed4 paintColor = tex2D(_PaintTex, IN.uv_MainTex);

            // Blend the base texture and paint texture
            fixed4 finalColor = lerp(baseColor, paintColor, paintColor.a);

            o.Albedo = finalColor.rgb * _Color.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}