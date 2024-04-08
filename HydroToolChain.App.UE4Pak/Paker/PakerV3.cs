using System.Security.Cryptography;
using System.Text;

namespace HydroToolChain.App.UE4Pak.Paker;

/// <summary>
/// Create UE4 Version 3 Paks
/// <see cref="https://github.com/panzi/rust-u4pak/blob/main/README.adoc"> Pak Format </see>
/// </summary>
public class PakerV3 : IPaker
{
    private PakInfo? _info;
        
    public async Task PakMod(string workingDirectory, string modName, int modIndex, CancellationToken ct)
    {
        _info = new PakInfo()
        {
            AssetsFolder = Path.Combine(workingDirectory, "Staging", modName, "Mining"),
            EngineFolder = Path.Combine(workingDirectory, "Staging", modName, "Engine"),
            OutputFileName = $"{modIndex}-{modName}_P.pak",
            OutputPath = Path.Combine(workingDirectory, "dist")
        };

        await using var fileStream = File.OpenWrite(Path.Combine(_info!.OutputPath, _info!.OutputFileName));

        Console.WriteLine("PakV3 Start");
        await BuildPak(fileStream, ct);
            
        await fileStream.FlushAsync(ct);
        fileStream.Close();
        Console.WriteLine("PakV3 Completed");
    }

    private async Task BuildPak(Stream pakFileStream, CancellationToken ct)
    {
        var filesToPakList = Directory.EnumerateFiles(_info!.AssetsFolder, "*", SearchOption.AllDirectories);

        if (Directory.Exists(_info!.EngineFolder))
        {
            filesToPakList = filesToPakList
                    .Concat(Directory.EnumerateFiles(_info!.EngineFolder, "*", SearchOption.AllDirectories));
        }

        var filesToPak = filesToPakList
            .ToArray();

        var fileCount = filesToPak.Length;
        long indexOffset = 0;
        var compressedFiles = new (List<byte>[] fileBytes, long fileSize, int blocks, int blockSize)[fileCount];
        var chunksOffsets = new List<(long start, long end)>[fileCount];
        var recordsOffsets = new long[fileCount];
        long chunkOffsetStart = 0;

        for (var i = 0; i < fileCount; i++)
        {

            var compressedFile = CompressFileInChunks(filesToPak[i]);
            compressedFiles[i] = compressedFile;
            
            var record = BuildRecordForDataRecord(
                compressedFile.fileBytes,
                compressedFile.fileSize,
                compressedFile.blocks,
                compressedFile.blockSize,
                chunkOffsetStart, indexOffset);
            chunksOffsets[i] = record.offsets.ToList();
            
            recordsOffsets[i] = indexOffset; 
            await pakFileStream.WriteAsync(record.record.AsMemory(0, record.record.Length), ct);
            indexOffset += record.record.Length;
            
            foreach (var fileChunk in compressedFile.fileBytes)
            {
                await pakFileStream.WriteAsync(fileChunk.ToArray().AsMemory(0, fileChunk.Count), ct);
                indexOffset += fileChunk.Count;
            }

            chunkOffsetStart = indexOffset;
        }
            
        const string mountPoint = "../../../";
        var index = new List<byte>();
        index.AddRange(BitConverter.GetBytes(mountPoint.Length + 1)); // Mount Point size || 4 Bytes
        index.AddRange(Encoding.ASCII.GetBytes(mountPoint+"\0")); // Mount Point || Mount Point size
        index.AddRange(BitConverter.GetBytes(fileCount)); // Record count || 4 Bytes

        for (var i = 0; i < fileCount; i++) //Index records || Record size + File name size + 4 Bytes
        {
            var fileName = filesToPak[i]
                .Replace(_info!.AssetsFolder, "Mining")
                .Replace(_info!.EngineFolder, "Engine")
                .Replace('\\', '/');
                
            index.AddRange(BitConverter.GetBytes(fileName.Length + 1)); // File name size || 4 Bytes
            index.AddRange(Encoding.ASCII.GetBytes(fileName+"\0")); // File name || File name size

            var compressedFile = compressedFiles[i];
            var record = BuildRecordForIndex(
                compressedFile.fileBytes,
                compressedFile.fileSize,
                compressedFile.blocks,
                compressedFile.blockSize,
                chunksOffsets[i].ToArray(),
                recordsOffsets[i]);
            
            index.AddRange(record); // Record || Record Size
        }

        var indexBytes = index.ToArray();

        await pakFileStream.WriteAsync(indexBytes, ct); // Add Index to Pak

        var footer = BuildFooter(indexOffset, indexBytes.Length, indexBytes);
        await pakFileStream.WriteAsync(footer, ct); // Add Footer to Pak

    }
    
