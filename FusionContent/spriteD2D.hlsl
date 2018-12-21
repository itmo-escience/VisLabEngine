/*-----------------------------------------------------------------------------
	Sprite batch shader :
-----------------------------------------------------------------------------*/

#if 0
$ubershader OPAQUE|ALPHA_BLEND|ALPHA_BLEND_PREMUL|ADDITIVE|SCREEN|MULTIPLY|NEG_MULTIPLY
#endif

struct PS_IN {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
};



SamplerState	Sampler		: 	register(s0);
Texture2D		Texture 	: 	register(t0);

 
/*-----------------------------------------------------------------------------
	Shader functions :
-----------------------------------------------------------------------------*/

PS_IN VSMain(uint vI : SV_VERTEXID)
{
	PS_IN output = (PS_IN)0;
	float2 texcoord = float2(vI % 2, vI / 2);

    output.pos = float4((texcoord.x - 0.5f) * 2, (0.5f - texcoord.y) * 2, 0, 1);
	output.uv = texcoord;
	return output;
}


float4 PSMain(PS_IN pos) : SV_Target
{
	float4 tex = Texture.Sample(Sampler, pos.uv);		
	return tex;
}
