using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace UniWaveLoop {
	unsafe class BytesReadStream : IDisposable {
		readonly byte[] array;
		byte* arrayPtr;
		long pos;
		ulong gcHandle;

		public long Position { get => pos; set => pos = value; }
		public long Length => array.LongLength;

		public BytesReadStream(byte[] array) {
			this.array = array;
			arrayPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out gcHandle);
		}

		public bool ReadAndCompareAscii(long length, string str) {
			if (pos + length > array.LongLength) { throw new IndexOutOfRangeException(); }
			if (str == null) { return length == 0; }
			bool isOk = true;
			for (int i = 0; i < str.Length; i++) {
				if (array[pos] != (byte)str[i]) { isOk = false; }
				pos++;
			}
			return isOk;
		}

		public string ReadAscii(int byteLength) {
			if (pos + byteLength > array.LongLength) { throw new IndexOutOfRangeException(); }
			byte* chrArray = stackalloc byte[byteLength + 1];
			UnsafeUtility.MemCpy(chrArray, arrayPtr + pos, byteLength);
			pos += byteLength;
			return Marshal.PtrToStringAnsi((IntPtr)chrArray);
		}

		public T Read<T>() where T : unmanaged {
			if (pos + sizeof(T) > array.LongLength) { throw new IndexOutOfRangeException(); }
			T value = *(T*)(arrayPtr + pos);
			pos += sizeof(T);
			return value;
		}

		public void ReadBytes(void* dstPtr, long byteLength) {
			if (pos + byteLength > array.LongLength) { throw new IndexOutOfRangeException(); }
			UnsafeUtility.MemCpy(dstPtr, arrayPtr + pos, byteLength);
			pos += byteLength;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing) {
			if (gcHandle != 0) {
				UnsafeUtility.ReleaseGCObject(gcHandle);
				gcHandle = 0;
				arrayPtr = null;
			}
		}

		~BytesReadStream() {
			Dispose(false);
		}
	}
}
