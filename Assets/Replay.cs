using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class Replay : MonoBehaviour
{
    #region Editable properties

    [Tooltip("Replay recording framerate.")]
    [SerializeField] int frameRate = 10;
    [Tooltip("Replaying speed multiplier.")]
    [SerializeField] float replaySpeed = 1;

    #endregion

    #region Private properties

    private GameObject[] objectsToRecord;
    private float timer = 0.0f;
    private bool isRecording = false;
    private bool isReplaying = false;
    private string destination;
    private float recordingInterval;
    private List<List<float>> data = new List<List<float>>();
    List<Tform> prevTransforms = new List<Tform>();
    List<Tform> newTransforms = new List<Tform>();
    int replayFrame = 0;
    float replayInterval;
    private bool isKinematic = false;

    #endregion

    // Transform information
    public struct Tform
    {
        public Vector3 pos;
        public Quaternion rot;
        
        public Tform(Vector3 position, Quaternion rotation)
        {
            pos = position;
            rot = rotation;
        }
    }

    #region Monobehaviour methods

    // Start is called before the first frame update
    void Start()
    {
        recordingInterval = 1.0f / frameRate;
        destination = Directory.GetCurrentDirectory() + "/Replays/replay.csv";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !isRecording)
        {
            StartRecord();
        }
        else if (Input.GetKeyDown(KeyCode.M) && isRecording)
        {
            StopRecord();
        }
        else if (Input.GetKeyDown(KeyCode.B) && data.Count > 0 && !isReplaying && !isRecording)
        {
            timer = 0.0f;
            isReplaying = true;
        }

        // These methods are WIP. The tracked prefabs need to be saved and loaded too.

        //else if (Input.GetKeyDown(KeyCode.V))
        //{
        //    SaveReplay();
        //}
        //else if (Input.GetKeyDown(KeyCode.C))
        //{
        //    LoadReplay();
        //}
    }

    void LateUpdate()
    {
        if (isRecording && timer >= recordingInterval)
        {
            SaveTransforms();
            timer = 0.0f;
        }
        else if (isReplaying)
        {
            replayInterval = recordingInterval / replaySpeed;
            if (prevTransforms.Count == 0)
            {
                SetReplayTransforms(replayFrame);
            }
            ReplayRecord(replayFrame, timer / replayInterval);
            if (timer >= replayInterval)
            {
                timer = 0.0f;
                ++replayFrame;
                SetReplayTransforms(replayFrame);
            }
        }
        timer += Time.deltaTime;
    }

    #endregion

    #region private methods

    // Finds all of the moving GameObjects we want to track for the replay
    private void StartRecord()
    {
        if (data.Count > 0)
        {
            data = new List<List<float>>();
        }
        objectsToRecord = GameObject.FindGameObjectsWithTag("Replayable");
        if (objectsToRecord.Length > 0)
        {
            isRecording = true;
            Debug.Log("Recording");
        }
        else
        {
            Debug.Log("No objects are tagged as \"Replayable\"");
        }
    }
    
    private void StopRecord()
    {
        isRecording = false;
        Debug.Log("Stopped recording");
    }

    private void SaveTransforms()
    {
        // Each line corresponds to one recorded frame
        List<float> line = new List<float>();
        for (int i = 0; i < objectsToRecord.Length; ++i)
        {
            Transform t = objectsToRecord[i].transform;
            for (int j = 0; j < 3; ++j)
            {
                line.Add(t.position[j]);
            }
            for (int k = 0; k < 4; ++k)
            {
                line.Add(t.rotation[k]);
            }
        }
        data.Add(line);
    }

    // Replays the recorded movement by directly setting the objects' transforms on each frame
    // int frame : the index of List<List<float>> data we are currently on
    // float lerpAmount : float between [0, 1] used for lerping
    private void ReplayRecord(int frame, float lerpAmount)
    {
        if (!isKinematic)
        {
            SetKinematic();
        }
        for (int i = 0; i < objectsToRecord.Length; ++i)
        {
            
            objectsToRecord[i].transform.position = Vector3.Lerp(prevTransforms[i].pos,
                                                                    newTransforms[i].pos,
                                                                    lerpAmount);
            objectsToRecord[i].transform.rotation = Quaternion.Lerp(prevTransforms[i].rot,
                                                                    newTransforms[i].rot,
                                                                    lerpAmount);
        }
        if (frame == data.Count - 1)
        {
            if (isReplaying)
            {
                isReplaying = false;
            }
            SetKinematic();
            replayFrame = 0;
            prevTransforms = new List<Tform>();
            newTransforms = new List<Tform>();
        }
    }

    private void SetReplayTransforms(int frame)
    {
        prevTransforms = new List<Tform>();
        newTransforms = new List<Tform>();
        // Picks each tracked object's transform information per frame
        for (int j = 0; j < data[frame].Count; j += 7)
        {
            Vector3 newPos = new Vector3(data[frame][j + 0],
                                            data[frame][j + 1],
                                            data[frame][j + 2]);
            Quaternion newRot = new Quaternion(data[frame][j + 3],
                                                data[frame][j + 4],
                                                data[frame][j + 5],
                                                data[frame][j + 6]);
            prevTransforms.Add(new Tform(objectsToRecord[j / 7].transform.position,
                                            objectsToRecord[j / 7].transform.rotation));
            newTransforms.Add(new Tform(newPos, newRot));

        }
    }

    private void SaveReplay()
    {
        Debug.Log("Replay saved");
        using (TextWriter writer = new StreamWriter(destination, true, Encoding.UTF8))
        {
            for (int i = 0; i < data.Count; ++i)
            {
                string csvLine = string.Join(",", data[i].ToArray());
                writer.WriteLine(csvLine);
                writer.Flush();
            }
            writer.Close();
        }
    }

    private void LoadReplay()
    {
        Debug.Log("Replay loaded");
        string line;
        using (TextReader reader = new StreamReader(destination))
        {
            while ((line = reader.ReadLine()) != null)
            {
                line.Trim();
                string[] lineArray = line.Split(',');
                data = new List<List<float>>();
                List<float> tempData = new List<float>();
                foreach (string item in lineArray)
                {
                    tempData.Add(float.Parse(item));
                }
                data.Add(tempData);
            }
            reader.Close();
        }
    }

    // Sets each tracked objects' rigidbodies kinematic modifier back and forth. This should make
    // replaying smoother.
    private void SetKinematic()
    {
        foreach (GameObject go in objectsToRecord)
        {
            if (go.GetComponent<Rigidbody>() != null)
            {
                go.GetComponent<Rigidbody>().isKinematic = !go.GetComponent<Rigidbody>().isKinematic;
                isKinematic = go.GetComponent<Rigidbody>().isKinematic;
            }
        }
    }

    #endregion
}