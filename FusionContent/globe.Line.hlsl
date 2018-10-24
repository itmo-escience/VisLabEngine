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

double dlerp(double a, double b, float t) 
{
	return a + (b - a) * double(t);
}

float DistanceBetweenTwoPoints(float2 from, float2 to, float r)
{
	float phi0 = (float) from.y;
	float phi1 = (float) to.y;
	float deltaPhi = phi1 - phi0;
	float deltaLam = (float) (to.x - from.x);

	float a = sin(deltaPhi / 2.0) * sin(deltaPhi / 2.0) + cos(phi0) * cos(phi1) * sin(deltaLam / 2.0) * sin(deltaLam / 2.0);
	float c = 2 * atan2(sqrt(a), sqrt(1 - a));

	return r * c;
}

float2 DecartToSpherical(float3 cart)
{
	float radius = length(cart);

	if (radius == 0.0) {
		return float2(0, 0);
	}

	float lon = atan2(cart.x, cart.z);
	float lat = asin(cart.y / radius);
	
	return float2(lon, lat);
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
	float3 XAxis		: TEXCOORD2		;
	uint2 lon				: TEXCOORD3	;
	uint2 lat				: TEXCOORD4	;
	float4	Tex1			: TEXCOORD5	;
};


struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float2 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
};

/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	; // Factor, Radius
};

struct LinesConstData {
	float	TransparencyMult	;
	float3 	Dummy				;
	float4	OverallColor		;
};

Texture2D		DiffuseMap		: register(t0);
Texture2D		PaletteMap		: register(t1);
SamplerState	Sampler			: register(s0);

cbuffer CBStage			: register(b0) 	{	ConstData		Stage		: 	packoffset( c0 );	}
cbuffer LinesCBStage	: register(b1) 	{	LinesConstData	LinesStage;	}


#if 0
$ubershader DRAW_LINES +ADD_CAPS +PALETTE_COLOR
$ubershader GEO_LINES +ADD_CAPS +PALETTE_COLOR
$ubershader ARC_LINE
$ubershader DRAW_SEGMENTED_LINES +TEXTURED_LINE +ADD_CAPS
$ubershader THIN_LINE +OVERALL_COLOR
$ubershader THIN_LINE PALETTE_COLOR
#endif


VS_OUTPUT VSMain ( VS_INPUT v )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	float angle = 0;

	double lon		= asdouble(v.lon.x, v.lon.y);
	double lat		= asdouble(v.lat.x, v.lat.y);
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + double(v.Tex1.x));

	angle = float(lon);

	double3 normPos = cPos*0.000156785594;
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

	output.Position	= float4(posX, posY, posZ, 1);
	output.Normal	= normalize(float3(normPos));
	output.Color	= v.Color;
	output.Tex		= v.Tex0;
	
#ifdef THIN_LINE
	float3 viewDirection 	= normalize(float3(cameraPos));
	float3 worldNormal 		= normalize(float3(normPos));
	
	float factor = 1;
	float dotRes = dot(viewDirection, worldNormal);
	
	if(dotRes < 0.4f) {
		factor = saturate(dotRes-0.2)/0.4f;
	}
	
	output.Color.a = output.Color.a * factor;
#endif

#ifdef GEO_LINES
	output.lon = v.lon;
	output.lat = v.lat;
#endif
	
	return output;
}


#define PI 3.141592f

#define LineVertexCount 4
#define SegmentedLineVertexCount 10
#define ArcVertexCount 60

#ifdef DRAW_LINES
	#ifdef ADD_CAPS
		#define TotalVertex (LineVertexCount + 8)
	#else
		#define TotalVertex LineVertexCount
	#endif
#endif

#ifdef GEO_LINES
	#define TotalVertex (68)
	// #ifdef ADD_CAPS
		// #define TotalVertex (SegmentedLineVertexCount + 8)
	// #else
		// #define TotalVertex SegmentedLineVertexCount
	// #endif
#endif

#ifdef DRAW_SEGMENTED_LINES
	#ifdef ADD_CAPS
		#define TotalVertex (SegmentedLineVertexCount + 8)
	#else
		#define TotalVertex SegmentedLineVertexCount
	#endif
