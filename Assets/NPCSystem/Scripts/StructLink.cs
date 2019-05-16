using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Represents the link between two waypoint nodes
/// </summary>
public struct Link
{
    private UniqueWaypoint origin;
    private UniqueWaypoint target;

    public UniqueWaypoint Origin { get { return origin; } set { origin = value; } }
    public UniqueWaypoint Target { get { return target; } set { target = value; } }

    public Link(UniqueWaypoint _origin, UniqueWaypoint _target)
    {
        origin = _origin;
        target = _target;
    }

    public bool IsNull()
    {
        return (origin == null || target == null);
    }

    public static bool operator ==(Link f, Link s)
    {
        return f.Equals(s);
    }

    public static bool operator !=(Link f, Link s)
    {
        return !f.Equals(s);
    }

    public override bool Equals(object obj)
    {
        Link otherLink = (Link)obj;
        return this.origin == otherLink.Origin && this.target == otherLink.Target;
    }
}