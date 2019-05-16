using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ToolTypes
{
    create,
    deleteWaypoints
};

public class RouteGenerator : MonoBehaviour
{

    private Camera thisCamera;

    [SerializeField] [Tooltip("Layers which won't be raycasted against")]
    private int[] ignoreLayers;

    private int layerMask;

    private RaycastHit roadHit;

    private Vector3 beginDrag;
    private Vector3 endDrag;
    private Vector3 currentHit;

    private LineRenderer lineRenderer;

    private List<Link> links = new List<Link>();
    private List<UniqueWaypoint> waypoints = new List<UniqueWaypoint>();

    public string WAYPOINTPATH = "waypointdata.data";
    public string LINKPATH = "linkdata.data";

    [SerializeField] private Material lineMaterial;

    [SerializeField] private float connectThreshold = 1f;

    bool needFlush = false;

    [SerializeField] GameObject deleteBrush;

    public ToolTypes currentTool = ToolTypes.create;

    void Start()
    {
        thisCamera = GetComponent<Camera>();

        beginDrag = Vector3.zero;
        endDrag = Vector3.zero;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;

        lineRenderer.enabled = false;

        // generate layermask
        foreach (int i in ignoreLayers)
            layerMask |= (1 << i);
        layerMask = ~layerMask;

        // disable junk
        try
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera c in cameras)
            {
                if (c != thisCamera)
                    c.enabled = false;
            }

