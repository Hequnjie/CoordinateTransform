using System;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace EMMUtilities
{

    public enum EMMLocationCoordinateType
    {
        WGS84 = 0,
        CGJ02 = 1,
        BD09 = 2
    }

    public struct EMMLocationCoordinate2D
    {
        public EMMLocationCoordinateType type;
        public double latitude;
        public double longitude;
    };

    public class EMMCoordinateTransform
    {
        static double pi = 3.14159265358979324;
        static double a = 6378245.0;  //WGS长轴半径
        static double ee = 0.00669342162296594323;  //WGS偏心率的平方

        public async System.Threading.Tasks.Task<bool> isInChina(Geopoint pointToReverseGeocode)
        {
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(pointToReverseGeocode);

            if (result.Status == MapLocationFinderStatus.Success)
            {
                string CountryCode = result.Locations[0].Address.CountryCode;
                if (CountryCode.Equals("CHN"))
                    return true;
            }
            return false;
        }

        public EMMLocationCoordinate2D EMMLocationCoordinate2DMake(EMMLocationCoordinateType type, double latitude, double longitude)
        {
            EMMLocationCoordinate2D emmLocationCoordinate2D;
            emmLocationCoordinate2D.type = type;
            emmLocationCoordinate2D.latitude = latitude;
            emmLocationCoordinate2D.longitude = longitude;
            return emmLocationCoordinate2D;
        }

        public EMMLocationCoordinate2D EMMWGS84CoordinateTransform(EMMLocationCoordinate2D coordinate)
        {
            if (coordinate.type == EMMLocationCoordinateType.WGS84)
            {
                return coordinate;
            }
            else if (coordinate.type == EMMLocationCoordinateType.CGJ02)
            {
                return _EMMCoordinateTransformWGS84FromCGJ02(coordinate);
            }
            else if (coordinate.type == EMMLocationCoordinateType.BD09)
            {
                return _EMMCoordinateTransformWGS84FromBD09(coordinate);
            }
            return EMMLocationCoordinate2DMake(0, 0, 0);
        }

        public EMMLocationCoordinate2D EMMCGJ02CoordinateTransform(EMMLocationCoordinate2D coordinate)
        {
            if (coordinate.type == EMMLocationCoordinateType.WGS84)
            {
                return _EMMCoordinateTransformCGJ02FromWGS84(coordinate);
            }
            else if (coordinate.type == EMMLocationCoordinateType.CGJ02)
            {
                return coordinate;
            }
            else if (coordinate.type == EMMLocationCoordinateType.BD09)
            {
                return _EMMCoordinateTransformCGJ02FromBD09(coordinate);
            }
            return EMMLocationCoordinate2DMake(0, 0, 0);
        }

        public EMMLocationCoordinate2D EMMBD09CoordinateTransform(EMMLocationCoordinate2D coordinate)
        {
            if (coordinate.type == EMMLocationCoordinateType.WGS84)
            {
                return _EMMCoordinateTransformBD09FromWGS84(coordinate);
            }
            else if (coordinate.type == EMMLocationCoordinateType.CGJ02)
            {
                return _EMMCoordinateTransformBD09FromCGJ02(coordinate);
            }
            else if (coordinate.type == EMMLocationCoordinateType.BD09)
            {
                return coordinate;
            }
            return EMMLocationCoordinate2DMake(0, 0, 0);
        }

        double emm_transformLat(double x, double y)
        {
            double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(y * pi) + 40.0 * Math.Sin(y / 3.0 * pi)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(y / 12.0 * pi) + 320 * Math.Sin(y * pi / 30.0)) * 2.0 / 3.0;
            return ret;
        }

        double emm_transformLon(double x, double y)
        {
            double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
            ret += (20.0 * Math.Sin(6.0 * x * pi) + 20.0 * Math.Sin(2.0 * x * pi)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(x * pi) + 40.0 * Math.Sin(x / 3.0 * pi)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(x / 12.0 * pi) + 300.0 * Math.Sin(x / 30.0 * pi)) * 2.0 / 3.0;
            return ret;
        }

        EMMLocationCoordinate2D _EMMCoordinateTransformCGJ02FromWGS84(EMMLocationCoordinate2D coordinate)
        {
            double dLat = emm_transformLat(coordinate.longitude - 105.0, coordinate.latitude - 35.0);
            double dLon = emm_transformLon(coordinate.longitude - 105.0, coordinate.latitude - 35.0);
            double radLat = coordinate.latitude / 180.0 * pi;
            double magic = Math.Sin(radLat);
            magic = 1 - ee * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
            dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
            return EMMLocationCoordinate2DMake(EMMLocationCoordinateType.CGJ02, coordinate.latitude + dLat, coordinate.longitude + dLon);
        }

        EMMLocationCoordinate2D _EMMCoordinateTransformWGS84FromCGJ02(EMMLocationCoordinate2D coordinate)
        {
            return EMMLocationCoordinate2DMake(EMMLocationCoordinateType.WGS84,
                                               coordinate.latitude - (_EMMCoordinateTransformCGJ02FromWGS84(coordinate).latitude - coordinate.latitude),
                                               coordinate.longitude - (_EMMCoordinateTransformCGJ02FromWGS84(coordinate).longitude - coordinate.longitude));
        }

        const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;

        EMMLocationCoordinate2D _EMMCoordinateTransformBD09FromCGJ02(EMMLocationCoordinate2D coordinate)
        {
            double x = coordinate.longitude, y = coordinate.latitude;
            double z = Math.Sqrt(x * x + y * y) + 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) + 0.000003 * Math.Cos(x * x_pi);
            double bd_lon = z * Math.Cos(theta) + 0.0065;
            double bd_lat = z * Math.Sin(theta) + 0.006;
            return EMMLocationCoordinate2DMake(EMMLocationCoordinateType.BD09, bd_lat, bd_lon);
        }

        EMMLocationCoordinate2D _EMMCoordinateTransformCGJ02FromBD09(EMMLocationCoordinate2D coordinate)
        {
            double x = coordinate.longitude - 0.0065, y = coordinate.latitude - 0.006;
            double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * x_pi);
            double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * x_pi);
            double gg_lon = z * Math.Cos(theta);
            double gg_lat = z * Math.Sin(theta);
            return EMMLocationCoordinate2DMake(EMMLocationCoordinateType.CGJ02, gg_lat, gg_lon);
        }

        EMMLocationCoordinate2D _EMMCoordinateTransformBD09FromWGS84(EMMLocationCoordinate2D coordinate)
        {
            return _EMMCoordinateTransformBD09FromCGJ02(_EMMCoordinateTransformCGJ02FromWGS84(coordinate));
        }

        EMMLocationCoordinate2D _EMMCoordinateTransformWGS84FromBD09(EMMLocationCoordinate2D coordinate)
        {
            return _EMMCoordinateTransformWGS84FromCGJ02(_EMMCoordinateTransformCGJ02FromBD09(coordinate));
        }
    }
}
