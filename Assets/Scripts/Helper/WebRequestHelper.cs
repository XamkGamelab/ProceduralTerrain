using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading;
using System;


public class WebRequestHelper : MonoBehaviour
{
    public static IEnumerator GetAudioClipWebRequest(string url, AudioType audioType, IObserver<AudioClip> observer, CancellationToken cancellationToken)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
        {
            yield return www.SendWebRequest();

            if (!www.isDone && !cancellationToken.IsCancellationRequested)
            {
                Debug.Log("loading...");
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested) 
                yield break;

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                observer.OnNext(audioClip);
                observer.OnCompleted();
            }
        }
    }
}
