using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using static GameValues;

public class AudioController : MonoBehaviour
{
    private AudioSource audioSource;
    private List<AudioClip> toPlay = new List<AudioClip> { };
    private float volumeMultiplier = 1.0f;
    private Coroutine audioPlayer;

    private State state;
    private bool switchingState = false;
    private byte updateTicks = 120;

    [SerializeField] AudioClip testTrack1;
    [SerializeField] AudioClip testTrack2;
    [SerializeField] AudioTrack actualTrack;

    private void Awake()
    {
        state = Settings.volume > 0 ? State.Paused : State.Stopped;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = Settings.volume;
        if (state == State.Stopped) audioSource.Stop();
    }

    public void playTrack(AudioClip track)
    {
        if (state == State.Stopped) return;
        toPlay.Clear();
        toPlay.Add(track);
        switchTrack(1);
    }

    public void playTrackQuick(AudioClip track)
    {
        if (state == State.Stopped) return;
        toPlay.Clear();
        toPlay.Add(track);
        switchTrack(0);
    }

    public void playTrack(List<AudioClip> tracks)
    {
        if (state == State.Stopped) return;
        toPlay = tracks;
        switchTrack(1);
    }

    public void stopPlaying()
    {
        StartCoroutine(fadeOutTrack());
        state = State.Paused;
    }

    public void updateVolume(float volume)
    {
        audioSource.volume = volume;
        updateTicks = 120;
        if (!switchingState) StartCoroutine(updateState());
    }

    public void unloadClips(List<AudioClip> clips)
    {
        StartCoroutine(unloadRoutine(clips));
    }

    private void switchTrack(float delay)
    {
        if (state != State.SwitchingTrack)
        {
            if (audioPlayer != null) StopCoroutine(audioPlayer);
            audioPlayer = StartCoroutine(playNext(delay));
        }
    }

    private void resumePlaying()
    {
        if (toPlay.Count > 0) playTrack(toPlay);
    }

    private void switchState(State state)
    {
        if (state == this.state) return;
        this.state = state;
        if (state == State.Stopped)
        {
            if (audioPlayer != null) StopCoroutine(audioPlayer);
            audioSource.Stop();
        }
        else
        {
            resumePlaying();
        }
    }

    private IEnumerator playNext(float delay)
    {
        // fade out previous track
        if (state == State.Playing)
        {
            state = State.SwitchingTrack;
            yield return fadeOutTrack();
        }
        yield return new WaitForSeconds(delay);
        state = State.Playing;

        // play current track
        audioSource.volume = Settings.volume;
        audioSource.clip = toPlay[0];
        audioSource.Play();

        // play more tracks, if any
        int index = 1;
        while (index < toPlay.Count)
        {
            yield return new WaitForSeconds(toPlay[index-1].length);
            audioSource.clip = toPlay[index];
            audioSource.Play();
            index++;
        }

        yield break;
    }

    private IEnumerator fadeOutTrack()
    {
        volumeMultiplier = 1.0f;
        while (volumeMultiplier > 0)
        {
            volumeMultiplier -= 0.01f;
            if (volumeMultiplier < 0) volumeMultiplier = 0;
            audioSource.volume = Settings.volume * volumeMultiplier;
            yield return null;
        }
        yield break;
    }

    private IEnumerator updateState()
    {
        switchingState = true;

        while (updateTicks > 0)
        {
            updateTicks--;
            yield return null;
        }

        if (Settings.volume > 0) switchState(State.Playing);
        else switchState(State.Stopped);

        switchingState = false;
        yield break;
    }

    private IEnumerator unloadRoutine(List<AudioClip> clips)
    {
        yield return new WaitForSeconds(5);
        foreach (AudioClip clip in clips)
        {
            clip.UnloadAudioData();
        }
    }

    private enum State
    {
        Paused,
        Stopped,
        Playing,
        SwitchingTrack
    }
}
