Shader "Unlit/Test_Shaders"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Gloss("Gloss", Range(0, 1)) = 0.5
        _Fernel("Fernel", Range(0, 1)) = 0.5
        _AmbientLight("Ambient Light", Range(0, 1)) = 0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #define BASE_PASS

            #include "MyShader.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend one one
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd

            #include "MyShader.cginc"
            ENDCG
        }
    }
}
// Unlit/Test_Shaders shader is not supported on this GPU (none of subshaders/fallbacks are suitable)
//
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
// #pragma exclude_renderers d3d11