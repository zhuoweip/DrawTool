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

    /// <summary>
    /// M:N 搜索
    /// </summary>
    public class BaiDuMultiSearch
    {
        public static BaiDuMultiSearch ParseJsonMultiSearch(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiDuMultiSearch>(json);
        }
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public System.Int64 log_id { get; set; }
        public int timestamp { get; set; }
        public int cached { get; set; }
        public Result result { get; set; }
        public class Result
        {
            public int face_num;
            public Face_List[] face_list;
            public class Face_List
            {
                public string face_token;
                public Location location { get; set; }
                public class Location
                {
                    public double left;
                    public double top;
                    public double width;
                    public double height;
                    public System.Int64 rotation;
                }
                public User_List[] user_list { get; set; }
                public class User_List
                {
                    public string group_id;
                    public string user_id;
                    public string user_info;
                    public double score;
                }
            }
        }
    }

    /// <summary>
    /// 1:N 搜索
    /// </summary>
    public class BaiDuSearch
    {
        public static BaiDuSearch ParseJsonSearch(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiDuSearch>(json);
        }
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public System.Int64 log_id { get; set; }
        public int timestamp { get; set; }
        public int cached { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public string face_token;
            public User_List[] user_list { get; set; }
            public class User_List
            {
                public string group_id;
                public string user_id;
                public string user_info;
                public double score;
            }
        }
    }

    public class BaiDuGetUserIdList
    {
        public static BaiDuGetUserIdList ParseJsonGetUserIdList(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiDuGetUserIdList>(json);
        }
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public System.Int64 log_id { get; set; }
        public int timestamp { get; set; }
        public int cached { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public string[] user_id_list;
        }
    }

    public class BaiduFaceList
    {
        public static BaiduFaceList ParseJsonFaceList(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiduFaceList>(json);
        }

        public int error_code { get; set; }
        public string error_msg { get; set; }
        public System.Int64 log_id { get; set; }
        public int timestamp { get; set; }
        public int cached { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public Face_list[] face_list { get; set; }
            public class Face_list
            {
                public string face_token { get; set; }
                public string ctime { get; set; }
            }
        }
    }

    public class BaiDuBodySeg
    {
        public static BaiDuBodySeg ParseJsonBodySeg(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiDuBodySeg>(json);
        }
        public System.Int64 log_id;
        public string labelmap;
        public string scoremap;
        public string foreground;
    }

    public class BaiduFaceDectect
    {
        public static BaiduFaceDectect ParseJsonFaceDetect(string json)
        {
            return LitJson.JsonMapper.ToObject<BaiduFaceDectect>(json);
        }

        public int error_code { get; set; }
        public string error_msg { get; set; }
        public System.Int64 log_id { get; set; }
        public int timestamp { get; set; }
        public int cached { get; set; }
        public Result result { get; set; }

        public class Result
        {
            public int face_num { get; set; }
            public Face_list[] face_list { get; set; }

            public class Face_list
            {
                public string face_token { get; set; }
                public Location location { get; set; }
                public double face_probability { get; set; }
                public Angle angle { get; set; }
                public Expression expression { get; set; }
                public Face_shape face_shape { get; set; }
                public Gender gender { get; set; }
                public Glasses glasses { get; set; }
                public Race race { get; set; }
                public Face_type face_type { get; set; }
                public Landmark[] landmark { get; set; }
                public Landmark72[] landmark72 { get; set; }
                public Quality quality { get; set; }
                public double age { get; set; }
                public double beauty { get; set; }

                public class Location
                {
                    public double left { get; set; }
                    public double top { get; set; }
                    public double width { get; set; }
                    public double height { get; set; }
                    public int rotation { get; set; }
                }
                public class Angle
                {
                    public double yaw { get; set; }
                    public double pitch { get; set; }
                    public double roll { get; set; }
                }
                public class Expression
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Face_shape
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Gender
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Glasses
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Race
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Face_type
                {
                    public string type { get; set; }
                    public double probability { get; set; }
                }
                public class Landmark
                {
                    public double x { get; set; }
                    public double y { get; set; }
                }
                public class Landmark72
                {
                    public double x { get; set; }
                    public double y { get; set; }
                }
                public class Quality
                {
                    public double blur { get; set; }
                    public double illumination { get; set; }
                    public int completeness { get; set; }

                    public Occlusion occlusion;
                    public class Occlusion
                    {
                        public double left_eye { get; set; }
                        public double right_eye { get; set; }
                        public double nose { get; set; }
                        public double mouth { get; set; }
                        public double left_cheek { get; set; }
                        public double right_cheek { get; set; }
                        public double chin_contour { get; set; }
                    }
                }
            }
        }
    }

    /// <summary>百度人脸检测</summary>
    public class TencentFaceDetect
    {
        public static TencentFaceDetect ParseJsonFaceDetect(string json)
        {
            return LitJson.JsonMapper.ToObject<TencentFaceDetect>(json);
        }

        public string session_id { get; set; }
        public int image_width { get; set; }
        public int image_height { get; set; }
        public int errorcode { get; set; }
        public string errormsg { get; set; }
        public Faces[] face { get; set; }

        public class Faces
        {

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
                public int x { get; set; }
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

        public class Results
        {
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
