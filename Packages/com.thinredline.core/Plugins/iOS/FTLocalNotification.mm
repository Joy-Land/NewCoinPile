//
//  FTLocalNotification.m
//  Unity-iPhone
//
//  Created by 程文 on 2017/11/24.
//

#import "FTLocalNotification.h"
#import "FTPluginCommon.h"
#import <UserNotifications/UserNotifications.h>
#define NOT_IOS10_OR_LATER ([[[UIDevice currentDevice] systemVersion] floatValue] < 10.0)
@implementation FTLocalNotification

+ (void)cancelNotificationByName:(NSString *)uidtodelete {
    UIApplication *app = [UIApplication sharedApplication];
    NSArray *eventArray = [app scheduledLocalNotifications];
    
    for (int i=0; i<[eventArray count]; i++)
    {
        UILocalNotification* oneEvent = [eventArray objectAtIndex:i];
        NSDictionary *userInfoCurrent = oneEvent.userInfo;
        NSString *uid = [userInfoCurrent valueForKey:@"uid"];
        //        NSLog(@"cevin get notification %@ %@", oneEvent.alertBody, userInfoCurrent);
        if (uid && [uid isEqualToString:uidtodelete])
        {
            //Cancelling local notification
            //            NSLog(@"cevin remove notification %@", uid);
            [app cancelLocalNotification:oneEvent];
            break;
        }
    }
    [UIApplication sharedApplication].applicationIconBadgeNumber = 0;
}

+ (void)cancelNotificationByName10:(NSString *)uidtodelete {
    if (NOT_IOS10_OR_LATER) {
        [FTLocalNotification cancelNotificationByName:uidtodelete];
        return;
    }
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    [center removePendingNotificationRequestsWithIdentifiers:@[uidtodelete]];
    [center removeDeliveredNotificationsWithIdentifiers:@[uidtodelete]];
}

+ (void)AddLocalNotification:(NSString *)uid content:(NSString *)content delaySecond:(int)delaySecond dailyLoop:(int)loop {
    UILocalNotification *notification = [[UILocalNotification alloc] init];
    notification.alertBody = content;
    notification.fireDate = [NSDate dateWithTimeIntervalSinceNow:delaySecond];
    notification.soundName = UILocalNotificationDefaultSoundName;
    notification.applicationIconBadgeNumber = 1;
    notification.hasAction = YES;
    if (loop == 1) {
        notification.repeatCalendar = NSCalendar.currentCalendar;
        notification.repeatInterval = NSDayCalendarUnit;
    }
    if (uid) {
        notification.userInfo = @{@"uid":uid};
    }
    //    NSLog(@"cevin add notification %@, %@", uid, content);
    [[UIApplication sharedApplication] scheduleLocalNotification:notification];
}

+ (void)AddLocalNotification10:(NSString *)uid content:(NSString *)content1 delaySecond:(int)delaySecond dailyLoop:(int)loop icon:(NSString*)icon {
    if (NOT_IOS10_OR_LATER) {
        [FTLocalNotification AddLocalNotification:uid content:content1 delaySecond:delaySecond dailyLoop:loop];
        return;
    }
    UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
    
    UNMutableNotificationContent *content = [UNMutableNotificationContent new];
    content.title = [FTLocalNotification getAppName];
    content.body = content1;
    content.sound = [UNNotificationSound defaultSound];
    content.categoryIdentifier = uid;
    NSString *iconPath = [NSString stringWithFormat:@"Data/Raw/notificationIcon/%@", icon];
    //    NSLog(@"cevin add local notification icon :%@", iconPath);
    NSURL *url = [NSURL fileURLWithPath:[[NSBundle mainBundle] pathForResource:iconPath ofType:@"png"]];
    if (url) {
        UNNotificationAttachment *attach = [UNNotificationAttachment attachmentWithIdentifier:icon URL:url options:nil error:nil];
        if (attach) {
            content.attachments = @[attach];
        }
    }
    UNNotificationRequest *request = [UNNotificationRequest requestWithIdentifier:uid content:content trigger:[UNTimeIntervalNotificationTrigger triggerWithTimeInterval:delaySecond repeats:NO]];
    
    [center addNotificationRequest:request withCompletionHandler:^(NSError * _Nullable error) {
        NSLog(@"cevin add local notification error %@",error);
    }];
    
}

+(NSString*) getAppName {
    return @"Golf Rival";//[[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleDisplayName"];
}
@end

#if defined (__cplusplus)
extern "C"
{
#endif
    void FTNotification_cancelLocalNotification(const char *uid) {
        NSString *uidStr = FTCreateNSString(uid);
        [FTLocalNotification cancelNotificationByName:uidStr];
    }
    
    void FTNotification_addNotification(const char *uid, const char *content, int delaySecond, int loop) {
        NSString *uidStr = FTCreateNSString(uid);
        NSString *contentStr = FTCreateNSString(content);
        [FTLocalNotification AddLocalNotification:uidStr content:contentStr delaySecond:delaySecond dailyLoop:loop];
    }
    
#if defined (__cplusplus)
}
#endif

