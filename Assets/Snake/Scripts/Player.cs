using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;


public class Player : MonoBehaviour
{
    public int length = 20;
    public float speed = 1f;
    public float diameter = 1f;
    public float distancePerSecond = 5f;
    public float degreesPerSecond = 270f;
    public Transform tail;
    public Segment segmentPrefab;

    List<Segment> segments = new List<Segment>();
    Stack<Segment> segmentPool = new Stack<Segment>();

    float speedCache;
    Mesh turnLeft;
    Mesh turnRight;
    Mesh moveForward;

    int initialLength;
    Vector3 initialPosition;
    Quaternion initialRotation;

    UIManager uiManager => UIManager.instance;
    GameManager gameManager => GameManager.instance;

    public static Player instance {
        get; private set;
    }

    void Awake() {
        instance = this;

        initialLength = length;
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        ResetPlayer();
    }

    public void ResetPlayer() {
        length = initialLength;
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        tail.transform.position = initialPosition;
        tail.transform.rotation = initialRotation;

        while (segments.Count > 0) {
            RemoveSegment();
        }

        CreateMeshSegments();
    }

    private void OnDestroy() {
        Destroy(turnLeft);
        Destroy(turnRight);
        Destroy(moveForward);

        turnLeft = null;
        turnRight = null;
        moveForward = null;
    }

    public void CreateMeshSegments() {
        if (speed == speedCache && turnLeft && turnRight && moveForward) {
            return;
        }

        speedCache = speed;
        if (!turnLeft) {
            turnLeft = new Mesh();
        }
        if (!turnRight) {
            turnRight = new Mesh();
        }
        if (!moveForward) {
            moveForward = new Mesh();
        }

        var d = GetDistanceDelta();
        var yaw = GetRotationDelta();
        PopulateSnakeMesh(turnLeft, d, -yaw);
        PopulateSnakeMesh(turnRight, d, +yaw);
        PopulateSnakeMesh(moveForward, d, 0);
    }

    void PopulateSnakeMesh(Mesh mesh, float d, float yaw) {
        var r = diameter * 0.5f;
        var c = 32;
        var vertices = new List<Vector3>(c * 2);
        var normals = new List<Vector3>(c * 2);

        for (int i = 0; i < c; i++) {
            var a = Mathf.Deg2Rad * ((i / (float)c) * 360f);
            var p = new Vector3(r * Mathf.Cos(a), r * Mathf.Sin(a), 0f);
            vertices.Add(p);
            normals.Add(p.normalized);
        }

        var m = Matrix4x4.Rotate(Quaternion.Euler(0, yaw, 0));
        for (int i = 0; i < c; i++) {
            var a = Mathf.Deg2Rad * ((i / (float)c) * 360f);
            var p = new Vector3(r * Mathf.Cos(a), r * Mathf.Sin(a), 0f);
            vertices.Add(d * Vector3.forward + m.MultiplyPoint(p));
            normals.Add(m.MultiplyVector(p.normalized));
        }

        var indices = new List<int>();
        for (int i = 0; i < c; i++) {
            indices.Add(i);
            indices.Add((i + 1) % c);
            indices.Add(i + c);
            indices.Add((i + 1) % c);
            indices.Add((i + 1) % c + c);
            indices.Add(i + c);
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        mesh.name = $"d={d},yaw={yaw}";
    }

    void AddSegment(Mesh mesh, Vector3 position, Quaternion rotation) {
        if (segmentPool.Count == 0) {
            segmentPool.Push(Instantiate(segmentPrefab));
        }
        var segment = segmentPool.Pop();
        segment.GetComponent<MeshFilter>().sharedMesh = mesh;
        segment.GetComponent<MeshCollider>().sharedMesh = mesh;
        segment.transform.position = position;
        segment.transform.rotation = rotation;
        segments.Add(segment);

        segment.gameObject.SetActive(true);
    }

    void RemoveSegment() {
        var segment = segments[0];
        segments.RemoveAt(0);

        segmentPool.Push(segment);

        segment.gameObject.SetActive(false);
    }

    float GetDistanceDelta() {
        return speed * distancePerSecond * Time.fixedDeltaTime;
    }
    float GetRotationDelta() {
        return speed * degreesPerSecond * Time.fixedDeltaTime;
    }

    void FixedUpdate() {
        var d = GetDistanceDelta();
        var yaw = GetRotationDelta();

        var p0 = transform.position;
        var p1 = transform.position + transform.forward * d;
        var r0 = transform.rotation;

        var mesh = moveForward;
        var rotation = 0;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || uiManager.IsLeftButtonPressed) {
            mesh = turnLeft;
            rotation = -1;
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || uiManager.IsRightButtonPressed) {
            mesh = turnRight;
            rotation = +1;
        }
        yaw *= rotation;

        if (rotation != 0) {
            var angle = transform.rotation.eulerAngles;
            angle.y += yaw;
            transform.rotation = Quaternion.Euler(angle);
        }
        transform.position = p1;

        AddSegment(mesh, p0, r0);
        //var segment = Instantiate(segmentPrefab);
        //segment.sharedMesh = mesh;
        //segment.transform.position = p0;
        //segment.transform.rotation = r0;
        //segments.Add(segment);

        if (segments.Count > length) {
            RemoveSegment();
        }

        tail.transform.position = segments[0].transform.position;
        tail.transform.rotation = segments[0].transform.rotation;
    }

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Food>() is Food food) {
            gameManager.EatFood(food);
            return;
        }
        if (other.GetComponent<Wall>()) {
            gameManager.GameOver();
            return;
        }
        if (other.GetComponent<Segment>() is Segment segment) {
            var index = segments.IndexOf(segment);
            if (index < segments.Count - 10) {
                gameManager.GameOver();
            }
        }
    }
}
