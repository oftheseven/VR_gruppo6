using UnityEngine;
using System.Collections.Generic;

public class SliderKeyframe
{
    public float time;
    public float position;
    public Quaternion cameraRotation;

    public SliderKeyframe(float time, float position, Quaternion cameraRotation)
    {
        this.time = time;
        this.position = position;
        this.cameraRotation = cameraRotation;
    }
}

public class SliderRecording
{
    public string recordingName;
    public float duration;
    public List<SliderKeyframe> keyframes = new List<SliderKeyframe>();

    public SliderRecording(string recordingName)
    {
        this.recordingName = recordingName;
        this.duration = 0f;
        keyframes = new List<SliderKeyframe>();
    }

    public void AddKeyframe(float time, float position, Quaternion cameraRotation)
    {
        keyframes.Add(new SliderKeyframe(time, position, cameraRotation));
        duration = time;
    }

    public int GetKeyframeCount()
    {
        return keyframes.Count;
    }

    public float GetSampleRate()
    {
        if (keyframes.Count <= 1) return 0f;
        return duration / (keyframes.Count - 1);
    }
}