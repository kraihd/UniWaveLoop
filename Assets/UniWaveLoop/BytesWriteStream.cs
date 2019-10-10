using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

namespace UniWaveLoop {
	unsafe class BytesWriteStream : IDisposable {
		readonly byte[] array;
		byte* arrayPtr;
		long pos;
		ulong gcHandle;

		public long Position { get => pos; set => pos = value; }
		public long Length => array.LongLength;

		public BytesWriteStream(long size) {
			array = new byte[size];
			arrayPtr = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out gcHandle);
		}

		public void WriteAscii(string str) {
			if (pos + str.Length > array.LongLength) { throw new IndexOutOfRangeException(); }
			if (str == null) { return; }
			for (int i = 0; i < str.Length; i++) {
				char chr = str[i];
				array[pos] = (byte)chr;
				pos++;
			}
		}

		public void Write<T>(T value) where T : unmanaged {
			if (pos + sizeof(T) > array.LongLength) { throw new IndexOutOfRangeException(); }
			*(T*)(arrayPtr + pos) = value;
			pos += sizeof(T);
		}

		public void WriteBytes(void* srcPtr, long byteLength) {
			if (pos + byteLength >= array.LongLength) { throw new IndexOutOfRangeException(); }
			UnsafeUtility.MemCpy(arrayPtr + pos, srcPtr, byteLength);
			pos += byteLength;
		}

		public void SaveToFile(string fileName) {
			File.WriteAllBytes(fileName, array);
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

		~BytesWriteStream() {
			Dispose(false);
		}
	}
}
