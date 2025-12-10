using System.Collections.Generic;
using System.IO;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED;

public class EasterEggsHandler
{
    public EasterEggsHandler()
    {
        Exiled.Events.Handlers.Server.RoundStarted += MelancholyNuke;
    }

    ~EasterEggsHandler()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= MelancholyNuke;
    }
    public static void CreateAndPlayAudio(string fileName, string audioPlayerName, Vector3 position, bool destroyOnEnd = false, Transform parent = null, bool isSpatial = false, float maxDistance = 5, float minDistance = 5, bool loadClip = true)
    {
            
        var audioPlayer = AudioPlayer.CreateOrGet(audioPlayerName);
            

        if (!audioPlayer.TryGetSpeaker(audioPlayerName, out Speaker speaker))
        {
            speaker = audioPlayer.AddSpeaker(audioPlayerName, isSpatial: isSpatial, maxDistance: maxDistance, minDistance: minDistance);
        }

        if (parent)
        {
            speaker.transform.SetParent(parent);
            speaker.transform.localPosition = Vector3.zero;
            speaker.transform.localRotation = Quaternion.identity;
        }
        else
        {
            speaker.Position = position;
        }

        if (loadClip)
        {
            AudioClipStorage.LoadClip(Path.Combine(Plugin.Singleton.Config.AudioReferences, fileName), fileName);
        }

        audioPlayer.AddClip(fileName, destroyOnEnd: destroyOnEnd);
    }

    public void loadClips()
    {
        AudioClipStorage.LoadClip(Path.Combine(Plugin.Singleton.Config.AudioReferences, "ee_melancholy.ogg"), "ee_melancholy.ogg");
    }
    public void MelancholyNuke()
    {
        Room SpawnRoom = Room.Get(RoomType.HczNuke);
        Log.Debug(SpawnRoom.Position);
        Vector3 offset = new Vector3(-2.25f,-5.65f,0f);
        Vector3 Position = SpawnRoom.Position + SpawnRoom.Rotation * offset;
        Timing.RunCoroutine(MelancholyPlay(Position));
    }

    private IEnumerator<float> MelancholyPlay(Vector3 position)
    {
        int i = 0;
        for (;;)
        {
            CreateAndPlayAudio("ee_melancholy.ogg",("EE_Melancholy"+i),position,true,null,false,5.99999f,0,false);
            i++;
            yield return Timing.WaitForSeconds(420f);
        }
    }
}