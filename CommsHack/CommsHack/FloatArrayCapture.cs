using System;
using System.Collections.Generic;
using System.IO;
using Dissonance.Audio.Capture;
using Exiled.API.Features;
using NAudio.Wave;
using UnityEngine;

namespace CommsHack
{
    public class FloatArrayCapture
        : MonoBehaviour, IMicrophoneCapture
    {
        public bool IsRecording { get; private set; }

        public TimeSpan Latency { get; private set; }

        private readonly List<IMicrophoneSubscriber> _subscribers = new List<IMicrophoneSubscriber>();

        private readonly WaveFormat _format = new WaveFormat(48000, 1);
        private readonly float[] _frame = new float[960];
        private readonly byte[] _frameBytes = new byte[960 * 4];
        private float _elapsedTime;
        public Stream _file;
        private int _readOffset;

        public WaveFormat StartCapture(string name)
        {
            if (_file == null || !_file.CanRead)
            {
                Exiled.API.Features.Log.Error("_file==null: " + (_file == null));
                if (_file != null)
                {
                    Exiled.API.Features.Log.Error("_file.CanRead==" + (_file.CanRead));
                }
                IsRecording = false;
                Latency = TimeSpan.FromMilliseconds(0);
                return _format;
            }

            IsRecording = true;
            Latency = TimeSpan.FromMilliseconds(0);
            Log.Info("[FloatArrayCapture] Enabled: " + name);
            return _format;
        }

        public void StopCapture()
        {
            IsRecording = false;
            Log.Info("[FloatArrayCapture] Disabled");
            if (_file != null)
            {
                _file.Dispose();
                _file.Close();
            }
            _file = null;
        }

        public void Subscribe(IMicrophoneSubscriber listener)
        {
            _subscribers.Add(listener);
        }

        public bool Unsubscribe(IMicrophoneSubscriber listener)
        {
            return _subscribers.Remove(listener);
        }

        public bool UpdateSubscribers()
        {
            if (_file == null)
            {
                return true;
            }

            _elapsedTime += Time.unscaledDeltaTime;

            while (_elapsedTime > 0.02f)
            {
                _elapsedTime -= 0.02f;

                // Read bytes from file
                var readLength = _file.Read(_frameBytes, 0, _frameBytes.Length);
                _readOffset += readLength;

                // Zero the entire buffer so bits not written to will be silent
                Array.Clear(_frame, 0, _frame.Length);

                // Copy the bytes that were read into the audio buffer as floats
                Buffer.BlockCopy(_frameBytes, 0, _frame, 0, readLength);

                foreach (var subscriber in _subscribers)
                {
                    subscriber.ReceiveMicrophoneData(new ArraySegment<float>(_frame), _format);
                }
            }

            return false;
        }
    }
}
