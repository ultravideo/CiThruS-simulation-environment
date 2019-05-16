/* 
 * A collection of static methods to be used here and there
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Threading;

/// <summary>
/// Used to draw arrows in scene -view
/// </summary>
public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 target, Color color, float arrowHeadLength = 0.5f)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(pos, target);

        Vector3 right = Quaternion.LookRotation(target - pos) * Quaternion.Euler(0, 180 + 35, 0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(target - pos) * Quaternion.Euler(0, 180 - 35, 0) * new Vector3(0,0,1);
        //Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        //Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(target, right * arrowHeadLength);
        Gizmos.DrawRay(target, left * arrowHeadLength);
    }


}

/// <summary>
/// A mismatch of some extensions methods
/// </summary>
public static class ExtensionMethods
{
    

    /// <summary>
    /// Checks if a lists contains an identical object, not necessarily the same object
    /// </summary>
    public static bool CustomContainsLink(List<Link> ll, Link c)
    {
        foreach (Link l in ll)
        {
            if (l == c)
                return true;
        }

        return false;

    }

    public static KeyValuePair<string, int> GetNextEmptyWaypointName(UniqueWaypoint lastWaypoint)
    {
        int nextInt = lastWaypoint.OrderNumber + 1;
        string nextName = "w : " + nextInt.ToString();

        KeyValuePair<string, int> pair = new KeyValuePair<string, int>(nextName, nextInt);
        return pair;
    }

    /// <summary>
    /// easier way of remembering to populate NPCDataHolder
    /// </summary>
    public static void PopulateNPCDataHolder()
    {
        PathManager.ReadNewFormatData();
        //PathManager.SetPathData();
    }

    public static Link[] GetLinkByOrigin(UniqueWaypoint origin)
    {
        List<Link> linklist = new List<Link>();

        foreach (Link l in NPCDataHolder.links)
        {
            if (l.Origin == origin)
                linklist.Add(l);
        }

        return linklist.ToArray();
    }

    public static Link[] GetLinkByTarget(UniqueWaypoint target)
    {
        List<Link> linklist = new List<Link>();

        foreach (Link l in NPCDataHolder.links)
        {
            if (l.Target == target)
                linklist.Add(l);
        }

        return linklist.ToArray();
    }

    /// <summary>
    /// gets a random UniqueWaypoint from NPCDataholder.waypoints
    /// </summary>
    public static UniqueWaypoint GetRandomWaypoint()
    {
        return NPCDataHolder.waypoints[Random.Range(0, NPCDataHolder.waypoints.Count - 1)];
    }

    /// <summary>
    /// Sorts UniqueWaypoints[] into rising order by name
    /// </summary>
    /// <param name="waypoints">UniqueWaypoints[]</param>
    /// <returns></returns>
    public static UniqueWaypoint[] SortWaypoints(UniqueWaypoint[] waypoints)
    {
        int i, j, n;
        n = waypoints.Length;
        for (i = 0; i < n - 1; i++)
        {
            for (j = 0; j < n - i - 1; j++)
            {
                if (waypoints[j] > waypoints[j + 1])
                {
                    UniqueWaypoint temp = waypoints[j];
                    waypoints[j] = waypoints[j + 1];
                    waypoints[j + 1] = temp;
                }
            }
        }

        return waypoints;
    }

    /// <summary>
    /// Find an array of waypoints, who have the same origin
    /// </summary>
    /// <param name="origin">UniqueWaypoint</param>
    /// <returns>UniqueWaypoint[]</returns>
    public static UniqueWaypoint[] FindChildArray(UniqueWaypoint origin)
    {
        Link[] links = NPCDataHolder.links.ToArray();
        List<UniqueWaypoint> children = new List<UniqueWaypoint>();

        foreach (Link l in links)
        {
            if (l.Origin == origin)
                children.Add(l.Target);
        }

        return children.ToArray();
    }

    public static UniqueWaypoint FindWaypointByName(string n)
    {
        if (NPCDataHolder.waypoints.Count == 0)
            Debug.LogError("Extensions Methods : NPCDataholder.waypoint count was zero!");

        foreach (UniqueWaypoint uw in NPCDataHolder.waypoints)
        {
            if (uw.Name == n)
                return uw;
        }

        return null;
    }

    /// <summary>
    /// used in getting the next waypoint. 
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="ignoreLongRoad">should never be true unless we are in windridge</param>
    /// <returns></returns>
    public static List<UniqueWaypoint> ShiftPathChainByOne(List<UniqueWaypoint> _path, bool ignoreLongRoad)
    {
        _path.RemoveAt(0);
        UniqueWaypoint new_u = _path[1].GetRandomChild();

        // check for forbidden waypoints, hardcoded for simplicity
        if (ignoreLongRoad)
        {
            if (new_u.Name == "w : 779")
                new_u = ExtensionMethods.FindWaypointByName("w : 782");
            else if (new_u.Name == "w : 170")
                new_u = ExtensionMethods.FindWaypointByName("w : 562");
        }

        _path.Add(new_u);
        return _path;
    }

    public static string CustomParseVec3(Vector3 vector3)
    {
        string str = vector3.x + " " + vector3.y + " " + vector3.z;

        str = str.Replace(',', '.');

        return str;
    }

    /// <summary>
    /// used to parse a string containing either , or . to currently in used regional format
    /// </summary>
    /// <param name="str">string</param>
    /// <returns></returns>
    public static float CustomParseFloat(string str)
    {

        string dec = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        string[] newstr;

        if (str.Contains(","))
            newstr = str.Split(",".ToCharArray());
        else
            newstr = str.Split(".".ToCharArray());

        return float.Parse(newstr[0] + dec + (newstr.Length == 2 ? newstr[1] : "00"));
    }

    public static float Remap(this float value, float aLow, float aHigh, float bLow, float bHigh)
    {
        float normal = Mathf.InverseLerp(aLow, aHigh, value);
        float bValue = Mathf.Lerp(bLow, bHigh, normal);
        return bValue;
    }
}
