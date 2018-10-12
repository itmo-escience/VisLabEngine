//////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////
/////////// Functions
double dDiv(double a, double b)
{
	double r = double(1.0f/float(b));

	r = r * (2.0 - b*r);
	r = r * (2.0 - b*r);

	return a*r;
}

double sine_limited(double x) 
{
  double r = x, mxx = -x*x;
  // Change dDiv to multiply to constants 
  r += (x *= dDiv(mxx, 6.0	)); // i=3
  r += (x *= dDiv(mxx, 20.0	)); // i=5
  r += (x *= dDiv(mxx, 42.0	)); // i=7
  r += (x *= dDiv(mxx, 72.0	)); // i=9
  r += (x *= dDiv(mxx, 110.0)); // i=11

  return r;
}

// works properly only for x >= 0
double sine_positive(double x) 
{
	double PI		=	3.141592653589793;
	double PI2		=	2.0*PI;
	double PI_HALF	=	0.5*PI;
	
	
	if (x <= PI_HALF) {
	  return sine_limited(x);
	} else if (x <= PI) {
	  return sine_limited(PI - x);
	} else if (x <= PI2) {
	  return -sine_limited(x - PI);
	} else {
	  return sine_limited(x - PI2*floor(float(dDiv(x,PI2))));
	}
}

double sine(double x) 
{
	return x < 0.0 ? -sine_positive(-x) : sine_positive(x);
}

double cosine(double x) 
{
	double PI=3.141592653589793;
	double PI_HALF=0.5*PI;
	return sine(PI_HALF - x);
}

double3 SphericalToDecart(double2 pos, double r)
{
	double3 res = double3(0,0,0);

	double sinX = sine(pos.x);
	double cosX = cosine(pos.x);
	double sinY = sine(pos.y);
	double cosY = cosine(pos.y);

	res.z = r*cosY*cosX;
	res.x = r*cosY*sinX;
	res.y = r*sinY;

	//res.z = r*cosine(pos.y)*cosine(pos.x);
	//res.x = r*cosine(pos.y)*sine(pos.x);
	//res.y = r*sine(pos.y);

	return res;
}
////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Dummy 		: TEXCOORD0  ;
	float4 Color        : TEXCOORD1  ;
	float4 Tex 			: TEXCOORD2  ;
};

struct VS_OUTPUT {
    uint index : TEXCOORD0	;	
};

/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct FieldData {
	uint2 Lat;
	uint2 Lon;	
	float3 FieldSize;		
	float  Lerp;
	uint4  Dimension; //xyz, depthsCount		
	float4 Right;
	float4 Forward;
	float4x4	View;
	float4x4	Proj;
	float2 Min_Max;	
	float2 Dummy;
};

//#ifdef Draw_points
Texture2D		Palette   			: register(t0);
Texture3D		FirstFrameData      : register(t1);
Texture3D		SecondFrameData     : register(t2);
StructuredBuffer<float> 	FieldDepths 	: register(t3);
StructuredBuffer<float4> 	FinalPositions 	: register(t4);
//#endif

RWStructuredBuffer<float> 	Distances      : register(u0);
RWStructuredBuffer<uint> 	SortedIndecies : register(u1);
RWStructuredBuffer<float4> 	Positions      : register(u2);

SamplerState	Sampler : register(s0);


cbuffer CBStage		: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBField	    : register(b1) { FieldData Field; }

cbuffer CB : register( b2 )
{
    uint g_iLevel;    
	uint g_iInnerLevel;
};


#if 0
$ubershader Draw_points +LerpBuffers +UsePalette +MoveVertices
$ubershader Depth_Calc
$ubershader Depth_Sort_Transpose
$ubershader Depth_Sort_FirstMerge
#endif

uint3 XYZFromIndex(uint index, uint3 size) 
{
	uint x = index / (size.y * size.z);
	uint y = (index / size.z) % size.y;
	uint z = (index % size.z);
	return uint3(x, y, z);
}

uint3 IndexFromXYZ(uint3 xyz, uint3 size) {
	return xyz.x * size.y * size.z + xyz.y * size.z + xyz.z;
}






float ilerp(float min, float max, float value) 
{
	return (value - min) / (max - min);
}

float random( float3 p )
{
    float3 K1 = float3(
        23.14069263277926, // e^pi (Gelfond's constant)
        2.665144142690225, // 2^sqrt(2) (Gelfondâ€“Schneider constant)
		1.0f
    );
    return frac( cos( dot(p,K1) ) * 12345.6789 );
}

