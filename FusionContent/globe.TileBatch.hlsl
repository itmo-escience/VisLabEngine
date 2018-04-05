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
    float2      ScreenData ;
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
    uint instid             : TEXCOORD3     ;
	uint Index 			: TEXCOORD4		;
    float4 xydd		    : TEXCOORD5     ;	
    float  Distance     : TEXCOORD6     ;
    uint4  lonlat     : TEXCOORD7     ;
};

cbuffer CBStage		: register(b0) 	{	ConstData	Stage		: 	packoffset( c0 );	}
StructuredBuffer<INST_INPUT> InstData : register(t2);

#if 0
$ubershader +SHOW_FRAMES +FIX_WATER +YANDEX +TESSELLATE
#endif

double2 getInnerPos(uint x, uint y, float inX, float inY, uint level, uint density) 
{
	double2 pos;	
	double mul = double(1 << level);	
	double left = dDiv(double(x), mul);
	double right = dDiv(double(x + 1), mul);
	double top = dDiv(double(y), mul);
	double bottom = dDiv(double(y + 1), mul);
	
	double	dStep	= dDiv(1.0, (double)(density));
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
    asuint(lon, output.lonlat.x, output.lonlat.y);
    asuint(lat, output.lonlat.z, output.lonlat.w);
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
    output.Distance = length(float3(posX, posY, posZ));
	output.Position	=	mul(float4(posX, posY, posZ, 1), Stage.ViewProj);
	output.Normal	=	normal;
    output.xydd = v.xydd;
    output.instid = id;
     //= distance;
	//output.Color	=	float4((float)v.xydd.x / InstData[id].density, (float)v.xydd.y / InstData[id].density, 0, 1);
	
	
	return output;
}

struct PATCH_OUTPUT
{
    float edges[4]	: SV_TessFactor;
    float inside[2]	: SV_InsideTessFactor;
	uint tess : GlobalTessFactor;
};

double dlerp(double x, double y, double t) {
	return x + t * (y - x);
}

float GetTessFactorForDistanceAndLevel(float distance, int level) {
	if (distance > 0) {
		return 6 - clamp(level + log2(distance) - 14, 0, 6);
	} else {
		return 0;
	}
}

PATCH_OUTPUT HullShaderConstantFunction(InputPatch<VS_OUTPUT, 4> inputPatch, uint patchId : SV_PrimitiveID)
{    
    PATCH_OUTPUT output = (PATCH_OUTPUT)0;
    #ifndef TESSELLATE
    output.tess = 0;
    #else
    
	float minDist = 10000000.0f, maxDist = 0.0f;
	for (int i = 0; i < 4; i++) {		
        for (int j = 0; j < 4; j++) {
            if (i != j) {
                minDist = min(minDist, inputPatch[i].Distance);
            }		
        }
	}		 
	output.tess = ceil(GetTessFactorForDistanceAndLevel(minDist, InstData[inputPatch[0].instid].level + log2(InstData[inputPatch[0].instid].density)));
	#endif
	//output.tess = 1;
	
	output.edges[0] = 1 << output.tess;
	output.edges[1] = 1 << output.tess;
	output.edges[2] = 1 << output.tess;
	output.edges[3] = 1 << output.tess;
	output.inside[0] = 1 << output.tess;	
	output.inside[1] = 1 << output.tess;	
    return output;
}

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("HullShaderConstantFunction")]
VS_OUTPUT HSMain(InputPatch<VS_OUTPUT, 4> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
    VS_OUTPUT output;
	
	//float2 vPos = (patch[pointId].Position.xy / patch[pointId].Position.w + float2(1, 1)) * 0.5 * Stage.ScreenSize;	
	
	output.Position = patch[pointId].Position;
    output.Color    = patch[pointId].Color;
    output.Tex      = patch[pointId].Tex;
    output.Normal	= patch[pointId].Normal;
    output.CapFade  = patch[pointId].CapFade;
    output.instid       = patch[pointId].instid;
    output.Index    = patch[pointId].Index;	
    output.xydd     = patch[pointId].xydd;
    output.Distance = patch[pointId].Distance;
	output.lonlat   = patch[pointId].lonlat;	
	    
    return output;
}

