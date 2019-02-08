#if 0
$ubershader CalculateForces
$ubershader MoveVertices
#endif


struct PARAMS {
	float4x4	View;
	float4x4	Projection;
	int			MaxParticles;
	float		DeltaTime;
	float		LinkSize;
	float 		SelectedId;
	float4 		Dummy;
};

struct SimpleParams {
	float4 GroupdimXXX;
};

cbuffer CB1 : register(b0) {
	PARAMS Params; 
};

cbuffer CB2 : register(b1) {
	SimpleParams simpleParams; 
};


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


struct LinkId {
	int id;
};

struct Link {
	int		par1;
	int		par2;
	float	length;
	float	strength;
	float4	Color;
	float	Width;
};


RWStructuredBuffer<PARTICLE3D>		particleRWBuffer	: 	register(u0);

StructuredBuffer<LinkId>			linksPtrBuffer		:	register(t0);
StructuredBuffer<Link>				linksBuffer			:	register(t1);


#define THREAD_GROUP_X 32
#define THREAD_GROUP_Y 32
#define THREAD_GROUP_TOTAL 1024

bool CheckParticle(PARTICLE3D prt) {
	return true;
}

bool ChceckInteraction(PARTICLE3D prt) {	
	return true;
}

#ifdef CalculateForces

[numthreads( THREAD_GROUP_X, THREAD_GROUP_Y, 1 )]
void CSMain( uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex )
{
}

#endif // SIMULATION

#ifdef MoveVertices
[numthreads( THREAD_GROUP_X, THREAD_GROUP_Y, 1 )]
void CSMain( uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex )
{
}
#endif // MOVE

