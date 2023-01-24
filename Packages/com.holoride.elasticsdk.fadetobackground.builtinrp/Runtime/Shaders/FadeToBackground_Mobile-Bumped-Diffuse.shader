// Modified by (c) holoride GmbH. All Rights Reserved.

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Bumped shader. Differences from regular Bumped one:
// - no Main Color
// - Normalmap uses Tiling/Offset of the Base texture
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "FadeToBackground/Mobile|Bumped Diffuse" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    [NoScaleOffset] _BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
    Tags { "RenderType"="Opaque" "FadeToBackgroundRole"="Opaque" }
    LOD 250

CGPROGRAM
#pragma surface surf Lambert noforwardadd keepalpha vertex:Vert finalcolor:FinalColor

sampler2D _MainTex;
sampler2D _BumpMap;

struct Input {
    float2 uv_MainTex;
    float2 viewPosXZ;
    UNITY_FOG_COORDS(0)
};

float4 _FadeToBackground_CameraPosition;
half _FadeToBackground_OpaqueDistanceSquared;
half _FadeToBackground_TransparentDistanceSquared;
half _FadeToBackground_Opacity = 1;

void Vert (inout appdata_full v, out Input data) {
    UNITY_INITIALIZE_OUTPUT(Input, data);
    float4 clipPos = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(data, clipPos);
    data.viewPosXZ = mul(unity_ObjectToWorld, v.vertex).xz - (_FadeToBackground_CameraPosition).xz;
}

void FinalColor(Input IN, SurfaceOutput o, inout fixed4 color) {
    UNITY_APPLY_FOG(IN.fogCoord, color);
    half fade = _FadeToBackground_Opacity * saturate((_FadeToBackground_TransparentDistanceSquared - dot(IN.viewPosXZ, IN.viewPosXZ)) / (_FadeToBackground_TransparentDistanceSquared - _FadeToBackground_OpaqueDistanceSquared));
    color *= fade;
}

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb;
    o.Alpha = c.a;
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Mobile/Diffuse"
}
