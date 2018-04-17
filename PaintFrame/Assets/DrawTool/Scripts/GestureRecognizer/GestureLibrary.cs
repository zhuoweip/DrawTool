using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
using GestureRecognizer;
using System.Globalization;

/// <summary>
/// Reads the gestures from an XML file and creates a library by adding each of them to a List<> of "Gesture"s.
/// Also adds a gesture to a library and saves it to a file if it is NOT a web player.
/// </summary>
namespace GestureRecognizer {
    public class GestureLibrary {

        string libraryName;
        string libraryFilename;
        string persistentLibraryPath;
        string resourcesPath;
        string xmlContents;
        XmlDocument gestureLibrary = new XmlDocument();
        List<Gesture> library = new List<Gesture>();

        public List<Gesture> Library { get { return library; } }


        public GestureLibrary(string libraryName, bool forceCopy = false) {
            this.libraryName = libraryName;
            this.libraryFilename = libraryName + ".xml";
            this.persistentLibraryPath = Path.Combine(Application.persistentDataPath, libraryFilename);
            this.resourcesPath = Path.Combine(Path.Combine(Application.dataPath, "Resources"), libraryFilename);

            CopyToPersistentPath(forceCopy);
            LoadLibrary();
        }


        /// <summary>
        /// Loads the library from an XML file
        /// </summary>
        public void LoadLibrary() {

            // Uses the XML file in resources folder if it is webplayer or the editor.
            string xmlContents = "";
			string floatSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

#if !UNITY_WEBPLAYER && !UNITY_EDITOR
            xmlContents = FileTools.Read(persistentLibraryPath);
#else
            xmlContents = Resources.Load<TextAsset>(libraryName).text;
#endif

            gestureLibrary.LoadXml(xmlContents);


            // Get "gesture" elements
            XmlNodeList xmlGestureList = gestureLibrary.GetElementsByTagName("gesture");

            // Parse "gesture" elements and add them to library
            foreach (XmlNode xmlGestureNode in xmlGestureList) {

                string gestureName = xmlGestureNode.Attributes.GetNamedItem("name").Value;
                XmlNodeList xmlPoints = xmlGestureNode.ChildNodes;
                List<Vector2> gesturePoints = new List<Vector2>();

                foreach (XmlNode point in xmlPoints) {

                    Vector2 gesturePoint = new Vector2();
                    gesturePoint.x = (float)System.Convert.ToDouble(point.Attributes.GetNamedItem("x").Value.Replace(",", floatSeparator).Replace(".", floatSeparator));
					gesturePoint.y = (float)System.Convert.ToDouble(point.Attributes.GetNamedItem("y").Value.Replace(",", floatSeparator).Replace(".", floatSeparator));
                    gesturePoints.Add(gesturePoint);

                }

                Gesture gesture = new Gesture(gesturePoints, gestureName);
                library.Add(gesture);
            }
        }


        /// <summary>
        /// Adds a new gesture to library and then saves it to the xml.
        /// The trick here is that we don't reload the newly saved xml.
        /// It would have been a waste of resources. Instead, we just add
        /// the new gesture to the list of gestures (the library).
        /// </summary>
        /// <param name="gesture">The gesture to add</param>
        /// <returns>True if addition is succesful</returns>
        public bool AddGesture(Gesture gesture) {

            // Create the xml node to add to the xml file
            XmlElement rootElement = gestureLibrary.DocumentElement;
            XmlElement gestureNode = gestureLibrary.CreateElement("gesture");
            gestureNode.SetAttribute("name", gesture.Name);

            foreach (Vector2 v in gesture.Points) {
                XmlElement gesturePoint = gestureLibrary.CreateElement("point");
                gesturePoint.SetAttribute("x", v.x.ToString());
                gesturePoint.SetAttribute("y", v.y.ToString());

                gestureNode.AppendChild(gesturePoint);
            }

            // Append the node to xml file contents
            rootElement.AppendChild(gestureNode);

            try {

                // Add the new gesture to the list of gestures
                this.Library.Add(gesture);

                // Save the file if it is not the web player, because
                // web player cannot have write permissions.
#if !UNITY_WEBPLAYER && !UNITY_EDITOR
                FileTools.Write(persistentLibraryPath, gestureLibrary.OuterXml);
#elif UNITY_EDITOR && !UNITY_WEBPLAYER
                FileTools.Write(resourcesPath, gestureLibrary.OuterXml);
#endif

				return true;
            } catch (Exception e) {
                Debug.Log(e.Message);
                return false;
            }

        }


        /// <summary>
        /// Copy to persistent data path so that we can save a new gesture
        /// on all platforms (except web player)
        /// </summary>
        /// <param name="forceCopy">Forces to copy over the existing XML file</param>
        void CopyToPersistentPath(bool forceCopy) {

#if !UNITY_WEBPLAYER && !UNITY_EDITOR
            if (!FileTools.Exists(persistentLibraryPath) || (FileTools.Exists(persistentLibraryPath) && forceCopy)) {
                string fileContents = Resources.Load<TextAsset>(libraryName).text;
                FileTools.Write(persistentLibraryPath, fileContents);
            }
#endif

        }

    }
}