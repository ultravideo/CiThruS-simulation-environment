using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used to get all waypoints in scene and their names
/// </summary>
public class UniqueWaypoint
{
    private string sname;
    public string Name { get { return sname; } set { sname = value; } }

    private Vector3 position;

    public Vector3 Position { get { return position; } set { position = value; } }

    private List<UniqueWaypoint> children = new List<UniqueWaypoint>();

    private uint orderNumber;
    public int OrderNumber
    {
        get
        {
            if (sname != null)
                return int.Parse(this.Name.Split(":".ToCharArray())[1]);
            return -1;
        }
    }

    /// <summary>
    /// Adds given UniqueWaypoint to this as a child
    /// </summary>
    /// <param name="u">UniqueWaypoint</param>
    public void AddChild(UniqueWaypoint u)
    {
        children.Add(u);
    }

    public void AddChildren(UniqueWaypoint[] u)
    {
        children.AddRange(u);
    }

    public UniqueWaypoint[] GetChildren()
    {
        return children.ToArray();
    }

    public UniqueWaypoint GetRandomChild()
    {
        return children.Count > 0 ? children[Random.Range(0, children.Count)] : null;
    }

    public void RemoveChild(UniqueWaypoint uw)
    {
        if (children.Contains(uw))
            children.Remove(uw);
    }

    // These are used for comparing name numbers
    public static bool operator >(UniqueWaypoint f, UniqueWaypoint s)
    {
        string[] fnumber = f.Name.Split(":".ToCharArray());
        string[] snumber = s.Name.Split(":".ToCharArray());

        return int.Parse(fnumber[1]) > int.Parse(snumber[1]);
    }

    public static bool operator <(UniqueWaypoint f, UniqueWaypoint s)
    {
        string[] fnumber = f.Name.Split(":".ToCharArray());
        string[] snumber = s.Name.Split(":".ToCharArray());

        return int.Parse(fnumber[1]) < int.Parse(snumber[1]);
    }

}
