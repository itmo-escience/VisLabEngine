#include "globe.Include.hlsl"

Texture2DArray	DiffuseMap		: register(t0);
Texture2D		FrameMap		: register(t1);
SamplerState	Sampler			: register(s0);

struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint2		CameraZ	;
	float2		SourceData ; // Factor, Radius
};


struct VS_INPUT {	
	//uint2 lon				: TEXCOORD0	;
	//uint2 lat				: TEXCOORD1	;
	float4  xydd			: TEXCOORD0 ;		
	float4	Tex				: TEXCOORD2	;	// Texture Coordinates
	float4	Tex1			: TEXCOORD3	;
	float4	Color			: COLOR		;

};

struct INST_INPUT {
	uint x;
	uint y;
	uint level ;
	uint density ;
	uint texIndex ;
	float3 dummy;
};

struct VS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Color		: COLOR			;
	float4 Tex			: TEXCOORD0		;
	float3 Normal		: TEXCOORD1		;
	float  CapFade		: TEXCOORD2		;
	uint Index 			: TEXCOORD3		;
};

cbuffer CBStage		: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}
StructuredBuffer<INST_INPUT> InstData : register(t2);

#if 0
$ubershader +SHOW_FRAMES +FIX_WATER +YANDEX
#endif

double2 getInnerPos(uint x, uint y, uint inX, uint inY, uint level, uint density) 
{
	double2 pos;	
	double mul = double(1 << level);	
	double left = dDiv(double(x), mul);
	double right = dDiv(double(x + 1), mul);
	double top = dDiv(double(y), mul);
	double bottom = dDiv(double(y + 1), mul);
	
	double	dStep	= dDiv(1.0, (double)(density + 1));
	pos.x = left * (1.0 - dStep * inX) + right * dStep * inX;
	pos.y = top * (1.0 - dStep * inY) + bottom * dStep * inY;
	#ifdef YANDEX
		return TileToWorlPosYandex(pos.x, pos.y);
	#else
		return TileToWorlPos(pos.x, pos.y, 0);
	#endif
}

VS_OUTPUT VSMain ( VS_INPUT v, uint id : SV_InstanceID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	
	double2 innerPos = getInnerPos(InstData[id].x, InstData[id].y, v.xydd.x, v.xydd.y, InstData[id].level, InstData[id].density);
	double lon		= innerPos.x;
	double lat		= innerPos.y;
		
	output.Tex		=	v.Tex;
	output.Index    =   InstData[id].texIndex;
	if (lat < -1.47) 
	{
		float t = float(1.4835298641951801403851371532153 - 1.47);
		output.CapFade = -saturate(-float(lat + 1.47) / t);		
	}
	if (lat > 1.47) 
	{
		float t = float(1.4835298641951801403851371532153 - 1.47);
		output.CapFade = saturate(float(lat - 1.47) / t);		
	}
	if (lat < -1.4835298641951801403851371532153) 
	{
		double PI		=	3.141592653589793;	
		double PI_HALF	=	0.5*PI;
		lat = -PI_HALF;				
		output.Tex = float4(v.Tex.x, 2, v.Tex.zw);		
	}	
	
	if (lat > 1.4835298641951801403851371532153) 
	{
		double PI		=	3.141592653589793;	
		double PI_HALF	=	0.5*PI;
		lat = PI_HALF;				
		output.Tex = float4(v.Tex.x, -1, v.Tex.zw);
	}
	
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137);
	//cPos = double3(v.xydd.x * 400.0 - 200, 200 - v.xydd.y * 400, InstData[id].x * 200);
	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;

	output.Position	=	mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	output.Normal	=	normal;

	//output.Color	=	float4((float)v.xydd.x / InstData[id].density, (float)v.xydd.y / InstData[id].density, 0, 1);
	
	
	return output;
}



////////////////////////// Draw map tiles and polygons
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
	float3 waterColor = float3(0, 0, 0), snowColor = float3(0, 0, 0);
	switch(Stage.SourceData.y) {
		case 0:
			waterColor = float3(0.0f, 16.0f/255.0f, 29.0f/255.0f);
			snowColor = float3(226.0f/255.0f, 230.0f/255.0f, 241.0f/255.0f);			
		break;
		case 1:
			waterColor = float3(14.0f/255.0f, 38.0f/255.0f, 82.0f/255.0f);
			snowColor = float3(247.0f/255.0f, 252.0f/255.0f, 1.0f);			
		break;
		case 2:
			waterColor = float3(172.0f/255.0f, 199.0f/255.0f, 242.0f/255.0f);
			snowColor = float3(242.0f/255.0f, 241.0f/255.0f, 236.0f/255.0f);			
		break;
		case 3:
			waterColor = float3(170.0f/255.0f, 203.0f/255.0f, 217.0f/255.0f);
			snowColor = float3(255.0f/255.0f, 255.0f/255.0f, 247.0f/255.0f);			
		break;
		case 4:
			waterColor = float3(181.0f/255.0f, 208.0f/255.0f, 208.0f/255.0f);
			snowColor = float3(242.0f/255.0f, 239.0f/255.0f, 239.0f/255.0f);			
		break;
		default:
		break;
	}		
	float4 color	= input.Index > 0 ? DiffuseMap.Sample(Sampler, float3(input.Tex.xy, input.Index)) : FrameMap.Sample(Sampler, input.Tex.xy);
	float3 ret		= color.rgb;	
	if (input.CapFade < 0 ) {
		ret = lerp(ret, snowColor, -input.CapFade);
	}
	if (input.CapFade > 0) {
		ret = lerp(ret, waterColor, input.CapFade);
	}
	#ifdef SHOW_FRAMES
		float4	frame	= FrameMap.Sample(Sampler, input.Tex.xy);
				ret		= color.rgb * (1.0f - frame.a) + frame.rgb*frame.a;
	#endif		
	#if defined(FIX_WATER)
	if (Stage.SourceData.y == 0 && (ret.b - ret.r > 0 && ret.b - ret.g > 0 && ret.r < 0.5f && ret.g < 0.5f) || ret.g < 0.05f && ret.r < 0.0f) {		
		ret = waterColor;
	}			
	#endif
	return float4(ret, color.a);
}
