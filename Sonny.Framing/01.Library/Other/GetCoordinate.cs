// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace SonnyBIM;

public class GetCoordinate
{
    public static XYZ GetElementCoordinate(Element element,
        bool isSurveyPoint, bool isProjectBasePoint,
        bool isInternalOrigin, BasePoint projectBasePoint,
        double angle, double eastWest, double northSouth)
    {
        LocationPoint? locationPoint = element.Location as LocationPoint;
        if (locationPoint == null) { return null; }

        if (isInternalOrigin) return locationPoint.Point;
        double versionNumber = Convert.ToDouble(element.Document.Application.VersionNumber);
        if (versionNumber > 2019 && isProjectBasePoint)
        {
            // nếu nhỏ 2019 thì comment lại
                return locationPoint.Point.Subtract(projectBasePoint.Position);
        }
        if (isSurveyPoint)
        {
            var point = locationPoint.Point;
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            var rotatedX = point.X * cos - point.Y * sin;
            var rotatedY = point.X * sin + point.Y * cos;

            var xCoordinate = eastWest + rotatedX;
            var yCoordinate = northSouth + rotatedY;

            return new XYZ(xCoordinate, yCoordinate, 0);
        }
        return null;
    }
    public static XYZ TransferCoordinate(XYZ point, string revitVersion,
        bool isSurveyPoint, bool isProjectBasePoint,
        bool isInternalOrigin, BasePoint projectBasePoint,
        double angleToTrueNorth, double eastWest, double northSouth, double elevation)
    {
        if (point == null) return null;
        if (isInternalOrigin) return point;
        var versionNumber = Convert.ToDouble(revitVersion);

        if (isSurveyPoint)
        {
            var cos = Math.Cos(angleToTrueNorth);
            var sin = Math.Sin(angleToTrueNorth);

            var rotatedX = point.X * cos - point.Y * sin;
            var rotatedY = point.X * sin + point.Y * cos;

            var xCoordinate = eastWest + rotatedX;
            var yCoordinate = northSouth + rotatedY;
            var zCoordinate = elevation + point.Z;
            return new XYZ(xCoordinate, yCoordinate, zCoordinate);
        }
        return null;
    }
}
