//
//  EMMCoordinateTransform.m
//  EMMCore
//
//  Created by Hequnjie on 16/9/20.
//  Copyright © 2016年 Nationsky. All rights reserved.
//

#import "EMMCoordinateTransform.h"

static double pi = 3.14159265358979324;
static double a = 6378245.0;  //WGS长轴半径
static double ee = 0.00669342162296594323;  //WGS偏心率的平方

EMMLocationCoordinate2D EMMLocationCoordinate2DMake(EMMLocationCoordinateType type, EMMLocationDegrees latitude, EMMLocationDegrees longitude) {
    EMMLocationCoordinate2D emmLocationCoordinate2D;
    emmLocationCoordinate2D.type = type;
    emmLocationCoordinate2D.latitude = latitude;
    emmLocationCoordinate2D.longitude = longitude;
    return emmLocationCoordinate2D;
}

double emm_transformLat(double x, double y) {
    double ret = -100.0+2.0* x +3.0* y +0.2* y * y +0.1* x * y +0.2*sqrt(fabs(x));
    ret += (20.0*sin(6.0* x *pi) +20.0*sin(2.0* x *pi)) *2.0/3.0;
    ret += (20.0*sin(y *pi) +40.0*sin(y /3.0*pi)) *2.0/3.0;
    ret += (160.0*sin(y /12.0*pi) +320*sin(y *pi/30.0)) *2.0/3.0;
    return ret;
}

double emm_transformLon(double x, double y) {
    double ret = 300.0+ x +2.0* y +0.1* x * x +0.1* x * y +0.1*sqrt(fabs(x));
    ret += (20.0*sin(6.0* x *pi) +20.0*sin(2.0* x *pi)) *2.0/3.0;
    ret += (20.0*sin(x *pi) +40.0*sin(x /3.0*pi)) *2.0/3.0;
    ret += (150.0*sin(x /12.0*pi) +300.0*sin(x /30.0*pi)) *2.0/3.0;
    return ret;
}

EMMLocationCoordinate2D _EMMCoordinateTransformCGJ02FromWGS84(EMMLocationCoordinate2D coordinate) {
    double dLat = emm_transformLat(coordinate.longitude - 105.0, coordinate.latitude - 35.0);
    double dLon = emm_transformLon(coordinate.longitude - 105.0, coordinate.latitude - 35.0);
    double radLat = coordinate.latitude / 180.0 * pi;
    double magic = sin(radLat);
    magic = 1 - ee * magic * magic;
    double sqrtMagic = sqrt(magic);
    dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
    dLon = (dLon * 180.0) / (a / sqrtMagic * cos(radLat) * pi);
    return EMMLocationCoordinate2DMake(EMMLocationCoordinateTypeCGJ02, coordinate.latitude + dLat, coordinate.longitude + dLon);
}

EMMLocationCoordinate2D _EMMCoordinateTransformWGS84FromCGJ02(EMMLocationCoordinate2D coordinate) {
    return EMMLocationCoordinate2DMake(EMMLocationCoordinateTypeWGS84,
                                       coordinate.latitude - (_EMMCoordinateTransformCGJ02FromWGS84(coordinate).latitude - coordinate.latitude),
                                       coordinate.longitude - (_EMMCoordinateTransformCGJ02FromWGS84(coordinate).longitude - coordinate.longitude));
}

const double x_pi = 3.14159265358979324 * 3000.0 / 180.0;

EMMLocationCoordinate2D _EMMCoordinateTransformBD09FromCGJ02(EMMLocationCoordinate2D coordinate) {
    double x = coordinate.longitude, y = coordinate.latitude;
    double z = sqrt(x * x + y * y) + 0.00002 * sin(y * x_pi);
    double theta = atan2(y, x) + 0.000003 * cos(x * x_pi);
    double bd_lon = z * cos(theta) + 0.0065;
    double bd_lat = z * sin(theta) + 0.006;
    return EMMLocationCoordinate2DMake(EMMLocationCoordinateTypeBD09, bd_lat, bd_lon);
}

