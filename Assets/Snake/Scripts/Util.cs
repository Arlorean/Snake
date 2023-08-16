using Unity.VisualScripting;
using UnityEngine;

public static class Util {
    public static T SelectRandom<T>(this T[] array) {
        if (array.Length > 0) {
            return array[Random.Range(0, array.Length)];
        }
        return default;
    }

    public static void Swap<T>(ref T a, ref T b) {
        var tmp = a;
        a = b;
        b = tmp;
    }

    public static void Play(this AudioSource audio, AudioClip clip) {
        audio.clip = clip;
        audio.Play();
    }
}
