#pragma pack_matrix(row_major) 

#define TEXTURE 1
#define COLOR 0
#define TEXT 2

struct output_struct {
	float2 texcoord : TEXCOORD;
	float4 projects : SV_POSITION;
};

cbuffer transform : register(b0) {
	matrix world;
	matrix project;
};

cbuffer render_config : register(b1) {
	float4 color;
	int4 config;
};

Texture2D texture0 : register(t0);
SamplerState samplerState : register(s0);

output_struct vs_main(
	float3 position : POSITION,
	float2 texcoord : TEXCOORD) 
{

	output_struct output;

	output.projects = mul(float4(position, 1.0f), world);
	output.projects = mul(output.projects, project);
	output.texcoord = texcoord;

	return output;
}

float4 ps_main(
	float2 texcoord : TEXCOORD,
	float4 projects : SV_POSITION) : SV_TARGET
{
	if (config.x == COLOR) return color;

	if (config.x == TEXTURE) return texture0.Sample(samplerState, texcoord) * color;

	if (config.x == TEXT) return float4(color.rgb, texture0.Sample(samplerState, texcoord).a * color.a);

	return float4(1, 0, 0, 1);
}