using System;
using System.Collections.Generic;
using System.Drawing;

namespace VXTManager;

public static class PS2
{
	public struct PS2Image
	{
		public Size TrueSize;

		public Size FixedSize;

		public uint ColorCount;

        public byte DMAIndex;
        public byte DMASubIndex;

		public byte[] Palette;

		public byte[] Data;
	}

	public static byte CalcAlpha(byte Input)
	{
		int val = Input * 2;
		return (byte)Math.Min(val, 255);
	}

	public static byte CalcIndex(byte Input)
	{
		int num = (int)Input % 32;
		if (num > 7 && num < 16)
		{
			return (byte)(Input + 8);
		}
		if (num > 15 && num < 24)
		{
			return (byte)(Input - 8);
		}
		return Input;
	}

    public static MarshalBitmap Unswizzle8(Size Size, List<Color> Pixels)
    {
        MarshalBitmap marshalBitmap = new MarshalBitmap(Size.Width, Size.Height);
        int count = Pixels.Count;
        int width = Size.Width;
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        for (int i = 0; i < count; i++)
        {
            if (i != 0)
            {
                if (i % (width * 2) == 0)
                {
                    switch (num3)
                    {
                        case 0:
                            num2++;
                            break;
                        case 1:
                            num2 += 3;
                            num4 = ((num4 == 0) ? 1 : 0);
                            break;
                    }
                    num = 0;
                    num3 = ((num3 == 0) ? 1 : 0);
                }
                else if (i % 32 == 0)
                {
                    num += 16;
                }
            }
            int x = num;
            int y = num2;
            int num5 = i % 16;
            int num6 = i / 16 % 2;
            if (num4 == 1)
            {
                num6 = ((num6 == 0) ? 1 : 0);
            }
            switch (num6)
            {
                case 0:
                    x = num + num5 % 4 * 4 + num5 / 4;
                    break;
                case 1:
                    {
                        int num7 = 4;
                        if (num5 % 4 >= 2)
                        {
                            num7 = 12;
                        }
                        x = num + (num7 - num5 % 2 * 4) + num5 / 4;
                        break;
                    }
            }
            if (i % 2 == 1)
            {
                y = num2 + 2;
            }
            marshalBitmap.SetPixel(x, y, Pixels[i]);
        }
        return marshalBitmap;
    }
  

    public static List<Color> Swizzle8(MarshalBitmap Input)
	{
		List<Color> list = new List<Color>();
		int num = Input.Width * Input.Height;
		int width = Input.Width;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < num; i++)
		{
			if (i != 0)
			{
				if (i % (width * 2) == 0)
				{
					switch (num4)
					{
					case 0:
						num3++;
						break;
					case 1:
						num3 += 3;
						num5 = ((num5 == 0) ? 1 : 0);
						break;
					}
					num2 = 0;
					num4 = ((num4 == 0) ? 1 : 0);
				}
				else if (i % 32 == 0)
				{
					num2 += 16;
				}
			}
			int x = num2;
			int y = num3;
			int num6 = i % 16;
			int num7 = i / 16 % 2;
			if (num5 == 1)
			{
				num7 = ((num7 == 0) ? 1 : 0);
			}
			switch (num7)
			{
			case 0:
				x = num2 + num6 % 4 * 4 + num6 / 4;
				break;
			case 1:
			{
				int num8 = 4;
				if (num6 % 4 >= 2)
				{
					num8 = 12;
				}
				x = num2 + (num8 - num6 % 2 * 4) + num6 / 4;
				break;
			}
			}
			if (i % 2 == 1)
			{
				y = num3 + 2;
			}
			list.Add(Input.GetPixel(x, y));
		}
		return list;
	}
}
