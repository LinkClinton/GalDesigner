# Application.Expresser

In Windows version, we will use `DirectX` to finish this task.

Expresser will support to draw sence, play music, play video.

## About Draw

There are some functions about drawing that expresser has to support:

- Draw Line, Text, Bitmap, Rectangle, Elipse.
- Fill Rectangle, Elipse.
- Transform 2D objects.

### Where to draw

In expresser, we have a surface surface to draw objects.
In fact, a surface is similar to a bitmap. So we will define a surface class.

But draw objects to surface do not mean that draw objects to screen. 
It only draw objects to a bitmap.
So we define a class callled present inherits from surface.
draw objects to present, and when finish all commands of drawing, we will display present to screen.

### Resource

We will create a pool to manager resource.
You can get resource from expresser and you do not need to create it.
If you first get this resource, we will create this resource.
But if you want to destory a resource, you should tell expresser that which resource you want to destory(that means if you get same resource again, we wiil create it again).

We do not have only one kind of resource, we have many kinds of resource:

- Font
- Bitmap
- Brush
- TextLayout(A Text you can think it is a bitmap)

### About Audio

We will use `XAudio2` to finish this task.



