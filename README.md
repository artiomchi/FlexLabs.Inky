# Inky

.NET Standard library for the [Inky pHAT](https://shop.pimoroni.com/products/inky-phat) and [Inky wHAT](https://shop.pimoroni.com/products/inky-what) e-paper displays.

## Inky pHAT

[Inky pHAT](https://shop.pimoroni.com/products/inky-phat) is a 212x104 pixel e-paper display, available in red/black/white, yellow/black/white and black/white. It's great for nametags and displaying very low frequency information such as a daily calendar or weather overview.


## Inky wHAT

[Inky wHAT](https://shop.pimoroni.com/products/inky-what) is a 400x300 pixel e-paper display available in red/black/white, yellow/black/white and black/white. It's got tons of resolution for detailed daily todo lists, multi-day weather forecasts, bus timetables and more.

# Installation

The core library is called `FlexLabs.Inky` and can be installed from NuGet into any .NET Standard compatible project.

The second library is called `FlexLabs.Inky.Text` and contains a basic text renderer, with a couple simple fonts that can be used in your projects.

# Usage

The Inky library contains modules for both the pHAT and wHAT, load the InkyPHAT one as follows. You'll need to pick your colour, one of `Red`, `Yellow` or `Black` and instantiate the class:

```csharp
var inky = new InkyPHat(InkyDisplayColour.Red);
```

If you're using the wHAT you'll need to load the InkyWHAT class from the Inky library like so:

```csharp
var inky = new InkyWHat(InkyDisplayColour.Red);
```

Once you've initialised Inky, there are only three methods you need to be concerned with:

## Set Border

Set the border colour of you pHAT or wHAT.

```csharp
inky.BorderColour = InkyPixelColour.Red;
```

The colour should be one of `InkyPixelColour.Red`, `InkyPixelColour.Yellow`, `InkyPixelColour.White` or `InkyPixelColour.Black` with available colours depending on your display type.

## Set Pixel

Set a pixel in the Inky's internal buffer. You'll need the pixel location and the colour of the pixel:

```csharp
inky.SetPixel(x, y, InkyPixelColour.Red);
```

## Update The Display

Once you've prepared and set your image, and chosen a border colour, you can update your e-ink display with:

```csharp
await inky.RenderBuffer();
```

# Extensions

There are several extension methods that can simplify rendering content on your inky pHAT or wHAT

## Draw a rectangle

Used to draw a rectangle.

```csharp
inky.DrawRectangle(x1, y1, x2, y2, borderColour);
```

You can also draw a filled rectangle by passing the optional `fillColour` parameter:

```csharp
inky.DrawRectangle(x1, y1, x2, y2, borderColour, fillColour);
```

## Draw a string

By using the `FlexLabs.Inky.Text` package, you can draw text on your screen.

As the screen can't render in greyscale, the library comes with several simple fonts optimised for the display. You can also create your own fonts, and use them instead.

```csharp
var font = DotMatrix.Size6;
inky.DrawString(x, y, colour, font, text);
```
