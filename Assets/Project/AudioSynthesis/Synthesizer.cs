using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.AudioSynthesis
{
    [RequireComponent(typeof(AudioSource))]
    public class Synthesizer : MonoBehaviour
    {
        public enum WaveType
        {
            Sin,
            Square,
            Saw,
            Triangle,
            Noise,
        }

        public WaveType waveType;

        [Range(0, 1f)] public float gain = 0.5f;

        private const int polyphony = 16;
        private Voice[] voicesPool;

        private Stack<Voice> freeVoices;
        private List<Voice> activeVoices;
        private Dictionary<int, Voice> noteDict;


        private void OnEnable()
        {
            NoteInput.NoteOn += NoteInput_NoteOn;
            NoteInput.NoteOff += NoteInput_NoteOff;

            voicesPool = new Voice[polyphony];
            freeVoices = new Stack<Voice>();

            for (int i = 0; i < voicesPool.Length; i++)
            {
                voicesPool[i] = new Voice();
                freeVoices.Push(voicesPool[i]);
            }

            activeVoices = new List<Voice>();
            noteDict = new Dictionary<int, Voice>();
        }

        private void OnDisable()
        {
            NoteInput.NoteOn -= NoteInput_NoteOn;
            NoteInput.NoteOff -= NoteInput_NoteOff;
        }

        public void NoteInput_NoteOn(int notenumber, float velocity)
        {
            if (noteDict.ContainsKey(notenumber)) return;
            if (freeVoices.Count <= 0) return;

            Voice voice = freeVoices.Pop();
            voice.waveType = waveType;
            voice.NoteOn(notenumber, velocity);

            activeVoices.Add(voice);

            noteDict.Add(notenumber, voice);
        }

        public void NoteInput_NoteOff(int notenumber)
        {
            if (!noteDict.ContainsKey(notenumber)) return;

            Voice voice = noteDict[notenumber];
            voice.NoteOff(notenumber,this);
        }

        public void VoiceFinishedPlaying(Voice voice)
        {
             activeVoices.Remove(voice);
             freeVoices.Push(voice);
            
            noteDict.Remove(voice.noteNumber);
        }

        private void OnAudioFilterRead(float[] buffer, int channels)
        {
            for (int i = 0; i < activeVoices.Count; i++)
            {
                activeVoices[i].WriteAudioBuffer(buffer, channels);
            }

            for (int i = 0; i < buffer.Length; i += channels)
            {
                buffer[i] *= gain;

                for (int c = 1; c < channels; c++)
                {
                    buffer[i + c] = buffer[i];
                }
            }
        }
    }
}