    private (byte[] record, (long start, long end)[] offsets) BuildRecordForDataRecord(List<byte>[] compressedFile, long uncompressedDataSize, int blockCount, int blockSize, long chuckOffsetPointer, long recordOffset)
    {
        var wholeFile = compressedFile
            .SelectMany(chunk => chunk)
            .ToArray();
        
        var record = new List<byte>();
        record.AddRange(BitConverter.GetBytes(recordOffset)); // RecordOffset || 8 Bytes
        record.AddRange(BitConverter.GetBytes((long)wholeFile.Length)); // File Data Compressed size || 8 Bytes
        record.AddRange(BitConverter.GetBytes(uncompressedDataSize)); // File Data uncompressed size || 8 Bytes
        record.AddRange(BitConverter.GetBytes(1)); // Compression method (zLib) || 4 Bytes
        record.AddRange(SHA1.Create().ComputeHash(wholeFile)); // File data sha1 hash || 20 Bytes
        record.AddRange(BitConverter.GetBytes(blockCount)); // Block count || 4 Bytes
        chuckOffsetPointer += 57; // Must include encryption byte and block size info already (+5 Bytes)

        var offsets = new (long start, long end)[compressedFile.Length];
        for (var i = 0; i < compressedFile.Length; i++)
        {
            var chunk = compressedFile[i];
            
            chuckOffsetPointer += 16;
            offsets[i].start = chuckOffsetPointer;
            record.AddRange(BitConverter.GetBytes(chuckOffsetPointer)); // Block start offset || 8 Bytes
            
            chuckOffsetPointer += chunk.Count;
            offsets[i].end = chuckOffsetPointer;
            record.AddRange(BitConverter.GetBytes(chuckOffsetPointer)); // Block end offset || 8 Bytes
        }
            
        record.Add(0x00); // Is encrypted || 1 Byte
        record.AddRange(BitConverter.GetBytes(blockSize)); // Size of uncompressed blocks || 4 Bytes
            
        return (record.ToArray(), offsets);
    }
    
    private byte[] BuildRecordForIndex(List<byte>[] compressedFile, long uncompressedDataSize, int blockCount, int blockSize, (long start, long end)[] chuckOffsets, long recordOffset)
    {
        var wholeFile = compressedFile
            .SelectMany(chunk => chunk)
            .ToArray();
        
        var record = new List<byte>();
        record.AddRange(BitConverter.GetBytes(recordOffset)); // RecordOffset || 8 Bytes
        record.AddRange(BitConverter.GetBytes((long)wholeFile.Length)); // File Data Compressed size || 8 Bytes
        record.AddRange(BitConverter.GetBytes(uncompressedDataSize)); // File Data uncompressed size || 8 Bytes
        record.AddRange(BitConverter.GetBytes(1)); // Compression method (zLib) || 4 Bytes
        record.AddRange(SHA1.Create().ComputeHash(wholeFile)); // File data sha1 hash || 20 Bytes
        record.AddRange(BitConverter.GetBytes(blockCount)); // Block count || 4 Bytes
        
        foreach (var offset in chuckOffsets) // Compression blocks || 16 Bytes x Block count
        {
            record.AddRange(BitConverter.GetBytes(offset.start)); // Block start offset || 8 Bytes
            
            record.AddRange(BitConverter.GetBytes(offset.end)); // Block end offset || 8 Bytes
        }
            
        record.Add(0x00); // Is encrypted || 1 Byte
        record.AddRange(BitConverter.GetBytes(blockSize)); // Size of uncompressed blocks || 4 Bytes
            
        return record.ToArray();
    }

    private (List<byte>[] fileBytes, long fileSize, int blocks, int blockSize) CompressFileInChunks(string filePath)
    {
        using var fileStream = new BinaryReader(File.OpenRead(filePath));
        var fileSize = new FileInfo(filePath).Length;
        const int sectionSize = 65536;
        var sectionsCount = (int)Math.Ceiling(fileSize / (decimal)sectionSize);
        (long size, List<byte>[] fileData) sections = new (0, new List<byte>[sectionsCount]);

        var remainingBytes = fileSize;
        for (var i = 0; i < sectionsCount; i++)
        {
            var currentSectionSize = (int)Math.Min(remainingBytes, sectionSize);
            remainingBytes -= currentSectionSize;
            var compressedChunk = Utilities.CompressChunk(fileStream.ReadBytes(currentSectionSize));
            sections.size += compressedChunk.Length;
            sections.fileData[i] = compressedChunk.ToList();
        }

        return (sections.fileData, fileSize, sectionsCount, sectionSize);
    }

    private static byte[] BuildFooter(long indexOffset, long indexSize, byte[] index)
    {
        var footer = new byte[44];
        BitConverter.GetBytes(0x5A6F12E1).CopyTo(footer, 0); //Magic || 4 Bytes
        BitConverter.GetBytes(3).CopyTo(footer, 4);// Pak Version || 4 Bytes
        BitConverter.GetBytes(indexOffset).CopyTo(footer, 8); // Index Offset || 8 Bytes
        BitConverter.GetBytes(indexSize).CopyTo(footer, 16); // Index Size || 8 Bytes
        SHA1.Create().ComputeHash(index).CopyTo(footer, 24); // Index SHA1 Hash || 20 Bytes

        return footer;
    }
    
    private class PakInfo
    {
        public string AssetsFolder { get; init; } = string.Empty;
        public string EngineFolder { get; init; } = string.Empty;
        public string OutputFileName { get; init; } = string.Empty;
        public string OutputPath { get; init; } = string.Empty;
    }
}