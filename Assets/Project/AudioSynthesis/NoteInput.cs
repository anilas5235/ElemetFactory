using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.AudioSynthesis
{
    public class NoteInput : MonoBehaviour
    {
        public delegate void NoteOnDelegate(int noteNumber, float velocity);
        public static event NoteOnDelegate NoteOn;

        public delegate void NoteOffDelegate(int noteNumber);
        public static event NoteOffDelegate NoteOff;

        private Dictionary<KeyCode, int> virtualKeysDict;
        
        public static readonly float TwelfthRoot = Mathf.Pow(2, 1 / 12f);
        public const int FixedBaseNote = 69;

        private void Awake()
        {
            virtualKeysDict = new Dictionary<KeyCode, int>();
            virtualKeysDict.Add(KeyCode.A, 60); // C
            virtualKeysDict.Add(KeyCode.W, 61); // C#
            virtualKeysDict.Add(KeyCode.S, 62); // D
            virtualKeysDict.Add(KeyCode.E, 63); // D#
            virtualKeysDict.Add(KeyCode.D, 64); // E
            virtualKeysDict.Add(KeyCode.F, 65); // F
            virtualKeysDict.Add(KeyCode.T, 66); // F#
            virtualKeysDict.Add(KeyCode.G, 67); // G
            virtualKeysDict.Add(KeyCode.Z, 68); // G#
            virtualKeysDict.Add(KeyCode.H, 69); // A
            virtualKeysDict.Add(KeyCode.U, 70); // A#
            virtualKeysDict.Add(KeyCode.J, 71); // B
            virtualKeysDict.Add(KeyCode.K, 72); // C

            // ...

            StartCoroutine(PlayNotes(new[] { 64, 65, 66, 67,68 },2f));
        }

        private void Update()
        {
            foreach (var key in virtualKeysDict.Keys)
            {
                if (Input.GetKeyDown(key))
                {
                    NoteOn?.Invoke(virtualKeysDict[key], 1f);
                }

                if (Input.GetKeyUp(key))
                {
                    NoteOff?.Invoke(virtualKeysDict[key]);
                }
            }
        }
        
        public static float NoteToFrequency(int nodeNumber)
        {
            return 440f* Mathf.Pow(TwelfthRoot,(nodeNumber - FixedBaseNote));
        }

        private IEnumerator PlayNotes(int[] sequence, float notesPerSecond =1f)
        {
            int index = 0;
            bool up = true;
            while (true)
            {
                NoteOff?.Invoke(sequence[index]);
                if (up) index++;
                else index--;

                if (index >= sequence.Length-1) 
                {
                    up = false;
                }
                else if (index <= 0)
                {
                    up = true;
                }
                NoteOn?.Invoke(sequence[index],1f);
                yield return new WaitForSeconds(1f/notesPerSecond);
            }
        }

        private IEnumerator PlayNote(int noteNumber, float notesPerSecond = 1f)
        {
            bool on = false;
            while (true)
            {
                if(on) NoteOff?.Invoke(noteNumber);
                else NoteOn?.Invoke(noteNumber,1f);
                on = !on;

                yield return new WaitForSeconds(1f/notesPerSecond);
            }
        }
    }
}
