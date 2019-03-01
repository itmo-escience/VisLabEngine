
#if 0
$ubershader DRAW POINT|LINE
#endif


struct PARTICLE3D {
	float3	Position; // 3 coordinates
	float3	Velocity;
	float3	Acceleration;
	float3	Force;
	float	Mass;
	float	Size0;
	float4	Color0;
	int		LinksPtr;
	int		LinksCount;
	int		Id;
	float3  Dummy;
};


struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	int			MaxParticles;
	float		DeltaTime;
	float		LinkSize;
	int 		SelectedId;
	float4 		Dummy;	
};


struct Link {
	int		par1;
	int		par2;
	float	length;
	float	strength;
	float4	Color;
	float	Width;
};


SamplerState						Sampler				: 	register(s0);
Texture2D							Texture 			: 	register(t0);

StructuredBuffer<PARTICLE3D>		GSResourceBuffer	:	register(t1);
StructuredBuffer<Link>				linksBuffer			:	register(t3);

cbuffer CB1 : register(b0) { 
	PARAMS Params; 
};

/*-----------------------------------------------------------------------------
	Simulation :
-----------------------------------------------------------------------------*/


#ifdef DRAW

struct VSOutput {
int vertexID : TEXCOORD0;
};

struct GSOutput {
	float4	Position : SV_Position;
	float2	TexCoord : TEXCOORD0;
	float4	Color    : COLOR0;
};


VSOutput VSMain( uint vertexID : SV_VertexID )
{
	VSOutput output;
	output.vertexID = vertexID;
	return output;
}


#ifdef POINT
[maxvertexcount(4)]
void GSMain( point VSOutput inputPoint[1], inout TriangleStream<GSOutput> outputStream )
{
	GSOutput p0, p1, p2, p3;
	PARTICLE3D prt = GSResourceBuffer[ inputPoint[0].vertexID ];
	int step = int(Params.Dummy.x);
	if (int(prt.Dummy.x) <= step && int(prt.Dummy.y) >= step || int(prt.Dummy.y) == -1) {	
		float4 Color = prt.Color0;	
		float at = saturate((Params.Dummy.y - Params.Dummy.z*0.5f) / Params.Dummy.z);
		float dt = saturate(Params.Dummy.y / Params.Dummy.z);
		
		if (-1 !=  Params.SelectedId)  
		{
			if (prt.Id == Params.SelectedId) 
			{
				Color = Color;//float4(1, 1, 1, 1);
			} else {
				Color = Color / 2;
			}
		}
		float sz =  prt.Size0 / 2;			
		
		if (int(prt.Dummy.x) == step && step != -1) {			
			if (at > 0) {
				Color = lerp(float4(1, 1, 1, Color.a), Color, at);
			} else {
				Color = float4(0, 0, 0, 0);
			}
			sz = lerp(sz * 2, sz, at);
		};
		if (int(prt.Dummy.y) == step && step != -1) {
			Color = lerp(float4(1, 0, 0, Color.a), float4(0, 0, 0, 0), dt);
			sz = lerp(sz * 2, sz * 0, dt);
		};

		float4 pos		=	float4( prt.Position.xyz, 1 );
		float4 posV		=	mul( pos, Params.View );
		
		p0.Position = mul( posV + float4( sz, sz, 0, 0 ) , Params.Projection );
		p0.TexCoord = float2(0, 0);
		p0.Color = Color;

		p1.Position = mul( posV + float4( -sz, sz, 0, 0 ) , Params.Projection );
		p1.TexCoord = float2(1, 0);
		p1.Color = Color;

		p2.Position = mul( posV + float4( -sz, -sz, 0, 0 ) , Params.Projection );
		p2.TexCoord = float2(1, 1);
		p2.Color = Color;

		p3.Position = mul( posV + float4( sz, -sz, 0, 0 ) , Params.Projection );
		p3.TexCoord = float2(0, 1);
		p3.Color = Color;
		
		outputStream.Append(p0);
		outputStream.Append(p1);
		outputStream.Append(p3);
		outputStream.Append(p2);
	}
}
#endif

