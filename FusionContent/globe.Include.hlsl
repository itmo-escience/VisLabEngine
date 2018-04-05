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

double log(double x) 
{
	double r = 1, mxx = x;
	r = r + x;
	r += (x *= dDiv(mxx, 2.0));
	r += (x *= dDiv(mxx, 3.0));
	r += (x *= dDiv(mxx, 4.0));
	r += (x *= dDiv(mxx, 5.0));
	r += (x *= dDiv(mxx, 6.0));
	r += (x *= dDiv(mxx, 7.0));
	r += (x *= dDiv(mxx, 8.0));
	r += (x *= dDiv(mxx, 9.0));
	r += (x *= dDiv(mxx, 10.0));
	r += (x *= dDiv(mxx, 11.0));
    
    return r;
}



double atan_positive_less(double x)
{
	double r = x, mxx = -x*x;
	r += dDiv(x *= mxx, 3);
	r += dDiv(x *= mxx, 5);
	r += dDiv(x *= mxx, 7);
	r += dDiv(x *= mxx, 9);
	r += dDiv(x *= mxx, 11);
	r += dDiv(x *= mxx, 13);
	r += dDiv(x *= mxx, 15);	
	r += dDiv(x *= mxx, 17);	
	r += dDiv(x *= mxx, 19);
	r += dDiv(x *= mxx, 21);
	r += dDiv(x *= mxx, 23);
	r += dDiv(x *= mxx, 25);
	r += dDiv(x *= mxx, 27);
	r += dDiv(x *= mxx, 29);
	
	return r;
}

double actan_positive_less(double x)
{
	double PI		=	3.141592653589793;
	double PI_HALF	=	0.5*PI;
	//return 2*0.8020464130654855 - atan_positive_less(x);
	return PI_HALF - atan_positive_less(x);
}

double atan_positive(double x)
{
	return x < 1.0 ? atan_positive_less(x) : actan_positive_less(dDiv(1, x));
}

double atan(double x) {
	return x < 0.0 ? -atan_positive(-x) : atan_positive(x);
}



double sinh_positive(double x)
{
	double r = x, mxx = x*x;
	r += (x *= dDiv(mxx, 6.0	)); // i=3
    r += (x *= dDiv(mxx, 20.0	)); // i=5
    r += (x *= dDiv(mxx, 42.0	)); // i=7
    r += (x *= dDiv(mxx, 72.0	)); // i=9
    r += (x *= dDiv(mxx, 110.0  )); // i=11
	r += (x *= dDiv(mxx, 156.0  )); // i=13
	r += (x *= dDiv(mxx, 210.0  )); // i=15
	r += (x *= dDiv(mxx, 272.0  )); // i=17
	r += (x *= dDiv(mxx, 342.0  )); // i=19
	r += (x *= dDiv(mxx, 420.0  )); // i=21
	r += (x *= dDiv(mxx, 506.0  )); // i=23
	r += (x *= dDiv(mxx, 600.0  )); // i=25
	r += (x *= dDiv(mxx, 702.0  )); // i=27
	r += (x *= dDiv(mxx, 812.0  )); // i=29
	return r;
}

double sinh(double x) {
	return x < 0.0 ? -sinh_positive(-x) : sinh_positive(x);
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
  r += (x *= dDiv(mxx, 156.0  )); // i=13
  r += (x *= dDiv(mxx, 210.0  )); // i=15

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

double powInt(double base, uint deg) {
	double res = 1;
	double mul = base;
	[unroll(25)]for(;;) 
	{
		if (deg == 0) return res;
		if (deg & 1) {
			res *= mul;
			deg = deg - 1;
		} else {
			mul *= mul;
			deg >>= 1;
		}
		
	}
}

double atsh_positive(double x) 
{
	double r = x, mxx = -x*x;
	// Change dDiv to multiply to constants 
	r += (dDiv(x *= mxx, 6.0	)); // i=3
	r += (dDiv(x *= mxx, 24.0	)); // i=5
	r += (61*dDiv(x *= mxx, 5040.0	)); // i=7
	r += (277*dDiv(x *= mxx, 72576.0	)); // i=9	
	r += (50521*dDiv(x *= mxx, 39916800.0)); // i=11
	r += (41581*dDiv(x *= mxx, 95800320.0)); // i=13
	r += (199360981*dDiv(x *= mxx, 1307674368000.0)); // i=15
	r += (228135437 *dDiv(x *= mxx, 4184557977600.0)); // i=17
	r += (2404879675441.0  *dDiv(x *= mxx, 121645100408832000.0)); // i=19
	r += (14814847529501.0  *dDiv(x *= mxx, 2043637686868377600.0)); // i=21
	r += (69348874393137901.0  *dDiv(x *= mxx, 25852016738884976640000.0)); // i=23
	r += (238685140977801337.0  *dDiv(x *= mxx, 238634000666630553600000.0)); // i=25
	r += (4087072509293123892361.0  *dDiv(x *= mxx, 10888869450418352160768000000.0)); // i=27
	r += (454540704683713199807.0  *dDiv(x *= mxx, 3209350995912777478963200000.0)); // i=29
	return r;
}

double atsh(double x)
{
	return x < 1.15 && x > -1.15 ? (x < 0.0 ? -atsh_positive(-x) : atsh_positive(x)) : atan(sinh(x));
}

double2 TileToWorlPos(double x, double y, uint zoom)
{
			double PI		=	3.141592653589793;
			double PI2		=	2.0*PI;
			double PI_HALF	=	0.5*PI;
			double2 lonLat;
			double n = PI - dDiv(PI2*y, 1 << zoom);
			lonLat.x = dDiv(x, double(1 << zoom)) * 2 * PI - PI;
			lonLat.y = atsh(n);
			return lonLat;
}

double exp(double x)
{
	double r = 1, mxx = x;  
	uint i;
	[unroll]for(i = 1; i < 29; i++)
	{
	  r += (x *= dDiv(mxx, i)); 
	}  
	return r;
}

double atrevexp(double x) 
{
	double PI		=	3.141592653589793;
	return PI * 0.25 - atsh(x) * 0.5;
}

double2 TileToWorlPosYandex(double x, double y)
{
			double PI		=	3.141592653589793;
			double PI2		=	2.0*PI;
			double PI_HALF	=	0.5*PI;
			double c1 = 0.00335655146887969;
			double c2 = 0.00000657187271079536;
			double c3 = 0.00000001764564338702;
			double c4 = 0.00000000005328478445;			
				
			double mercX = x*PI2 - PI;
			double mercY = PI - y*PI2;
			
			double g = PI_HALF - 2 * atrevexp(mercY);
			double z = g + c1 * sine(2 * g) + c2 * sine(4 * g) + c3 * sine(6 * g) + c4 * sine(8 * g);					
			
			double2 lonLat;			
			lonLat.y = z;
			lonLat.x = mercX;
			return lonLat;
}

float3 SphericalToDecart2f(float2 lonLat, float r)
{
	float3 res = float3(0, 0, 0);

	float sinX = sin(lonLat.x);
	float cosX = cos(lonLat.x);
	float sinY = sin(lonLat.y);
	float cosY = cos(lonLat.y);

	res.z = r*cosY*cosX;
	res.x = r*cosY*sinX;
	res.y = r*sinY;

	//res.z = r*cosine(pos.y)*cosine(pos.x);
	//res.x = r*cosine(pos.y)*sine(pos.x);
	//res.y = r*sine(pos.y);

	return res;
}
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
