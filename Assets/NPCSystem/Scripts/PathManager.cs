using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Windows;

/// <summary>
/// Handle path data serializations and generates a new random path
/// </summary>
public class PathManager : MonoBehaviour
{
#if (UNITY_EDITOR)
    private static string DATAPATH = "Assets/NPCSystem/PersistentData/serialize.data";
    private static string DATAPATH_ALL = "Assets/NPCSystem/PersistentData/serialize_all.data";
#endif

    public static string WAYPOINTPATH = "waypointdata.data";
    public static string LINKTPATH = "linkdata.data";

// old paths
//#if !(UNITY_EDITOR)
//    private static string DATAPATH = "PersistentData/serialize.data";
//    private static string DATAPATH_ALL = "PersistentData/serialize_all.data";

//    private static string WAYPOINTPATH = "waypointdata.data";
//    private static string LINKTPATH = "linkdata.data";
//#endif

    public static List<Link> links;
    public static List<UniqueWaypoint> waypoints;

    private static GameObject waypointPrefab;


    /// <summary>
    /// Automatically sets both links and waypoints. Is also used for updating values.
    /// </summary>
    public static void SetPathData()
    {
        //if (NPCDataHolder.links == null || NPCDataHolder.waypoints == null)
        //ReadSerializeData();
        //ReadAllWaypointData();
        ReadAllWaypointDataLightweight();
    }
    
    /// <summary>
    /// Writes all required path data to disk. Plaintext is fine since this 
    /// is not a game
    /// </summary>
    //[MenuItem("Menu/WriteLinkData")]
    public static void WriteSerializeData()
    {
        //SetPathData();

        if (NPCDataHolder.links != null && NPCDataHolder.waypoints != null && 
            NPCDataHolder.waypoints.Count > 0 && NPCDataHolder.links.Count > 0)
        {
            List<UniqueWaypoint> uniqueWaypoints = NPCDataHolder.waypoints; //new List<UniqueWaypoint>(FindObjectsOfType<UniqueWaypoint>());
            List<string> waypointNames = new List<string>();

            string writedata = "";

            writedata += "waypoints" + Environment.NewLine;

            foreach (UniqueWaypoint u in uniqueWaypoints)
            {
                waypointNames.Add(u.Name);
                writedata += u.Name + Environment.NewLine;
            }

            writedata += Environment.NewLine + "links" + Environment.NewLine;

            foreach (Link l in NPCDataHolder.links)
            {
                if (!l.IsNull())
                    writedata += l.Origin.Name + "," + l.Target.Name + Environment.NewLine;
            }

            writedata += "EOF" + Environment.NewLine;

            System.IO.File.WriteAllText(DATAPATH, string.Empty);
            System.IO.File.WriteAllText(DATAPATH, writedata);
        }
        else
            Debug.LogError("PathManager : Cannot write serialization data because data is null!");
    }

    /// <summary>
    /// Reads all path data from disk
    /// </summary>
    //[MenuItem("Menu/ReadLinkData")]
    public static void ReadSerializeData()
    {
        links = new List<Link>();
        waypoints = new List<UniqueWaypoint>();
        //
        string readata = System.IO.File.ReadAllText(DATAPATH);

        string[] lines = readata.Split(Environment.NewLine.ToCharArray());

        bool doWaypoints = false;

        foreach (string l in lines)
        {
            if (l == "")
                continue;

            if (l == "waypoints")
                doWaypoints = true;
            else if (l == "links")
                doWaypoints = false;

            if (l == "EOF")
                break;

            if (doWaypoints)
            {
                // waypoints 
                try
                {
                    UniqueWaypoint u = GameObject.Find(l).GetComponent<UniqueWaypoint>();
                    if (u != null)
                    {
                        //u.name = l;
                        waypoints.Add(u);
                        u.Name = l;
                    }
                }
                catch {}
            }
            else
            {
                // links 
                try
                {
                    string[] line = l.Split(',');
                    UniqueWaypoint origin = ExtensionMethods.FindWaypointByName(line[0]);
                        //GameObject.Find(line[0]).GetComponent<UniqueWaypoint>();
                    UniqueWaypoint target = ExtensionMethods.FindWaypointByName(line[1]);
                    //GameObject.Find(line[1]).GetComponent<UniqueWaypoint>();

                    if (origin != null && target != null)
                    {
                        origin.AddChild(target);
                        links.Add(new Link(origin, target));
                    }
                    else
                        Debug.LogError("Pathmanager : no origin or target found!");
                }
                catch {}
            }
        }

        UniqueWaypoint[] uw_sorted = ExtensionMethods.SortWaypoints(waypoints.ToArray());
        
        NPCDataHolder.links = links;
        //NPCDataHolder.waypoints = new List<UniqueWaypoint>(uw_sorted);
    }


