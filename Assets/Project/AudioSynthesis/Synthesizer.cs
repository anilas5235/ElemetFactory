using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.AudioSynthesis
{
    [RequireComponent(typeof( AudioSource))]
    public class Synthesizer : MonoBehaviour
    {
        public enum WaveTypes
        {
            Sin,
            Square,
            Triangle,
            RufSin,
        }

        [Range(220, 1760)] public double frequency = 440;
        [Range(0, 1f)] public float amplitude = 0.1f;

        public WaveTypes waveType;

        public bool frequencyShifter;
        public Vector2 range = new Vector2(440,880);
        public float shiftingSpeed = 10;

        private int _bufferLength,_numBuffers, _sampleRate;

        private double _phase, _increment;
        
        private void OnAudioFilterRead(float[] buffer, int channels)
        {
            _increment = frequency/_sampleRate;

            for (int sample = 0; sample < buffer.Length; sample += channels)
            {
                _phase = (_phase + _increment) % 1;
                
                float t = (float) _phase *2* Mathf.PI;

                float sampleValue = 0;

                switch (waveType)
                {
                    case WaveTypes.Sin:sampleValue = Mathf.Sin(t);
                        break;
                    case WaveTypes.Square:sampleValue = Mathf.Sin(t) >= 0 ? 1 : -1;
                        break;
                    case WaveTypes.Triangle:sampleValue = Mathf.PingPong(t, 2) - 1;
                        break;
                    case WaveTypes.RufSin: sampleValue = Mathf.Sin(t + .1f* Mathf.Cos(t));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sampleValue *= amplitude;
                buffer[sample] = buffer[sample + 1] = sampleValue;
            }
        }

        private void Start()
        {

            AudioSettings.GetDSPBufferSize(out _bufferLength, out _numBuffers);
            _sampleRate = AudioSettings.outputSampleRate;

            print($"sampleRate = {_sampleRate} | bufferLength = {_bufferLength} | numBuffers = {_numBuffers}");
            
        }

        private void OnEnable()
        {
            if(frequencyShifter) StartCoroutine(FrequencyShifter(range.x, range.y,shiftingSpeed));
        }

        private IEnumerator FrequencyShifter(float minRange, float maxRange, float speed)
        {
            do
            {
                frequency+=speed;
                if (frequency > maxRange) frequency = minRange;
                yield return new WaitForFixedUpdate();
            } while (true);
        }

    }
}
