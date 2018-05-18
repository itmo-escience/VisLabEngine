#include "globe.Include.hlsl"
////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct VechicleEvent {
	float4 StimeEtimeIdLength;
	float4 Direction;
	float4 SHeightEHeightXX;
	uint2 SLon;
	uint2 SLat;
	uint2 ELon;
	uint2 ELat;
};

struct Particle
{
	uint2 Lon;
	uint2 Lat;
	float4 Direction;
	float4 Color;
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
	float3 XAxis		: TEXCOORD2		;
};

struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float2 Tex			: TEXCOORD0		;
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
	float4 TimeXXX;
};

struct ParticlesData {
	float4 GroupdimMaxparticlesXX;
};


SamplerState	Sampler		: register(s0);

cbuffer CBStage			: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBEachStage		: register(b1) { EachFrameData EachFrame;	}
cbuffer CBEachStage		: register(b2) { ParticlesData ParticlesCB;	}



#if 0
$ubershader FillTrafficBuffer
$ubershader DrawTraffic +XRAY +ALPHA_BLEND
#endif


#ifdef DrawTraffic
StructuredBuffer<Particle> 	Particles 	: register(t0);

VS_OUTPUT VSMain ( VS_INPUT input, uint vertInd : SV_InstanceID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	Particle p = Particles[vertInd];
	
	double lon		= asdouble(p.Lon.x, p.Lon.y);
	double lat		= asdouble(p.Lat.x, p.Lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(p.Direction.w));


	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;
	
	float3 pos 		= float3(posX, posY, posZ);
	float3 xAxis 	= p.Direction.xyz;
	float3 zAxis 	= normalize(cross(xAxis, normal)); 
	//xAxis = normalize(cross(normal, zAxis));
	
	float3 modelPos = input.Position.xyz * EachFrame.TimeXXX.z;
	
	output.Position	= mul(float4(pos + xAxis*modelPos.x + normal*modelPos.z + zAxis*modelPos.y, 1.0f), Stage.ViewProj);
	output.Normal	= normal.xyz;
	output.XAxis 	= p.Direction.xyz;
	output.Color	= p.Color;
	output.Tex		= float4(xAxis*input.Normal.x + normal*input.Normal.z + zAxis*input.Normal.y, 0.0f);
	
	return output;	
}


float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4 color = input.Color;
	
	#ifdef XRAY
		float3 norm = normalize(input.Tex.xyz);
		float3 ndir	= normalize(float3(1.0f, 1.0f, 1.0f));
		
		float  ndot = abs(dot( ndir, norm ));
		float  frsn	= pow(saturate(1.1f-ndot), 0.5);
		
		color = frsn*input.Color;
	#endif
	
	return color;
}
#endif



#ifdef FillTrafficBuffer
StructuredBuffer<VechicleEvent> 	Events 		: register(t0);
AppendStructuredBuffer<Particle> 	TrafficBuf 	: register(u0);


#define THREAD_GROUP_X 32
#define THREAD_GROUP_Y 32
#define THREAD_GROUP_TOTAL 1024

[numthreads(THREAD_GROUP_X, THREAD_GROUP_Y, 1)]
void CSMain(uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	int index = groupID.x * THREAD_GROUP_TOTAL + groupID.y * ParticlesCB.GroupdimMaxparticlesXX.x * THREAD_GROUP_TOTAL + groupIndex;
	
	[flatten]
	if (index >= ParticlesCB.GroupdimMaxparticlesXX.y)
		return;

	VechicleEvent event = Events[index];
	
	//if(event.StimeEtimeIdLength.z != EachFrame.TimeXXX.y) return;
	
	if(EachFrame.TimeXXX.x >= event.StimeEtimeIdLength.x && EachFrame.TimeXXX.x <= event.StimeEtimeIdLength.y) 
	{
		Particle p = (Particle)0;
		
		double SLon = asdouble(event.SLon[0], event.SLon[1]);
		double SLat = asdouble(event.SLat[0], event.SLat[1]);
		
		double ELon = asdouble(event.ELon[0], event.ELon[1]);
		double ELat = asdouble(event.ELat[0], event.ELat[1]);
		
		float factor = (EachFrame.TimeXXX.x - event.StimeEtimeIdLength.x)/(event.StimeEtimeIdLength.y - event.StimeEtimeIdLength.x);
		factor = saturate(factor);
		
		double dFactor = double(factor);
		
		double lon = SLon + (ELon - SLon)*dFactor;
		double lat = SLat + (ELat - SLat)*dFactor;
		
		uint lonL, lonH;
		asuint(lon, lonL, lonH);
		uint latL, latH;
		asuint(lat, latL, latH);
		
		p.Lon = uint2(lonL, lonH);
		p.Lat = uint2(latL, latH);
		
		float time = (event.StimeEtimeIdLength.y - event.StimeEtimeIdLength.x);		
		float speed = 0.0f;
		if(time != 0) speed = event.StimeEtimeIdLength.w / time;
		float f = saturate(speed/17.0f);
		
		if(f < 0.5f)
			p.Color = float4(1.0f, f*2, 0.0f, 0.8f);
		else
			p.Color = float4(1.0f - f*2, 1.0f, 0.0f, 0.8f);

		//if(index%2 == 0) p.Color = float4(1.0f, 0.0f, 0.0f, 0.8f);
		
		//float3 s = SphericalToDecart2f(float2(SLon, SLat), 6378.137f);
		//float3 e = SphericalToDecart2f(float2(ELon, ELat), 6378.137f);
		p.Direction = event.Direction;

		p.Direction.w = event.SHeightEHeightXX.x + (event.SHeightEHeightXX.y - event.SHeightEHeightXX.x)*factor;
		
		TrafficBuf.Append(p);
	}
		
}

#endif
