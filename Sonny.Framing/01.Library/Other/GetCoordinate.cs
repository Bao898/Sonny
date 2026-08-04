// // Licensed to the.NET Foundation under one or more agreements.
// // The.NET Foundation licenses this file to you under the MIT license.
//
// namespace SonnyBIM;
//
// public class GetCoordinate
// {
//     public static XYZ GetElementCoordinate(Element e,
//         bool isAccordingToSurveyPont, bool isAccordingToProjectBasePoint,
//         bool isAccordingToInternalOrigin, BasePoint projectBasePoint,
//         double angle, double eastWest, double northSouth)
//     {
//         LocationPoint? locationPoint = e.Location as LocationPoint;
//         if (locationPoint == null) { return null; }
//
//         if (isAccordingToInternalOrigin) return locationPoint.Point;
//     }
// }
