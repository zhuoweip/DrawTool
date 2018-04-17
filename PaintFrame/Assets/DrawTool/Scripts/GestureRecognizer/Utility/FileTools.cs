#if !UNITY_WEBPLAYER

#if UNITY_WINRT && !UNITY_EDITOR
using File = UnityEngine.Windows.File;
#else
using File = System.IO.File;
#endif

using GestureRecognizer;

namespace GestureRecognizer {
    /// <summary>
    /// Windows 8 Phones and Windows 8 Store applications use a different
    /// file i/o approach now which keeps us from using conventional file i/o.
    /// This class abstracts that approach and combines these methods together.
    /// </summary>
    public static class FileTools {

        /// <summary>
        /// Read contents of a file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>Contents of the file</returns>
        public static string Read(string path) {

            string result = "";

#if UNITY_WINRT && !UNITY_EDITOR
            byte[] bytes = File.ReadAllBytes(path);
            result = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
#else
            result = File.ReadAllText(path);
#endif

            return result;
        }

        /// <summary>
        /// Write contents to a file
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <param name="contents">Contents of the file</param>
        public static void Write(string path, string contents) {

#if UNITY_WINRT && !UNITY_EDITOR
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(contents);
            File.WriteAllBytes(path, bytes);
#else
            File.WriteAllText(path, contents);
#endif
        }

        /// <summary>
        /// Check if the file exists
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>bool</returns>
        public static bool Exists(string path) {
            return File.Exists(path);
        }
    } 
}
#endif