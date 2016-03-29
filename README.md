
## Description

This repo is my attempt at exploring fractals in C# / WPF / OpenCL.

## TODO

* Get a basic visualiser working
* Parallelise the calculations (e.g. using Parallel.For)
* Run the calculations on the GPU using OpenCL
* Enhance the UI to allow different areas to be explored and zoomed into
* Make the UI look nice e.g. by using [Material Design In XAML Toolkit](http://materialdesigninxaml.net/)
* MVVM

## Screenshot

![Screenshot](https://raw.github.com/taylorjg/FractalsWpf/master/Images/Screenshot.png)

![ScreenshotColour](https://raw.github.com/taylorjg/FractalsWpf/master/Images/ScreenshotColour.png)

![ScreenshotBarnsleyFern](https://raw.github.com/taylorjg/FractalsWpf/master/Images/ScreenshotBarnsleyFern.png)

## Colour Maps

I initially based my code on
[Make Your Own Mandelbrot](http://makeyourownmandelbrot.blogspot.co.uk/2014/04/book-links.html)
which is written in Python and uses the following line of code to plot the data:

```Python
imshow(atlas.T, interpolation="nearest")
```

[imshow](http://matplotlib.org/api/pyplot_api.html#matplotlib.pyplot.imshow)
is part of [Matplotlib](http://matplotlib.org/). By default, it uses a colour map
called "jet". A colour bar for this and other colour maps can be found [here](http://matplotlib.org/examples/color/colormaps_reference.html).

![jet colour map](https://raw.github.com/taylorjg/FractalsWpf/master/Images/JetColourMap.png)

I ended up porting some of the Matplotlib Python code to C# in order to
obtain the same results. Here are links to the relevant Python source code files:

* https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/_cm.py
    * see *_jet_data*
* https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/colors.py
    * see *makeMappingArray* and *LinearSegmentedColormap*

Having made a colour map, the next step was to map each value in the fractal data
to an entry in the colour map. Each value represents the iteration number at which
a given point diverges. I use the following code to normalise the values and then map
them to entries in the colour map:

```C#
private static int[] ValuesToPixels(int[] values, IReadOnlyList<int> colourMap)
{
    var lastIndex = colourMap.Count - 1;
    var vmin = (double)values.Min();
    var vmax = (double)values.Max();
    var divisor = vmax - vmin;
    var normalisedValues = values.Select(p => (p - vmin) / divisor).ToArray();
    return normalisedValues.Select(p => colourMap[(int)Math.Floor(p * lastIndex)]).ToArray();
}
```

## Links

* [Fractal (Wikipedia)](https://en.wikipedia.org/wiki/Fractal)
* [Mandelbrot set (Wikipedia)](https://en.wikipedia.org/wiki/Mandelbrot_set)
* [Julia set (Wikipedia)](https://en.wikipedia.org/wiki/Julia_set)
* [Barnsley fern (Wikipedia)](https://en.wikipedia.org/wiki/Barnsley_fern)
* [Make Your Own Mandelbrot](http://makeyourownmandelbrot.blogspot.co.uk/2014/04/book-links.html)
