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
	uint2 lon				: TEXCOORD0	;
	uint2 lat				: TEXCOORD1	;
	float4	Tex0			: TEXCOORD2	;	// Texture Coordinates
	float4	Tex1			: TEXCOORD3	;
	float4	Color			: COLOR		;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
};

struct VEL_OUTPUT {
	float4 Position : SV_POSITION;
	float2 Vector	: TEXCOORD0;
	float2 Angles	: TEXCOORD1;
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
	float4 OpacityInitialMinMax;
};

Texture2D		Palette			: register(t0);
Texture2D		VelocityMap		: register(t1);
Texture2D		ArrowMap		: register(t7);

StructuredBuffer<float> 	ScalarDataFirstFrame	: register(t3);
StructuredBuffer<float> 	ScalarDataSecondFrame	: register(t4);
StructuredBuffer<float> 	VectorDataXComponentFirstFrame	: register(t5);
StructuredBuffer<float> 	VectorDataXComponentSecondFrame	: register(t6);
StructuredBuffer<float> 	VectorDataYComponentFirstFrame	: register(t7);
StructuredBuffer<float> 	VectorDataYComponentSecondFrame	: register(t8);

SamplerState	Sampler : register(s0);
SamplerState	VelSampler : register(s1);


cbuffer CBStage		: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBEachStage	: register(b1) { EachFrameData EachFrame; }



#if 0
$ubershader DrawScalarData +DrawIsolines +Normalize +ClipValues +ClipMin +ClipMax +DrawArrows +ClipZero +CullCW +CullEarth
$ubershader DrawVectorData +NorthPoleRegion
$ubershader DrawVectorArrows +NorthPoleRegion
#endif



#ifdef DrawScalarData
VS_OUTPUT VSMain ( VS_INPUT v, uint vertInd : SV_VertexID)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(v.Tex1.x));

	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;
	
	output.Position	= mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	
	output.Normal	= normal.xyz;
	output.Color	= v.Color;

	float firstVal 	= ScalarDataFirstFrame[vertInd];
	float secondVal = ScalarDataSecondFrame[vertInd];

	float lerpVal = lerp(firstVal, secondVal, EachFrame.FactorMinMaxDeltatime.x);
	output.Tex.w = lerpVal;

#ifdef Normalize
	lerpVal = (lerpVal - EachFrame.FactorMinMaxDeltatime.y) / (EachFrame.FactorMinMaxDeltatime.z - EachFrame.FactorMinMaxDeltatime.y);
	lerpVal = saturate(lerpVal);
#endif

#ifdef DrawArrows
	float longitude = (float)lon;
	float latitude = (float)lat;

	float x = (longitude - EachFrame.VectorLeftRightTopBottomMargins.x) / (EachFrame.VectorLeftRightTopBottomMargins.y - EachFrame.VectorLeftRightTopBottomMargins.x);
	float y = (latitude - EachFrame.VectorLeftRightTopBottomMargins.w) / (EachFrame.VectorLeftRightTopBottomMargins.z - EachFrame.VectorLeftRightTopBottomMargins.w);

	output.Tex.y = x;
	output.Tex.z = 1.0f - y;
#endif

	output.Tex.x = lerpVal;

	return output;
}



float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
#ifdef CullEarth
	clip(-input.Color.r);
#endif

	float val 	= input.Tex.x;
	float pval 	= input.Tex.w;
	#if defined(ClipValues) || defined(ClipMin)
	if (pval < EachFrame.FactorMinMaxDeltatime.y) {
		val = -1;
	}
	#endif
	#if defined(ClipValues) || defined(ClipMax)
	if (pval > EachFrame.FactorMinMaxDeltatime.z) {
		val = -1;
	}
	#endif
	// float tmp = (val + 1) / 2;
	// return float4(tmp, tmp, tmp, 1);
	clip(val);
#ifdef ClipZero
	if(pval == 0.0f) clip(-1);
#endif
	//return float4(input.Color.rgb, 0.5f);
	
	float4 color = Palette.Sample(Sampler, float2(val, 0.5f));

#ifdef DrawIsolines
	float z0 = input.Tex.w;
	float dx = ddx(z0);
	float dy = ddy(z0);
	float z1 = z0 + dx;
	float z2 = z0 + dy;

	z0 = frac(z0 / EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.z);
	z1 = frac(z1 / EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.z);
	z2 = frac(z2 / EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.z);
	if ((z0<0.5 && z1>0.5) || (z0>0.5 && z1<0.5)) color = float4(1, 1, 1, 0.5);
	if ((z0<0.5 && z2>0.5) || (z0>0.5 && z2<0.5)) color = float4(1, 1, 1, 0.5);
#endif

