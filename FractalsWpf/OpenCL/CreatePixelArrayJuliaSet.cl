kernel void CreatePixelArrayJuliaSet(
	float2 c,
	float2 bottomLeft,
	float2 delta,
	int maxIterations,
	global int *results)
{
	int gid0 = get_global_id(0);
	int gid1 = get_global_id(1);
	int width = get_global_size(1);

	float zr = bottomLeft.x + (delta.x * gid1);
	float zi = bottomLeft.y + (delta.y * gid0);

	int iter = 0;

	for (; iter < maxIterations; ++iter)
	{
		float zrNext = ((zr * zr) - (zi * zi)) + c.x;
		float ziNext = (2 * zr * zi) + c.y;

		if (zrNext * zrNext + ziNext * ziNext >= 4.0f) break;

		zr = zrNext;
		zi = ziNext;
	}

	results[gid0 * width + gid1] = iter;
}
