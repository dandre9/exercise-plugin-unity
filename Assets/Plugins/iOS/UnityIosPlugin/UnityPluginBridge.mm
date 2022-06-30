#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>
#include "UnityFramework/UnityFramework-Swift.h"

extern "C" {
    
#pragma mark - Functions
    
    void _addTwoNumberInIOS(int a , int b) {
       
        // int result = [[UnityPlugin shared] AddTwoNumberWithA:(a) b:(b)];
        [[LocationService shared] requestAuthorization];
        [[LocationService shared] start];
        // return result;
    }
}