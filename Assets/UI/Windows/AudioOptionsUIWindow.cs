using Project.Scripts.UI.InteractableUI;
using Project.Scripts.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace UI.Windows
{
    public class AudioOptionsUIWindow : UIWindowHandler
    {
        [Header("Slider")] [SerializeField] private AudioSlider main;
        [SerializeField] private AudioSlider music, effects, voice;

        [Header("NamesOfExposedParameters")] [SerializeField]
        private string paramMaster = "Master";

        [SerializeField] private string paramMusic = "Music", paramEffects = "Effects", paramVoice = "Voice";

        [Header("AudioMixer")] [SerializeField]
        private AudioMixer mainAudioMixer;

        private void OnEnable()
        {
            LoadFromSaveText();
        }

        private void Start()
        {
            UpdateSoundOptions();
        }

        private void OnDisable()
        {
            SaveOptionsToText();
        }

        public void UpdateSoundOptions()
        {
            mainAudioMixer.SetFloat(paramMaster, ConvertSliderValueTodB(main.Value));
            mainAudioMixer.SetFloat(paramMusic, ConvertSliderValueTodB(music.Value));
            mainAudioMixer.SetFloat(paramEffects, ConvertSliderValueTodB(effects.Value));
            mainAudioMixer.SetFloat(paramVoice, ConvertSliderValueTodB(voice.Value));
        }

        public void SaveOptionsToText()
        {
            float[] optionsValues = SaveSettings.CurrentSaveSettings.audioSettings;
            mainAudioMixer.GetFloat(paramMaster, out optionsValues[0]);
            mainAudioMixer.GetFloat(paramMusic, out optionsValues[1]);
            mainAudioMixer.GetFloat(paramEffects, out optionsValues[2]);
            mainAudioMixer.GetFloat(paramVoice, out optionsValues[3]);
            
            SaveSettings.Save();
        }

        public void LoadFromSaveText()
        {
            float[] optionsValues = SaveSettings.CurrentSaveSettings.audioSettings;
            mainAudioMixer.SetFloat(paramMaster, optionsValues[0]);
            mainAudioMixer.SetFloat(paramMusic, optionsValues[1]);
            mainAudioMixer.SetFloat(paramEffects, optionsValues[2]);
            mainAudioMixer.SetFloat(paramVoice, optionsValues[3]);

            main.Value = ConvertDBToSliderValue(optionsValues[0]);
            music.Value = ConvertDBToSliderValue(optionsValues[1]);
            effects.Value = ConvertDBToSliderValue(optionsValues[2]);
            voice.Value = ConvertDBToSliderValue(optionsValues[3]);
        }

        private float ConvertSliderValueTodB(float sliderValue)
        {
            return Mathf.Log10(sliderValue) * 20f;
        }

        private float ConvertDBToSliderValue(float dBValue)
        {
            return Mathf.Pow(10, (dBValue) / 20f);
        }

    }
}
