using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
public class SoundManager : MonoBehaviour
{
    const string MASTER_PARAM = "MasterVolume";
    const string BGM_PARAM = "BGMVolume";
    const string SFX_PARAM = "SFXVolume";

    [SerializeField] AudioMixer _mixer;
    
    [SerializeField] string _bgmClipName;
    private AudioSource _bgmSource; // BGM 전용 소스 추가

    ObjectPool<AudioSource> _soundPool;

    void Awake()
    {
        _mixer = Resources.Load<AudioMixer>("MasterMixer");
        if (_mixer == null) Debug.Log("Audio Mixer Is NULL");

        InitPool();
        CreatBGMSource();

        SoundEvents.SetMasterVolumeEvent    += HandleSetMasterVolume;
        SoundEvents.SetBGMVolumeEvent       += HandleSetBGMVolume;
        SoundEvents.SetSFXVolumeEvent       += HandleSetSFXVolume;
        SoundEvents.SFXPlayedEvent          += HandleSFXPlayed;
        SoundEvents.BGMChangeEvent           += PlayBGM;
        SceneManager.sceneLoaded            += HandleSceneLoaded;
    }
    void OnDestroy()
    {
        SoundEvents.SetMasterVolumeEvent    -= HandleSetMasterVolume;
        SoundEvents.SetBGMVolumeEvent       -= HandleSetBGMVolume;
        SoundEvents.SetSFXVolumeEvent       -= HandleSetSFXVolume;
        SoundEvents.SFXPlayedEvent          -= HandleSFXPlayed;
        SoundEvents.BGMChangeEvent           -= PlayBGM;
        SceneManager.sceneLoaded            -= HandleSceneLoaded;
    }
    void InitPool()
    {
        var sfxGroups = _mixer.FindMatchingGroups("SFX"); // 믹서에서 SFX 그룹 찾기

        _soundPool = new ObjectPool<AudioSource>(5, () => {
            GameObject audioSourceObj = new GameObject("SFXAudioSource");
            audioSourceObj.transform.parent = transform;
            var audioSource = audioSourceObj.AddComponent<AudioSource>();

            if (sfxGroups.Length > 0) audioSource.outputAudioMixerGroup = sfxGroups[0];

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 0.4f;
            return audioSource;
        });
    }
    void SetVolumeParameter(string param, float volume)
    {
        // 볼륨 범위 클램프
        volume = Mathf.Clamp01(volume);
        // 0.0001~1 사이 값 → -80dB ~ 0dB
        float dB = (volume <= 0f) ? -80f : Mathf.Log10(volume) * 20f;
        _mixer.SetFloat(param, dB);
    }
    void CreatBGMSource()
    {
        if (_bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGMAudioSource");
            bgmObj.transform.parent = transform;
            _bgmSource = bgmObj.AddComponent<AudioSource>();
            _bgmSource.loop = true; 
            _bgmSource.playOnAwake = false;
            var bgmGroups = _mixer.FindMatchingGroups("BGM");
            if (bgmGroups.Length > 0) _bgmSource.outputAudioMixerGroup = bgmGroups[0];
        }
    }
    
    void PlayBGM(string name)
    {
        AudioClip clip = LoadClip(name);
        if (clip != null)
        {
            _bgmSource.clip = clip;
            _bgmSource.Play();
        }
    }

    AudioClip LoadClip(string name)
    {
        var audioClip = Resources.Load<AudioClip>("Sounds/Clips/" + name);
        if (audioClip == null) Debug.LogWarning($"{name} clip is NULL");
        return audioClip;
    }
    IEnumerator Co_ReturnToPool(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        _soundPool.Set(source);
    }
    void HandleSetMasterVolume(float volume)
    {
        SetVolumeParameter(MASTER_PARAM, volume);
    }
    void HandleSetBGMVolume(float volume)
    {
        SetVolumeParameter(BGM_PARAM, volume);
    }
    void HandleSetSFXVolume(float volume)
    {
        SetVolumeParameter(SFX_PARAM, volume);
    }
    void HandleSFXPlayed(string name)
    {
        var audioSource = _soundPool.Get();
        audioSource.clip = LoadClip(name);
        audioSource.loop = false;
        audioSource.Play();
        StartCoroutine(Co_ReturnToPool(audioSource));
    }
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Init") return;
        else if (scene.name == "StartScene_LCS") PlayBGM("Start_BGM");
        else if (scene.name == "Tutorial") PlayBGM("Tutorial_BGM");
        else if (scene.name == "Stage1") PlayBGM("Stage1_BGM__");
        else if (scene.name == "Lobby") PlayBGM("Lobby_BGM");
        else if (scene.name == "Stage2") PlayBGM("Stage2_BGM");
        else if (scene.name == "Dungeon") PlayBGM("Dungeon_BGM");
        else if (scene.name == "Stage3") PlayBGM("Stage3_BGM");
    }
}
