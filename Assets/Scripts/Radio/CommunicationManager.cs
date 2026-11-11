using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class RadioChannel
{
    public string channelID;
    public string contactName;
    [Tooltip("Frequency in MHz. The prototype tunes in 0.1 MHz steps.")]
    public float frequencyMHz = 152.3f;
}


public class CommunicationManager : MonoBehaviour
{
    public static CommunicationManager Instance { get; private set; }

    [Header("Known Radio Channels")]
    [SerializeField] private List<RadioChannel> channels = new();

    // Use integer tenths of MHz so station matching has no float = issues
    private readonly Dictionary<int, RadioChannel> channelsByFrequency = new();
    private readonly Dictionary<RadioTuner, int> radioFrequencies = new();

    // Future story / dialogue / quest scripts can use this event
    public event Action<RadioTuner, RadioChannel> OnChannelConnected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        channelsByFrequency.Clear();

        foreach (RadioChannel channel in channels)
        {
            int frequency = Mathf.RoundToInt(channel.frequencyMHz * 10f);

            channelsByFrequency[frequency] = channel;

            Debug.Log($"Registered channel: {channel.contactName} at {channel.frequencyMHz:F1} MHz");
        }
    }

    public void RegisterRadio(RadioTuner radio, int frequencyTenths)
    {
        if (radio == null) { return; }

        radioFrequencies[radio] = frequencyTenths;
    }

    public void UnregisterRadio(RadioTuner radio)
    {
        if (radio != null) { radioFrequencies.Remove(radio); }
    }

    public void UpdateRadioFrequency(RadioTuner radio, int frequencyTenths)
    {
        if (radio == null) { return; }

        if (radioFrequencies.TryGetValue(radio, out int previousFrequency) && previousFrequency == frequencyTenths) { return; }

        radioFrequencies[radio] = frequencyTenths;
        TryConnect(radio);
    }

    public bool TryConnect(RadioTuner radio)
    {
        if (radio == null || !radioFrequencies.TryGetValue(radio, out int frequency)) { return false; }
        if (!channelsByFrequency.TryGetValue(frequency, out RadioChannel channel)) { return false; }

        Debug.Log("Connected");
        OnChannelConnected?.Invoke(radio, channel);
        return true;
    }

    public bool TryGetRadioFrequency(RadioTuner radio, out float frequencyMHz)
    {
        if (radio != null && radioFrequencies.TryGetValue(radio, out int frequencyTenths))
        {
            frequencyMHz = frequencyTenths / 10f;
            return true;
        }

        frequencyMHz = 0f;
        return false;
    }

    public List<RadioTuner> GetRadiosOnFrequency(float frequencyMHz)
    {
        int targetFrequency = Mathf.RoundToInt(frequencyMHz * 10f);
        List<RadioTuner> matches = new();

        foreach(KeyValuePair<RadioTuner, int> entry in radioFrequencies)
        {
            if (entry.Key != null && entry.Value == targetFrequency)
            {
                matches.Add(entry.Key);
            }
        }

        return matches;
    }

    private void OnDestroy()
    {
        if (Instance == this) { Instance = null; }
    }
}
