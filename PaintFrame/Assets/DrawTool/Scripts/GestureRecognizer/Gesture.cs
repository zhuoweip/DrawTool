using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GestureRecognizer;

namespace GestureRecognizer {

    public class Gesture {

        /// <summary>
        /// Name of the gesture. It acts like an ID for this gesture,
        /// so you should give your gestures unique names.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Points that form this gesture.
        /// </summary>
        public List<Vector2> Points { get; set; }

        /// <summary>
        /// Vector of the gesture.
        /// </summary>
        public List<float> Vector { get; set; }

        /// <summary>
        /// Angle between the center and the first point on standard
        /// cartesian coordinate system.
        /// </summary>
        public float IndicativeAngle { get; set; }

        /// <summary>
        /// This gesture will be resampled to have this much of points. 
        /// Best between 32 and 256
        /// </summary>
        int NUMBER_OF_POINTS { get { return 64; } }

        /// <summary>
        /// Size of the bounding box.
        /// </summary>
        float SQUARE_SIZE { get { return 250f; } }

        /// <summary>
        /// Origin of the gestures.
        /// </summary>
        Vector2 ORIGIN { get { return Vector2.zero; } }

        /// <summary>
        /// Size of the diagonal of the bounding box.
        /// </summary>
        float DIAGONAL { get { return Mathf.Sqrt(Mathf.Pow(SQUARE_SIZE, 2) * 2); } }

        /// <summary>
        /// Half size of the diagonal of the bounding box.
        /// </summary>
        float HALF_DIAGONAL { get { return DIAGONAL * 0.5f; } }

        /// <summary>
        /// Range to apply Golden Section Search
        /// </summary>
        float ANGLE_RANGE { get { return 45f * Mathf.Deg2Rad; } }

        /// <summary>
        /// Precision of Golden Section Search
        /// </summary>
        float ANGLE_PRECISION { get { return 2f * Mathf.Deg2Rad; } }

        /// <summary>
        /// Phi to be used in Golden Section Search
        /// </summary>
        float PHI { get { return 0.5f * (-1f + Mathf.Sqrt(5f)); } }


        public Gesture(List<Vector2> points, string name = "") {
            this.Name = name;
            this.Points = points;
            this.IndicativeAngle = Gesture.GetIndicativeAngle(points);
            this.Points = this.Resample(NUMBER_OF_POINTS);
            this.Points = this.RotateBy(-this.IndicativeAngle);
            this.Points = this.ScaleTo(this.SQUARE_SIZE);
            this.Points = this.TranslateTo(this.ORIGIN);
            this.Vector = this.Vectorize();
        }


        /// <summary>
        /// Recognize the gesture by comparing it to every single gesture in the library,
        /// scoring them and finding the highest score. Don't recognize the gesture if there
        /// is less than 2 points.
        /// 
        /// There are two algorithms to recognize a gesture: $1 and Protractor. $1 is slower
        /// compared to Protractor, however, $1 provides a scoring system in [0, 1] interval
        /// which is very useful to determine "how much" the captured gesture looks like the 
        /// library gesture.
        /// 
        /// To find out more about the algorithms and how they work, see the respective method
        /// comments.
        /// </summary>
        /// <param name="gestureLibrary">The library to run the gesture against.</param>
        /// <param name="useProtractor">If this is true, the faster Protractor algorithm will be used.</param>
        /// <returns>Recognized gesture's name and its score</returns>
        public Result Recognize(GestureLibrary gestureLibrary, bool useProtractor = false) {

            if (this.Points.Count <= 2) {
                return new Result("Not enough points captured", 0f);
            } else {
                List<Gesture> library = gestureLibrary.Library;

                float bestDistance = float.MaxValue;
                int matchedGesture = -1;

                // Match the gesture against all the gestures in the library
                for (int i = 0; i < library.Count; i++) {

                    float distance = 0;

                    if (useProtractor) {
                        // See ProtractorAlgorithm() method's comments to find out more about it.
                        distance = ProtractorAlgorithm(library[i].Vector, this.Vector);
                    } else {
                        // See DollarOneAlgorithm() method's comments to find out more about it.
                        distance = DollarOneAlgorithm(library[i], -this.ANGLE_RANGE, +this.ANGLE_RANGE, this.ANGLE_PRECISION);
                    }

                    // If distance is better than the best distance take it as the best distance, 
                    // and gesture as the recognized one.
                    if (distance < bestDistance) {
                        bestDistance = distance;
                        matchedGesture = i;
                    }
                }

                // No match, score zero. If there is a match, send the name of the recognized gesture and a score.
                if (matchedGesture == -1) {
                    return new Result("No match", 0f);
                } else {
                    return new Result(library[matchedGesture].Name, useProtractor ? 1f / bestDistance : 1f - bestDistance / this.HALF_DIAGONAL);
                }
            }
        }


