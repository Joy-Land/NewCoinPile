//
//  NativeUtils.m
//  UnityFramework
//
//  Created by fotoable on 2020/12/7.
//
#import <KeyChain.h>
#import <AudioToolbox/AudioToolbox.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>

const char * nativeUtilsListenerObjName = "NativeUtilsListener";

void saveUDID(const char *str){
    NSLog(@"save:%s",str);
    NSString *str2=[[NSString alloc]initWithCString:(const char *) str encoding:NSASCIIStringEncoding];
    [[RHKeyChain SingleTon] rhKeyChainSave: str2];
}


char * getUDID(){
    //NSLog(@"get");
    return [[RHKeyChain SingleTon] rhKeyChainLoad];
    //const char* s = [str2 UTF8String];
    //NSLog(@"get:%@",str2);
}

char* MakeStringCopy1 (const char* string)
{
    
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    
    strcpy(res, string);
    
    return res;
    
}

char * GetCurrLanguage(){
    NSString *localeLanguageCode = [[NSLocale preferredLanguages] firstObject];
    //NSLog(@"Preferred Language:%@", localeLanguageCode);
    return MakeStringCopy1([localeLanguageCode cStringUsingEncoding:NSASCIIStringEncoding]);
}

char * GetCurrCountry(){
    NSString *localeCountryCode = [[NSLocale currentLocale] objectForKey:NSLocaleCountryCode];
    //NSLog(@"Preferred Language:%@", localeLanguageCode);
    return MakeStringCopy1([localeCountryCode cStringUsingEncoding:NSASCIIStringEncoding]);
}

char * GetCurrCurrencyCode(){
    NSString *localCurrencySymble = [[NSLocale currentLocale] objectForKey:NSLocaleCurrencyCode];
    //    NSLog(@"local currency sym %@", localCurrencySymble);
    return MakeStringCopy1([localCurrencySymble cStringUsingEncoding:NSASCIIStringEncoding]);
}

char *getIDFA() {
	return [[RHKeyChain SingleTon] getIDFA];
}

void performVibrate(){
    AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);//默认震动效果
}

void RequestATTAuthorization(){
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            UnitySendMessage(nativeUtilsListenerObjName, "OnRequestATTPermissionFinished", [[NSString stringWithFormat:@"%d", (int)status] cStringUsingEncoding:NSUTF8StringEncoding]);
        }];
    } else {
        UnitySendMessage(nativeUtilsListenerObjName, "OnRequestATTPermissionFinished", [[NSString stringWithFormat:@"%d", 3] cStringUsingEncoding:NSUTF8StringEncoding]);
    }
}

bool CheckIsUserDarkMode(){
    if (@available(iOS 12, *)) {
        if(UIScreen.mainScreen.traitCollection.userInterfaceStyle == UIUserInterfaceStyleDark) {
            return true;
        }
    }
    return false;
}