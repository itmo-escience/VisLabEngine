#include "globe.Include.hlsl"
////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct Particle {
	uint2 Lon;
	uint2 Lat;
	float4	Color;
	float4  XVectorSize;
	//float4  YVectorHeight;
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
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
};


/////////////////////////////// Constant Buffers ////////////////
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct EachFrameData {
	float4 SunDirectionTransparency;
	float4 OverallColor;
};



SamplerState	Sampler		: register(s0);

cbuffer CBStage			: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBEachStage		: register(b1) { EachFrameData EachFrame;	}



#if 0
$ubershader +USE_OVERALL_COLOR
#endif


StructuredBuffer<Particle> 	Particles 	: register(t0);

VS_OUTPUT VSMain ( VS_INPUT input, uint vertInd : SV_InstanceID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	Particle p = Particles[vertInd];
	
	double lon		= asdouble(p.Lon.x, p.Lon.y);
	double lat		= asdouble(p.Lat.x, p.Lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137);


	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;
	
	float3 pos 		= float3(posX, posY, posZ);
	
	float3 xAxis = p.XVectorSize.xyz;
	float3 zAxis 	= normalize(cross(xAxis, normal)); 
	xAxis = normalize(cross(zAxis, normal));
	
	float4x4 fMat = {xAxis.xyz, 0, normal.xyz, 0, zAxis.xyz, 0, 0,0,0,1};
	
	float3 outNormal = normalize(mul(float4(input.Normal, 0), fMat).xyz);
	
	float3 modelPos = input.Position.xyz * 0.001f * p.XVectorSize.w;
	
	output.Position	= mul(float4(pos + xAxis*modelPos.x + normal*modelPos.z + zAxis*modelPos.y, 1.0f), Stage.ViewProj);
	output.Normal	= outNormal;
	output.Color	= p.Color;
	output.Tex		= float4(0,0,0,0);
	
	return output;	
}


float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float t = dot(normalize(EachFrame.SunDirectionTransparency.xyz), input.Normal);	
	float v = 0.5 * (1 + t);
	
	return float4(v * input.Color.rgb, input.Color.a);
	//float4 color = input.Color;
	////color = float4(0.0f, 1.0f, 0.0f, 1.0f);
	//return color;
}