#ifdef LINE
[maxvertexcount(4)]
void GSMain( point VSOutput inputLine[1], inout TriangleStream<GSOutput> outputStream )
{
	Link lk = linksBuffer[ inputLine[0].vertexID ];

	PARTICLE3D end1 = GSResourceBuffer[ lk.par1 ];
	PARTICLE3D end2 = GSResourceBuffer[ lk.par2 ];
	int step = int(Params.Dummy.x);
	if ((int(end1.Dummy.x) <= step && int(end1.Dummy.y) >= step || int(end1.Dummy.y) == -1) && (int(end2.Dummy.x) <= step && int(end2.Dummy.y) >= step || int(end2.Dummy.y) == -1)) {	
		
		float at = clamp((Params.Dummy.y - Params.Dummy.z*0.5f) / Params.Dummy.z, -1, 1);
		float dt = saturate(Params.Dummy.y / Params.Dummy.z);
		
		int st = max(int(end1.Dummy.x), int(end2.Dummy.x));
		int fn = min(int(end1.Dummy.y), int(end2.Dummy.y));
		if (fn == -1) fn = max(int(end1.Dummy.y), int(end2.Dummy.y));
		
		float4 pos1 = float4( end1.Position.xyz, 1 );
		float4 pos2 = float4( end2.Position.xyz, 1 );
		float4 Color = lk.Color;
		if (-1 !=  Params.SelectedId) 
		{			
			if (end1.Id == Params.SelectedId || end2.Id == Params.SelectedId) {
				Color = float4(Color.xyz, 1);
			} else {
				Color = float4(Color.xyz, Color.a * 0.25f);
			}
		}
		
		if (int(st) == step && step != -1) {			
			if (at > 0) {
				Color = lerp(float4(1, 1, 1, Color.a), Color, at);			
			} else {
				Color = float4(0, 0, 0, 0);
			}
		};
		
		if (int(fn) == step && step != -1) {
			Color = lerp(float4(1, 0, 0, Color.a), float4(0, 0, 0, 0), dt);
		};

		GSOutput p1, p2, p3, p4;

		pos1 = mul(pos1 , Params.View);
		pos2 = mul(pos2 , Params.View);
		float3 dir = normalize(pos2.xyz - pos1.xyz);
		if (length(dir) == 0 ) return;

		float3 side = normalize(cross(dir, float3(0,0,-1)));					
		
		p1.TexCoord		=	float2(0, 1);
		p2.TexCoord		=	float2(0, 0);
		p3.TexCoord		=	float2(0, 1);
		p4.TexCoord		=	float2(1, 1);
										
		p1.Color	=	Color;
		p2.Color	=	Color;
		p3.Color	=	Color;
		p4.Color	=	Color;
					
		p1.Position = mul( pos1 + float4(side * lk.Width, 0), Params.Projection ) ;	
		p2.Position = mul( pos1 - float4(side * lk.Width, 0), Params.Projection ) ;	
		p3.Position = mul( pos2 + float4(side * lk.Width, 0), Params.Projection ) ;	
		p4.Position = mul( pos2 - float4(side * lk.Width, 0), Params.Projection ) ;	
		
		outputStream.Append(p1);
		outputStream.Append(p2);
		outputStream.Append(p3);
		outputStream.Append(p4);	
	}
}
#endif

#ifdef LINE
float4 PSMain( GSOutput input ) : SV_Target
{
	return input.Color;
}
#endif

#ifdef POINT
float4 PSMain( GSOutput input ) : SV_Target
{
	float4 color = Texture.Sample( Sampler, input.TexCoord ) * input.Color;
	clip( color.a < 0.7f ? -1:1 );
	//clip( input.Color.a < 0.1f ? -1:1 );
	
	return color;
}
#endif

#endif