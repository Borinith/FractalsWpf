kernel void CreatePixelArray(
	float minReal,
	float minImaginary,
	float realDelta,
	float imaginaryDelta,
	int numWidthDivisions,
	int numHeightDivisions,
	int maxIterations,
	global const int* colourTable,
	global int* pixels)
{
	int gid0 = get_global_id(0);
	int gid1 = get_global_id(1);
	int m = maxIterations - 1;
	pixels[gid0 * numWidthDivisions + gid1] = colourTable[m];
}