        /// <summary>
        /// Resample the point list so that the list has NUMBER_OF_POINTS number of points
        /// and points are equidistant to each other.
        /// 
        /// First calculate the length of the path. Divided it by (numberOfPoints - 1)
        /// to find the increment. Step through the path, and if the distance covered is
        /// equal to or greater than the increment add a new point to the list by lineer
        /// interpolation.
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        public List<Vector2> Resample(int numberOfPoints) {

            float increment = Gesture.GetPathLength(this.Points) / (numberOfPoints - 1);
            float distanceCovered = 0.0f;

            List<Vector2> resampledPoints = new List<Vector2>();
            resampledPoints.Add(this.Points[0]);

            for (int i = 1; i < this.Points.Count; i++) {
                float distance = Vector2.Distance(this.Points[i - 1], this.Points[i]);

                if (distanceCovered + distance >= increment) {

                    float x = this.Points[i - 1].x + ((increment - distanceCovered) / distance) * (this.Points[i].x - this.Points[i - 1].x);
                    float y = this.Points[i - 1].y + ((increment - distanceCovered) / distance) * (this.Points[i].y - this.Points[i - 1].y);
                    Vector2 q = new Vector2(x, y);
                    resampledPoints.Add(q);
                    this.Points.Insert(i, q);
                    distanceCovered = 0.0f;

                } else {
                    distanceCovered += distance;
                }
            }

            if (resampledPoints.Count == numberOfPoints - 1) {
                resampledPoints.Add(this.Points[this.Points.Count - 1]);
            }

            return resampledPoints;
        }


        /// <summary>
        /// Rotate the points in this gesture by this angle, so that the indicative angle is at zero degrees.
        /// </summary>
        /// <param name="angle">How much the gesture will be rotated.</param>
        /// <returns>List of rotated points</returns>
        public List<Vector2> RotateBy(float angle) {

            Vector2 center = Gesture.GetCenter(this.Points);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            List<Vector2> rotatedPoints = new List<Vector2>();

            for (int i = 0; i < this.Points.Count; i++) {
                float x = (this.Points[i].x - center.x) * cos - (this.Points[i].y - center.y) * sin + center.x;
                float y = (this.Points[i].x - center.x) * sin + (this.Points[i].y - center.y) * cos + center.y;
                rotatedPoints.Add(new Vector2(x, y));
            }

            return rotatedPoints;
        }


        /// <summary>
        /// Scale the gesture so that it can fit into predefined bounding box 
        /// (which is a square originated on ORIGIN with the size of SQUARE_SIZE).
        /// </summary>
        /// <param name="size">Size of the bounding box to scale to</param>
        /// <returns>List of points which now fits in the predefined bounding box.</returns>
        public List<Vector2> ScaleTo(float size) {

            Rect boundingBox = Gesture.GetBoundingBox(this.Points);
            List<Vector2> scaledPoints = new List<Vector2>();

            for (int i = 0; i < this.Points.Count; i++) {
                float x = this.Points[i].x * (size / boundingBox.width);
                float y = this.Points[i].y * (size / boundingBox.height);
                scaledPoints.Add(new Vector2(x, y));
            }

            return scaledPoints;
        }


        /// <summary>
        /// Move the gesture so that it can fit into predefined bounding box 
        /// (which is a square originated on ORIGIN with the size of SQUARE_SIZE).
        /// </summary>
        /// <param name="point">Points to move</param>
        /// <returns>List of moved points</returns>
        public List<Vector2> TranslateTo(Vector2 point) {

            Vector2 center = Gesture.GetCenter(this.Points);
            List<Vector2> translatedPoints = new List<Vector2>();

            for (int i = 0; i < this.Points.Count; i++) {
                float x = this.Points[i].x + point.x - center.x;
                float y = this.Points[i].y + point.y - center.y;
                translatedPoints.Add(new Vector2(x, y));
            }

            return translatedPoints;
        }


        /// <summary>
        /// The heart of the Protractor algorithm. Creates a vector representation for the gesture.
        /// </summary>
        /// <returns></returns>
        public List<float> Vectorize() {

            float sum = 0f;
            List<float> vector = new List<float>();

            for (int i = 0; i < this.Points.Count; i++) {
                vector.Add(this.Points[i].x);
                vector.Add(this.Points[i].y);
                sum += Mathf.Pow(this.Points[i].x, 2) + Mathf.Pow(this.Points[i].y, 2);
            }

            float magnitude = Mathf.Sqrt(sum);

            for (int i = 0; i < vector.Count; i++) {
                vector[i] /= magnitude;
            }

            return vector;
        }


        /// <summary>
        /// Calculate the center of the points
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns></returns>
        public static Vector2 GetCenter(List<Vector2> points) {
            Vector2 center = Vector2.zero;

            for (int i = 0; i < points.Count; i++) {
                center += points[i];
            }

            return center / points.Count;
        }


        /// <summary>
        /// Calculate the bounding box.
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns></returns>
        public static Rect GetBoundingBox(List<Vector2> points) {

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Count; i++) {
                minX = Mathf.Min(minX, points[i].x);
                minY = Mathf.Min(minY, points[i].y);
                maxX = Mathf.Max(maxX, points[i].x);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }


