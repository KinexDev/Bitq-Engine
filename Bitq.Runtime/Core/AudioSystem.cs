using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using Silk.NET.OpenAL;

namespace Bitq.Audio;

public class AudioSystem : IDisposable
{
    private ALContext _alc;
    private AL _al;
    private unsafe Device* _device;
    private unsafe Context* _context;

    public AudioSystem()
    {
        unsafe
        {
            _alc = ALContext.GetApi();
            _al = AL.GetApi();
            _device = _alc.OpenDevice("");
            if (_device == null)
                throw new Exception("Failed to open audio device.");
            _context = _alc.CreateContext(_device, null);
            _alc.MakeContextCurrent(_context);
        }
    }
    
    public AudioClip LoadWav(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        return AudioClip.CreateFromWave(fileBytes, _al);
    }

    public AudioSource CreateSource()
    {
        return new AudioSource(_al);
    }

    public void Dispose()
    {
        unsafe
        {
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);
            _al.Dispose();
            _alc.Dispose();
        }
    }
}

public class AudioClip : IDisposable
{
    private AL _al;
    public uint Buffer { get; private set; }

    private AudioClip(AL al, uint buffer)
    {
        _al = al;
        Buffer = buffer;
    }

    public static AudioClip CreateFromWave(byte[] fileBytes, AL al)
    {
        ReadOnlySpan<byte> file = fileBytes;
        int index = 0;

        if (file[index++] != 'R' || file[index++] != 'I' || file[index++] != 'F' || file[index++] != 'F')
            throw new Exception("File is not in RIFF format.");

        index += 4;

        if (file[index++] != 'W' || file[index++] != 'A' || file[index++] != 'V' || file[index++] != 'E')
            throw new Exception("File is not in WAVE format.");

        short numChannels = 0;
        int sampleRate = 0;
        short bitsPerSample = 0;
        BufferFormat format = 0;

        uint bufferId = al.GenBuffer();

        while (index + 4 < file.Length)
        {
            string identifier = "" + (char)file[index++] + (char)file[index++] + (char)file[index++] +
                                (char)file[index++];
            int size = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
            index += 4;

            if (identifier == "fmt ")
            {
                if (size < 16)
                    throw new Exception($"Unexpected fmt chunk size: {size}");

                short audioFormat = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                index += 2;
                if (audioFormat != 1)
                    throw new Exception($"Unsupported audio format: {audioFormat}");

                numChannels = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                index += 2;
                sampleRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                index += 4;
                index += 4;
                index += 2;
                bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                index += 2;

                if (numChannels == 1)
                {
                    if (bitsPerSample == 8)
                        format = BufferFormat.Mono8;
                    else if (bitsPerSample == 16)
                        format = BufferFormat.Mono16;
                    else
                        throw new Exception($"Unsupported mono bit depth: {bitsPerSample}");
                }
                else if (numChannels == 2)
                {
                    if (bitsPerSample == 8)
                        format = BufferFormat.Stereo8;
                    else if (bitsPerSample == 16)
                        format = BufferFormat.Stereo16;
                    else
                        throw new Exception($"Unsupported stereo bit depth: {bitsPerSample}");
                }
                else
                {
                    throw new Exception($"Unsupported number of channels: {numChannels}");
                }
            }
            else if (identifier == "data")
            {
                unsafe
                {
                    ReadOnlySpan<byte> data = file.Slice(index, size);
                    index += size;

                    fixed (byte* pData = data)
                    {
                        al.BufferData(bufferId, format, pData, size, sampleRate);
                    }
                }
            }
            else
            {
                index += size;
            }
        }

        return new AudioClip(al, bufferId);
    }

    public void Dispose()
    {
        _al.DeleteBuffer(Buffer);
    }
}

public class AudioSource : IDisposable
{
    private AL _al;
    public uint Source;

    public AudioSource(AL al)
    {
        _al = al;
        Source = _al.GenSource();
        _al.SetSourceProperty(Source, SourceBoolean.Looping, false);
    }
    
    public void Play(AudioClip clip, bool loop = false)
    {
        _al.SetSourceProperty(Source, SourceInteger.Buffer, clip.Buffer);
        _al.SetSourceProperty(Source, SourceBoolean.Looping, loop);
        _al.SourcePlay(Source);
    }

    public void Stop()
    {
        _al.SourceStop(Source);
    }

    public void Dispose()
    {
        _al.DeleteSource(Source);
    }
}