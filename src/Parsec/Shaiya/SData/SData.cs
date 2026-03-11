using System.Text;
using Newtonsoft.Json;
using Parsec.Common;
using Parsec.Cryptography;
using Parsec.Helpers;
using Parsec.Serialization;
using Parsec.Shaiya.Core;

namespace Parsec.Shaiya.SData;

public abstract class SData : FileBase, IEncryptable
{
    /// <summary>
    /// The signature present in the header of encrypted files
    /// </summary>
    [JsonIgnore]
    private const string SeedSignature = "0001CBCEBC5B2784D3FC9A2A9DB84D1C3FEB6E99";

    /// <summary>
    /// KISA SEED chunk size in bytes
    /// </summary>
    [JsonIgnore]
    private const int SeedChunkSize = 16;

    /// <summary>
    /// KISA SEED Header size in bytes
    /// </summary>
    [JsonIgnore]
    private const int SeedHeaderSize = 64;

    [JsonIgnore]
    public override string Extension => "SData";

    /// <inheritdoc />
    public void DecryptBuffer(SBinaryReader binaryReader, bool validateChecksum = false)
    {
        var fileBuffer = binaryReader.ReadAllBytes();

        if (!IsEncrypted(fileBuffer))
        {
            binaryReader.ResetOffset();
            return;
        }

        var decryptedBuffer = Decrypt(fileBuffer, validateChecksum);
        binaryReader.ResetBuffer(decryptedBuffer);
    }

    /// <inheritdoc />
    public byte[] GetEncryptedBytes()
    {
        var version = Episode == Episode.EP8 ? SDataVersion.Binary : SDataVersion.Regular;
        var serializationOptions = new BinarySerializationOptions(Episode, Encoding);

        using var memoryStream = new MemoryStream();
        using var binaryWriter = new SBinaryWriter(memoryStream, serializationOptions);
        Write(binaryWriter);

        var encryptedBuffer = Encrypt(memoryStream.ToArray(), version);
        return encryptedBuffer;
    }

    /// <inheritdoc />
    public void WriteEncrypted(string path)
    {
        var encryptedBuffer = GetEncryptedBytes();
        FileHelper.WriteFile(path, encryptedBuffer);
    }

    /// <inheritdoc />
    public void WriteDecrypted(string path) => Write(path);

    /// <summary>
    /// Checks if the file is encrypted with the SEED algorithm
    /// </summary>
    public static bool IsEncrypted(byte[] buffer)
    {
        if (buffer.Length < SeedSignature.Length)
            return false;

        var sDataHeader = Encoding.ASCII.GetString(buffer, 0, SeedSignature.Length);
        return sDataHeader == SeedSignature;
    }

    /// <summary>
    /// Function that encrypts the provided byte array using the SEED algorithm
    /// </summary>
    /// <param name="inputBuffer">The decrypted byte array</param>
    /// <param name="version">Indicates whether the SData version is Pre-EP8 (Regular) or EP8 (Binary)</param>
    public static byte[] Encrypt(byte[] inputBuffer, SDataVersion version = SDataVersion.Regular)
    {
        if (IsEncrypted(inputBuffer))
            return inputBuffer;

        var padding = version == SDataVersion.Regular ? new byte[16] : new byte[12];
        var header = new SeedHeader(SeedSignature, 0, (uint)inputBuffer.Length, padding);
        var alignmentSize = header.RealSize;

        if (alignmentSize % SeedChunkSize != 0)
            alignmentSize = header.RealSize + (SeedChunkSize - header.RealSize % SeedChunkSize);

        // Create data array including the extra alignment bytes
        var data = new byte[alignmentSize];
        Array.Copy(inputBuffer, data, inputBuffer.Length);

        var checksum = uint.MaxValue;

        for (var i = 0; i < header.RealSize; i++)
        {
            var index = (checksum & 0xFF) ^ inputBuffer[i];
            var key = Seed.ByteArrayToUInt32(SeedConstants.ChecksumTable, index * 4);
            Seed.EndiannessSwap(ref key);
            checksum >>= 8;
            checksum ^= key;
        }

        // Final checksum is the bitwise complement of the previously calculated value
        header.Checksum = ~checksum;

        var outputBuffer = new List<byte>((int)alignmentSize);
        outputBuffer.AddRange(header.GetBytes(version));

        var dataSpan = data.AsSpan();

        // Encrypt data in chunks
        for (int i = 0; i < alignmentSize / SeedChunkSize; ++i)
        {
            var chunk = dataSpan.Slice(i * SeedChunkSize, SeedChunkSize).ToArray();
            Seed.EncryptChunk(chunk, out var encryptedChunk);
            outputBuffer.AddRange(encryptedChunk);
        }

        return outputBuffer.ToArray();
    }

    /// <summary>
    /// Function that decrypts the SData buffer using the SEED algorithm
    /// </summary>
    public static byte[] Decrypt(byte[] inputBuffer, bool validateChecksum = false)
    {
        if (!IsEncrypted(inputBuffer))
            return inputBuffer;

        if (inputBuffer.Length % SeedChunkSize != 0)
            throw new FormatException("SData file is not properly aligned.");

        var header = new SeedHeader(inputBuffer);
        var encryptedData = inputBuffer.AsSpan().Slice(SeedHeaderSize);

        var data = new List<byte>();

        // Decrypt data in chunks
        for (var i = 0; i < encryptedData.Length / SeedChunkSize; ++i)
        {
            var chunk = encryptedData.Slice(i * SeedChunkSize, SeedChunkSize);
            Seed.DecryptChunk(chunk.ToArray(), out var decryptedChunk);
            data.AddRange(decryptedChunk);
        }

        if (validateChecksum)
        {
            var checksum = uint.MaxValue;

            // Checksum is calculated with the whole file's data except for the header (not with the real size)
            for (var i = 0; i < header.RealSize; i++)
            {
                var index = (checksum & 0xFF) ^ data[i];
                var key = Seed.ByteArrayToUInt32(SeedConstants.ChecksumTable, index * 4);
                Seed.EndiannessSwap(ref key);
                checksum >>= 8;
                checksum ^= key;
            }

            // Final checksum is the bitwise complement of the previously calculated value
            checksum = ~checksum;

            if (checksum != header.Checksum)
                throw new FormatException("Invalid SEED checksum.");
        }

        var outputBuffer = new byte[header.RealSize];
        Array.Copy(data.ToArray(), outputBuffer, header.RealSize);
        return outputBuffer;
    }

    public static void EncryptFile(string inputFilePath, string outputFilePath, SDataVersion sDataVersion = SDataVersion.Regular)
    {
        var fileData = FileHelper.ReadBytes(inputFilePath);
        var encryptedData = Encrypt(fileData, sDataVersion);
        FileHelper.WriteFile(outputFilePath, encryptedData);
    }

    public static void DecryptFile(string inputFilePath, string outputFilePath, bool validateChecksum = false)
    {
        var fileData = FileHelper.ReadBytes(inputFilePath);
        var decryptedData = Decrypt(fileData, validateChecksum);
        FileHelper.WriteFile(outputFilePath, decryptedData);
    }
}
