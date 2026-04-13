using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace VXTManager;

public class VXT
{
	public struct VXTInfo
	{
		public uint ColorOffset;
		public uint ColorSize;

		public uint DataOffset;
		public uint DataSize;
		public uint ColorCount;
	}

    public class GsTex0
    {

        private ulong _value;
        public GsTex0(ulong value)
        {
            _value = value;
        }

        public GsTex0() : this(0) { }
        public uint TbAddr
        {
            get => (uint)((_value >> 0) & 0x3FFF);
            set => _value = (_value & ~((ulong)0x3FFF)) | ((ulong)value & 0x3FFF);
        }
        public uint TbWidth
        {
            get => (uint)((_value >> 14) & 0x3F);
            set => _value = (_value & ~((ulong)0x3F << 14)) | ((ulong)value & 0x3F) << 14;
        }
        public uint Psm
        {
            get => (uint)((_value >> 20) & 0x3F);
            set => _value = (_value & ~((ulong)0x3F << 20)) | ((ulong)value & 0x3F) << 20;
        }
        public uint TexWidth
        {
            get => (uint)((_value >> 26) & 0xF);
            set => _value = (_value & ~((ulong)0xF << 26)) | ((ulong)value & 0xF) << 26;
        }
        public uint TexHeight
        {
            get => (uint)((_value >> 30) & 0xF);
            set => _value = (_value & ~((ulong)0xF << 30)) | ((ulong)value & 0xF) << 30;
        }
        public uint TexCc
        {
            get => (uint)((_value >> 34) & 0x1);
            set => _value = (_value & ~((ulong)0x1 << 34)) | ((ulong)value & 0x1) << 34;
        }
        public uint TexFunction
        {
            get => (uint)((_value >> 35) & 0x3);
            set => _value = (_value & ~((ulong)0x3 << 35)) | ((ulong)value & 0x3) << 35;
        }
        public uint CbAddr
        {
            get => (uint)((_value >> 37) & 0x3FFF);
            set => _value = (_value & ~((ulong)0x3FFF << 37)) | ((ulong)value & 0x3FFF) << 37;
        }
        public uint ClutPixmode
        {
            get => (uint)((_value >> 51) & 0xF);
            set => _value = (_value & ~((ulong)0xF << 51)) | ((ulong)value & 0xF) << 51;
        }
        public uint ClutSmode
        {
            get => (uint)((_value >> 55) & 0x1);
            set => _value = (_value & ~((ulong)0x1 << 55)) | ((ulong)value & 0x1) << 55;
        }
        public uint ClutOffset
        {
            get => (uint)((_value >> 56) & 0x1F);
            set => _value = (_value & ~((ulong)0x1F << 56)) | ((ulong)value & 0x1F) << 56;
        }
        public uint ClutLoadmode
        {
            get => (uint)((_value >> 61) & 0x7);
            set => _value = (_value & ~((ulong)0x7 << 61)) | ((ulong)value & 0x7) << 61;
        }
    }

    public List<PS2.PS2Image> Images;


	public VXTInfo[][] Info;

	public byte ver;

	public int CInfoOffset;

