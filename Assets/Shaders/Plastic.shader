// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "cfantauzzo/LU/Plastic"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		_Decal("Decal", 2D) = "white" {}
		_GlowMap("Glow Map", 2D) = "black" {}
		_Specular("Specular", Range( 0 , 1)) = 0.9
		_SpecColor("Specular Color",Color)=(1,1,1,1)
		_ReflectionMap("Reflection Map", CUBE) = "white" {}
		_ReflectionStrength("Reflection Strength", Range( 0 , 1)) = 0.06
		_Glow("Glow", Range( 0 , 1)) = 0
		_MinGlow("Min Glow", Range( 0 , 1)) = 0.9
		_FresnelScale("Fresnel Scale", Range( 0 , 5)) = 1
		_FresnelPower("Fresnel Power", Range( 0 , 5)) = 1.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 worldRefl;
			INTERNAL_DATA
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color;
		uniform sampler2D _Decal;
		uniform float4 _Decal_ST;
		uniform float _ReflectionStrength;
		uniform samplerCUBE _ReflectionMap;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform sampler2D _GlowMap;
		uniform float4 _GlowMap_ST;
		uniform float _Glow;
		uniform float _MinGlow;
		uniform float _Specular;

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Decal = i.uv_texcoord * _Decal_ST.xy + _Decal_ST.zw;
			float4 temp_output_35_0 = ( ( _Color * i.vertexColor ) * tex2D( _Decal, uv_Decal ) );
			o.Albedo = temp_output_35_0.rgb;
			float3 ase_worldReflection = i.worldRefl;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV7 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode7 = ( 0.0 + _FresnelScale * pow( max( 1.0 - fresnelNdotV7 , 0.0001 ), _FresnelPower ) );
			float4 temp_cast_1 = (i.vertexColor.a).xxxx;
			float2 uv_GlowMap = i.uv_texcoord * _GlowMap_ST.xy + _GlowMap_ST.zw;
			float4 tex2DNode30 = tex2D( _GlowMap, uv_GlowMap );
			float4 lerpResult33 = lerp( temp_cast_1 , tex2DNode30 , tex2DNode30.a);
			o.Emission = ( ( ( _ReflectionStrength * texCUBE( _ReflectionMap, ase_worldReflection ) ) + ( temp_output_35_0 * fresnelNode7 ) ) + ( ( lerpResult33 * _Glow ) * saturate( (_MinGlow + (_CosTime.w - -1.0) * (1.0 - _MinGlow) / (1.0 - -1.0)) ) ) ).rgb;
			o.Specular = _Specular;
			o.Gloss = 1.0;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf BlinnPhong keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
400;894.6667;1570;464;2660.952;177.8525;1.417746;True;True
Node;AmplifyShaderEditor.CommentaryNode;12;-1259.097,-692.3602;Inherit;False;1137.938;496.4566;;5;35;5;4;6;34;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;6;-1166.187,-450.0727;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-1209.097,-642.3602;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;15;-2114.492,-39.41864;Inherit;False;859.2242;408.2527;;4;13;2;1;3;Reflection;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;37;-340.8818,318.399;Inherit;False;1035.06;608.3256;;9;26;27;28;29;32;23;33;24;30;Glow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;36;-1198.866,187.6694;Inherit;False;764.8776;347.0615;;4;25;20;7;21;Fresnel;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldReflectionVector;13;-2064.493,146.2104;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-895.8889,-546.749;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;34;-825.3156,-415.1043;Inherit;True;Property;_Decal;Decal;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-1147.235,419.7308;Inherit;False;Property;_FresnelPower;Fresnel Power;10;0;Create;True;0;0;0;False;0;False;1.5;1.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1148.866,344.0273;Inherit;False;Property;_FresnelScale;Fresnel Scale;9;0;Create;True;0;0;0;False;0;False;1;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosTime;27;-181.1142,644.0817;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;30;-290.8818,368.399;Inherit;True;Property;_GlowMap;Glow Map;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;-285.3476,811.7248;Inherit;False;Property;_MinGlow;Min Glow;8;0;Create;True;0;0;0;False;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;33;112.4089,375.2565;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;24;1.864267,538.6522;Inherit;False;Property;_Glow;Glow;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1770.396,138.834;Inherit;True;Property;_ReflectionMap;Reflection Map;5;0;Create;True;0;0;0;False;0;False;-1;90d53d80ac152e54d8c0b4ed2fbe148a;90d53d80ac152e54d8c0b4ed2fbe148a;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-408.8915,-525.8295;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;7;-860.111,280.5014;Inherit;False;Standard;WorldNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;48.03491,647.8632;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-1802.878,10.58137;Inherit;False;Property;_ReflectionStrength;Reflection Strength;6;0;Create;True;0;0;0;False;0;False;0.06;0.06;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;354.6174,439.8079;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-597.3239,237.6694;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-1418.602,39.14286;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;29;321.4751,571.8947;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;530.8447,517.4769;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-10.17638,36.65947;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;837.3979,422.2211;Inherit;False;Constant;_Gloss;Gloss;6;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;828.861,184.3658;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;8;833.5106,339.9577;Inherit;False;Property;_Specular;Specular;3;0;Create;True;0;0;0;False;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1311.379,200.3723;Float;False;True;-1;2;ASEMaterialInspector;0;0;BlinnPhong;cfantauzzo/LU/Plastic;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;4;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;0;4;0
WireConnection;5;1;6;0
WireConnection;33;0;6;4
WireConnection;33;1;30;0
WireConnection;33;2;30;4
WireConnection;1;1;13;0
WireConnection;35;0;5;0
WireConnection;35;1;34;0
WireConnection;7;2;20;0
WireConnection;7;3;25;0
WireConnection;28;0;27;4
WireConnection;28;3;26;0
WireConnection;23;0;33;0
WireConnection;23;1;24;0
WireConnection;21;0;35;0
WireConnection;21;1;7;0
WireConnection;3;0;2;0
WireConnection;3;1;1;0
WireConnection;29;0;28;0
WireConnection;32;0;23;0
WireConnection;32;1;29;0
WireConnection;16;0;3;0
WireConnection;16;1;21;0
WireConnection;22;0;16;0
WireConnection;22;1;32;0
WireConnection;0;0;35;0
WireConnection;0;2;22;0
WireConnection;0;3;8;0
WireConnection;0;4;9;0
ASEEND*/
//CHKSM=8BC07340C093DE539E8966AE182C7F0B2387E0B4