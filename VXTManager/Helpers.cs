using System.Runtime.InteropServices;

namespace VXTManager;

public static class Helpers
{
	public static T AssignArray<T>(byte[] bytes) where T : struct
	{
		GCHandle gCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			return (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
		}
		finally
		{
			gCHandle.Free();
		}
	}
}
