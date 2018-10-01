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

struct VS_INPUT {	
	uint2 	X		: TEXCOORD0	;
	uint2 	Y		: TEXCOORD1	;
	uint2 	Z		: TEXCOORD2	;
	float4	Tex		: TEXCOORD3	;
	float4	Color	: COLOR		;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION ;
	float4 Color		: COLOR       ;
	float4 Tex			: TEXCOORD0   ;
};

/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	; // Factor, Radius
};
		
struct ValueData {
	float  Min;
	float  Max;
	float  Time;
	float  Dummy;
};

Texture2D		Palette		: register(t0);
SamplerState	Sampler		: register(s0);

StructuredBuffer<float> ValuesPrev	: register(t1);
StructuredBuffer<float> ValuesNext  : register(t2);

cbuffer CBStage	: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBValue	: register(b1) { ValueData ValueBounds; }


#if 0
$ubershader DRAW_TEXTURED_POLY
#endif


VS_OUTPUT VSMain ( VS_INPUT v, uint vertInd : SV_VertexID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos = double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	double3 vertexPos = double3(asdouble(v.X[0], v.X[1]), asdouble(v.Y[0], v.Y[1]), asdouble(v.Z[0], v.Z[1]));
	
	double posX = vertexPos.x - cameraPos.x;
	double posY = vertexPos.y - cameraPos.y;
	double posZ = vertexPos.z - cameraPos.z;
	
	output.Position	= mul(float4(posX, posY, posZ, 1), Stage.ViewProj);	
	output.Color = v.Color;
	
	float valRange = ValueBounds.Max - ValueBounds.Min;
	float valPrev = saturate((ValuesPrev[vertInd] - ValueBounds.Min) / valRange);
	float valNext = saturate((ValuesNext[vertInd] - ValueBounds.Min) / valRange);
	
	float time = saturate(ValueBounds.Time);
	
	float lerpVal = lerp(valPrev, valNext, time);
	
	output.Tex = float4(lerpVal, 0, 0, 0);
	
	return output;
}

float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float4 color = Palette.Sample(Sampler, float2(input.Tex.x, 0.5f));

	//color.rgb *= input.Color.rgb;
	color.a *= input.Color.a;
	
	return color;
}