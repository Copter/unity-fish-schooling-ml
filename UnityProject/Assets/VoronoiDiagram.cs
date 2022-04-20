using UnityEngine;
using System.Collections.Generic;
using csDelaunay;

public class VoronoiDiagram : MonoBehaviour {

    private Dictionary<Vector2f, Site> sites = new Dictionary<Vector2f, Site>();
    private List<Edge> edges = new List<Edge>();
    public List<Vector2f> points = new List<Vector2f>();
    public Rectf bounds = new Rectf(-200f, -200f, 400f, 400f);

    void Start() {
    }

    private void Update() {
        if (points.Count > 0) {
            // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
            // Here I used it with 2 iterations of the lloyd relaxation
            Voronoi voronoi = new Voronoi(points, bounds);

            // But you could also create it without lloyd relaxtion and call that function later if you want
            //Voronoi voronoi = new Voronoi(points,bounds);
            //voronoi.LloydRelaxation(5);

            // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
            sites = voronoi.SitesIndexedByLocation;
            edges = voronoi.Edges;
        }
    }

    private void OnDrawGizmos() {
        if (edges.Count > 1) {
            Gizmos.color = Color.yellow;
            foreach (Edge edge in edges) {
                // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
                if (edge.ClippedEnds == null) continue;
                Vector2f start = edge.ClippedEnds[LR.LEFT];
                Vector2f end = edge.ClippedEnds[LR.RIGHT];
                Gizmos.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
            }
        }
    }
}