EMMLocationCoordinate2D _EMMCoordinateTransformCGJ02FromBD09(EMMLocationCoordinate2D coordinate) {
    double x = coordinate.longitude - 0.0065, y = coordinate.latitude - 0.006;
    double z = sqrt(x * x + y * y) - 0.00002 * sin(y * x_pi);
    double theta = atan2(y, x) - 0.000003 * cos(x * x_pi);
    double gg_lon = z * cos(theta);
    double gg_lat = z * sin(theta);
    return EMMLocationCoordinate2DMake(EMMLocationCoordinateTypeCGJ02, gg_lat, gg_lon);
}

EMMLocationCoordinate2D _EMMCoordinateTransformBD09FromWGS84(EMMLocationCoordinate2D coordinate) {
    return _EMMCoordinateTransformBD09FromCGJ02(_EMMCoordinateTransformCGJ02FromWGS84(coordinate));
}

EMMLocationCoordinate2D _EMMCoordinateTransformWGS84FromBD09(EMMLocationCoordinate2D coordinate) {
    return _EMMCoordinateTransformWGS84FromCGJ02(_EMMCoordinateTransformCGJ02FromBD09(coordinate));
}

#pragma mark -

EMMLocationCoordinate2D EMMWGS84CoordinateTransform(EMMLocationCoordinate2D coordinate) {
    if (coordinate.type == EMMLocationCoordinateTypeWGS84) {
        return coordinate;
    } else if (coordinate.type == EMMLocationCoordinateTypeCGJ02) {
        return _EMMCoordinateTransformWGS84FromCGJ02(coordinate);
    } else if (coordinate.type == EMMLocationCoordinateTypeBD09) {
        return _EMMCoordinateTransformWGS84FromBD09(coordinate);
    }
    return EMMLocationCoordinate2DMake(0, 0, 0);
}

EMMLocationCoordinate2D EMMCGJ02CoordinateTransform(EMMLocationCoordinate2D coordinate) {
    if (coordinate.type == EMMLocationCoordinateTypeWGS84) {
        return _EMMCoordinateTransformCGJ02FromWGS84(coordinate);
    } else if (coordinate.type == EMMLocationCoordinateTypeCGJ02) {
        return coordinate;
    } else if (coordinate.type == EMMLocationCoordinateTypeBD09) {
        return _EMMCoordinateTransformCGJ02FromBD09(coordinate);
    }
    return EMMLocationCoordinate2DMake(0, 0, 0);
}

EMMLocationCoordinate2D EMMBD09CoordinateTransform(EMMLocationCoordinate2D coordinate) {
    if (coordinate.type == EMMLocationCoordinateTypeWGS84) {
        return _EMMCoordinateTransformBD09FromWGS84(coordinate);
    } else if (coordinate.type == EMMLocationCoordinateTypeCGJ02) {
        return _EMMCoordinateTransformBD09FromCGJ02(coordinate);
    } else if (coordinate.type == EMMLocationCoordinateTypeBD09) {
        return coordinate;
    }
    return EMMLocationCoordinate2DMake(0, 0, 0);
}

#pragma mark -

EMMLocationDistance EMMLocationsDistance(EMMLocationCoordinate2D firstLocationCoordinate, EMMLocationCoordinate2D secondLocationCoordinate) {
    EMMLocationCoordinate2D firstLocationWGS84 = EMMWGS84CoordinateTransform(firstLocationCoordinate);
    EMMLocationCoordinate2D secondLocationWGS84 = EMMWGS84CoordinateTransform(secondLocationCoordinate);
    CLLocation *firstLocation = [[CLLocation alloc] initWithLatitude:firstLocationWGS84.latitude longitude:firstLocationWGS84.longitude];
    CLLocation *secondLocation = [[CLLocation alloc] initWithLatitude:secondLocationWGS84.latitude longitude:secondLocationWGS84.longitude];
    return [firstLocation distanceFromLocation:secondLocation];
}