        /// <summary>
        /// Calculate total path length: sum of distance between each points
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns></returns>
        public static float GetPathLength(List<Vector2> points) {

            float length = 0;

            for (int i = 1; i < points.Count; i++) {
                length += Vector2.Distance(points[i - 1], points[i]);
            }

            return length;
        }


        /// <summary>
        /// The heart of the $1 algorithm. Basically, calculates the average of distances
        /// between points
        /// </summary>
        /// <param name="points1">List of points</param>
        /// <param name="points2">List of points</param>
        /// <returns>Average distance between points</returns>
        public static float GetDistanceBetweenPaths(List<Vector2> points1, List<Vector2> points2) {

            float distance = 0;

            for (int i = 0; i < points1.Count; i++) {
                distance += Vector2.Distance(points1[i], points2[i]);
            }

            return distance / points1.Count;
        }


        /// <summary>
        /// Indicative angle of a gesture: the angle between the center and the first point on standard
        /// cartesian coordinate system.
        /// </summary>
        /// <param name="points">Points of a gesture</param>
        /// <returns>Indicative angle</returns>
        public static float GetIndicativeAngle(List<Vector2> points) {
            Vector2 centroid = Gesture.GetCenter(points);
            return Mathf.Atan2(centroid.y - points[0].y, centroid.x - points[0].x);
        }


        /// <summary>
        /// Gets the average distance between two gestures at a specified angle
        /// </summary>
        /// <param name="gesture">Gesture to compare to</param>
        /// <param name="angle">The angle to rotate this gesture</param>
        /// <returns>Distance between two gestures</returns>
        public float GetDistanceAtAngle(Gesture gesture, float angle) {
            List<Vector2> newPoints = this.RotateBy(angle);
            return Gesture.GetDistanceBetweenPaths(newPoints, gesture.Points);
        }


        /// <summary>
        /// $1 algorithm works like this:
        /// - Take a gesture: with any number of points
        /// - Resample: so that it has exactly NUMBER_OF_POINTS number of points which are equidistant to each other
        /// - Rotate: based on the indicative angle (angle between the centroid and the first point)
        /// - Scale and translate: so that it can fit into a predefined sized square
        /// - Find the most optimized angle by Golden Section Search so that it has the best score
        /// 
        /// Resampling, rotating, scaling and translating ensures that gestures are somewhat "more" similar. By doing these
        /// operations, we make them easier to compare. After these operations, the distance between two gestures 
        /// (which is the average distance between each points: d = sum(d1, d2, ..., dn) / n) is calculated. The gesture
        /// with the least distance is the result and this result is converted to a score between [0, 1].
        /// 
        /// To get the best score, each gesture is rotated to the best indicative angle which is found by using
        /// Golden Section Search.
        /// </summary>
        /// <param name="gesture">Gesture to compare</param>
        /// <param name="a">Negative angle range</param>
        /// <param name="b">Positive angle range</param>
        /// <param name="threshold">Angle precision</param>
        /// <returns>Score</returns>
        public float DollarOneAlgorithm(Gesture gesture, float a, float b, float threshold) {

            float x1 = this.PHI * a + (1f - this.PHI) * b;
            float f1 = this.GetDistanceAtAngle(gesture, x1);
            float x2 = (1f - this.PHI) * a + this.PHI * b;
            float f2 = this.GetDistanceAtAngle(gesture, x2);

            while (Mathf.Abs(b - a) > threshold) {
                if (f1 < f2) {
                    b = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = this.PHI * a + (1f - this.PHI) * b;
                    f1 = this.GetDistanceAtAngle(gesture, x1);
                } else {
                    a = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1f - this.PHI) * a + this.PHI * b;
                    f2 = this.GetDistanceAtAngle(gesture, x2);
                }
            }

            return Mathf.Min(f1, f2);
        }


        /// <summary>
        /// This algorithm uses the nearest neighbor approach. Protractor converts gestures into 
        /// equal length vectors and calculates their optimal angular distance. 
        /// </summary>
        /// <param name="gestureVector"></param>
        /// <param name="otherGestureVector"></param>
        /// <returns></returns>
        public float ProtractorAlgorithm(List<float> gestureVector, List<float> otherGestureVector) {
            float a = 0f;
            float b = 0f;

            for (int i = 0; i < gestureVector.Count; i += 2) {
                a += gestureVector[i] * otherGestureVector[i] + gestureVector[i + 1] * otherGestureVector[i + 1];
                b += gestureVector[i] * otherGestureVector[i + 1] - gestureVector[i + 1] * otherGestureVector[i];
            }

            float angle = Mathf.Atan(b / a);
            return Mathf.Acos(a * Mathf.Cos(angle) + b * Mathf.Sin(angle));
        }


        public override string ToString() {

            string message = this.Name + "; ";

            foreach (Vector2 v in this.Points) {
                message += v.ToString() + " ";
            }

            return message;
        }

    } // end of Gesture
}