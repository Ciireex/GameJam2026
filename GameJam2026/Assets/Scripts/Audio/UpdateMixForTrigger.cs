using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SnapshotEntry
{
    public string tag;
    public AudioMixerSnapshot snapshot;
    public float transitionTime = 0.5f;
}

public class UpdateMixForTrigger : MonoBehaviour
{
    public List<SnapshotEntry> snapshots;

    private Stack<SnapshotEntry> snapshotStack;

    void Start()
    {
        if (snapshots.Count == 0)
            return;

        snapshots[0].snapshot.TransitionTo(0.0f);

        snapshotStack = new Stack<SnapshotEntry>();
        snapshotStack.Push(snapshots[0]);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (snapshots.Count == 0)
            return;

        foreach (var snapshotEntry in snapshots)
        {
            if (snapshotEntry.tag == other.tag)
            {
                snapshotEntry.snapshot.TransitionTo(snapshotEntry.transitionTime);
                snapshotStack.Push(snapshotEntry);
                break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (snapshots.Count <= 1)
            return;

        UnityEngine.Debug.Log("JURL");

        snapshotStack.Pop();

        SnapshotEntry nextSnapshotEntry = snapshotStack.Pop();
        nextSnapshotEntry.snapshot.TransitionTo(nextSnapshotEntry.transitionTime);
        snapshotStack.Push(nextSnapshotEntry);
    }
}
