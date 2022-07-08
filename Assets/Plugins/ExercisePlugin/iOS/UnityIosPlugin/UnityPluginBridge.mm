#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#include "UnityFramework/UnityFramework-Swift.h"

char* convertNSStringToCString(const NSString* nsString)
{
    if (nsString == NULL)
        return NULL;

    const char* nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);

    return cString;
}

extern "C" {
    
#pragma mark - Functions
    
    void _requestPermission(){
        [[LocationService shared] requestAuthorization];
    }
    
    void _startService() {        
        [[LocationService shared] start];
    }

    void _stopService() {
        [[LocationService shared] stop];
    }

    char* _getData() {
        NSArray *temp = [[LocationService shared] getData];
        NSError* error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:temp options:NSJSONWritingPrettyPrinted error:&error];
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        
        return convertNSStringToCString(jsonString);
    }

    char* _getRouteCoords() {
        NSArray *temp = [[LocationService shared] getRouteCoords];
        NSError* error = nil;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:temp options:NSJSONWritingPrettyPrinted error:&error];
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        
        return convertNSStringToCString(jsonString);
    }
    
    char* _hasPermission(){
        NSString *permissionState = [[LocationService shared] hasAuthorization];
        
        return convertNSStringToCString(permissionState);
    }

    void _openSettings () {
        NSURL * url = [NSURL URLWithString: UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL: url];
    }
}