    /// <summary>
    /// This is the recommended way of reading the data
    /// </summary>
    public static void ReadNewFormatData()
    {
        // read waypoints
        string positionData = System.IO.File.ReadAllText(WAYPOINTPATH);

        string[] posDataArr = positionData.Split(Environment.NewLine.ToCharArray());

        foreach (string s in posDataArr)
        {

            string[] strData = s.Split(';');
            if (strData.Length != 4)
                continue;

            string uwName = strData[0];

            Vector3 posData = new
                Vector3(ExtensionMethods.CustomParseFloat(strData[1]), ExtensionMethods.CustomParseFloat(strData[2]), ExtensionMethods.CustomParseFloat(strData[3]));

            UniqueWaypoint newUW = new UniqueWaypoint(); // uwName, posData);
            newUW.Name = uwName;
            newUW.Position = posData;

            if (NPCDataHolder.waypoints == null)
                NPCDataHolder.waypoints = new List<UniqueWaypoint>();

            NPCDataHolder.waypoints.Add(newUW);
        }

        // read links
        links = new List<Link>();
        waypoints = NPCDataHolder.waypoints;


        string readata = System.IO.File.ReadAllText(LINKTPATH);
        string[] lines = readata.Split(Environment.NewLine.ToCharArray());

        foreach (string l in lines)
        {
            if (l == string.Empty)
                continue;

            try
            {
                string[] line = l.Split(',');

                string origin_name = line[0];
                string target_name = line[1];

                UniqueWaypoint origin = ExtensionMethods.FindWaypointByName(origin_name);
                UniqueWaypoint target = ExtensionMethods.FindWaypointByName(target_name);

                if (origin != null && target != null)
                {
                    origin.AddChild(target);
                    links.Add(new Link(origin, target));
                }
                else
                {
                    Debug.LogError("Pathmanager : no origin or target found! : " + origin_name + " ; " + target_name);
                }
            }
            catch { print("err"); }
        }

        NPCDataHolder.links = links;
    }

    public static void ReadOnlyLinks()
    {
        links = new List<Link>();
        waypoints = NPCDataHolder.waypoints;

        string readata = System.IO.File.ReadAllText(DATAPATH);

        string[] lines = readata.Split(Environment.NewLine.ToCharArray());

        bool doLinks = false;

        foreach (string l in lines)
        {
            if (l == "links")
                doLinks = true;

            if (!doLinks)
                continue;

            if (l == "EOF")
                break;

            // links 
            try
            {
                string[] line = l.Split(',');
                string origin_name = line[0];
                string target_name = line[1];

                UniqueWaypoint origin = ExtensionMethods.FindWaypointByName(origin_name);
                UniqueWaypoint target = ExtensionMethods.FindWaypointByName(target_name);

                if (origin != null && target != null)
                {
                    origin.AddChild(target);
                    links.Add(new Link(origin, target));
                }
                else
                    Debug.LogError("Pathmanager : no origin or target found! : ");
            }
            catch { }
        }

        //UniqueWaypoint[] uw_sorted = ExtensionMethods.SortWaypoints(waypoints.ToArray());

        // NOTE: this might break stuff. Uncomment if things don't wörk
        //NPCDataHolder.links = links;
    }

    public static void ReadAllWaypointDataLightweight()
    {
        //UniqueWaypoint[] uw = FindObjectsOfType<UniqueWaypoint>();

        //foreach (UniqueWaypoint u in uw)
        //    DestroyImmediate(u.gameObject);

        string positionData = System.IO.File.ReadAllText(DATAPATH_ALL);

        string[] posDataArr = positionData.Split(Environment.NewLine.ToCharArray());

        foreach (string s in posDataArr)
        {

            string[] strData = s.Split(';');
            if (strData.Length != 4)
                continue;

            string uwName = strData[0];

            Vector3 posData = new
                Vector3(ExtensionMethods.CustomParseFloat(strData[1]), ExtensionMethods.CustomParseFloat(strData[2]), ExtensionMethods.CustomParseFloat(strData[3]));

            UniqueWaypoint newUW = new UniqueWaypoint(); // uwName, posData);
            newUW.Name = uwName;
            newUW.Position = posData;

            if (NPCDataHolder.waypoints == null)
                NPCDataHolder.waypoints = new List<UniqueWaypoint>();

            NPCDataHolder.waypoints.Add(newUW);
        }

        ReadOnlyLinks();

    }
}
