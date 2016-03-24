#pragma OPENCL EXTENSION cl_khr_fp64 : enable

kernel void CreatePixelArrayJuliaSet(
	double2 c,
	double2 bottomLeft,
	double2 delta,
	int maxIterations,
	global ushort *results)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int width = get_global_size(0);

	double zr = bottomLeft.x + (delta.x * x);
	double zi = bottomLeft.y + (delta.y * y);

	ushort iter = 0;

	for (; iter < maxIterations; ++iter)
	{
		double zrNext = ((zr * zr) - (zi * zi)) + c.x;
		double ziNext = (2 * zr * zi) + c.y;

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
