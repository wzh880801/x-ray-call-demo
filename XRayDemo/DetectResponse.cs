using System;

namespace XRayDemo
{
    public class DetectResponse
    {
        public Box[] boxes_detected { get; set; }

        public string request_id { get; set; }
    }

    public class Box
    {
        public float[] box { get; set; }
        public string puid { get; set; }
        public float score { get; set; }
        public string type { get; set; }
    }
}
