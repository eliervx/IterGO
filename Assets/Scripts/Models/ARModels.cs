using UnityEngine;

namespace IterGO.Models
{
    public class ARMarkerData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public bool isTracking;

        public ARMarkerData(string name, Vector3 position, Quaternion rotation, bool isTracking = true)
        {
            this.name = name;
            this.position = position;
            this.rotation = rotation;
            this.isTracking = isTracking;
        }
    }

    public class PhotoData
    {
        public Texture2D texture;
        public string filePath;
        public System.DateTime timestamp;

        public PhotoData(Texture2D texture, string filePath)
        {
            this.texture = texture;
            this.filePath = filePath;
            this.timestamp = System.DateTime.Now;
        }
    }
}