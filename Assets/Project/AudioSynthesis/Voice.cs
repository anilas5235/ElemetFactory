using System;
using Unity.Burst;
using UnityEngine;

namespace Project.AudioSynthesis
{
    [BurstCompile]
    public class Voice
    {
        public Synthesizer.WaveType waveType = Synthesizer.WaveType.Sin;

        public int noteNumber;
        public float velocity;

        private float frequency;

        private float sampleRate;
        private float fadeProgress;
        private double increment;
        private double phase;

        private System.Random random;

        private bool fade;
        private Synthesizer caller;

        public Voice()
        {
            noteNumber = -1;
            velocity = 0;
            random = new System.Random();
        }

        public void NoteOn(int noteNumber, float velocity)
        {
            this.noteNumber = noteNumber;
            this.velocity = velocity;

            frequency = NoteInput.NoteToFrequency(noteNumber);

            phase = 0;
            sampleRate = AudioSettings.outputSampleRate;
        }

        public void NoteOff(int noteNumber, Synthesizer caller)
        {
            if (noteNumber != this.noteNumber) return;
        
            // TODO: start release time
            fade = true;
            this.caller = caller;
            fadeProgress =0;
        }

        public void WriteAudioBuffer(float[] buffer, int channels)
        {
            increment = frequency * 2 * Math.PI / sampleRate;
        
            for (int i = 0; i < buffer.Length; i += channels)
            {
                phase += increment;
                if (phase > Math.PI * 2) phase -= Math.PI * 2;

                switch (waveType)
                {
                    case Synthesizer.WaveType.Sin:
                        buffer[i] += Mathf.Sin((float)phase+ .05f * Mathf.Cos((float)phase));
                        break;
                
                    case Synthesizer.WaveType.Square:
                        buffer[i] += Mathf.Sin((float)phase) >= 0 ? 1 : -1;
                        break;
                
                    case Synthesizer.WaveType.Saw:
                        buffer[i] += Mathf.Clamp(Mathf.Sin((float)phase),0,1f);
                        break;
                
                    case Synthesizer.WaveType.Triangle:
                        buffer[i] += Mathf.PingPong((float)phase, 2) - 1;
                        break;
                
                    case Synthesizer.WaveType.Noise:
                    default:
                        buffer[i] += (float)random.NextDouble();
                        break;
                }

                if (fade)
                {
                    fadeProgress += .01f;
                    buffer[i] *= Mathf.SmoothStep(1,0,fadeProgress);
                }
            }
            
            if (fade && fadeProgress >= 1f)
            {
                caller.VoiceFinishedPlaying(this);
                fade = false;
            }
        }
    }
}
