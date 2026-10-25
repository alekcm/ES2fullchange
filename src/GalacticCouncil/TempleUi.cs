// Endless Temples - minimal always-on UI: temple markers projected on the galaxy map and dismissible
// messages (temple located / temple opened / the "radio hum" for everybody else).
//
// This intentionally does NOT reuse the Council IMGUI host (destroyed once the native Council window
// exists) and does not pop up native windows: it is a tiny overlay host of its own that only draws while
// the local player has a revealed temple or a pending temple message.
using System;
using System.Collections.Generic;
using UnityEngine;

public class TempleUiBehaviour : MonoBehaviour
{
    void OnGUI()
    {
        try { TempleUi.Draw(); } catch (Exception) { }
    }
}

public static class TempleUi
{
    sealed class Msg { public string Title; public string Text; }

    static readonly Queue<Msg> messages = new Queue<Msg>();
    static GameObject host;
    static GUIStyle titleStyle, wrapStyle, markerStyle;

    static List<TempleMarkerInfo> marks;
    static float nextMarksAt;
    static GalaxySpecialNode[] nodeObjects;
    static float nextScanAt;

    public static void EnsureHost()
    {
        try
        {
            if (host != null) return;
            var found = GameObject.Find("__ES2EndlessTemplesUI__");
            if (found == null)
            {
                found = new GameObject("__ES2EndlessTemplesUI__");
                UnityEngine.Object.DontDestroyOnLoad(found);
            }
            if (found.GetComponent<TempleUiBehaviour>() == null) found.AddComponent<TempleUiBehaviour>();
            host = found;
        }
        catch (Exception) { }
    }

    /// <summary>Queues a message for the local player (callers already check who the local player is).</summary>
    public static void QueueMessage(string title, string text)
    {
        try
        {
            messages.Enqueue(new Msg { Title = title ?? "", Text = text ?? "" });
            EnsureHost();
        }
        catch (Exception) { }
    }

    static void InitStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.45f);
        wrapStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
        markerStyle = new GUIStyle(GUI.skin.box) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        markerStyle.normal.textColor = new Color(1f, 0.85f, 0.45f);
    }

    public static void Draw()
    {
        var game = CouncilGame.GetGame();
        if (game == null) { if (messages.Count > 0) messages.Clear(); marks = null; return; }
        InitStyles();

        float sc = Mathf.Max(1f, Screen.height / 1080f);
        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(sc, sc, 1f));
        try
        {
            float sw = Screen.width / sc;
            DrawMessages(sw);
            DrawMarkers(sc);
        }
        finally { GUI.matrix = oldMatrix; }
    }

    static void DrawMessages(float sw)
    {
        if (messages.Count == 0) return;
        var m = messages.Peek();
        float w = 560f, h = 164f;
        var rect = new Rect((sw - w) / 2f, 54f, w, h);
        GUI.Box(rect, GUIContent.none);
        GUI.Label(new Rect(rect.x + 14, rect.y + 8, w - 28, 24), m.Title, titleStyle);
        GUI.Label(new Rect(rect.x + 14, rect.y + 34, w - 28, h - 76), m.Text, wrapStyle);
        string ok = CouncilGame.Tr("Понятно", "OK") + (messages.Count > 1 ? " (" + messages.Count + ")" : "");
        if (GUI.Button(new Rect(rect.x + w - 124, rect.y + h - 36, 110, 26), ok)) messages.Dequeue();
    }

    static void DrawMarkers(float sc)
    {
        float now = Time.realtimeSinceStartup;
        if (marks == null || now >= nextMarksAt) { marks = TempleGame.LocalMarkers(); nextMarksAt = now + 0.5f; }
        if (marks == null || marks.Count == 0) return;

        if (nodeObjects == null || now >= nextScanAt)
        {
            try { nodeObjects = UnityEngine.Object.FindObjectsOfType<GalaxySpecialNode>(); } catch (Exception) { nodeObjects = null; }
            nextScanAt = now + 2f;
        }
        if (nodeObjects == null || nodeObjects.Length == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;
        for (int i = 0; i < marks.Count; i++)
        {
            var obj = FindNodeObject(marks[i].NodeGuid);
            if (obj == null) continue;
            Vector3 sp;
            try
            {
                if (!obj.gameObject.activeInHierarchy) continue;
                sp = cam.WorldToScreenPoint(obj.transform.position);
            }
            catch (Exception) { continue; }
            if (sp.z < 0f) continue;
            float x = sp.x / sc, y = (Screen.height - sp.y) / sc;
            float w = 280f, h = 24f;
            if (x < -w || y < -h || x > Screen.width / sc + w || y > Screen.height / sc + h) continue;
            GUI.Box(new Rect(x - w / 2f, y - h - 14f, w, h), marks[i].Label, markerStyle);
        }
    }

    static GalaxySpecialNode FindNodeObject(ulong guid)
    {
        for (int i = 0; i < nodeObjects.Length; i++)
        {
            var o = nodeObjects[i];
            try
            {
                if (o != null && o.SpecialNode != null && o.SpecialNode.GUID == guid) return o;
            }
            catch (Exception) { }
        }
        return null;
    }
}
