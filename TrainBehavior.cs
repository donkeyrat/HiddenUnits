using UnityEngine;

namespace HiddenUnits;

public class TrainBehavior : MonoBehaviour
{
    private CountEvent CountEvent;
    private Cooldown Cooldown;
    private bool DoEvilTrains;

    private void Start()
    {
        DoEvilTrains = HUMain.EvilTrainEnabled;
        CountEvent = GetComponent<CountEvent>();
        Cooldown = GetComponent<Cooldown>();
        if (DoEvilTrains)
        {
            CountEvent.seconds = 1f;
            CountEvent.randomMax = 1f;
            Cooldown.cooldown = 10f;
        }

        GetComponent<SpawnObject>().objectToSpawn.GetComponentInChildren<AudioSource>().outputAudioMixerGroup = HUMain.AudioMixer;
    }
}