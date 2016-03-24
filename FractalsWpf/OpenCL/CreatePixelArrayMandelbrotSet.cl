#pragma OPENCL EXTENSION cl_khr_fp64 : enable

kernel void CreatePixelArrayMandelbrotSet(
	double2 bottomLeft,
	double2 delta,
	int maxIterations,
	global ushort *results)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int width = get_global_size(0);

	double zr = 0.0f;
	double zi = 0.0f;

	double cr = bottomLeft.x + (delta.x * x);
	double ci = bottomLeft.y + (delta.y * y);

	ushort iter = 0;

	for (; iter < maxIterations; ++iter)
	{
		double zrNext = ((zr * zr) - (zi * zi)) + cr;
		double ziNext = (2 * zr * zi) + ci;

		if (fabs(zrNext) >= 2.0f ||
			fabs(ziNext) >= 2.0f ||
			zrNext * zrNext + ziNext * ziNext >= 4.0f)
		{
			break;
		}

		zr = zrNext;
		zi = ziNext;
	}

	results[y * width + x] = iter;
}
