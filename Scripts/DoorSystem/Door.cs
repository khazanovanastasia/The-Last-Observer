using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Door : MonoBehaviour
{
    public bool opened = true;
    [SerializeField] private KeyCode openKeyCode;
    [SerializeField] private TimelineAsset openAsset, closeAsset;

    private PlayableDirector director;

    [SerializeField] private static List<Door> allDoors = new List<Door>();

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();

        if (!allDoors.Contains(this))
            allDoors.Add(this);

        director.playableAsset = opened? closeAsset : openAsset;
    }

    private void Update()
    {
        if (Input.GetKeyDown(openKeyCode))
        {
            if (!opened)
                OpenDoor();
            else
                CloseDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened) return;
        if (other.TryGetComponent<PrisonerAI>(out var prisoner))
            prisoner.BlockedByDoor();
    }

    public void OpenDoor()
    {
        SetDoorState(true);
    }

    private void CloseDoor()
    {
        SetDoorState(false);
        foreach (Door door in allDoors)
        {
            if (door != this && !door.opened)
                door.SetDoorState(true);
        }
    }

    private void SetDoorState(bool newState)
    {
        if (opened != newState)
        {
            opened = newState;
            PlayEffects();
        }
    }
    private void PlayEffects()
    {
        director.playableAsset = opened ? openAsset : closeAsset;
        director.Play();
    }
}