[domain("quad")]
VS_OUTPUT DSMain(PATCH_OUTPUT input, float2 uvwCoord : SV_DomainLocation, const OutputPatch<VS_OUTPUT, 4> patch)
{		
	VS_OUTPUT output = (VS_OUTPUT)0;	
    int id = patch[0].instid;
	
    double2 innerPos = getInnerPos(InstData[id].x, InstData[id].y, lerp(patch[0].xydd.x, patch[2].xydd.x, uvwCoord.x), lerp(patch[0].xydd.y, patch[2].xydd.y, uvwCoord.y), InstData[id].level, InstData[id].density);
	double lon		= innerPos.x;
	double lat		= innerPos.y;
	
	float4 heightData = 0;//HeightMap.SampleLevel(Sampler, output.Tex.xy, 0) * 256;
	float height = 0;//-10 + ((heightData.r * 256 * 256 + heightData.g * 256 + heightData.b) * 0.1 * 0.001);
	if (height < 0) height = 0;	
       
	double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + height);

	double3 normPos = cPos*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
	double posX = cPos.x - cameraPos.x;
	double posY = cPos.y - cameraPos.y;
	double posZ = cPos.z - cameraPos.z;
	output.Position = mul(float4(posX, posY, posZ, 1), Stage.ViewProj);				
	#ifdef TESSELLATE
    if (1) {
        float dist = length(float3(posX, posY, posZ));		
		float tess = GetTessFactorForDistanceAndLevel(dist, InstData[id].level + log2(InstData[id].density));		
		
		float morph = saturate((input.tess - tess));
		if (uvwCoord.x * (1 << input.tess) % 2 > 0.000001) {
			uvwCoord.x -= morph * (1.0f / (1 << input.tess));		
		}
		if (uvwCoord.y * (1 << input.tess) % 2 > 0.000001) {
			uvwCoord.y -= morph * (1.0f / (1 << input.tess));		
		}
        
        double2 innerPos = getInnerPos(InstData[id].x, InstData[id].y, lerp(patch[0].xydd.x, patch[2].xydd.x, uvwCoord.x), lerp(patch[0].xydd.y, patch[2].xydd.y, uvwCoord.y), InstData[id].level, InstData[id].density);
        double lon		= innerPos.x;
        double lat		= innerPos.y;
        
        double3 cPos	= SphericalToDecart(double2(lon, lat), 6378.137 + height);

        double3 normPos = cPos*0.000156785594;
        float3	normal	= normalize(float3(normPos));
        
        double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));
        double posX = cPos.x - cameraPos.x;
        double posY = cPos.y - cameraPos.y;
        double posZ = cPos.z - cameraPos.z;
        output.Position = mul(float4(posX, posY, posZ, 1), Stage.ViewProj);				
    }   
    #endif
	output.Tex = lerp(lerp(patch[0].Tex, patch[1].Tex, uvwCoord.x), lerp(patch[3].Tex, patch[2].Tex, uvwCoord.x), uvwCoord.y);	
	output.Normal	=	lerp(lerp(patch[0].Normal, patch[1].Normal, uvwCoord.x), lerp(patch[3].Normal, patch[2].Normal, uvwCoord.x), uvwCoord.y);	
	output.CapFade  =   lerp(lerp(patch[0].CapFade, patch[1].CapFade, uvwCoord.x), lerp(patch[3].CapFade, patch[2].CapFade, uvwCoord.x), uvwCoord.y);
    output.Index = patch[0].Index;
	// output.lon = 0;
	// output.lat = 0;
	//output.Color = float4(normal, 1);
		
	return output;
}


////////////////////////// Draw map tiles and polygons
float4 PSMain ( VS_OUTPUT input ) : SV_Target
{
    //return float4(1, 1, 1, 1);
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
