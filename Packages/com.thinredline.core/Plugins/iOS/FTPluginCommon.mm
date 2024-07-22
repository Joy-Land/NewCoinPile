//
//  FTPluginCommon.m
//  Unity-iPhone
//
//  Created by 程文 on 2017/11/6.
//

#import "FTPluginCommon.h"

@implementation FTPluginCommon

@end


#if defined (__cplusplus)
extern "C"
{
#endif
    NSString* FTCreateNSString (const char* string)
    {
        if (string)
            return [NSString stringWithUTF8String: string];
        else
            return [NSString stringWithUTF8String: ""];
    }
    char* FTMakeStringCopy (const char* string)
    {
        
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        
        strcpy(res, string);
        
        return res;
        
    }
    char *FTGetCurrentLanguageCode()
    {
        NSString *languageCode = [[NSLocale preferredLanguages] firstObject];
        return FTMakeStringCopy([languageCode UTF8String]);
    }
#if defined (__cplusplus)
}
#endif