float3 random3( float3 p )
{
    float3 K1 = float3(
        23.14069263277926, // e^pi (Gelfond's constant)
        2.665144142690225, // 2^sqrt(2) (Gelfondâ€“Schneider constant)
		1.0f
    );
    return float3(frac( cos( dot(p,K1) + Field.Dummy.x * 0.000165146) * 12345.6789 ),
				  frac( cos( dot(p,K1.xzy) + Field.Dummy.x * 0.000198786 ) * 32498.4984 ),
 				  frac( cos( dot(p,K1.yxz) + Field.Dummy.x * 0.000261464) * 15644.4897 )) % float3(1, 1, 1);
}
#ifdef Draw_points
VS_OUTPUT VSMain ( uint vertInd : SV_VertexID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	output.index = vertInd;
	return output;
}

[maxvertexcount(16)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{	

	uint vertInd = inputArray[0].index;
	float3 xyz = float3(XYZFromIndex(vertInd, Field.Dimension.xyz));	

	float minV = FieldDepths[0];
	float maxV = FieldDepths[Field.Dimension.w - 1];
	float vz = lerp(minV, maxV, xyz.z/float(Field.Dimension.z));
	float zLerp = 0;
	float zInd = 0;
	[unroll(32)]
	for (uint i = 0; i < Field.Dimension.w; i++) {
		if (FieldDepths[i] > vz) {
			zLerp = ilerp(FieldDepths[i - 1], FieldDepths[i], vz);			
			zInd = i - 1;
			break;
		}
	}
	
	float f = lerp (
			FirstFrameData.Load(float4(xyz.x    , xyz.y    , zInd    , 0)).r, 
			FirstFrameData.Load(float4(xyz.x    , xyz.y    , zInd + 1    , 0)).r, zLerp);	
	#if LerpBuffers
		float f1 = lerp (
				SecondFrameData.Load(float4(xyz.x    , xyz.y    , zInd    , 0)).r, 
				SecondFrameData.Load(float4(xyz.x    , xyz.y    , zInd + 1    , 0)).r, zLerp);	
		f = lerp(f, f1, Field.Lerp);
	#endif
	//if (f < 0) return; // ignore null values
	
	if (f >= 0 || f < 0) {
	//if (1) {
		GS_OUTPUT output = (GS_OUTPUT)0 ;		
		double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

		double lon		= asdouble(Field.Lon.x, Field.Lon.y);
		double lat		= asdouble(Field.Lat.x, Field.Lat.y);
		double3 originD	= SphericalToDecart(double2(lon, lat), 6378.137);

		double3 normPos = originD*0.000156785594;	
		
		double posX = originD.x - cameraPos.x;
		double posY = originD.y - cameraPos.y;
		double posZ = originD.z - cameraPos.z;
		
		float3 origin = float3(posX, posY, posZ);	
		float3 Right = Field.Right.xyz;
		float3 Forward = Field.Forward.xyz;
		float3 Up = cross(Right, Forward);			
		float4 positions[4];		
		float4 position = float4(Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * xyz.y / (float(Field.Dimension.y)))
				+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * xyz.x / (float(Field.Dimension.x)))
				+ Up.xyz * Field.FieldSize.z * xyz.z / (float(Field.Dimension.z)), 1);

		
				
		float3 d =  Right.xyz * Field.FieldSize.x / Field.Dimension.y
				  + Forward.xyz * Field.FieldSize.y / Field.Dimension.x
				  + Up.xyz * Field.FieldSize.z / Field.Dimension.z;
		position += float4((normalize(random3(position.xyz)) - 1) * d, 0);
		position += float4(origin.xyz, 0);
		//float4 position = FinalPositions[vertInd];
		float4 localPosition = mul(position , Field.View);
		float2 tex[4];
		tex[0] = float2(-1, -1);
		tex[1] = float2( 1, -1);
		tex[2] = float2(-1,  1);
		tex[3] = float2(1, 1);		
		d *= 0.25f;
		float delta = min(min(d.x, d.y), d.z);
		positions[0] = localPosition + float4(-1, -1, 0, 0) * delta;
		positions[1] = localPosition + float4( 1, -1, 0, 0) * delta;
		positions[2] = localPosition + float4(-1, 1, 0, 0) * delta;
		positions[3] = localPosition + float4(1,  1, 0, 0) * delta;		

		output.Color = Palette.SampleLevel(Sampler, float2(ilerp(Field.Min_Max.x, Field.Min_Max.y, f), 0.5f), 0);
		output.Color.a = 1.0f;	
		[unroll]
		for ( i = 0; i < 4; i++) {		
			output.Tex = float4(tex[i], 0, 0);
			output.Position = mul(positions[i], Field.Proj);
			stream.Append( output );
		}
		stream.RestartStrip();
	}
}



float4 PSMain (GS_OUTPUT  input ) : SV_Target
{		
	float l = length(input.Tex);
	clip (1 - l);
	float a = pow(saturate(1 - l), 0.25f) * input.Color.a * 0.25f;
	//clip(a > 0.00000001f);
	return float4(input.Color.xyz, a);
}
#endif



