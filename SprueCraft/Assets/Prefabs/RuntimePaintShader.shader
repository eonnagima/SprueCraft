Shader "Custom/RuntimePaintShader"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {} // The base texture of the object
        _PaintTex ("Paint Texture", 2D) = "white" {} // The paint texture (updated dynamically)
        _Color ("Color Tint", Color) = (1,1,1,1) // Optional color tint
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex; // Base texture
        sampler2D _PaintTex; // Paint texture
        fixed4 _Color; // Color tint

        struct Input
        {
            float2 uv_MainTex; // UV coordinates for the base texture
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the base texture
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);

            // Sample the paint texture
            fixed4 paintColor = tex2D(_PaintTex, IN.uv_MainTex);

            // Blend the base texture and paint texture
            fixed4 finalColor = lerp(baseColor, paintColor, paintColor.a);

            // Apply the final color to the surface
            o.Albedo = finalColor.rgb * _Color.rgb; // Apply the color tint
            o.Alpha = finalColor.a; // Set the alpha value
        }
        ENDCG
    }
    FallBack "Standard"
}
