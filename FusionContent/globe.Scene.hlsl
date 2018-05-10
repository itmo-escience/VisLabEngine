
double dDiv(double a, double b) // 
{
	double r = double(1.0f/float(b));

	r = r * (2.0 - b*r);
	r = r * (2.0 - b*r);

	return a*r;
}


double sine_limited(double x) {
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
double sine_positive(double x) {
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

double sine(double x) {
	return x < 0.0 ? -sine_positive(-x) : sine_positive(x);
}

double cosine(double x) {
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




struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct VS_INPUT {	
	uint2 lon				: TEXCOORD0	;
	uint2 lat				: TEXCOORD1	;
	float4	Tex0			: TEXCOORD2	;	// Texture Coordinates
	float4	Tex1			: TEXCOORD3	;
	float4	Color			: COLOR		;
	uint ID 				: StructID  ;
};

struct VS_OUTPUT {
    float4 Position	: SV_POSITION	;
	float4 Color	: COLOR			;
	float4 Tex		: TEXCOORD0		;
	float4 Tex1		: TEXCOORD1		;
	float3 Normal	: TEXCOORD2		;
	float3 WPos		: TEXCOORD3		;
	uint ID 		: StructID  	;	
};

struct SceneData {
	float Time;
	float3 SunDir;
	float2 AppearStartEndTime;
	float2 DisappearStartEndTime;
	float4x4 WorldMatrix;
};

struct BuildingsData {
	float2 BuildingTime;
	float2 DestroyingTime;
	float3 HeightDummy;
	uint RenderType;
	float4 ColorMult;	
};

cbuffer CBStage		: register(b0) 	{ ConstData Stage : packoffset( c0 ); }
cbuffer SceneStage	: register(b1) 	{ SceneData SceneStage; }

StructuredBuffer<BuildingsData> 	BuildingsBuffer	: register(t3);

Texture2D		DiffuseMap		: register(t0);
Texture2D		FloatMap		: register(t1);
Texture2D		FrameMap		: register(t2);
SamplerState	Sampler			: register(s0);
SamplerState	PointSampler	: register(s1);


#if 0
$ubershader PIXEL_SHADER VERTEX_SHADER +DRAW_HEAT +UV_TRANSPARENCY +CULL_NONE +XRAY +NO_DEPTH +GLASS
#endif

#ifdef VERTEX_SHADER
VS_OUTPUT VSMain ( VS_INPUT v)
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	float angle = 0;
	output.Color	=	float4(v.Color.rgb , v.Color.a);
	#ifdef UV_TRANSPARENCY		
		float delta = 0.0f;
	#else 
		float state = saturate((SceneStage.Time - BuildingsBuffer[v.ID].BuildingTime.x)/(BuildingsBuffer[v.ID].BuildingTime.y - BuildingsBuffer[v.ID].BuildingTime.x));	
		state = (min(((state - SceneStage.AppearStartEndTime.x)/(SceneStage.AppearStartEndTime.y - SceneStage.AppearStartEndTime.x)),
				(1 - (state - SceneStage.DisappearStartEndTime.x)/(SceneStage.DisappearStartEndTime.y - SceneStage.DisappearStartEndTime.x))));			 				
		float delta = BuildingsBuffer[v.ID].HeightDummy.x * (1 - saturate(state));
		
		if (state < 0.1f) {
			output.Color.a = 0;
		}
	#endif
		double lon		= asdouble(v.lon.x, v.lon.y);
		double lat		= asdouble(v.lat.x, v.lat.y);
		double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(v.Tex1.w - delta));

		
		double4 wPos = double4(double(cPos.x - cameraPos.x), double(cPos.y - cameraPos.y), double(cPos.z - cameraPos.z), 1);
		angle = float(lon);
		
		double3 normPos = cPos*0.000156785594;
		float3	normal	= normalize(float3(normPos));

		output.Position	=	mul(float4(wPos), Stage.ViewProj);
		output.Normal	=	v.Tex0.xyz;
		output.Tex1     =   v.Tex1;
		output.WPos 	=   float4(wPos).xyz;
		output.ID = v.ID;


	
	
	#ifdef DRAW_HEAT
		output.Color	 = output.Color * BuildingsBuffer[v.ID].ColorMult;
	#endif
	
	output.Tex		=	v.Tex0;
	
	return output;
}
#endif


#ifdef PIXEL_SHADER
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	
	clip(input.Color.a - 0.01);
	float4 color;
	#ifdef USE_OVERALL_COLOR
		color = ModelStage.OverallColor;
	#else
		color = input.Color;
	#endif
		
	#ifdef UV_TRANSPARENCY
		//clip(BuildingsBuffer[input.ID].StartTime - SceneStage.Time);
		float time1 = lerp(BuildingsBuffer[input.ID].BuildingTime.x, BuildingsBuffer[input.ID].BuildingTime.y, 1 - input.Tex1.y);
		float time2 = lerp(BuildingsBuffer[input.ID].DestroyingTime.x, BuildingsBuffer[input.ID].DestroyingTime.y, 1 - input.Tex1.y);
		color = float4(input.Color.rgb, 1);				
		clip(SceneStage.Time - time1);
		clip(time2 - SceneStage.Time);		
	#endif
	
	#ifdef XRAY
		float3 ndir	= normalize(-input.WPos);
		
		float  ndot = abs(dot( ndir, input.Normal ));
		float  frsn	= pow(saturate(1.2f-ndot), 0.5);
		
		return frsn*float4(color.rgb, color.a);
	#else
		#ifdef UV_TRANSPARENCY
			return color;
		#endif
		if (BuildingsBuffer[input.ID].RenderType == 1) {
			float3 ndir	= normalize(-input.WPos);
		
			float  ndot = abs(dot( ndir, input.Normal ));
			float  frsn	= pow(saturate(1.2f-ndot), 0.5);
			
			return frsn*float4(color.rgb * 0.7, color.a);
		} else {
			float t = dot(normalize(SceneStage.SunDir), input.Normal);	
			float v = 0.5 * (1 + t);
			
			return float4(v * color.rgb, color.a); 
		}
	#endif
}
#endif