#ifdef Depth_Calc
#define BITONIC_BLOCK_SIZE 1024
[numthreads(BITONIC_BLOCK_SIZE,1,1)]
void CSMain(uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	uint vertInd = groupID.x * BITONIC_BLOCK_SIZE + groupIndex;
	if (vertInd >= Field.Dimension.x * Field.Dimension.y * Field.Dimension.z) return;
	float3 xyz = float3(XYZFromIndex(vertInd, Field.Dimension.xyz));
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	double lon		= asdouble(Field.Lon.x, Field.Lon.y);
	double lat		= asdouble(Field.Lat.x, Field.Lat.y);
	double3 originD	= SphericalToDecart(double2(lon, lat), 6378.137);

	double3 normPos = originD*0.000156785594;	
	
	double posX = originD.x - cameraPos.x;
	double posY = originD.y - cameraPos.y;
	double posZ = originD.z - cameraPos.z;
	
	float3 origin = float3(posX, posY, posZ);	
	float3 Right = Field.Right.xyz;
	float3 Forward = Field.Forward.xyz;
	float3 Up = cross(Right, Forward);				
	float4 position = float4(Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * xyz.y / (float(Field.Dimension.y)))
			+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * xyz.x / (float(Field.Dimension.x)))
			+ Up.xyz * Field.FieldSize.z * xyz.z / (float(Field.Dimension.z)), 1);
	//float3 d =  Field.FieldSize.xyz / (float3(Field.Dimension.xyz)) * 0.5f;
	// position += float4(normalize(random3(position.xyz) * d), 0);
	position += float4(origin.xyz, 0);
	float4 localPosition = mul(position , Stage.ViewProj);
	Positions[vertInd] = localPosition;
	Distances[vertInd] = localPosition.z;
	SortedIndecies[vertInd] = vertInd;
	//length(localPosition);
}
#endif

#if defined(Depth_Sort_FirstMerge) || defined(Depth_Sort_Transpose) 
#define BITONIC_BLOCK_SIZE 1024
bool CompareAndSwap(uint first, uint second) {	
	if (Distances[first] < Distances[second]) {
		
		float d = Distances[first];
		uint i = SortedIndecies[first];
		
		Distances[first] = Distances[second];
		SortedIndecies[first] = SortedIndecies[second];
		
		Distances[second] = d;		
		SortedIndecies[second] = i;
		return true;
	}
	return false;
}

void TransposeSort(uint index, uint inLevel) 
{ 	
	if (inLevel >= 2) {
		uint inIndex = index % inLevel;
		uint pIndex = index + inLevel / 2;		
		
		if (inIndex < inLevel / 2) CompareAndSwap(index, pIndex);
		
		if (inLevel <= BITONIC_BLOCK_SIZE) {	
			[unroll(12)]
			for (inLevel = inLevel / 2; inLevel > 1; inLevel >>= 1)
			{	
				AllMemoryBarrierWithGroupSync();	
		
				inIndex = index % inLevel;
				pIndex = index + (inLevel / 2);					
				
				if (inIndex < (inLevel / 2)) CompareAndSwap(index, pIndex);							
			}
		}
	}
}

#endif
#ifdef Depth_Sort_FirstMerge 
[numthreads(BITONIC_BLOCK_SIZE, 1 ,1)]
void CSMain(uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	uint index = groupID.x * BITONIC_BLOCK_SIZE + groupIndex;	
	uint inLevel = g_iLevel;
	
	uint inIndex = index % inLevel;
	
	uint offset = index - (inIndex);
	uint pIndex = offset + (inLevel - inIndex - 1);	
	
	AllMemoryBarrierWithGroupSync();
	if (index < pIndex) CompareAndSwap(index, pIndex);
	
	if (inLevel <= BITONIC_BLOCK_SIZE) {
		[unroll(12)]
		for (inLevel = inLevel / 2; inLevel > 1; inLevel >>= 1)
		{					
			AllMemoryBarrierWithGroupSync();	
			inIndex = index % inLevel;
			pIndex = index + (inLevel / 2);								
			if (inIndex < (inLevel / 2)) CompareAndSwap(index, pIndex);							
		}					
	}
	
	// uint inIndex = index % g_iLevel;
	
	// uint offset = index - (inIndex);
	// uint pIndex = offset + (g_iLevel - inIndex - 1);			
	// if (index < pIndex)
		// CompareAndSwap(index, pIndex);
}
#endif

#ifdef Depth_Sort_Transpose 

[numthreads(BITONIC_BLOCK_SIZE, 1 ,1)]
void CSMain(uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex)
{
	
	uint index = groupID.x * BITONIC_BLOCK_SIZE + groupIndex;
	TransposeSort(index, g_iInnerLevel);
	// uint inIndex = index % g_iInnerLevel;			
							
	// uint pIndex = index + g_iInnerLevel / 2;
	// if (inIndex < g_iInnerLevel / 2)
		// CompareAndSwap(index, pIndex);
		
}
#endif