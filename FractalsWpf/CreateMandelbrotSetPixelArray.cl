kernel void CreateMandelbrotSetPixelArray(
	float minReal,
	float minImaginary,
	float realDelta,
	float imaginaryDelta,
	int maxIterations,
	global const int* colourTable,
	global int* pixels)
{
	int gid0 = get_global_id(0);
	int gid1 = get_global_id(1);

	int h = get_global_size(0);
	int w = get_global_size(1);

	float zr = 0.0f;
	float zi = 0.0f;

	float cr = minReal + (realDelta * gid1);
	float ci = minImaginary + (imaginaryDelta * gid0);

	int iter = 0;

	for (; iter < maxIterations - 1; ++iter)
	{
		float z2r = ((zr * zr) - (zi * zi)) + cr;
		float z2i = (2 * (zr * zi)) + ci;

		if (z2r * z2r + z2i * z2i >= 4.0f) break;

		zr = z2r;
		zi = z2i;
	}

	pixels[gid0 * w + gid1] = colourTable[iter];
}
