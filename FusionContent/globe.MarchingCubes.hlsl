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
////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

struct GS_OUTPUT {
    float4 Position		: SV_POSITION	;
	float4 Normal 		: TEXCOORD0  ;
	float3 Color        : TEXCOORD1  ;
	float4 WPos			: TEXCOORD2  ;
};

struct VS_OUTPUT {
    uint index : TEXCOORD0	;	
};

/////////////////////////////// Constant Buffers
struct ConstData {
	float4x4	ViewProj;
	uint2		CameraX	;
	uint2		CameraY	;
	uint4		CameraZ	;
	float4		Dummy	;
};

struct FieldData {
	uint2 Lat;
	uint2 Lon;	
	float2 FieldSize;	
	float2 Iso_Lerp;	
	uint4 Dimension;	
	float4 Color;		
	float4 Right;
	float4 Forward;
};

#define X 255
const static uint triTable[256][16] =
{
    {X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X},    // 0
    {0, 8, 3, X, X, X, X, X, X, X, X, X, X, X, X, X},    // 1
    {0, 1, 9, X, X, X, X, X, X, X, X, X, X, X, X, X},    // 2
    {1, 8, 3, 9, 8, 1, X, X, X, X, X, X, X, X, X, X},    // 3 = 1 + 2
    {1, 2, 10, X, X, X, X, X, X, X, X, X, X, X, X, X},   // 4
    {0, 8, 3, 1, 2, 10, X, X, X, X, X, X, X, X, X, X},   // 5 = 1 + 4
    {9, 2, 10, 0, 2, 9, X, X, X, X, X, X, X, X, X, X},   // 6 = 2 + 4
    {2, 8, 3, 2, 10, 8, 10, 9, 8, X, X, X, X, X, X, X},  // 7 = 1 + 2 + 4
    {3, 11, 2, X, X, X, X, X, X, X, X, X, X, X, X, X},   // 8
    {0, 11, 2, 8, 11, 0, X, X, X, X, X, X, X, X, X, X},  // 9 = 1 + 8
    {1, 9, 0, 2, 3, 11, X, X, X, X, X, X, X, X, X, X},   // 10 = 2 + 8
    {1, 11, 2, 1, 9, 11, 9, 8, 11, X, X, X, X, X, X, X}, // 11 = 1 + 2 + 8
    {3, 10, 1, 11, 10, 3, X, X, X, X, X, X, X, X, X, X}, // 12 = 4 + 8
    {0, 10, 1, 0, 8, 10, 8, 11, 10, X, X, X, X, X, X, X},// 13 = 1 + 4 + 8
    {3, 9, 0, 3, 11, 9, 11, 10, 9, X, X, X, X, X, X, X}, // 14 = 2 + 4 + 8
    {9, 8, 10, 10, 8, 11, X, X, X, X, X, X, X, X, X, X}, // 15 = 1 + 2 + 4 + 8
    {4, 7, 8, X, X, X, X, X, X, X, X, X, X, X, X, X},    // 16
    {4, 3, 0, 7, 3, 4, X, X, X, X, X, X, X, X, X, X},	 // 1 + 16
    {0, 1, 9, 8, 4, 7, X, X, X, X, X, X, X, X, X, X},	 // 2 + 16
    {4, 1, 9, 4, 7, 1, 7, 3, 1, X, X, X, X, X, X, X},	 // 1 + 2 + 16
    {1, 2, 10, 8, 4, 7, X, X, X, X, X, X, X, X, X, X},	 // 4 + 16
    {3, 4, 7, 3, 0, 4, 1, 2, 10, X, X, X, X, X, X, X},
    {9, 2, 10, 9, 0, 2, 8, 4, 7, X, X, X, X, X, X, X},
    {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, X, X, X, X},
    {8, 4, 7, 3, 11, 2, X, X, X, X, X, X, X, X, X, X},
    {11, 4, 7, 11, 2, 4, 2, 0, 4, X, X, X, X, X, X, X},
    {9, 0, 1, 8, 4, 7, 2, 3, 11, X, X, X, X, X, X, X},
    {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, X, X, X, X},
    {3, 10, 1, 3, 11, 10, 7, 8, 4, X, X, X, X, X, X, X},
    {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, X, X, X, X},
    {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, X, X, X, X},
    {4, 7, 11, 4, 11, 9, 9, 11, 10, X, X, X, X, X, X, X},
    {9, 5, 4, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {9, 5, 4, 0, 8, 3, X, X, X, X, X, X, X, X, X, X},
    {0, 5, 4, 1, 5, 0, X, X, X, X, X, X, X, X, X, X},
    {8, 5, 4, 8, 3, 5, 3, 1, 5, X, X, X, X, X, X, X},
    {1, 2, 10, 9, 5, 4, X, X, X, X, X, X, X, X, X, X},
    {3, 0, 8, 1, 2, 10, 4, 9, 5, X, X, X, X, X, X, X},
    {5, 2, 10, 5, 4, 2, 4, 0, 2, X, X, X, X, X, X, X},
    {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, X, X, X, X},
    {9, 5, 4, 2, 3, 11, X, X, X, X, X, X, X, X, X, X},
    {0, 11, 2, 0, 8, 11, 4, 9, 5, X, X, X, X, X, X, X},
    {0, 5, 4, 0, 1, 5, 2, 3, 11, X, X, X, X, X, X, X},
    {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, X, X, X, X},
    {10, 3, 11, 10, 1, 3, 9, 5, 4, X, X, X, X, X, X, X},
    {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, X, X, X, X},
    {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, X, X, X, X},
    {5, 4, 8, 5, 8, 10, 10, 8, 11, X, X, X, X, X, X, X},
    {9, 7, 8, 5, 7, 9, X, X, X, X, X, X, X, X, X, X},
    {9, 3, 0, 9, 5, 3, 5, 7, 3, X, X, X, X, X, X, X},
    {0, 7, 8, 0, 1, 7, 1, 5, 7, X, X, X, X, X, X, X},
    {1, 5, 3, 3, 5, 7, X, X, X, X, X, X, X, X, X, X},
    {9, 7, 8, 9, 5, 7, 10, 1, 2, X, X, X, X, X, X, X},
    {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, X, X, X, X},
    {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, X, X, X, X},
    {2, 10, 5, 2, 5, 3, 3, 5, 7, X, X, X, X, X, X, X},
    {7, 9, 5, 7, 8, 9, 3, 11, 2, X, X, X, X, X, X, X},
    {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, X, X, X, X},
    {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, X, X, X, X},
    {11, 2, 1, 11, 1, 7, 7, 1, 5, X, X, X, X, X, X, X},
    {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, X, X, X, X},
    {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, X},
    {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, X},
    {11, 10, 5, 7, 11, 5, X, X, X, X, X, X, X, X, X, X},
    {10, 6, 5, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {0, 8, 3, 5, 10, 6, X, X, X, X, X, X, X, X, X, X},
    {9, 0, 1, 5, 10, 6, X, X, X, X, X, X, X, X, X, X},
    {1, 8, 3, 1, 9, 8, 5, 10, 6, X, X, X, X, X, X, X},
    {1, 6, 5, 2, 6, 1, X, X, X, X, X, X, X, X, X, X},
    {1, 6, 5, 1, 2, 6, 3, 0, 8, X, X, X, X, X, X, X},
    {9, 6, 5, 9, 0, 6, 0, 2, 6, X, X, X, X, X, X, X},
    {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, X, X, X, X},
    {2, 3, 11, 10, 6, 5, X, X, X, X, X, X, X, X, X, X},
    {11, 0, 8, 11, 2, 0, 10, 6, 5, X, X, X, X, X, X, X},
    {0, 1, 9, 2, 3, 11, 5, 10, 6, X, X, X, X, X, X, X},
    {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, X, X, X, X},
    {6, 3, 11, 6, 5, 3, 5, 1, 3, X, X, X, X, X, X, X},
    {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, X, X, X, X},
    {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, X, X, X, X},
    {6, 5, 9, 6, 9, 11, 11, 9, 8, X, X, X, X, X, X, X},
    {5, 10, 6, 4, 7, 8, X, X, X, X, X, X, X, X, X, X},
    {4, 3, 0, 4, 7, 3, 6, 5, 10, X, X, X, X, X, X, X},
    {1, 9, 0, 5, 10, 6, 8, 4, 7, X, X, X, X, X, X, X},
    {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, X, X, X, X},
    {6, 1, 2, 6, 5, 1, 4, 7, 8, X, X, X, X, X, X, X},
    {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, X, X, X, X},
    {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, X, X, X, X},
    {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, X},
    {3, 11, 2, 7, 8, 4, 10, 6, 5, X, X, X, X, X, X, X},
    {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, X, X, X, X},
    {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, X, X, X, X},
    {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, X},
    {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, X, X, X, X},
    {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, X},
    {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, X},
    {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, X, X, X, X},
    {10, 4, 9, 6, 4, 10, X, X, X, X, X, X, X, X, X, X},
    {4, 10, 6, 4, 9, 10, 0, 8, 3, X, X, X, X, X, X, X},
    {10, 0, 1, 10, 6, 0, 6, 4, 0, X, X, X, X, X, X, X},
    {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, X, X, X, X},
    {1, 4, 9, 1, 2, 4, 2, 6, 4, X, X, X, X, X, X, X},
    {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, X, X, X, X},
    {0, 2, 4, 4, 2, 6, X, X, X, X, X, X, X, X, X, X},
    {8, 3, 2, 8, 2, 4, 4, 2, 6, X, X, X, X, X, X, X},
    {10, 4, 9, 10, 6, 4, 11, 2, 3, X, X, X, X, X, X, X},
    {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, X, X, X, X},
    {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, X, X, X, X},
    {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, X},
    {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, X, X, X, X},
    {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, X},
    {3, 11, 6, 3, 6, 0, 0, 6, 4, X, X, X, X, X, X, X},
    {6, 4, 8, 11, 6, 8, X, X, X, X, X, X, X, X, X, X},
    {7, 10, 6, 7, 8, 10, 8, 9, 10, X, X, X, X, X, X, X},
    {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, X, X, X, X},
    {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, X, X, X, X},
    {10, 6, 7, 10, 7, 1, 1, 7, 3, X, X, X, X, X, X, X},
    {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, X, X, X, X},
    {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, X},
    {7, 8, 0, 7, 0, 6, 6, 0, 2, X, X, X, X, X, X, X},
    {7, 3, 2, 6, 7, 2, X, X, X, X, X, X, X, X, X, X},
    {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, X, X, X, X},
    {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, X},
    {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, X},
    {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, X, X, X, X},
    {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, X},
    {0, 9, 1, 11, 6, 7, X, X, X, X, X, X, X, X, X, X},
    {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, X, X, X, X},
    {7, 11, 6, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {7, 6, 11, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {3, 0, 8, 11, 7, 6, X, X, X, X, X, X, X, X, X, X},
    {0, 1, 9, 11, 7, 6, X, X, X, X, X, X, X, X, X, X},
    {8, 1, 9, 8, 3, 1, 11, 7, 6, X, X, X, X, X, X, X},
    {10, 1, 2, 6, 11, 7, X, X, X, X, X, X, X, X, X, X},
    {1, 2, 10, 3, 0, 8, 6, 11, 7, X, X, X, X, X, X, X},
    {2, 9, 0, 2, 10, 9, 6, 11, 7, X, X, X, X, X, X, X},
    {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, X, X, X, X},
    {7, 2, 3, 6, 2, 7, X, X, X, X, X, X, X, X, X, X},
    {7, 0, 8, 7, 6, 0, 6, 2, 0, X, X, X, X, X, X, X},
    {2, 7, 6, 2, 3, 7, 0, 1, 9, X, X, X, X, X, X, X},
    {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, X, X, X, X},
    {10, 7, 6, 10, 1, 7, 1, 3, 7, X, X, X, X, X, X, X},
    {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, X, X, X, X},
    {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, X, X, X, X},
    {7, 6, 10, 7, 10, 8, 8, 10, 9, X, X, X, X, X, X, X},
    {6, 8, 4, 11, 8, 6, X, X, X, X, X, X, X, X, X, X},
    {3, 6, 11, 3, 0, 6, 0, 4, 6, X, X, X, X, X, X, X},
    {8, 6, 11, 8, 4, 6, 9, 0, 1, X, X, X, X, X, X, X},
    {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, X, X, X, X},
    {6, 8, 4, 6, 11, 8, 2, 10, 1, X, X, X, X, X, X, X},
    {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, X, X, X, X},
    {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, X, X, X, X},
    {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, X},
    {8, 2, 3, 8, 4, 2, 4, 6, 2, X, X, X, X, X, X, X},
    {0, 4, 2, 4, 6, 2, X, X, X, X, X, X, X, X, X, X},
    {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, X, X, X, X},
    {1, 9, 4, 1, 4, 2, 2, 4, 6, X, X, X, X, X, X, X},
    {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, X, X, X, X},
    {10, 1, 0, 10, 0, 6, 6, 0, 4, X, X, X, X, X, X, X},
    {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, X},
    {10, 9, 4, 6, 10, 4, X, X, X, X, X, X, X, X, X, X},
    {4, 9, 5, 7, 6, 11, X, X, X, X, X, X, X, X, X, X},
    {0, 8, 3, 4, 9, 5, 11, 7, 6, X, X, X, X, X, X, X},
    {5, 0, 1, 5, 4, 0, 7, 6, 11, X, X, X, X, X, X, X},
    {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, X, X, X, X},
    {9, 5, 4, 10, 1, 2, 7, 6, 11, X, X, X, X, X, X, X},
    {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, X, X, X, X},
    {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, X, X, X, X},
    {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, X},
    {7, 2, 3, 7, 6, 2, 5, 4, 9, X, X, X, X, X, X, X},
    {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, X, X, X, X},
    {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, X, X, X, X},
    {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, X},
    {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, X, X, X, X},
    {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, X},
    {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, X},
    {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, X, X, X, X},
    {6, 9, 5, 6, 11, 9, 11, 8, 9, X, X, X, X, X, X, X},
    {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, X, X, X, X},
    {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, X, X, X, X},
    {6, 11, 3, 6, 3, 5, 5, 3, 1, X, X, X, X, X, X, X},
    {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, X, X, X, X},
    {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, X},
    {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, X},
    {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, X, X, X, X},
    {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, X, X, X, X},
    {9, 5, 6, 9, 6, 0, 0, 6, 2, X, X, X, X, X, X, X},
    {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, X},
    {1, 5, 6, 2, 1, 6, X, X, X, X, X, X, X, X, X, X},
    {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, X},
    {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, X, X, X, X},
    {0, 3, 8, 5, 6, 10, X, X, X, X, X, X, X, X, X, X},
    {10, 5, 6, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {11, 5, 10, 7, 5, 11, X, X, X, X, X, X, X, X, X, X},
    {11, 5, 10, 11, 7, 5, 8, 3, 0, X, X, X, X, X, X, X},
    {5, 11, 7, 5, 10, 11, 1, 9, 0, X, X, X, X, X, X, X},
    {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, X, X, X, X},
    {11, 1, 2, 11, 7, 1, 7, 5, 1, X, X, X, X, X, X, X},
    {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, X, X, X, X},
    {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, X, X, X, X},
    {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, X},
    {2, 5, 10, 2, 3, 5, 3, 7, 5, X, X, X, X, X, X, X},
    {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, X, X, X, X},
    {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, X, X, X, X},
    {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, X},
    {1, 3, 5, 3, 7, 5, X, X, X, X, X, X, X, X, X, X},
    {0, 8, 7, 0, 7, 1, 1, 7, 5, X, X, X, X, X, X, X},
    {9, 0, 3, 9, 3, 5, 5, 3, 7, X, X, X, X, X, X, X},
    {9, 8, 7, 5, 9, 7, X, X, X, X, X, X, X, X, X, X},
    {5, 8, 4, 5, 10, 8, 10, 11, 8, X, X, X, X, X, X, X},
    {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, X, X, X, X},
    {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, X, X, X, X},
    {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, X},
    {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, X, X, X, X},
    {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, X},
    {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, X},
    {9, 4, 5, 2, 11, 3, X, X, X, X, X, X, X, X, X, X},
    {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, X, X, X, X},
    {5, 10, 2, 5, 2, 4, 4, 2, 0, X, X, X, X, X, X, X},
    {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, X},
    {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, X, X, X, X},
    {8, 4, 5, 8, 5, 3, 3, 5, 1, X, X, X, X, X, X, X},
    {0, 4, 5, 1, 0, 5, X, X, X, X, X, X, X, X, X, X},
    {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, X, X, X, X},
    {9, 4, 5, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {4, 11, 7, 4, 9, 11, 9, 10, 11, X, X, X, X, X, X, X},
    {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, X, X, X, X},
    {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, X, X, X, X},
    {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, X},
    {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, X, X, X, X},
    {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, X},
    {11, 7, 4, 11, 4, 2, 2, 4, 0, X, X, X, X, X, X, X},
    {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, X, X, X, X},
    {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, X, X, X, X},
    {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, X},
    {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, X},
    {1, 10, 2, 8, 7, 4, X, X, X, X, X, X, X, X, X, X},
    {4, 9, 1, 4, 1, 7, 7, 1, 3, X, X, X, X, X, X, X},
    {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, X, X, X, X},
    {4, 0, 3, 7, 4, 3, X, X, X, X, X, X, X, X, X, X},
    {4, 8, 7, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {9, 10, 8, 10, 11, 8, X, X, X, X, X, X, X, X, X, X},
    {3, 0, 9, 3, 9, 11, 11, 9, 10, X, X, X, X, X, X, X},
    {0, 1, 10, 0, 10, 8, 8, 10, 11, X, X, X, X, X, X, X},
    {3, 1, 10, 11, 3, 10, X, X, X, X, X, X, X, X, X, X},
    {1, 2, 11, 1, 11, 9, 9, 11, 8, X, X, X, X, X, X, X},
    {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, X, X, X, X},
    {0, 2, 11, 8, 0, 11, X, X, X, X, X, X, X, X, X, X},
    {3, 2, 11, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {2, 3, 8, 2, 8, 10, 10, 8, 9, X, X, X, X, X, X, X},
    {9, 10, 2, 0, 9, 2, X, X, X, X, X, X, X, X, X, X},
    {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, X, X, X, X},
    {1, 10, 2, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {1, 3, 8, 9, 1, 8, X, X, X, X, X, X, X, X, X, X},
    {0, 9, 1, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {0, 3, 8, X, X, X, X, X, X, X, X, X, X, X, X, X},
    {X, X, X, X, X, X, X, X, X, X, X, X, X, X, X, X}
};
#undef X

const static float3 verts[12] = {
	float3(0.5f, 0.0f, 0.0f), //0
	float3(1.0f, 0.5f, 0.0f), //1
	float3(0.5f, 1.0f, 0.0f), //2
	float3(0.0f, 0.5f, 0.0f), //3
	float3(0.5f, 0.0f, 1.0f), //4
	float3(1.0f, 0.5f, 1.0f), //5
	float3(0.5f, 1.0f, 1.0f), //6
	float3(0.0f, 0.5f, 1.0f), //7
	float3(0.0f, 0.0f, 0.5f), //8
	float3(1.0f, 0.0f, 0.5f), //9
	float3(1.0f, 1.0f, 0.5f), //10
	float3(0.0f, 1.0f, 0.5f), //11
};

Texture3D		FirstFrameData      : register(t1);
Texture3D		SecondFrameData      : register(t2);

StructuredBuffer<float> 	FieldDepths 	: register(t3);

SamplerState	Sampler : register(s0);


cbuffer CBStage		: register(b0) { ConstData Stage : packoffset( c0 ); }
cbuffer CBField	    : register(b1) { FieldData Field; }


#if 0
$ubershader DrawIsoSurface +LerpBuffers
#endif

uint3 XYZFromIndex(uint index, uint3 size) 
{
	uint x = index / (size.y * size.z);
	uint y = (index / size.z) % size.y;
	uint z = (index % size.z);
	return uint3(x, y, z);
}

uint3 IndexFromXYZ(uint3 xyz, uint3 size) {
	return xyz.x * size.y * size.z + xyz.y * size.z + xyz.z;
}




VS_OUTPUT VSMain ( uint vertInd : SV_VertexID )
{
	VS_OUTPUT	output = (VS_OUTPUT)0;
	output.index = vertInd;
	return output;
}

[maxvertexcount(16)]
void GSMain ( point VS_OUTPUT inputArray[1], inout TriangleStream<GS_OUTPUT> stream )
{	

	uint vertInd = inputArray[0].index;
	float3 xyz = float3(XYZFromIndex(vertInd, Field.Dimension.xyz - uint3(1, 1, 1)));	
	uint cubeInd = 0; 	
	float4 s = float4(Field.Dimension.xyz, 1); //normalisation coefficient for texture sample
	float4 d = float4(float3(0.5f, 0.5f, 0.5f) / Field.Dimension.xyz, 0); //half pixel offset

	float f0 = FirstFrameData.Load(float4(xyz.x    , xyz.y    , xyz.z    , 0)).r;
	float f1 = FirstFrameData.Load(float4(xyz.x + 1, xyz.y    , xyz.z    , 0)).r;
	float f2 = FirstFrameData.Load(float4(xyz.x + 1, xyz.y + 1, xyz.z    , 0)).r;
	float f3 = FirstFrameData.Load(float4(xyz.x    , xyz.y + 1, xyz.z    , 0)).r;
	float f4 = FirstFrameData.Load(float4(xyz.x    , xyz.y    , xyz.z + 1, 0)).r;
	float f5 = FirstFrameData.Load(float4(xyz.x + 1, xyz.y    , xyz.z + 1, 0)).r;
	float f6 = FirstFrameData.Load(float4(xyz.x + 1, xyz.y + 1, xyz.z + 1, 0)).r;
	float f7 = FirstFrameData.Load(float4(xyz.x    , xyz.y + 1, xyz.z + 1, 0)).r;
	
	#if LerpBuffers
		f0 = lerp(f0, SecondFrameData.Load(float4(xyz.x    , xyz.y    , xyz.z    , 0)).r, Field.Iso_Lerp.y);
		f1 = lerp(f1, SecondFrameData.Load(float4(xyz.x + 1, xyz.y    , xyz.z    , 0)).r, Field.Iso_Lerp.y);
		f2 = lerp(f2, SecondFrameData.Load(float4(xyz.x + 1, xyz.y + 1, xyz.z    , 0)).r, Field.Iso_Lerp.y);
		f3 = lerp(f3, SecondFrameData.Load(float4(xyz.x    , xyz.y + 1, xyz.z    , 0)).r, Field.Iso_Lerp.y);
		f4 = lerp(f4, SecondFrameData.Load(float4(xyz.x    , xyz.y    , xyz.z + 1, 0)).r, Field.Iso_Lerp.y);
		f5 = lerp(f5, SecondFrameData.Load(float4(xyz.x + 1, xyz.y    , xyz.z + 1, 0)).r, Field.Iso_Lerp.y);
		f6 = lerp(f6, SecondFrameData.Load(float4(xyz.x + 1, xyz.y + 1, xyz.z + 1, 0)).r, Field.Iso_Lerp.y);
		f7 = lerp(f7, SecondFrameData.Load(float4(xyz.x    , xyz.y + 1, xyz.z + 1, 0)).r, Field.Iso_Lerp.y);					
	#endif
	if (f0 > Field.Iso_Lerp.x) cubeInd += 1 << 0;
	if (f1 > Field.Iso_Lerp.x) cubeInd += 1 << 1;
	if (f2 > Field.Iso_Lerp.x) cubeInd += 1 << 2;
	if (f3 > Field.Iso_Lerp.x) cubeInd += 1 << 3;
	if (f4 > Field.Iso_Lerp.x) cubeInd += 1 << 4;
	if (f5 > Field.Iso_Lerp.x) cubeInd += 1 << 5;
	if (f6 > Field.Iso_Lerp.x) cubeInd += 1 << 6;
	if (f7 > Field.Iso_Lerp.x) cubeInd += 1 << 7;
	//cubeInd = 4;
	
	GS_OUTPUT output;		
	double3 cameraPos =  double3(asdouble(Stage.CameraX[0], Stage.CameraX[1]), asdouble(Stage.CameraY[0], Stage.CameraY[1]), asdouble(Stage.CameraZ[0], Stage.CameraZ[1]));

	double lon		= asdouble(Field.Lon.x, Field.Lon.y);
	double lat		= asdouble(Field.Lat.x, Field.Lat.y);
	double3 originD	= SphericalToDecart(double2(lon, lat), 6378.137);

	double3 normPos = originD*0.000156785594;
	float3	normal	= normalize(float3(normPos));
	
	double posX = originD.x - cameraPos.x;
	double posY = originD.y - cameraPos.y;
	double posZ = originD.z - cameraPos.z;
	
	float3 origin = float3(posX, posY, posZ);	
	float3 Right = Field.Right.xyz;
	float3 Forward = Field.Forward.xyz;
	float3 Up = cross(Right, Forward);
	// output.Color = xyz / float3(3, 3,3) ;
	// output.Position	= mul(float4(origin + 5 * Right.xyz * xyz.x + 5 * Forward.xyz * xyz.y + 5 * normal * xyz.z, 1), Stage.ViewProj);
	// stream.Append( output );	
	// output.Position	= mul(float4(origin + 5 * Right.xyz * (xyz.x + 1) + 5 * Forward.xyz * xyz.y+ 5 * normal * xyz.z, 1), Stage.ViewProj);
	// stream.Append( output );	
	// output.Position	= mul(float4(origin + 5 * Right.xyz * xyz.x + 5 * Forward.xyz * (xyz.y + 1)+ 5 * normal * xyz.z, 1), Stage.ViewProj);
	// stream.Append( output );		
	// output.Position	= mul(float4(origin + 5 * Right.xyz * (xyz.x + 1) + 5 * Forward.xyz * (xyz.y + 1)+ 5 * normal * xyz.z, 1), Stage.ViewProj);
	// stream.Append( output );		
	// stream.RestartStrip();
			
	output.Color = float3(1, 1, 1);//float3(float(cubeInd / 256.0f), float(cubeInd / 256.0f), float(cubeInd / 256.0f));
	[loop]
	for (uint i1 = 0; i1 < 5; i1++) 
	{				
		if (triTable[cubeInd][i1 * 3] == 255) break;
		float4 positions[3];// = {float4(0), float4(0), float4(0)};
		[unroll]
		for(uint j = 0; j < 3; j++) {
			uint i = i1 * 3 + j;
			
			float3 vertex = verts[triTable[cubeInd][i]];
			float4 pos = float4(xyz.xy + vertex.xy, lerp(FieldDepths[xyz.z], FieldDepths[xyz.z + 1], vertex.z), 1);
			
			positions[j] = float4(origin.xyz 
					+ Right.xyz * (-Field.FieldSize.x * 0.5f + Field.FieldSize.x * pos.x / (float(Field.Dimension.x) - 1)) 
					+ Forward.xyz * (-Field.FieldSize.y * 0.5f + Field.FieldSize.y * pos.y / (float(Field.Dimension.y) - 1)) 
					+ pos.z * normal, 1);			
		}
		float4 normal = float4(normalize(cross(positions[2].xyz - positions[0].xyz, positions[1].xyz - positions[0].xyz)), 1);
		for(j = 0; j < 3; j++) 
		{
			output.WPos = positions[j];
			output.Position = mul(positions[j], Stage.ViewProj);
			output.Normal = normal;
			stream.Append( output );
		}
		stream.RestartStrip();
	}	
}



float4 PSMain (GS_OUTPUT  input ) : SV_Target
{
	//return float4(abs(input.Normal.xyz), 1);
	float3 norm = normalize(input.Normal.xyz);
	float3 ndir	= normalize(-input.WPos.xyz);
	
	float  ndot = abs(dot( ndir, norm ));
	float  frsn	= pow(saturate(1.1f-ndot), 0.5);
				
	return Field.Color * ndot;
}











