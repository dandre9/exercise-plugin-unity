#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern UIViewController *UnityGetGLViewController();

@interface IOSPlugin : NSObject

@end

@implementation IOSPlugin

+(void)alertView:(NSString *)title addMessage:(NSString*) message
{
    UIAlertController *alert = [UIAlertController alertControllerWithTitle:title
                    message:message
                    preferredStyle:UIAlertControllerStyleAlert];

    UIAlertAction *defaultAction = [UIAlertAction actionWithTitle:@"CERTO" style:UIAlertActionStyleDefault
                    handler:^(UIAlertAction *action){}];

    [alert addAction:defaultAction];
    [UnityGetGLViewController() presentViewController:alert animated:YES completion:nil];
}

@end

extern "C"
{
    void _ShowAlert(const char *title, const char *message)
    {
        [IOSPlugin alertView:[NSString stringWithUTF8String:title] addMessage:[NSString stringWithUTF8String:message]];
    }
}
