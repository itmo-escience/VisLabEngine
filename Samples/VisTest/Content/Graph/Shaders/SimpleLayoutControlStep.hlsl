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
	int step = int(Params.Dummy.x);
	if (int(prt.Dummy.x == -1)) return step == -1;
	return (int(prt.Dummy.x) <= step && (int(prt.Dummy.y) >= step || int(prt.Dummy.y) == -1));
}

bool ChceckInteraction(PARTICLE3D prt) {
	int step = int(Params.Dummy.x);
	return (int(prt.Dummy.x) <= step && (int(prt.Dummy.y) > step || int(prt.Dummy.y) == -1));
}

#ifdef CalculateForces

inline float3 PairBodyForce(PARTICLE3D p1, PARTICLE3D p2)
{
	float3 dist = p1.Position - p2.Position;
	float3 dir 	= normalize(dist);
	float len = length(dist);

	float force = (p1.Mass * p2.Mass)/(len*len);
	
	force = clamp(force, 0.0f, 100.0f); // !!!!!!!!
	
	return dir * force;
}

float3 CaclNBodyForces( PARTICLE3D p )										
{
	float3 forceAcc = (float3)0;
	
	for ( int i = 0; i < Params.MaxParticles; i+= 1 ) {
		PARTICLE3D other = particleRWBuffer[i];		
		if(p.Id != other.Id && ChceckInteraction(other)) {
			forceAcc += PairBodyForce(p, other);
		}
	}

	return forceAcc * 10;
}


inline float3 SpringForce(float3 pos1, float3 pos2, float linkLen, float strength)
{
	float3 dist = pos2 - pos1;
	float3 dir 	= normalize(dist);
	float len 		= length(dist);
	float lenDif 	= len - linkLen;
	
	lenDif = clamp(lenDif/linkLen, -10.0f, 10.0f);
	
	return dir * lenDif * strength;
}


///*
float3 CalcLinksForces( float3 pos, int id, int linkListStart, int linkCount )
{
	float3 force = float3( 0, 0, 0 );
	PARTICLE3D otherP;
	for ( int i = 0; i < linkCount; i++ ) {
		Link link = linksBuffer[linksPtrBuffer[linkListStart + i].id];
		int otherId = link.par1;
		if ( id == otherId ) {
			otherId = link.par2;
		}
		otherP = particleRWBuffer[otherId];
		float4 otherPos = float4( otherP.Position, link.length );
		//pos.w = link.strength ; //1.0f;//
		if (ChceckInteraction(otherP)) {
			force += SpringForce( pos, otherP.Position, link.length, link.strength );
		}
	}
	return force;
}
//*/

[numthreads( THREAD_GROUP_X, THREAD_GROUP_Y, 1 )]
void CSMain( uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex )
{
	int id = groupID.x * THREAD_GROUP_TOTAL + groupID.y * simpleParams.GroupdimXXX.x * THREAD_GROUP_TOTAL + groupIndex;
	
	[flatten]
	if (id >= Params.MaxParticles)
		return;
	
	PARTICLE3D p = particleRWBuffer[id];
	if (!CheckParticle(p)) return;
	float3 force = float3( 0, 0, 0 );
    
	force += CaclNBodyForces (p);
	force += CalcLinksForces ( p.Position, p.Id, p.LinksPtr, p.LinksCount );
    
	p.Force	= force * Params.DeltaTime;
	// if (length(p.Force) > 100) {
		// p.Force = normalize(p.Force) * 10;
	// }
	particleRWBuffer[id] = p;
}

#endif // SIMULATION

#ifdef MoveVertices
[numthreads( THREAD_GROUP_X, THREAD_GROUP_Y, 1 )]
void CSMain( uint3 groupID : SV_GroupID, uint groupIndex : SV_GroupIndex )
{
	int id = groupID.x * THREAD_GROUP_TOTAL + groupID.y * simpleParams.GroupdimXXX.x * THREAD_GROUP_TOTAL + groupIndex;
	
	[flatten]
	if (id >= Params.MaxParticles)
		return;
		
	PARTICLE3D p = particleRWBuffer[ id ];	
	
	if (!CheckParticle(p)) return;
	
	float3 vel = p.Velocity  + p.Force * 500 / p.Mass;
	if (length(vel) > 100) {
		vel = 100 * normalize(vel);
	}	
	p.Velocity = vel * (1 - 0.25f * Params.DeltaTime);
	p.Position.xyz += vel * Params.DeltaTime * 5;// mul( p.Force, Params.DeltaTime * 100 );
	particleRWBuffer[ id ] = p;
}
#endif // MOVE

