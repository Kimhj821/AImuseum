using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip clip, string filepath, bool saveToDisk = true)
    {
        byte[] wavFile = ConvertAudioClipToWav(clip);
        if (saveToDisk)
        {
            File.WriteAllBytes(filepath, wavFile);
        }

        return wavFile;
    }

    public static byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        byte[] wav = new byte[HEADER_SIZE + samples.Length * 2];
        WriteHeader(wav, clip);

        int offset = HEADER_SIZE;
        for (int i = 0; i < samples.Length; i++)
        {
            short intData = (short)(samples[i] * short.MaxValue);
            byte[] byteData = BitConverter.GetBytes(intData);
            wav[offset++] = byteData[0];
            wav[offset++] = byteData[1];
        }

        return wav;
    }

    private static void WriteHeader(byte[] stream, AudioClip clip)
    {
        int sampleCount = clip.samples;
        int channels = clip.channels;
        int sampleRate = clip.frequency;
        int byteRate = sampleRate * channels * 2;

        // Chunk ID "RIFF"
        stream[0] = (byte)'R';
        stream[1] = (byte)'I';
        stream[2] = (byte)'F';
        stream[3] = (byte)'F';

        // ChunkSize
        int fileSize = stream.Length - 8;
        BitConverter.GetBytes(fileSize).CopyTo(stream, 4);

        // Format "WAVE"
        stream[8] = (byte)'W';
        stream[9] = (byte)'A';
        stream[10] = (byte)'V';
        stream[11] = (byte)'E';

        // Subchunk1ID "fmt "
        stream[12] = (byte)'f';
        stream[13] = (byte)'m';
        stream[14] = (byte)'t';
        stream[15] = (byte)' ';

        // Subchunk1Size (16 for PCM)
        BitConverter.GetBytes(16).CopyTo(stream, 16);

        // AudioFormat (1 for PCM)
        BitConverter.GetBytes((short)1).CopyTo(stream, 20);

        // NumChannels
        BitConverter.GetBytes((short)channels).CopyTo(stream, 22);

        // SampleRate
        BitConverter.GetBytes(sampleRate).CopyTo(stream, 24);

        // ByteRate
        BitConverter.GetBytes(byteRate).CopyTo(stream, 28);

        // BlockAlign
        BitConverter.GetBytes((short)(channels * 2)).CopyTo(stream, 32);

        // BitsPerSample
        BitConverter.GetBytes((short)16).CopyTo(stream, 34);

        // Subchunk2ID "data"
        stream[36] = (byte)'d';
        stream[37] = (byte)'a';
        stream[38] = (byte)'t';
        stream[39] = (byte)'a';

        // Subchunk2Size
        int subchunk2Size = sampleCount * channels * 2;
        BitConverter.GetBytes(subchunk2Size).CopyTo(stream, 40);
    }
}
