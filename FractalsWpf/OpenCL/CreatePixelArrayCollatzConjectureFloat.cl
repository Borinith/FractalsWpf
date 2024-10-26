kernel void CreatePixelArrayCollatzConjectureFloat(
	float2 bottomLeft,
	float2 delta,
	int maxIterations,
	global ushort *results)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int width = get_global_size(0);

	float pi = 3.14159265358979323846f;

	float zr = 0.0f;
	float zi = 0.0f;

	float cr = bottomLeft.x + (delta.x * x);
	float ci = bottomLeft.y + (delta.y * y);

	ushort iter = 0;

	for (; iter < maxIterations; ++iter)
	{
		float zrNext = 0.25 * (2 + 7 * zr - 2 * cos(pi * zr) * cosh(pi * zi) - 5 * zr * cos(pi * zr) * cosh(pi * zi) - 5 * zi * sin(pi * zr) * sinh(pi * zi)) + cr;
		float ziNext = 0.25 * (7 * zi + 2 * sin(pi * zr) * sinh(pi * zi) + 5 * zr * sin(pi * zr) * sinh(pi * zi) - 5 * zi * cos(pi * zr) * cosh(pi * zi)) + ci;

		if (zrNext * zrNext + ziNext * ziNext >= 4000.0f)
		{
			break;
		}

		zr = zrNext;
		zi = ziNext;
	}

	results[y * width + x] = iter;
}
