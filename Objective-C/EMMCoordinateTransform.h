//
//  EMMCoordinateTransform.h
//  EMMCore
//
//  Created by Hequnjie on 16/9/20.
//  Copyright © 2016年 Nationsky. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>


typedef NS_ENUM(NSInteger, EMMLocationCoordinateType) {
    EMMLocationCoordinateTypeWGS84 = 0,  //GPS
    EMMLocationCoordinateTypeCGJ02,  //国测局
    EMMLocationCoordinateTypeBD09  //百度
};

typedef double EMMLocationDegrees;
typedef double EMMLocationDistance;

typedef struct {
    EMMLocationCoordinateType type;
    EMMLocationDegrees latitude;
    EMMLocationDegrees longitude;
} EMMLocationCoordinate2D;

EMMLocationCoordinate2D EMMLocationCoordinate2DMake(EMMLocationCoordinateType type, EMMLocationDegrees latitude, EMMLocationDegrees longitude);

EMMLocationCoordinate2D EMMWGS84CoordinateTransform(EMMLocationCoordinate2D coordinate);
EMMLocationCoordinate2D EMMCGJ02CoordinateTransform(EMMLocationCoordinate2D coordinate);
EMMLocationCoordinate2D EMMBD09CoordinateTransform(EMMLocationCoordinate2D coordinate);

EMMLocationDistance EMMLocationsDistance(EMMLocationCoordinate2D firstLocationCoordinate, EMMLocationCoordinate2D secondLocationCoordinate);