            FindObjectOfType<NPCMovementManager>().enabled = false;
            FindObjectOfType<PedestrianHiveMind>().enabled = false;
            FindObjectOfType<WeatherSystemController>().enabled = false;
        }
        catch {}

        if (!System.IO.File.Exists(WAYPOINTPATH))
        {
            System.IO.File.Create(WAYPOINTPATH);
        }

        if (!System.IO.File.Exists(LINKPATH))
        {
            System.IO.File.Create(LINKPATH);
        }

        if (NPCDataHolder.links == null || NPCDataHolder.waypoints == null)
            ExtensionMethods.PopulateNPCDataHolder();

    }


    void Update()
    {
        // ignore everything else if we are moving ATM
        if (MoveCamera())
        {
            deleteBrush.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (currentTool == ToolTypes.create)
            {
                deleteBrush.SetActive(true);
                currentTool = ToolTypes.deleteWaypoints;
            }
            else
            {
                deleteBrush.SetActive(false);
                currentTool = ToolTypes.create;
            }
        }


        if (currentTool == ToolTypes.create)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (GetRayHit(out currentHit) != Vector3.zero)
                {
                    if (beginDrag == Vector3.zero)
                    {
                        beginDrag = currentHit + new Vector3(0, 0.3f, 0);
                        lineRenderer.enabled = true;
                        lineRenderer.SetPosition(0, beginDrag);
                    }

                    endDrag = currentHit + new Vector3(0, 0.3f, 0);
                    lineRenderer.SetPosition(1, endDrag);
                }
            }
            else if (beginDrag != Vector3.zero && endDrag != Vector3.zero)
            {
                ConnectPoints();
            }
        }
        else if (currentTool == ToolTypes.deleteWaypoints)
        {
            deleteBrush.SetActive(true);
            Vector3 outhit;
            if (GetRayHit(out outhit) != Vector3.zero)
            {
                if (deleteBrush != null)
                    deleteBrush.transform.position = outhit;

                if (Input.GetKey(KeyCode.Mouse0))
                    DeleteWaypointsUnderCursor(outhit, 1f);

                if (Input.GetKeyUp(KeyCode.Mouse0) && needFlush)
                    FlushWriteData();
            }
        }

    }

    void DeleteWaypointsUnderCursor(Vector3 point, float radius)
    {
        bool close;
        List<Link> toDelete = new List<Link>();
        List<UniqueWaypoint> toDeleteWaypoints = new List<UniqueWaypoint>();

        foreach (UniqueWaypoint uw in NPCDataHolder.waypoints)
        {
            close = Mathf.Abs(uw.Position.x - point.x) < radius && Mathf.Abs(uw.Position.y - point.y) < radius && Mathf.Abs(uw.Position.z - point.z) < radius;

            if (close)
            {
                Link[] rLinks = ExtensionMethods.GetLinkByOrigin(uw);
                if (rLinks.Length != 0)
                {
                    toDelete.AddRange(rLinks);
                    foreach (Link l in rLinks)
                    {
                        //l.Origin.RemoveChild(l.Target);

                        if (!toDeleteWaypoints.Contains(l.Origin))
                            toDeleteWaypoints.Add(l.Origin);
                    }
                }

                rLinks = ExtensionMethods.GetLinkByTarget(uw);
                if (rLinks.Length != 0)
                {
                    toDelete.AddRange(rLinks);

                    foreach (Link l in rLinks)
                    {
                        l.Origin.RemoveChild(uw);
                    }
                }

            }
        }

        foreach (Link l in toDelete)
            NPCDataHolder.links.Remove(l);

        foreach (UniqueWaypoint uw in toDeleteWaypoints)
            NPCDataHolder.waypoints.Remove(uw);

        needFlush = true;
    }

    private void FlushWriteData()
    {
        string waypointData = "";

        foreach (UniqueWaypoint uw in NPCDataHolder.waypoints)
        {
            string line = uw.Name + ";" + uw.Position.x + ";" + uw.Position.y + ";" + uw.Position.z + Environment.NewLine;
            waypointData += line;
        }

        System.IO.File.WriteAllText(WAYPOINTPATH, waypointData);

        string linkData = "";

        foreach (Link l in NPCDataHolder.links)
        {
            string line = l.Origin.Name + "," + l.Target.Name + System.Environment.NewLine;
            linkData += line;
        }

        System.IO.File.WriteAllText(LINKPATH, linkData);

        needFlush = false;
    }

    void ConnectPoints()
    {

        UniqueWaypoint begin = new UniqueWaypoint();
        UniqueWaypoint end = new UniqueWaypoint();
        begin.Position = beginDrag;
        end.Position = endDrag;

        // check if should connect
        foreach (UniqueWaypoint uw in NPCDataHolder.waypoints)
        {
            if (Mathf.Abs(uw.Position.x - beginDrag.x) < connectThreshold && Mathf.Abs(uw.Position.y - beginDrag.y) < connectThreshold && Mathf.Abs(uw.Position.z - beginDrag.z) < connectThreshold)
                begin = uw;

            if (Mathf.Abs(uw.Position.x - endDrag.x) < connectThreshold && Mathf.Abs(uw.Position.y - endDrag.y) < connectThreshold && Mathf.Abs(uw.Position.z - endDrag.z) < connectThreshold)
                end = uw;
        }

        if (begin.Name == null)
        {
            if (NPCDataHolder.waypoints.Count == 0)
                begin.Name = "w : 0";
            else
                begin.Name = "w : " + (NPCDataHolder.waypoints[NPCDataHolder.waypoints.Count - 1].OrderNumber + 1).ToString();

            NPCDataHolder.waypoints.Add(begin);
            string posData = begin.Name + ";" + begin.Position.x + ";" + begin.Position.y + ";" + begin.Position.z + Environment.NewLine;
                
            System.IO.File.AppendAllText(WAYPOINTPATH, posData);
        }

        if (end.Name == null)
        {
            end.Name = "w : " + (NPCDataHolder.waypoints[NPCDataHolder.waypoints.Count - 1].OrderNumber + 1).ToString();
            NPCDataHolder.waypoints.Add(end);

            string posData = end.Name + ";" + end.Position.x + ";" + end.Position.y + ";" + end.Position.z + Environment.NewLine;
            System.IO.File.AppendAllText(WAYPOINTPATH, posData);
        }

        begin.AddChild(end);

        string writeData = begin.Name + "," + end.Name + Environment.NewLine;

        System.IO.File.AppendAllText(LINKPATH, writeData);

        NPCDataHolder.links.Add(new Link(begin, end));
        
        lineRenderer.enabled = false;
        endDrag = Vector3.zero;
        beginDrag = Vector3.zero;
    }

    public void OnPostRender()
    {
        lineMaterial.color = Color.red;
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        foreach (Link l in NPCDataHolder.links)
        {
            Vector3 rightDir = Quaternion.LookRotation(l.Target.Position - l.Origin.Position) * Quaternion.Euler(0, 180 + 35, 0) * new Vector3(0, 0, 1);
            Vector3 right = l.Target.Position + (rightDir.normalized * 0.5f);

            Vector3 leftDir = Quaternion.LookRotation(l.Target.Position - l.Origin.Position) * Quaternion.Euler(0, 180 - 35, 0) * new Vector3(0, 0, 1);
            Vector3 left = l.Target.Position + (leftDir.normalized * 0.5f);

            GL.Color(Color.red);
            GL.Vertex(l.Origin.Position);
            GL.Vertex(l.Target.Position);

            GL.Vertex(l.Target.Position);
            GL.Vertex(right);

            GL.Vertex(l.Target.Position);
            GL.Vertex(left);
        }

        GL.End();
    }


    Vector3 GetRayHit(out Vector3 hitPoint)
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = thisCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, thisCamera.nearClipPlane));

        if (Physics.Raycast(thisCamera.ScreenPointToRay(mouseScreenPos), out roadHit, Mathf.Infinity, layerMask))
        {
            hitPoint = roadHit.point;
            return roadHit.point;
        }

        hitPoint = Vector3.zero;
        return Vector3.zero;
    }

    bool MoveCamera()
    {
        // move only if right mouse is pressed
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                GetComponent<DevFlyCam>().enabled = true;
            }

            return true;
        }
        else
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                GetComponent<DevFlyCam>().enabled = false;
            }

            return false;
        }
    }
}
