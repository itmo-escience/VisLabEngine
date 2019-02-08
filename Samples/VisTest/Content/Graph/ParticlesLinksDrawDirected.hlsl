
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
Texture2D							Arrow				: 	register(t2);

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
	float4 Color = float4(prt.Color0.rgb, prt.Color0.a);
	if (-1 !=  Params.SelectedId)  
	{
		if (prt.Id == Params.SelectedId) 
		{
			Color = Color;//float4(1, 1, 1, 1);
		} else {
			Color = Color * 0.66f;
		}
	}	
	float sz		=  lerp(Params.Dummy.x, Params.Dummy.y, prt.Size0) / 2;

	float4 pos		=	float4( prt.Position.xyz, 1 );
	float4 posV		=	mul( pos, Params.View );
	Color = float4(Color.rgb * Color.a, Color.a);
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
	// outputStream.RestartStrip( );
	
	// outputStream.Append(p0);
	// outputStream.Append(p2);
	outputStream.Append(p2);
}
#endif

#ifdef LINE
[maxvertexcount(4)]
void GSMain( point VSOutput inputLine[1], inout TriangleStream<GSOutput> outputStream )
{
	Link lk = linksBuffer[ inputLine[0].vertexID ];

	PARTICLE3D end1 = GSResourceBuffer[ lk.par1 ];
	PARTICLE3D end2 = GSResourceBuffer[ lk.par2 ];

	float4 pos1 = float4( end1.Position.xyz, 1 );
	float4 pos2 = float4( end2.Position.xyz, 1 );
	float4 delta = dot((pos2 - pos1), float4(1, 0, 0, 1)) * 10;	
	float4 Color = lk.Color;
	if (-1 !=  Params.SelectedId) 
	{
		if (end1.Id == Params.SelectedId || end2.Id == Params.SelectedId) {
			Color = float4(Color.xyz, 1);
		} else {
			Color = float4(Color.xyz, Color.a * 0.25f);
		}
	}

	GSOutput p1, p2, p3, p4;

	pos1 = mul(pos1 , Params.View);
	pos2 = mul(pos2 , Params.View);
	float3 dir = normalize(pos2.xyz - pos1.xyz);
	float len = length(pos2.xyz - pos1.xyz);
	if (length(dir) == 0 ) return;	
	float3 side = normalize(cross(dir, float3(0,0,-1)));			

	float width = lerp(Params.Dummy.z, Params.Dummy.w, lk.Width);
	p1.TexCoord		=  float2(0, 0);
	p2.TexCoord     =  float2(0, 1);
	p3.TexCoord		=  float2(len / (16 * width), 0);
	p4.TexCoord     =  float2(len / (16 * width), 1);
									
	p1.Color	=	Color;
	p2.Color	=	Color;
	p3.Color	=	Color;
	p4.Color	=	Color;
				
	p1.Position = mul( pos1 + float4(side * width, 0), Params.Projection ) ;
	p2.Position = mul( pos1 - float4(side * width, 0), Params.Projection ) ;
	p3.Position = mul( pos2 + float4(side * width, 0), Params.Projection ) ;
	p4.Position = mul( pos2 - float4(side * width, 0), Params.Projection ) ;
	
	outputStream.Append(p1);
	outputStream.Append(p2);
	outputStream.Append(p3);
	outputStream.Append(p4);	
}
#endif

#ifdef LINE
float4 PSMain( GSOutput input ) : SV_Target
{
	float4 color = Arrow.Sample(Sampler, input.TexCoord);
	clip(color.a < 0.7f ? -1 : 1);
	clip(input.Color.a < 0.001f ? -1 : 1);	
	return float4(color * input.Color);
}
#endif

#ifdef POINT
float4 PSMain( GSOutput input ) : SV_Target
{
	float4 color = Texture.Sample( Sampler, input.TexCoord );
	clip( color.a < 0.7f ? -1:1 );
	//clip( input.Color.a < 0.1f ? -1:1 );
	clip(input.Color.a < 0.001f ? -1 : 1);
	return color * input.Color * input.Color.a;
}
#endif

#endif