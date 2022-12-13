#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
#include "noise_perlin.cginc"



struct appdata
{
    fixed4 vertex : POSITION;
    fixed2 uv : TEXCOORD0;
    fixed3 normal : NORMAL;
    fixed4 vertColor : COLOR;
};
struct v2f
{
    fixed2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)
    fixed4 vertex : SV_POSITION;
    LIGHTING_COORDS(3,4)
    fixed3 normal : TEXCOORD5;
    fixed3 wPos : TEXCOORD6;
    fixed4 vertColor : COLOR;
};
sampler2D _MainTex;
fixed4 _MainTex_ST;
fixed4 _ColorTint;
fixed _AmbientIntensity;
fixed _Gloss;
fixed _Fernel;
fixed fernel;
fixed _Saturation;

#define MAX_NOISE_VALUE 0.15f
#define NOISE_STEP 5

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.wPos = mul(unity_ObjectToWorld, v.vertex);
 
    o.vertColor = v.vertColor; 
    UNITY_TRANSFER_FOG(o,o.vertex);
    TRANSFER_VERTEX_TO_FRAGMENT(o); // lighting
    // TRANSFER_SHADOW(o)
    // UNITY_TRANSFER_LIGHTING(o, v.uv);
    return o; 
}

fixed4 frag (v2f i) : SV_Target
{
    // FRAGMENT_SETUP(s)
    fixed4 output = tex2D(_MainTex, i.uv);
    // fixed shadow = SHADOW_ATTENUATION(i);
    fixed attenuation = LIGHT_ATTENUATION(i);
    // UNITY_LIGHT_ATTENUATION(attenuation, i, s.posWorld);
    
    fixed noiseColor = 0;
    Unity_SimpleNoise_float(i.wPos.xz, 0.5f, noiseColor);
    // noiseColor = (fixed(int(noiseColor * NOISE_STEP)) / NOISE_STEP) * MAX_NOISE_VALUE;
    noiseColor *= MAX_NOISE_VALUE;

    fixed3 N = normalize(i.normal);
    fixed3 L = normalize(UnityWorldSpaceLightDir(i.wPos));
    fixed3 V = normalize(_WorldSpaceCameraPos - i.wPos);
    fixed3 H =  normalize(L + V); 
    fixed glossExp = exp2(_Gloss * 10);

    fixed diffuse = saturate(dot(N, L)) * attenuation - MAX_NOISE_VALUE;
    fixed specular = saturate(dot(H, N)) * (diffuse > 0);
    specular = pow(specular, glossExp) * (_Gloss * 0.2f);
    
    output = ((i.vertColor * _Saturation) * (1 - _AmbientIntensity));

    output = output * diffuse;
    output = output + (specular * attenuation);

    output += (noiseColor * i.vertColor);

    #ifdef BASE_PASS
    

    
    fixed fernel = 1 - saturate(dot(N, V));
    fernel = pow(fernel, _Fernel * 10);
    fernel = fernel * _Fernel; 
    output = output + (_AmbientIntensity * i.vertColor * attenuation);
    output = output + fernel;
    #endif

    // output *= shadow;
    UNITY_APPLY_FOG(i.fogCoord, output);
    
    return saturate(output);
}