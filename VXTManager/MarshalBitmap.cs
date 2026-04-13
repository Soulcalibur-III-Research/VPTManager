using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VXTManager;

public class MarshalBitmap : IDisposable
{
	public Bitmap Bitmap { get; private set; }

	public int[] Bits { get; private set; }

	public bool Disposed { get; private set; }

	public int Height { get; private set; }

	public int Width { get; private set; }

	protected GCHandle BitsHandle { get; private set; }

	public MarshalBitmap(int width, int height)
	{
		Width = width;
		Height = height;
		Bits = new int[width * height];
		BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
	}

	public MarshalBitmap(Bitmap Input)
	{
		BitmapData bitmapData = Input.LockBits(new Rectangle(default(Point), Input.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		Width = Input.Width;
		Height = Input.Height;
		int num = bitmapData.Stride * Input.Height;
		Bits = new int[num / 4];
		Marshal.Copy(bitmapData.Scan0, Bits, 0, num / 4);
		Input.UnlockBits(bitmapData);
		BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, BitsHandle.AddrOfPinnedObject());
	}

	public void SetPixel(int x, int y, Color colour)
	{
		int num = x + y * Width;
		int num2 = colour.ToArgb();
		Bits[num] = num2;
	}

	public Color GetPixel(int x, int y)
	{
		int num = x + y * Width;
		return Color.FromArgb(Bits[num]);
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Bitmap.Dispose();
			BitsHandle.Free();
		}
	}
}