#ifdef DrawArrows
	float2 tex		= input.Tex.yz * EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.x;
	float2 temp		= frac(tex);
	float2 velTex	= (floor(tex) + float2(0.5, 0.5)) / EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.x;
	
	float2 currentSpeed = VelocityMap.Sample(Sampler, velTex).xy;

	float		angle = atan2(-currentSpeed.y, currentSpeed.x);
	float2x2	rotationMat = { cos(angle), -sin(angle), sin(angle), cos(angle) };

	float	normLen	= length(currentSpeed) / (EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.y);
	float	velScale = lerp(1.0, 15.0, 1.0f - normLen * 40 - 0.5f);
			velScale = clamp(velScale, 1.0, 15.0f);

	float2	texCoords = mul(temp - float2(0.5, 0.5), rotationMat)*1.0f + float2(0.5, 0.5);
	
	color *= ArrowMap.Sample(Sampler, texCoords);
#endif

	//return input.Color;

	color.a = color.a * EachFrame.OpacityInitialMinMax.x;
	return color;
}

#endif


#ifdef DrawVectorArrows
VS_OUTPUT VSMain ( VS_INPUT v, uint vertInd : SV_VertexID)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(v.Tex1.x));
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;
	
	output.Position	= mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	
	float longitude = (float)lon;
	float latitude = (float)lat;
	
	float x = 0;
	float y = 0;

#ifdef NorthPoleRegion
	float piOverTwo = 1.57079637f;
	float p = piOverTwo - latitude;
	
	x = p*sin(longitude);
	y = -p*cos(longitude);
	
	x = (x + 1.0f) * 0.5f;
	y = 1.0f - (y + 1.0f) * 0.5f;
#else
	x = (longitude - EachFrame.VectorLeftRightTopBottomMargins.x) / (EachFrame.VectorLeftRightTopBottomMargins.y - EachFrame.VectorLeftRightTopBottomMargins.x);
	y = 1.0f - (latitude - EachFrame.VectorLeftRightTopBottomMargins.w) / (EachFrame.VectorLeftRightTopBottomMargins.z - EachFrame.VectorLeftRightTopBottomMargins.w);
#endif

	output.Tex.y = x;
	output.Tex.z = y;
	
	return output;
}

float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float2 tex		= input.Tex.yz * EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.x;
	float2 temp		= frac(tex);
	float2 velTex	= (floor(tex) + float2(0.5, 0.5)) / EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.x;
	
	float4 velData = VelocityMap.SampleLevel(VelSampler, velTex, 0);

	float2 currentSpeed = velData.xy;
	
	float angle =
#ifdef NorthPoleRegion
	atan2(velData.z, velData.w)	
#else
	+ 1.57079637f
#endif
	- atan2(currentSpeed.y, currentSpeed.x);
	
	
	//angle = 1.57079637f;

	float2x2	rotationMat = { cos(angle), -sin(angle), sin(angle), cos(angle) };

	float	normLen	= length(currentSpeed) / (EachFrame.ArrowsscaleMaxspeedIsolineVelocitymult.y);
	float	velScale = lerp(1.0, 15.0, 1.0f - normLen * 40 - 0.5f);
			velScale = clamp(velScale, 1.0, 15.0f);

	float2	texCoords 	= mul(temp - float2(0.5, 0.5), rotationMat)*1.0f + float2(0.5, 0.5);
	float 	arrCol 		= ArrowMap.Sample(Sampler, texCoords).x;
	
	return float4(arrCol,arrCol,arrCol, arrCol);
}
#endif

#ifdef DrawVectorData
VEL_OUTPUT VSMain ( VS_INPUT v, uint vertInd : SV_VertexID)
{
	VEL_OUTPUT	output = (VEL_OUTPUT)0;
	
	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	
	float longitude = (float)lon;
	float latitude 	= (float)lat;
	
	float x = 0;
	float y = 0;
	
#ifdef NorthPoleRegion
	float piOverTwo = 1.57079637f;
	float p = piOverTwo - latitude;
	
	x = p*sin(longitude);
	y = -p*cos(longitude);
#else
		
	x = (longitude	- EachFrame.VectorLeftRightTopBottomMargins.x) / (EachFrame.VectorLeftRightTopBottomMargins.y - EachFrame.VectorLeftRightTopBottomMargins.x);
	y = (latitude	- EachFrame.VectorLeftRightTopBottomMargins.w) / (EachFrame.VectorLeftRightTopBottomMargins.z - EachFrame.VectorLeftRightTopBottomMargins.w);

	x = x * 2.0f - 1.0f;
	y = y * 2.0f - 1.0f;
#endif
		
	float firstValX		= VectorDataXComponentFirstFrame[vertInd];
	float firstValY		= VectorDataYComponentFirstFrame[vertInd];
	float secondValX	= VectorDataXComponentSecondFrame[vertInd];
	float secondValY	= VectorDataYComponentSecondFrame[vertInd];
	
	float2 firstVal 	= float2(firstValX, firstValY);
	float2 secondVal 	= float2(secondValX, secondValY);
	
	output.Vector 	= lerp(firstVal, secondVal, EachFrame.FactorMinMaxDeltatime.x);
	output.Position = float4(x, y, 0.5f, 1.0f);
	
	output.Angles = float2(cos(longitude), sin(longitude));
	
	return output;
}

float4 PSMain ( VEL_OUTPUT input ) : SV_Target
{
	//return float4(1.0f,0, input.Angles.xy);
	return float4(input.Vector.xy, input.Angles.xy);
}
#endif







