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
	float4 Normal 		: TEXCOORD0  ;
	float3 Color        : TEXCOORD1  ;
	float4 WPos			: TEXCOORD2  ;
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
	float2 FieldSize;	
	float2 Mul_Dummy;	
	uint4 Dimension;	
	float4 Color;		
	float4 Right;
	float4 Forward;
	float4 Dummy;
};

Texture2D		tex   			: register(t0);

SamplerState	Sampler : register(s0);


cbuffer CBStage		: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBField	    : register(b1) { FieldData Field; }


#if 0
$ubershader DrawLocal
//$ubershader DrawGeo
#endif

uint2 XYFromIndex(uint index, uint2 size) 
{
	uint x = index % (size.x);
	uint y = index / size.x;	
	return uint2(x, y);
}

uint3 IndexFromXY(uint2 xy, uint3 size) {
	return xy.y * size.x + xy.x;
}




VS_OUTPUT VSMain ( uint vertInd : SV_VertexID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	output.index = vertInd;
	return output;
}

float ilerp(float min, float max, float value) 
{
	return (value - min) / (max - min);
}

[maxvertexcount(4)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{	

	uint vertInd = inputArray[0].index;
	float2 xy = float2(XYFromIndex(vertInd, Field.Dimension.xy));		
	
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
			
	positions[0] = float4(origin.xyz 
				+ Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * xy.x / Field.Dimension.x) 
				+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * xy.y / Field.Dimension.y) 
				+ Up.xyz * tex.SampleLevel(Sampler, (xy + float2(0, 0)) / Field.Dimension.xy, 0).r* Field.Mul_Dummy.x, 1);			
				
	positions[1] = float4(origin.xyz 
				+ Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * (xy.x + 1) / Field.Dimension.x) 
				+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * xy.y / Field.Dimension.y) 
				+ Up.xyz * tex.SampleLevel(Sampler, (xy + float2(1, 0)) / Field.Dimension.xy, 0).r* Field.Mul_Dummy.x, 1);			
				
	positions[3] = float4(origin.xyz 
				+ Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * (xy.x + 1) / Field.Dimension.x) 
				+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * (xy.y + 1) / Field.Dimension.y) 
				+ Up.xyz * tex.SampleLevel(Sampler, (xy + float2(1, 1)) / Field.Dimension.xy, 0).r* Field.Mul_Dummy.x, 1);			
				
	positions[2] = float4(origin.xyz 
				+ Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * xy.x / Field.Dimension.x) 
				+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * (xy.y + 1) / Field.Dimension.y) 
				+ Up.xyz * tex.SampleLevel(Sampler, (xy + float2(0, 1)) / Field.Dimension.xy, 0).r * Field.Mul_Dummy.x, 1);			
	float4 normal = float4(normalize(cross(positions[2].xyz - positions[0].xyz, positions[1].xyz - positions[0].xyz)), 1);				
	
	[unroll]
	for (int j = 0; j < 4; j++) {
		output.WPos = positions[j];
		output.Position = mul(positions[j], Stage.ViewProj);
		output.Normal = normal;
		stream.Append( output );
	}
	
	//stream.RestartStrip();	
}



float4 PSMain (GS_OUTPUT  input ) : SV_Target
{
	float3 norm = normalize(input.Normal.xyz);
	float3 ndir	= normalize(-input.WPos.xyz);
	
	float  ndot = abs(dot( ndir, norm ));
	float  frsn	= pow(saturate(1.1f-ndot), 0.5);		
	float4 color = Field.Color * (0.25f + 0.75f*ndot);
	//return float4(input.Color.xyz, color.a * Field.Color.a);
	return float4(color.xyz, 1.0f);
}