	public VXT(Stream VXTStream)
	{
		Images = new List<PS2.PS2Image>();
		
		using BinaryReader binaryReader = new BinaryReader(VXTStream, Encoding.UTF8, leaveOpen: true);
        binaryReader.BaseStream.Position = 0L;
        ver = binaryReader.ReadByte();
		long trueoffset = 0;
		if (ver == 8)//Where a model morty!(SC2)
        {
            binaryReader.BaseStream.Position = 0x14;
            trueoffset = (long)binaryReader.ReadUInt32();


        }
        else if(ver == 9)//SC3
		{
            binaryReader.BaseStream.Position = 0x24;
            trueoffset = (long)binaryReader.ReadUInt32();
        }
        binaryReader.BaseStream.Position = trueoffset;
        ver = binaryReader.ReadByte();
        binaryReader.BaseStream.Position = trueoffset + 4L;
        byte DMA_Chain_Count = binaryReader.ReadByte();
		byte Material_Count = binaryReader.ReadByte();
		binaryReader.BaseStream.Position = trueoffset + 8L;
		uint dma_array_pointer = (uint)trueoffset + binaryReader.ReadUInt32();


        binaryReader.BaseStream.Position = trueoffset + 12L;
		uint TextureTableOffset = (uint)trueoffset + binaryReader.ReadUInt32();
		binaryReader.BaseStream.Position = trueoffset + (32 + 16 * Material_Count + 20);
		CInfoOffset = (int)trueoffset + binaryReader.ReadInt32();
		binaryReader.BaseStream.Position = trueoffset + 0x24;
        Info = new VXTInfo[DMA_Chain_Count][];
        for (int i = 0; i < DMA_Chain_Count; i++)
		{
            
			//3 has an extra 64bit number after, idk why
            binaryReader.BaseStream.Position = dma_array_pointer + (i*(0xC +(ver == 3 ? 8 : 0)));
			uint base_dma_offset = (uint)trueoffset +  binaryReader.ReadUInt32();
			binaryReader.BaseStream.Position += 4;
			ushort dma_chain_len = binaryReader.ReadUInt16();
            VXTInfo[] dma_list = new VXTInfo[dma_chain_len];

            for (int j = 0;  j < dma_chain_len; j++)

			{
				VXTInfo cur_info = new VXTInfo();

				//Pallet Data
				binaryReader.BaseStream.Position = base_dma_offset + (j * 0x10) + 4;
				uint dataoffset = (uint)trueoffset + binaryReader.ReadUInt32();
				binaryReader.BaseStream.Position = dataoffset;
				byte trans_data = binaryReader.ReadByte();
				long head_pal = dataoffset + (0x10 * trans_data)+0x10;
                binaryReader.BaseStream.Position = head_pal;
				cur_info.DataSize = (uint)(binaryReader.ReadUInt16()) * 16;
				binaryReader.ReadUInt16(); // skip the tag
				cur_info.DataOffset = (uint)trueoffset + binaryReader.ReadUInt32();

                //Texel Data
                binaryReader.BaseStream.Position = head_pal+0x10;
                trans_data = binaryReader.ReadByte();
                long head_tex = (head_pal + 0x10) + (0x10 * trans_data)+0x10;
				binaryReader.BaseStream.Position = head_tex;
                cur_info.ColorSize = (uint)(binaryReader.ReadUInt16()) * 16;
                binaryReader.ReadUInt16(); // skip the tag
				cur_info.ColorOffset = (uint)trueoffset + binaryReader.ReadUInt32();
                dma_list[j] = cur_info;
            }
			Info[i] = dma_list;
		}
		for (int j = 0; j < Material_Count; j++)
		{
			try
			{
				PS2.PS2Image item2 = default(PS2.PS2Image);
				binaryReader.BaseStream.Position = TextureTableOffset + 4 * j;
				uint OffsetToTextureInfo =  binaryReader.ReadUInt32();
				OffsetToTextureInfo = (uint)trueoffset +( OffsetToTextureInfo - (OffsetToTextureInfo % 0x10));

				binaryReader.BaseStream.Position = OffsetToTextureInfo + 9;
				byte dma_index = binaryReader.ReadByte();
				byte dma_sub_index = binaryReader.ReadByte();

                item2.DMAIndex = dma_index;
				item2.DMASubIndex= dma_sub_index;
                binaryReader.BaseStream.Position = OffsetToTextureInfo + 0xC;
				item2.TrueSize = new Size(binaryReader.ReadUInt16(), binaryReader.ReadUInt16());

                binaryReader.BaseStream.Position = OffsetToTextureInfo + 0x20;
				ulong rawData = binaryReader.ReadUInt64();
				GsTex0 gsTex = new GsTex0(rawData);

                item2.FixedSize = item2.TrueSize;
                if (item2.TrueSize.Width % 16 != 0)
				{
					item2.FixedSize = new Size(1 << ((int)gsTex.TexWidth), 1 << ((int)gsTex.TexHeight));
                }
				uint color_cnt = Info[dma_index][dma_sub_index].ColorSize / 4;
				item2.ColorCount = color_cnt;
				Images.Add(item2);
			}
			catch (Exception)
			{
                Console.WriteLine("Bad 1");
            }
		}
		for (int k = 0; k < Material_Count; k++)
		{
			try
			{
				PS2.PS2Image value = Images[k];
				VXTInfo vXTInfo = Info[value.DMAIndex][value.DMASubIndex];
				binaryReader.BaseStream.Position = vXTInfo.DataOffset;
				value.Data = binaryReader.ReadBytes((int)vXTInfo.DataSize);
				binaryReader.BaseStream.Position = vXTInfo.ColorOffset;
				value.Palette = binaryReader.ReadBytes((int)vXTInfo.ColorSize);
				Images[k] = value;
			}
			catch (Exception)
			{
                Console.WriteLine("Bad 2");
            }
		}
	}