#endif

#ifdef ARC_LINE
	#ifdef ADD_CAPS
		#define TotalVertex (ArcVertexCount + 8)
	#else
		#define TotalVertex ArcVertexCount
	#endif
#endif

#ifdef THIN_LINE
	#define TotalVertex 2
#endif

[maxvertexcount(TotalVertex)]
void GSMain ( line VS_OUTPUT inputArray[2], 
#ifdef THIN_LINE
inout LineStream<GS_OUTPUT> stream 
#else
inout TriangleStream<GS_OUTPUT> stream 
#endif
)
{
	GS_OUTPUT	output;// = (GS_OUTPUT)0;
	VS_OUTPUT	p0	=	inputArray[0];
	VS_OUTPUT	p1	=	inputArray[1];	
	
#ifdef PALETTE_COLOR
	// For roads graph load
	p0.Color = PaletteMap.SampleLevel(Sampler, float2(p0.Tex.w, 0.5f), 0);
	p1.Color = p0.Color;
#endif
	
	#ifdef GEO_LINES
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	#endif
	
#ifdef THIN_LINE
	output.Normal 	= float3(0,0,0);
	output.Tex		= float2(0,0);
	output.Color	= p0.Color;
	output.Position	= mul(float4(p0.Position.xyz, 1), Stage.ViewProj);	
	stream.Append( output );
	
	output.Color	= p1.Color;
	output.Position	= mul(float4(p1.Position.xyz, 1), Stage.ViewProj);	
	stream.Append( output );
	
#else

	float halfWidth0 = p0.Tex.x;
	float halfWidth1 = p1.Tex.x;

	float3 dis = p0.Position.xyz - p1.Position.xyz;
	float3 dir = normalize(dis);
	
	float3 sideVec0 = normalize(cross(p0.Normal, dir));
	float3 sideVec1 = normalize(cross(p1.Normal, dir));

	float3 sideOffset0 = sideVec0*halfWidth0;
	float3 sideOffset1 = sideVec1*halfWidth1;

	
#ifdef DRAW_LINES
	float slicesCount = 2;
#endif	
	
#ifdef DRAW_SEGMENTED_LINES
	float slicesCount = 5;
#endif

#ifdef GEO_LINES
	float2 from		= float2(asdouble(p0.lon.x, p0.lon.y), asdouble(p0.lat.x, p0.lat.y));
	float2 to 		= float2(asdouble(p1.lon.x, p1.lon.y), asdouble(p1.lat.x, p1.lat.y));
	
	float slicesCount = 10;//max(length(float2(float(lon1-lon0), float(lat1 - lat0))), 30);	
	
	float3 u = normalize(float3(SphericalToDecart(from, 6378.137)));
	float3 v = normalize(float3(SphericalToDecart(to, 6378.137)));
    float3 w = normalize(cross(cross(u, v), u));

	//float TWO_PI = 6.2831853;
    float len = DistanceBetweenTwoPoints(from, to, 6378.137) / 6378.137;
	
#endif

#ifdef ARC_LINE	
	float slicesCount	= 30;
	float radius 		= length(dis)/2.0f;
#endif
	
	float texMaxX = 1.0f;
#ifdef TEXTURED_LINE
	texMaxX = length(dis) * 3.0f;
	texMaxX = texMaxX - frac(texMaxX);
#endif


	[unroll]
	for(float i = 0; i < slicesCount; i = i + 1) {

	#ifdef GEO_LINES
		float f = i / (slicesCount-1);		
		float t = len * f;
		float3 p = u * cos(t) + w * sin(t);
		float2 geoP = DecartToSpherical(p);
		
		double lon = geoP.x; //dlerp(lon0, lon1, f);
		double lat = geoP.y; //dlerp(lat0, lat1, f);
		float h = lerp(p0.Tex1.x, p1.Tex1.x, f);
		double3 cPos = SphericalToDecart(double2(lon, lat), 6378.137 + h);
		
		double3 normPos = cPos*0.000156785594;
	
		double posX = cPos.x - cameraPos.x;
		double posY = cPos.y - cameraPos.y;
		double posZ = cPos.z - cameraPos.z;
		
		float3 pos			= float3(posX, posY, posZ);
		float3 sideOffset	= lerp(sideOffset0, sideOffset1, f);
		float3 normal		= float3(normPos)
		;//normalize(lerp(p0.Normal, p1.Normal, f));
		
		float texX = texMaxX*f;
		
		float height = h;
		output.Color = lerp(p0.Color, p1.Color, f); 
	#else
		float f = i / (slicesCount-1);

		float3 pos			= lerp(p0.Position.xyz, p1.Position.xyz, f);
		float3 sideOffset	= lerp(sideOffset0, sideOffset1, f);
		float3 normal		= normalize(lerp(p0.Normal, p1.Normal, f));

		float texX = texMaxX*f;
		
		float height = 0;
		#ifdef ARC_LINE
			height = sin(PI * f) * radius;
		#endif
			// Determine color
			output.Color = lerp(p0.Color, p1.Color, f);
			
		#ifdef 	FADING_LINE
			if(i == slicesCount-1) {
				output.Color.a = 0;
			}
		#endif
	#endif
		output.Normal = normal;
		
		output.Tex		= float2(texX, 0.0f);
		output.Position	= mul(float4(pos.xyz + sideOffset + normal*height, 1), Stage.ViewProj);	
		stream.Append( output );

		output.Tex		= float2(texX, 1.0f);
		output.Position	= mul(float4(pos.xyz - sideOffset + normal*height, 1), Stage.ViewProj);	
		stream.Append( output );

	}
	
#endif
	
#ifdef ADD_CAPS
	// Add caps
	float f = 0.55f;
	
	stream.RestartStrip();
	{
		output.Normal	= p0.Normal;
		output.Color	= p0.Color;

		output.Position	= mul(float4(p0.Position.xyz + sideOffset0, 1), Stage.ViewProj);
		output.Tex		= float2(0.0f, 0.5f);
		stream.Append( output );
		
		output.Position	= mul(float4(p0.Position.xyz - sideOffset0, 1), Stage.ViewProj);
		output.Tex		= float2(1.0f, 0.5f);
		stream.Append( output );

		output.Position	= mul(float4(p0.Position.xyz + sideOffset0*f  + dir*f*halfWidth0, 1), Stage.ViewProj);
		output.Tex		= float2(0.0f, 0.0f);
		stream.Append( output );
		
		output.Position	= mul(float4(p0.Position.xyz - sideOffset0*f  + dir*f*halfWidth0, 1), Stage.ViewProj);
		output.Tex		= float2(1.0f, 0.0f);
		stream.Append( output );
	}
	stream.RestartStrip();
	{
		output.Normal	= p1.Normal;
		output.Color	= p1.Color;
		
		output.Position	= mul(float4(p1.Position.xyz + sideOffset1, 1), Stage.ViewProj);
		output.Tex		= float2(0.0f, 0.5f);
		stream.Append( output );
		
		output.Position	= mul(float4(p1.Position.xyz - sideOffset1, 1), Stage.ViewProj);
		output.Tex		= float2(1.0f, 0.5f);
		stream.Append( output );

		output.Position	= mul(float4(p1.Position.xyz + sideOffset1*f  - dir*f*halfWidth1, 1), Stage.ViewProj);
		output.Tex		= float2(0.0f, 0.0f);
		stream.Append( output );
		
		output.Position	= mul(float4(p1.Position.xyz - sideOffset1*f  - dir*f*halfWidth1, 1), Stage.ViewProj);
		output.Tex		= float2(1.0f, 0.0f);
		stream.Append( output );
	}
#endif
}


float4 PSMain ( GS_OUTPUT input ) : SV_Target
{
	float4 color;
#ifdef TEXTURED_LINE
	color = DiffuseMap.Sample(Sampler, input.Tex.xy);
#elif OVERALL_COLOR
	color = LinesStage.OverallColor;
#else
	color = input.Color;
#endif

	color.a = color.a * LinesStage.TransparencyMult;
	return color;
}
