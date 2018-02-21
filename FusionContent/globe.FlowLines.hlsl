#include "globe.Include.hlsl"
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct Particle {
	float4 LonLatDefaultLonLat;
	float4 LifetimeTotallifetime;	// Texture Coordinates
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
};


/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};


struct EachFrameData {
	float4 FactorMinMaxDeltatime;
	float4 VectorLeftRightTopBottomMargins;
	float4 ArrowsscaleMaxspeedIsolineVelocitymult;
	float4 InitialMinMax;
};

struct ParticlesData {
	float4 LineLengthWidthOpacityRed;
	float4 GroupdimMaxparticlesGreenBlue;
};

StructuredBuffer<Particle> 	Particles 	: register(t5);

Texture2D		VelocityMap	: register(t1);
SamplerState	Sampler		: register(s0);

cbuffer CBStage			: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBEachStage		: register(b1) { EachFrameData EachFrame;	}
cbuffer ParticlesStage	: register(b2) { ParticlesData ParticlesCB; }



#if 0
$ubershader DrawSopli
$ubershader UpdateSopli +NorthPoleRegion
#endif



#ifdef DrawSopli
VS_OUTPUT VSMain (uint vertInd : SV_VertexID)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	
	Particle p = Particles[vertInd];
	float3 pos = SphericalToDecart2f(p.LonLatDefaultLonLat.xy, 6378.137f);

	float3 camFloatPos	= float3(cameraPos);
	float3 normal		= normalize(pos);
	float3 camDirection = normalize(camFloatPos);

	pos = pos - camFloatPos;

	float visibility = dot(normal, camDirection);
	if (visibility < 0.3f)
		visibility = 0.0f;
	else
		visibility = 1.0f;

	output.Position	= mul(float4(pos, 1), Stage.ViewProj);

	float f = p.LifetimeTotallifetime.x / p.LifetimeTotallifetime.y - 0.5f;

	float alphaFactor = clamp(-f*f * 8 + 2, 0, 1);

	output.Color	= float4(ParticlesCB.LineLengthWidthOpacityRed.w, ParticlesCB.GroupdimMaxparticlesGreenBlue.z, ParticlesCB.GroupdimMaxparticlesGreenBlue.w, 0.5f * alphaFactor * ParticlesCB.LineLengthWidthOpacityRed.z * visibility);
	
	return output;
}



float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4 color = input.Color;

	return color;
}

#endif


#ifdef UpdateSopli

RWStructuredBuffer<Particle> ParticlesUav : register(u0);

float2 CheckCoordinates(float2 coords)
{
	float piOverTwo = 1.57079637f;
	float pi 		= 3.14159265f;

	if(coords.y > piOverTwo) {
		coords.x = coords.x + pi;
		coords.y = pi - coords.y;
	} 
	else if(coords.y < -piOverTwo) {
		coords.x = coords.x + pi;
		coords.y = -pi - coords.y;
	}
	
	if(coords.x > pi) {
		coords.x = coords.x - 2*pi;
	} 
	else if(coords.x < -pi) {
		coords.x = coords.x + 2*pi;
	}
	
	return coords;
}

#define THREAD_GROUP_X 32
#define THREAD_GROUP_Y 32
#define THREAD_GROUP_TOTAL 1024

[numthreads(THREAD_GROUP_X, THREAD_GROUP_Y, 1)]
void CSMain(uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	int index = groupID.x * THREAD_GROUP_TOTAL + groupID.y * ParticlesCB.GroupdimMaxparticlesGreenBlue.x * THREAD_GROUP_TOTAL + groupIndex;

	[flatten]
	if (index >= ParticlesCB.GroupdimMaxparticlesGreenBlue.y)
		return;

	Particle particle = ParticlesUav[index];

	if (index%ParticlesCB.LineLengthWidthOpacityRed.x == 0) {		// This is head particle
		float x = 0;
		float y = 0;
	
#ifdef NorthPoleRegion
	float piOverTwo = 1.57079637f;
	float p = piOverTwo - particle.LonLatDefaultLonLat.y;
	
	x = p*sin(particle.LonLatDefaultLonLat.x);
	y = -p*cos(particle.LonLatDefaultLonLat.x);
	
	x = (x + 1.0f) * 0.5f;
	y = 1.0f - (y + 1.0f) * 0.5f;
#else	
		x = (particle.LonLatDefaultLonLat.x - EachFrame.VectorLeftRightTopBottomMargins.x) / (EachFrame.VectorLeftRightTopBottomMargins.y - EachFrame.VectorLeftRightTopBottomMargins.x);
		y = 1.0f - (particle.LonLatDefaultLonLat.y - EachFrame.VectorLeftRightTopBottomMargins.w) / (EachFrame.VectorLeftRightTopBottomMargins.z - EachFrame.VectorLeftRightTopBottomMargins.w);
#endif	
		float2 velocity = VelocityMap.SampleLevel(Sampler, float2(x, y), 0).xy; // Read velocity in km/s and make it to phi/s
		
		float xDiff = velocity.x / (6378.137 * cos(particle.LonLatDefaultLonLat.y));
		float yDiff = velocity.y * 0.000156785594f; 

		particle.LonLatDefaultLonLat.xy += float2(xDiff, yDiff) * EachFrame.FactorMinMaxDeltatime.w * EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.w;
		
		particle.LonLatDefaultLonLat.xy = CheckCoordinates(particle.LonLatDefaultLonLat.xy);
	}
	else { // This is body particle
		particle.LonLatDefaultLonLat.xy = ParticlesUav[index-1].LonLatDefaultLonLat.xy;
	}

	particle.LifetimeTotallifetime.x += EachFrame.FactorMinMaxDeltatime.w;

	if (particle.LifetimeTotallifetime.x >= particle.LifetimeTotallifetime.y) {
		particle.LonLatDefaultLonLat.xy = particle.LonLatDefaultLonLat.zw;
		particle.LifetimeTotallifetime.x = 0;
	}

	ParticlesUav[index] = particle;
}

#endif







