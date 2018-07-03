using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonParse
{
    /// <summary>人脸融合</summary>
    public class FaceMerge
    {
        public string img_base64 { get; set; }
        public string img_base64_thumb { get; set; }
        public string img_url { get; set; }
        public string img_url_thumb { get; set; }
        public string ret { get; set; }
        public string msg { get; set; }
        public static FaceMerge ParseJsonFaceMerge(string json)
        {
            return LitJson.JsonMapper.ToObject<FaceMerge>(json);
        }
    }

    /// <summary>人脸检测</summary>
    public class FaceDetect
    {
        public static FaceDetect ParseJsonFaceDetect(string json)
        {
            return LitJson.JsonMapper.ToObject<FaceDetect>(json);
        }

        public string session_id { get; set; }
        public int image_width { get; set; }
        public int image_height { get; set; }
        public int errorcode { get; set; }
        public string errormsg { get; set; }
        public Faces[] face { get; set; }

        public class Faces {

            public string face_id { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public double width { get; set; }
            public double height { get; set; }
            public int gender { get; set; }//性别[0/(female)~100(male)]
            public int age { get; set; }
            public int expression { get; set; }//微笑[0(normal)~50(smile)~100(laugh)]
            public int beauty { get; set; }
            public int glasses { get; set; }
            public int pitch { get; set; }
            public int yaw { get; set; }
            public int roll { get; set; }
            public int hat { get; set; }
            public int mask { get; set; }
            public bool glass { get; set; }

            public Face_Shape face_shape { get; set; }

            public class Face_Shape
            {
                public Face_rect[] face_profile { get; set; } 
                public Face_rect[] left_eye { get; set; }
                public Face_rect[] right_eye { get; set; }
                public Face_rect[] left_eyebrow { get; set; }
                public Face_rect[] right_eyebrow { get; set; }
                public Face_rect[] mouth { get; set; }
                public Face_rect[] nose { get; set; }
                public Face_rect[] pupil { get; set; }
            }

            public class Face_rect
            {
                public int x{ get; set; }
                public int y { get; set; }
            }
        }
    }

    /// <summary>多重人脸检索</summary>
    public class Multifaceidentify
    {
        public string session_id { get; set; }
        public Results[] results { get; set; }
        public int errorcode { get; set; }
        public string errormsg { get; set; }
        public int group_size { get; set; }
        public int time_ms { get; set; }
        public static Multifaceidentify ParseMultifaceidentify(string json)
        {
            return LitJson.JsonMapper.ToObject<Multifaceidentify>(json);
        }

        public class Results {
            public string[] candidates { get; set; }
            public Face_rect face_rect { get; set; }
            public int id { get; set; }
            public int errorcode { get; set; }

            public class Face_rect
            {
                public int x { get; set; }
                public int y { get; set; }
                public int width { get; set; }
                public int height { get; set; }
            }
        }
    }
}
