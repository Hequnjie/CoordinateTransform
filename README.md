# CoordinateTransform
GPS  CGJ02 BD09 transform. Objective-C &amp; C#（Windows）


# Example

# Objective-C 
NSArray<CLLocation *> *locations;
CLLocationCoordinate2D coordinate = locations.firstObject.coordinate;
EMMLocationCoordinate2D emmWGSCoor = EMMLocationCoordinate2DMake(EMMLocationCoordinateTypeWGS84, coordinate.latitude, coordinate.longitude);
EMMLocationCoordinate2D emmCoor = EMMCGJ02CoordinateTransform(emmWGSCoor);
NSLog(@"[[DeLog] GCJ-02] %f, %f", emmCoor.longitude, emmCoor.latitude);
emmCoor = EMMBD09CoordinateTransform(emmWGSCoor);
NSLog(@"[[DeLog] bd09] %f, %f", emmCoor.longitude, emmCoor.latitude);
    
    

# C#（Windows）
EMMCoordinateTransform coordinateTransform = new EMMCoordinateTransform();
EMMLocationCoordinate2D bdo9 = coordinateTransform.EMMBD09CoordinateTransform(coordinateTransform.EMMLocationCoordinate2DMake(EMMLocationCoordinateType.WGS84, point.Position.Latitude, point.Position.Longitude));
BasicGeoposition bdo9Geoposition = new BasicGeoposition();
bdo9Geoposition.Latitude = bdo9.latitude;
bdo9Geoposition.Longitude = bdo9.longitude;
Geopoint geopoint new Geopoint(bdo9Geoposition);

EMMLocationCoordinate2D cgj02 = coordinateTransform.EMMCGJ02CoordinateTransform(coordinateTransform.EMMLocationCoordinate2DMake(EMMLocationCoordinateType.WGS84, point.Position.Latitude, point.Position.Longitude));
BasicGeoposition cgj02Geoposition = new BasicGeoposition();
cgj02Geoposition.Latitude = cgj02.latitude;
cgj02Geoposition.Longitude = cgj02.longitude;
mapShowpoint = new Geopoint(cgj02Geoposition);
