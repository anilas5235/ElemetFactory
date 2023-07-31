using System;
using System.Collections;
using UnityEngine;

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

        private System.Random random;

        [Range(220, 1760)] public double frequency = 440;

        [Range(0, 1f)] public float gain = 0.5f;

        public WaveTypes WaveType;

        public bool frequencyShifter;
        public Vector2 Range = new Vector2(440,880);
        public float shiftingSpeed = 10;

        private int bufferLength,numBuffers, sampleRate;

        private double phase, increment;

        private void Start()
        {
            random = new System.Random();

            AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
            sampleRate = AudioSettings.outputSampleRate;

            print($"sampleRate = {sampleRate} | bufferLength = {bufferLength} | numBuffers = {numBuffers}");
            
        }

        private void OnEnable()
        {
            if(frequencyShifter) StartCoroutine(FrequencyShifter(Range.x, Range.y,shiftingSpeed));
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

        private void OnAudioFilterRead(float[] buffer, int channels)
        {
            increment = frequency * 2 * Math.PI / sampleRate;

            for (int i = 0; i < buffer.Length; i += channels)
            {
                phase += increment;
                if (phase > Math.PI * 2) phase -= Math.PI * 2;

                float sampleValue = 0;


                switch (WaveType)
                {
                    case WaveTypes.Sin:sampleValue = Mathf.Sin((float)phase);
                        break;
                    case WaveTypes.Square:sampleValue = Mathf.Sin((float)phase) >= 0 ? 1 : -1;
                        break;
                    case WaveTypes.Triangle:sampleValue = Mathf.PingPong((float)phase, 2) - 1;
                        break;
                    case WaveTypes.RufSin: sampleValue = Mathf.Sin((float)phase + .1f* Mathf.Sin((float) (phase * increment)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                sampleValue *= gain;
                buffer[i] = buffer[i + 1] = sampleValue;
            }
        }
    }
}
