struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct ModelConstData {
	float4x4 	World;
	float4 		ViewPositionTransparency;
	float4 		OverallColor;
	float4 		HeightDummy;	
};

struct InstancingData {
	float4x4 	World;
	uint   BuildingID;
	float3 Dummy;	
};


struct VS_INPUT {
	float3 Position : POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT {
    float4 Position	: SV_POSITION;
	float3 Tangent 	: TANGENT;
	float3 Binormal	: BINORMAL;
	float3 Normal 	: NORMAL;
	float4 Color 	: COLOR;
	float2 TexCoord : TEXCOORD0;
	float3 WPos		: TEXCOORD1;
	uint ID 		: ID;
};


struct SceneData {
	float Time;
	float3 SunDir;
	float2 AppearStartEndTime;
	float2 DisappearStartEndTime;
};

struct BuildingsData {
	float2 BuildingTime;
	float2 DestroyingTime;
	float3 HeightDummy;
	uint RenderType;
	float4 ColorMult;	
};

cbuffer CBStage		: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer PolyStage	: register(b1) 	{ ModelConstData ModelStage; }

Texture2D		DiffuseMap	: register(t0);
SamplerState	Sampler		: register(s0);

StructuredBuffer<InstancingData> InstData : register(t1);

cbuffer SceneStage	: register(b2) 	{ SceneData SceneStage; }

StructuredBuffer<BuildingsData> 	BuildingsBuffer	: register(t3);

#if 0
$ubershader VERTEX_SHADER PIXEL_SHADER DRAW_COLORED +INSTANCED +GLASS
$ubershader VERTEX_SHADER PIXEL_SHADER USE_OVERALL_COLOR +INSTANCED +GLASS
$ubershader VERTEX_SHADER PIXEL_SHADER XRAY +INSTANCED +USE_OVERALL_COLOR +GLASS
#endif



#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( VS_INPUT v 

#ifdef INSTANCED
, uint id : SV_InstanceID
#endif

)
{
	VS_OUTPUT output;
	
	float4x4 worldMatrix = ModelStage.World;
	output.ID = 0;
	int bId = -100;
	//float4 appearDisappearStartEnd = float4(SceneStage.AppearStartEndTime, SceneStage.DisappearStartEndTime);
	float2 AppearStartEndTime = SceneStage.AppearStartEndTime;
	float2 DisappearStartEndTime = SceneStage.DisappearStartEndTime;
	#ifdef INSTANCED
		worldMatrix 	= mul(InstData[id].World, worldMatrix);
		bId = InstData[id].BuildingID;	
		AppearStartEndTime = saturate(AppearStartEndTime + InstData[id].Dummy.x);
		DisappearStartEndTime = saturate(DisappearStartEndTime + InstData[id].Dummy.y);
		output.ID = bId;
	#endif
	float4 normal	= mul( float4(v.Normal.xyz,		0), worldMatrix );
	output.Color = v.Color;// * BuildingsBuffer[bId].ColorMult;
	
	float state = saturate(float(SceneStage.Time - BuildingsBuffer[bId].BuildingTime.x)/float(BuildingsBuffer[bId].BuildingTime.y - BuildingsBuffer[bId].BuildingTime.x));
	state = (min(((state - AppearStartEndTime.x)/(AppearStartEndTime.y - AppearStartEndTime.x)),
		(1 - (state - DisappearStartEndTime.x)/(DisappearStartEndTime.y - DisappearStartEndTime.x))));
	
	
	float delta = ModelStage.HeightDummy.x * (1 - saturate(state));
	
	float4 tempPos 	= mul( float4(v.Position.xy, v.Position.z - delta, 1), worldMatrix ) + float4(ModelStage.ViewPositionTransparency.xyz, 0);
	if (state < 0.1f) output.Color.a = 0;
	output.Position	= mul(float4(tempPos.xyz, 1), Stage.ViewProj);
	output.Normal 	= normalize(normal.xyz);
	
	output.Tangent 	= v.Tangent;
	output.Binormal = v.Binormal;
	output.TexCoord = v.TexCoord;

	output.WPos = tempPos.xyz;

	
	return output;
}
#endif


#ifdef PIXEL_SHADER
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	clip(input.Color.a - 0.01);
	float4 color = float4(0,0,0,0);
	#ifdef XRAY
		color = input.Color * BuildingsBuffer[input.ID].ColorMult;
		if (BuildingsBuffer[input.ID].RenderType == 1) {
			float3 ndir	= normalize(-input.WPos);
			
			float  ndot = abs(dot( ndir, input.Normal ));
			float  frsn	= pow(saturate(1.2f-ndot), 0.5);
			
			return frsn*float4(color.xyz, color.a * ModelStage.ViewPositionTransparency.a);
		} else {
			float t = dot(normalize(SceneStage.SunDir), input.Normal);
			float v = 0.5 * (1 + t);
			return float4(color.rgb * v, color.a * ModelStage.ViewPositionTransparency.a);
		}
	#else
		color = input.Color;
		float t = dot(normalize(SceneStage.SunDir), input.Normal);
		float v = 0.5 * (1 + t);
		return float4(color.rgb * v, color.a * ModelStage.ViewPositionTransparency.a);
	#endif
}
#endif
