using System;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UniWaveLoop {
	public unsafe class WaveFile : IDisposable {
		const int idLength = 4;
		const string headerIdRiff = "RIFF";
		const string headerIdWave = "WAVE";
		const string chunkIdFmt = "fmt ";
		const string chunkIdData = "data";
		const string chunkIdSmpl = "smpl";

		[StructLayout(LayoutKind.Sequential, Size = 0x10)]
		struct Fmt {
			public ushort format;
			public ushort channelCnt;
			public uint samplingRate;
			public uint bytesPerSec; // = samplingRate * channnelCnt * bitRate
			public ushort blockAlign;
			public ushort bitRate;
		}

		[StructLayout(LayoutKind.Sequential, Size = 0x3C)]
		struct Smpl {
			public uint manufacturer;
			public uint product;
			public uint samplePeriod;
			public uint midiUnityNote;
			public uint midiPitchFraction;
			public uint SmpteFormat;
			public uint SmpteOffset;
			public uint loopPointsCnt; // = forcing 1
			public uint optionalDataSize;
			public LoopMarker loopMarker;

			[StructLayout(LayoutKind.Sequential, Size = 0x18)]
			public struct LoopMarker {
				public uint id;
				public uint type;
				public uint start;
				public uint end;
				public uint fraction;
				public uint playCount;
			}
		}

		Fmt fmt;
		void* waveForm;
		Smpl smpl;

		public bool IsValid => waveForm != null;
		public uint SamplingRate => fmt.samplingRate;
		public uint SampleCount { get; private set; }

		public uint LoopPoint {
			get => smpl.loopMarker.start;
			set => smpl.loopMarker.start = value;
		}

		public WaveFile(string fileName) : this(File.ReadAllBytes(fileName)) { }

		public WaveFile(byte[] fileData) {
			using (var stream = new BytesReadStream(fileData)) {
				if (!stream.ReadAndCompareAscii(4, headerIdRiff)) {
					throw new FormatException("Unexpected RIFF header");
				}

				var predictedFileSize = stream.Read<uint>() + 8;

				if (!stream.ReadAndCompareAscii(4, headerIdWave)) {
					throw new FormatException("Unexpected WAVE header");
				}

				bool smplExists = false;

				while (stream.Position < stream.Length) {
					var header = stream.ReadAscii(4);
					var chunkSize = stream.Read<uint>();
					var startPos = stream.Position;

					switch (header) {
						case chunkIdFmt:
							if (chunkSize < sizeof(Fmt)) {
								throw new FormatException("Unsupported fmt chunk size: " + chunkSize);
							}
							fmt = stream.Read<Fmt>();
							if (fmt.channelCnt != 1 && fmt.channelCnt != 2) {
								throw new FormatException("Unsupported channel count: " + fmt.channelCnt);
							}
							break;
						case chunkIdData:
							waveForm = UnsafeUtility.Malloc(chunkSize, 4, Allocator.Persistent);
							stream.ReadBytes(waveForm, chunkSize);
							SampleCount = chunkSize / fmt.channelCnt / sizeof(ushort);
							break;
						case chunkIdSmpl:
							if (chunkSize == sizeof(Smpl)) {
								smpl = stream.Read<Smpl>();
								smplExists = true;
							} else {
								Debug.LogWarning("Unsupported smpl chunk. Skipped.");
							}
							break;
						default:
							Debug.LogFormat("Chunk \"{0}\" will be removed.", header);
							break;
					}

					stream.Position = startPos + chunkSize;
				}

				if (predictedFileSize != stream.Position) {
					throw new FormatException("Unexpected file size. Definition: " + predictedFileSize + " Result: " + stream.Position);
				}

				smpl.loopPointsCnt = 1;
				if (!smplExists) {
					// ループ範囲の終端はファイル終端
					smpl.loopMarker.end = SampleCount - 1;
				}
			}
		}

		public void Publish(string fileName) {
			// header
			long size = idLength + sizeof(uint) + idLength;

			// fmt
			size += idLength + sizeof(uint) + sizeof(Fmt);

			// data
			long dataChunkLength = (int)SampleCount * fmt.channelCnt * sizeof(ushort);
			size += idLength + sizeof(uint) + dataChunkLength;

			// smpl
			size += idLength + sizeof(uint) + sizeof(Smpl);

			using (var stream = new BytesWriteStream(size)) {
				stream.WriteAscii(headerIdRiff);
				stream.Write((uint)(size - 8));
				stream.WriteAscii(headerIdWave);

				stream.WriteAscii(chunkIdFmt);
				stream.Write((uint)sizeof(Fmt));
				stream.Write(fmt);

				stream.WriteAscii(chunkIdData);
				stream.Write((uint)dataChunkLength);
				stream.WriteBytes(waveForm, dataChunkLength);

				stream.WriteAscii(chunkIdSmpl);
				stream.Write((uint)sizeof(Smpl));
				stream.Write(smpl);

				stream.SaveToFile(fileName);
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing) {
			if (waveForm != default) {
				UnsafeUtility.Free(waveForm, Allocator.Persistent);
				waveForm = default;
			}
		}

		~WaveFile() {
			Dispose(false);
		}
	}
}
