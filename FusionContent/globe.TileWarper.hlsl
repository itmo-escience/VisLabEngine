#include "globe.Include.hlsl"

Texture2D		Texture : register(t0);
Texture2DArray	Atlas   : register(t1);
SamplerState	Sampler	: register(s0);

struct ConstData {
	uint		Right	;
	uint		Bottom	;
	uint		BotomRight;
	uint		Resolution ; // Factor, Radius
};



struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float2 uv           : TEXCOORD0     ;
};

cbuffer CBStage		: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}


#if 0
$ubershader Empty
#endif

VS_OUTPUT VSMain(uint vertInd : SV_VertexID)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;

	uint i = vertInd;
	output.Position = float4(float2(i % 2, i / 2) * 2 - 1, 0, 1);
	output.uv = float2(i % 2, 1 - i / 2);

	return output;
}



////////////////////////// Draw map tiles and polygons
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float2 uv = float2(input.uv * Stage.Resolution);
	float Resolution = float(Stage.Resolution) - 1;
	if (uv.x <= Resolution && uv.y <= Resolution) {
		return Texture.SampleLevel(Sampler, float2(uv) / (Resolution), 0);
    }
	else if (uv.x <= Resolution) {
		return Atlas.SampleLevel(Sampler, float3(uv.x / (Resolution), 0, Stage.Bottom), 0);
	}
	else if (uv.y <= Resolution) {
		return Atlas.SampleLevel(Sampler, float3(0, uv.y / (Resolution), Stage.Right), 0);
	}
	else {
		return Atlas.SampleLevel(Sampler, float3(0, 0, Stage.BotomRight), 0);
	}	
}