    public MarshalBitmap Parse8(int Index)
    {
        PS2.PS2Image pS2Image = Images[Index];
        if (pS2Image.ColorCount > 16)
        {
            MarshalBitmap marshalBitmap = new MarshalBitmap(pS2Image.FixedSize.Width, pS2Image.FixedSize.Height);
            List<Color> list = new List<Color>();
            List<byte> list2 = new List<byte>();
            int num = 0;
            for (int i = 0; i < pS2Image.Palette.Length; i += 4)
            {
                byte red = pS2Image.Palette[i];
                byte green = pS2Image.Palette[i + 1];
                byte blue = pS2Image.Palette[i + 2];
                byte input = pS2Image.Palette[i + 3];
                list.Add(Color.FromArgb(PS2.CalcAlpha(input), red, green, blue));
            }
            for (int j = 0; j < pS2Image.Data.Length; j++)
            {
                list2.Add(PS2.CalcIndex(pS2Image.Data[j]));
            }
            for (int k = 0; k < marshalBitmap.Height; k++)
            {
                for (int l = 0; l < marshalBitmap.Width; l++)
                {
                    if (num != list2.Count)
                    {
                        byte index = list2[num];
                        marshalBitmap.SetPixel(l, k, list[index]);
                        num++;
                    }
                }
            }
            return marshalBitmap;
        }
        /*else
        {
            MarshalBitmap marshalBitmap = new MarshalBitmap(pS2Image.FixedSize.Width, pS2Image.FixedSize.Height);
            List<Color> list = new List<Color>();
            List<byte> list2 = new List<byte>();
            int num = 0;
            for (int i = 0; i < pS2Image.Palette.Length; i += 4)
            {
                byte red = pS2Image.Palette[i];
                byte green = pS2Image.Palette[i + 1];
                byte blue = pS2Image.Palette[i + 2];
                byte input = pS2Image.Palette[i + 3];
                list.Add(Color.FromArgb(PS2.CalcAlpha(input), red, green, blue));
            }
            for (int j = 0; j < pS2Image.Data.Length; j++)
            {
                byte v1 = (byte)(pS2Image.Data[j] & 0xF);
                byte v2 = (byte)((byte)(pS2Image.Data[j] & 0xF0) >> 4);
                list2.Add(PS2.CalcIndex(v1));
                list2.Add(PS2.CalcIndex(v2));
            }
            for (int k = 0; k < marshalBitmap.Height; k++)
            {
                for (int l = 0; l < marshalBitmap.Width; l++)
                {
                    if (num != list2.Count)
                    {
                        byte index = list2[num];
                        marshalBitmap.SetPixel(l, k, list[index]);
                        num++;
                    }
                }
            }
            return marshalBitmap;
        }*/
            return null;
    }
    public MarshalBitmap Parse4(int Index)
    {
        PS2.PS2Image pS2Image = Images[Index];

            MarshalBitmap marshalBitmap = new MarshalBitmap(pS2Image.FixedSize.Width, pS2Image.FixedSize.Height);
            List<Color> list = new List<Color>();
            List<byte> list2 = new List<byte>();
            int num = 0;
            for (int i = 0; i < pS2Image.Palette.Length; i += 4)
            {
                byte red = pS2Image.Palette[i];
                byte green = pS2Image.Palette[i + 1];
                byte blue = pS2Image.Palette[i + 2];
                byte input = pS2Image.Palette[i + 3];
                list.Add(Color.FromArgb(PS2.CalcAlpha(input), red, green, blue));
            }
            for (int j = 0; j < pS2Image.Data.Length; j++)
            {
            byte v1 = (byte)(pS2Image.Data[j] & 0xF);
            byte v2 = (byte)((byte)(pS2Image.Data[j] & 0xF0)>>4);
            list2.Add(PS2.CalcIndex(v1));
            list2.Add(PS2.CalcIndex(v2));
        }
            for (int k = 0; k < marshalBitmap.Height; k++)
            {
                for (int l = 0; l < marshalBitmap.Width; l++)
                {
                    if (num != list2.Count)
                    {
                        byte index = list2[num];
                        marshalBitmap.SetPixel(l, k, list[index]);
                        num++;
                    }
                }
            }
            return marshalBitmap;
    }
}
