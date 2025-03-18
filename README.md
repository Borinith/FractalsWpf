
## Description

This repo is a fork of [taylorjg](https://github.com/taylorjg/FractalsWpf) version.
Below is an updated description of the project, taking into account my edits.

## Colour Maps

I initially based my code on [Make Your Own Mandelbrot](http://makeyourownmandelbrot.blogspot.co.uk/2014/04/book-links.html), which is written in Python and uses the following line of code to plot the data:

```Python
imshow(atlas.T, interpolation="nearest")
```

[imshow](http://matplotlib.org/api/pyplot_api.html#matplotlib.pyplot.imshow) is part of [Matplotlib](http://matplotlib.org/). By default, it uses a colour map called "jet".
A colour bar for this and other colour maps can be found [here](http://matplotlib.org/examples/color/colormaps_reference.html).

![jet colour map](https://raw.github.com/taylorjg/FractalsWpf/master/Images/JetColourMap.png)

I ended up porting some of the Matplotlib Python code to C# in order to obtain the same results. Here are links to the relevant Python source code files:

* https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/_cm.py
    * see *_jet_data*
* https://github.com/matplotlib/matplotlib/blob/master/lib/matplotlib/colors.py
    * see *makeMappingArray* and *LinearSegmentedColormap*

Having made a colour map, the next step was to map each value in the fractal data to an entry in the colour map. Each value represents the iteration number at which a given point diverges. I use the following code to normalise the values and then map them to entries in the colour map:

```C#
private static int[] ValuesToPixels(ushort[] values, int[] colourMap)
{
    var lastIndex = colourMap.Length - 1;

    var vmin = Convert.ToDouble(values.Min());
    var vmax = Convert.ToDouble(values.Max());

    var divisor = vmax - vmin;

    var normalisedValues = values.Select(p => (p - vmin) / divisor).ToArray();
    return normalisedValues.Select(p => colourMap[(int)Math.Floor(p * lastIndex)]).ToArray();
}
```

## Screenshots

![Screenshot](https://raw.github.com/taylorjg/FractalsWpf/master/Images/Screenshot.png)

![ScreenshotColour](https://raw.github.com/taylorjg/FractalsWpf/master/Images/ScreenshotColour.png)

![ScreenshotBarnsleyFern](https://raw.github.com/taylorjg/FractalsWpf/master/Images/ScreenshotBarnsleyFern.png)

## Links

* [Fractal (Wikipedia)](https://en.wikipedia.org/wiki/Fractal)
* [Mandelbrot set (Wikipedia)](https://en.wikipedia.org/wiki/Mandelbrot_set)
* [Julia set (Wikipedia)](https://en.wikipedia.org/wiki/Julia_set)
* [Barnsley fern (Wikipedia)](https://en.wikipedia.org/wiki/Barnsley_fern)
* [The Collatz Conjecture (Wikipedia)](https://en.wikipedia.org/wiki/Collatz_conjecture#Iterating_on_real_or_complex_numbers)
* [The Collatz Conjecture](https://soulofmathematics.com/index.php/the-collatz-conjecture)
* [Make Your Own Mandelbrot](http://makeyourownmandelbrot.blogspot.co.uk/2014/04/book-links.html)
